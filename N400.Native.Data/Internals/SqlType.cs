using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Native.Data.Internals
{
    internal enum SqlType : short
    {
        CHAR            =    1,
        NUMERIC         =    2,
        DECIMAL         =    3,
        INTEGER         =    4,
        SMALLINT        =    5,
        FLOAT           =    6,
        REAL            =    7,
        DOUBLE          =    8,
        DATETIME        =    9,
        VARCHAR         =   12, /* LONGVARCHAR */
        BLOB            =   13,
        CLOB            =   14,
        DBCLOB          =   15,
        DATALINK        =   16,
        WCHAR           =   17,
        WVARCHAR        =   18, /* also WLONGVARCHAR */
        BIGINT          =   19,
        BLOB_LOCATOR    =   20,
        CLOB_LOCATOR    =   21,
        DBCLOB_LOCATOR  =   22,
        UTF8_CHAR       =   23,
        GRAPHIC         =   95,
        VARGRAPHIC      =   96, /* also LONGVARGRAPHIC */
        BINARY          =   -2,
        VARBINARY       =   -3, /* also LONGVARBINARY */
        DATE            =   91,
        TIME            =   92,
        TIMESTAMP       =   93,
        //CODE_DATE       =    1,
        //CODE_TIME       =    2,
        //CODE_TIMESTAMP  =    3,
        ALL_TYPES       =    0,
        DECFLOAT        = -360,
        XML             = -370,
    }
}
