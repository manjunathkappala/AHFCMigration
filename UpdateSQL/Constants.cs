using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateSQL
{
    public class Constants
    {
        public const string IR_ARTICLE_IMAGE = "ir_article_image";
        public const string IR_ARTICLE = "ir_article";

        public const string DOCUMENTUM_I_CHRONICLE_ID = "documentum_i_chronicle_id";
        public const string DOCUMENTUM_R_OBJECT_ID = "documentum_r_object_id";
        public const string I_CHRONICLE_ID = "i_chronicle_id";
        public const string R_OBJECT_ID = "r_object_id";

        public const string CONTENT_ID = "content_id";
        public const string R_FOLDER_PATH = "r_folder_path";
        public const string I_FULL_FORMAT = "i_full_format";
        public const string R_OBJECT_TYPE = "r_object_type";

        public const string FILE_PATH = "Filepath";
        public const string CONNECTION_STRING = "connectionString";

        //SQL PARAMETERS
        public const string SP_I_CHRONICLE_ID = "@i_chronicle_id";
        public const string SP_R_OBJECT_ID = "@r_object_id";
        public const string SP_CONTENT_ID = "@content_id";
        public const string SP_R_FOLDER_PATH = "@r_folder_path";
        public const string SP_A_WEBC_URL = "@a_webc_url";
        public const string SP_TITLE = "@title";
        public const string SP_OBJECT_NAME = "@object_name";
        public const string SP_DISPLAY_ORDER = "@display_order";

        //used in where condition
        public const string SP_R_OBJECT_TYPE = "@r_object_type";
        public const string SP_I_FULL_FORMAT = "@i_full_format";
        public const string SP_DOCUMENTUM_R_OBJECT_ID = "@documentum_r_object_id";
        public const string SP_DOCUMENTUM_I_CHRONICLE_ID = "@documentum_i_chronicle_id";

    }
}
