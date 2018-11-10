using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace N400.Native.Data.Internals
{
    // https://www.ibm.com/support/knowledgecenter/ssw_ibm_i_73/cli/rzadphdhed.htm
    internal static class SqlCli
    {
        internal const short SQL_NTS = -3; // Null terminated string (for length)
        internal const short SQL_MAX_MESSAGE_LENGTH = 512;

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLOverrideCCSID400(int ccsid);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLError(int env, int connect, int stmt, StringBuilder sqlState, out int nativeErr, StringBuilder errorMessage, short messageMax, out short outLength); // This last param is weird?

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLAllocEnv(out int env);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLFreeEnv(int env);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLReleaseEnv(int env);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLAllocConnect(int env, out int connect);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLFreeConnect(int connect);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLConnect(int connect, string server, short serverLen, string user, short userLen, string password, short passwordLen);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLDisconnect(int connect);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLAllocStmt(int connect, out int stmt);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLFreeStmt(int stmt, SqlFreeStmtOptions options);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLCancel(int stmt);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLCloseCursor(int stmt);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLExecDirect(int stmt, string sql, int sqlLen);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLNumResultCols(int stmt, out short cols);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLRowCount(int stmt, out int rowCount);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLDescribeCol(int stmt, short col, StringBuilder name, short nameLen, out short nameOutLen, out SqlType type, out int precision, out short scale, out short nullable);

        // Very overloadable
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLBindCol(int stmt, short col, short type, IntPtr value, int valueLen, out int outLen);
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLBindCol(int stmt, short col, short type, IntPtr value, int valueLen, IntPtr outLen);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLGetCol(int stmt, short col, SqlType type, IntPtr value, int valueLen, out int outLen);
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLGetCol(int stmt, short col, SqlType type, StringBuilder value, int valueLen, out int outLen);
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLGetCol(int stmt, short col, SqlType type, out int value, int valueLen, out int outLen);

        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLFetch(int stmt);

        // IBM docs say we should be using SQLColAttribute instead, but it has an ambigious API and isn't bound by libdb400.
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLColAttributes(int stmt, short column, short field, StringBuilder stringBuf, int stringBufLen, out int outStringBufLen, out int numericAttrib);

        // Overloadable but with limited possibilities, out on infoValue should do, but IBM implies it may also act as input?
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLGetInfo(int connect, short infoType, out short infoValue, short infoValueLen, out short outInfoValueLen);
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLGetInfo(int connect, short infoType, out int infoValue, short infoValueLen, out short outInfoValueLen);
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLGetInfo(int connect, short infoType, StringBuilder infoValue, short infoValueLen, out short outInfoValueLen);
        [DllImport("libdb400")]
        internal static extern SqlStatusReturnCode SQLGetInfo(int connect, short infoType, byte[] infoValue, short infoValueLen, out short outInfoValueLen);

    }
}
