
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace UpdateSQL
{
    public class Program
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static string Filepath = ConfigurationManager.AppSettings[Constants.FILE_PATH];
        static string connectionString = ConfigurationManager.AppSettings[Constants.CONNECTION_STRING];


        static void Main(string[] args)
        {
            string documentum_i_chronicle_id = string.Empty, documentum_r_object_id = string.Empty, i_chronicle_id = string.Empty, a_webc_url = string.Empty, title = string.Empty, display_order = string.Empty,
            r_object_id = string.Empty, content_id = string.Empty, r_folder_path = string.Empty, i_full_format = string.Empty, r_object_type = string.Empty;
            log4net.GlobalContext.Properties[Constants.R_OBJECT_ID] = string.Empty;
            try
            {
                DataTable csvData = ConvertCSVtoDataTable(Filepath);
                log.Info($"Number of rows in CSV file : - {csvData.Rows.Count}");
                SqlConnection sqlConnection = new SqlConnection(connectionString);

                foreach (DataRow dataRow in csvData.Rows)
                {
                    sqlConnection.Open();
                    documentum_i_chronicle_id = Convert.ToString(dataRow[Constants.DOCUMENTUM_I_CHRONICLE_ID]);
                    documentum_r_object_id = Convert.ToString(dataRow[Constants.DOCUMENTUM_R_OBJECT_ID]);
                    log4net.GlobalContext.Properties[Constants.R_OBJECT_ID] = documentum_r_object_id;
                    i_chronicle_id = Convert.ToString(dataRow[Constants.I_CHRONICLE_ID]);
                    r_object_id = Convert.ToString(dataRow[Constants.R_OBJECT_ID]);

                    content_id = Convert.ToString(dataRow[Constants.CONTENT_ID]);
                    r_folder_path = Convert.ToString(dataRow[Constants.R_FOLDER_PATH]);
                    i_full_format = Convert.ToString(dataRow[Constants.I_FULL_FORMAT]);
                    r_object_type = Convert.ToString(dataRow[Constants.R_OBJECT_TYPE]);

                    int rowCount = GetDataFromSQL(sqlConnection, documentum_i_chronicle_id, documentum_r_object_id, i_full_format, r_object_type);

                    if (rowCount > 0)
                    {
                        if (rowCount == 1)
                        {
                            if (r_object_type.ToUpper() != Constants.IR_ARTICLE.ToUpper())
                                UpdateDB(sqlConnection, i_chronicle_id, r_object_id, content_id, r_folder_path, r_object_type, i_full_format, documentum_r_object_id, documentum_i_chronicle_id);
                            else
                            {
                                a_webc_url = Convert.ToString(dataRow["a_webc_url"]);
                                UpdateDB_Articles(sqlConnection, i_chronicle_id, r_object_id, content_id, r_folder_path, r_object_type, i_full_format, a_webc_url, documentum_r_object_id, documentum_i_chronicle_id);
                            }
                        }
                        else
                            log.Debug($"SQL data Row count more than 1 - RowCount : {rowCount} for documentum_r_object_id  : {documentum_r_object_id},r_object_id : {r_object_id}, i_chronicle_id : {i_chronicle_id}, content_id : {content_id}, r_folder_path : {r_folder_path}");
                    }
                    else
                    {
                        if (r_object_type.ToUpper() == Constants.IR_ARTICLE_IMAGE.ToUpper())
                        {
                            a_webc_url = Convert.ToString(dataRow["a_webc_url"]);
                            title = dataRow["title"].ToString();
                            display_order = !string.IsNullOrEmpty(dataRow["display_order"].ToString()) ? dataRow["display_order"].ToString() : "0";
                            InsertDB(sqlConnection, r_object_id, content_id, r_object_type, r_folder_path, i_full_format, a_webc_url, title, documentum_r_object_id, i_chronicle_id, display_order);
                        }
                        else
                            log.Info($"For the given for documentum_r_object_id : {documentum_r_object_id}, content not available in sql r_object_id : {r_object_id}, i_chronicle_id : {i_chronicle_id}, content_id : {content_id}, r_folder_path : {r_folder_path}");
                    }
                    sqlConnection.Close();
                }

            }
            catch (Exception ex)
            {
                log.Error($"Exception occurred in Main method :{ex.Message} ,Details: {ex.InnerException}");
            }

        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable csvdatarows = new DataTable();
            try
            {

                using (StreamReader streamReader = new StreamReader(strFilePath))
                {
                    string[] headers = streamReader.ReadLine().Split(',');
                    foreach (string header in headers)
                    {
                        csvdatarows.Columns.Add(header);
                    }
                    while (!streamReader.EndOfStream)
                    {
                        string[] rows = streamReader.ReadLine().Split(',');
                        DataRow dr = csvdatarows.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                        }
                        csvdatarows.Rows.Add(dr);
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception occurred in ConvertCSVtoDataTable method :{ex.Message} ,Details: {ex.InnerException}");
            }

            return csvdatarows;
        }

        public static int GetDataFromSQL(SqlConnection sqlConnection, string documentum_i_chronicle_id, string documentum_r_object_id, string i_full_format, string r_object_type)
        {
            int count = 0;
            try
            {
                using (SqlCommand command = new SqlCommand("Select COUNT (*) from dbo.ahfc_s_table_Migration Where r_object_id = @documentum_r_object_id and i_chronicle_id = @documentum_i_chronicle_id and i_full_format= @i_full_format and r_object_type =@r_object_type", sqlConnection))
                {
                    command.Parameters.AddWithValue(Constants.SP_R_OBJECT_TYPE, r_object_type);
                    command.Parameters.AddWithValue(Constants.SP_I_FULL_FORMAT, i_full_format);
                    command.Parameters.AddWithValue(Constants.SP_DOCUMENTUM_R_OBJECT_ID, documentum_r_object_id);
                    command.Parameters.AddWithValue(Constants.SP_DOCUMENTUM_I_CHRONICLE_ID, documentum_i_chronicle_id);
                    count = (int)command.ExecuteScalar();

                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception occurred in GetDataFromSQL method :{ex.Message} ,Details: {ex.InnerException}");
            }
            return count;
        }

        public static void UpdateDB(SqlConnection sqlConnection, string i_chronicle_id, string r_object_id, string content_id, string r_folder_path, string r_object_type, string i_full_format, string documentum_r_object_id, string documentum_i_chronicle_id)
        {
            try
            {
                log.Info($"In UpdateDB Method for documentum_r_object_id  : {documentum_r_object_id}");
                using (SqlCommand command = sqlConnection.CreateCommand())
                {
                    command.CommandText = "UPDATE dbo.ahfc_s_table_Migration SET r_object_id = @r_object_id, i_chronicle_id = @i_chronicle_id,content_id = @content_id,r_folder_path = @r_folder_path Where r_object_id = @documentum_r_object_id and i_chronicle_id = @documentum_i_chronicle_id and i_full_format= @i_full_format and r_object_type =@r_object_type";
                    //update column in sql
                    command.Parameters.AddWithValue(Constants.SP_I_CHRONICLE_ID, i_chronicle_id);
                    command.Parameters.AddWithValue(Constants.SP_R_OBJECT_ID, r_object_id);
                    command.Parameters.AddWithValue(Constants.SP_CONTENT_ID, content_id);
                    command.Parameters.AddWithValue(Constants.SP_R_FOLDER_PATH, r_folder_path);
                    //used in where condition
                    command.Parameters.AddWithValue(Constants.SP_R_OBJECT_TYPE, r_object_type);
                    command.Parameters.AddWithValue(Constants.SP_I_FULL_FORMAT, i_full_format);
                    command.Parameters.AddWithValue(Constants.SP_DOCUMENTUM_R_OBJECT_ID, documentum_r_object_id);
                    command.Parameters.AddWithValue(Constants.SP_DOCUMENTUM_I_CHRONICLE_ID, documentum_i_chronicle_id);
                    command.ExecuteNonQuery();
                    log.Debug($"Updated SQL data successfully for documentum_r_object_id  : {documentum_r_object_id}, r_object_id : {r_object_id}, i_chronicle_id : {i_chronicle_id}, content_id : {content_id}, r_folder_path : {r_folder_path}");

                }
            }
            catch (Exception ex)
            {
                log.Debug($"Exception in UpdateDB Method for documentum_r_object_id  : {documentum_r_object_id}, r_object_id : {r_object_id}, i_chronicle_id : {i_chronicle_id}, content_id : {content_id}, r_folder_path : {r_folder_path}, Error : {ex.Message}");
            }
        }

        public static void UpdateDB_Articles(SqlConnection sqlConnection, string i_chronicle_id, string r_object_id, string content_id, string r_folder_path, string r_object_type, string i_full_format, string a_webc_url, string documentum_r_object_id, string documentum_i_chronicle_id)
        {
            try
            {
                log.Info($"In UpdateDB Method for documentum_r_object_id  : {documentum_r_object_id}");
                using (SqlCommand command = sqlConnection.CreateCommand())
                {
                    command.CommandText = "UPDATE dbo.ahfc_s_table_Migration SET r_object_id = @r_object_id, i_chronicle_id = @i_chronicle_id,content_id = @content_id,r_folder_path = @r_folder_path,a_webc_url = @a_webc_url Where r_object_id = @documentum_r_object_id and i_chronicle_id = @documentum_i_chronicle_id and i_full_format= @i_full_format and r_object_type =@r_object_type";
                    //update column in sql
                    command.Parameters.AddWithValue(Constants.SP_I_CHRONICLE_ID, i_chronicle_id);
                    command.Parameters.AddWithValue(Constants.SP_R_OBJECT_ID, r_object_id);
                    command.Parameters.AddWithValue(Constants.SP_CONTENT_ID, content_id);
                    command.Parameters.AddWithValue(Constants.SP_R_FOLDER_PATH, r_folder_path);
                    command.Parameters.AddWithValue(Constants.SP_A_WEBC_URL, a_webc_url);
                    //used in where condition
                    command.Parameters.AddWithValue(Constants.SP_R_OBJECT_TYPE, r_object_type);
                    command.Parameters.AddWithValue(Constants.SP_I_FULL_FORMAT, i_full_format);
                    command.Parameters.AddWithValue(Constants.SP_DOCUMENTUM_R_OBJECT_ID, documentum_r_object_id);
                    command.Parameters.AddWithValue(Constants.SP_DOCUMENTUM_I_CHRONICLE_ID, documentum_i_chronicle_id);
                    command.ExecuteNonQuery();
                    log.Debug($"Updated SQL data successfully for documentum_r_object_id  : {documentum_r_object_id}, r_object_id : {r_object_id}, i_chronicle_id : {i_chronicle_id}, content_id : {content_id}, r_folder_path : {r_folder_path}, a_webc_url : {a_webc_url}");

                }
            }
            catch (Exception ex)
            {
                log.Debug($"Exception in UpdateDB Method for documentum_r_object_id  : {documentum_r_object_id}, r_object_id : {r_object_id}, i_chronicle_id : {i_chronicle_id}, content_id : {content_id}, r_folder_path : {r_folder_path}, Error : {ex.Message}");
            }
        }

        public static void InsertDB(SqlConnection sqlConnection, string r_object_id, string content_id, string r_object_type, string r_folder_path, string i_full_format, string a_webc_url, string title, string documentum_r_object_id, string i_chronicle_id, string display_order)
        {
            string query = string.Empty;
            try
            {
                query = "INSERT INTO dbo.ahfc_s_table_Migration (r_object_id, i_chronicle_id, content_id, r_object_type, r_folder_path,  i_full_format,a_webc_url,title,object_name,display_order)";
                query += " VALUES (@r_object_id, @i_chronicle_id, @content_id, @r_object_type, @r_folder_path, @i_full_format,@a_webc_url,@title,@object_name,@display_order)";

                using (SqlCommand command = sqlConnection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddWithValue(Constants.SP_R_OBJECT_ID, r_object_id);
                    command.Parameters.AddWithValue(Constants.SP_I_CHRONICLE_ID, r_object_id);
                    command.Parameters.AddWithValue(Constants.SP_CONTENT_ID, content_id);
                    command.Parameters.AddWithValue(Constants.SP_R_OBJECT_TYPE, r_object_type);
                    command.Parameters.AddWithValue(Constants.SP_R_FOLDER_PATH, r_folder_path);
                    command.Parameters.AddWithValue(Constants.SP_I_FULL_FORMAT, i_full_format);
                    command.Parameters.AddWithValue(Constants.SP_A_WEBC_URL, a_webc_url);
                    command.Parameters.AddWithValue(Constants.SP_TITLE, title);
                    command.Parameters.AddWithValue(Constants.SP_OBJECT_NAME, title);
                    command.Parameters.AddWithValue(Constants.SP_DISPLAY_ORDER, display_order);
                    command.ExecuteNonQuery();
                    log.Debug($"Inserted SQL data successfully for documentum_r_object_id  : {documentum_r_object_id}, r_object_id : {r_object_id}, i_chronicle_id : {i_chronicle_id}, content_id : {content_id}, r_folder_path : {r_folder_path}");
                }
            }
            catch (Exception ex)
            {
                log.Debug($"Exception in InsertDB Method for documentum_r_object_id  : {documentum_r_object_id}, r_object_id : {r_object_id}, i_chronicle_id : {i_chronicle_id}, content_id : {content_id}, r_folder_path : {r_folder_path}, Error : {ex.Message}");
            }
        }
    }

}
