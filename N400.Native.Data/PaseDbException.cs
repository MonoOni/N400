using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Native.Data
{
    /// <summary>
    /// Represents an exception originating as an error from DB2 for i.
    /// </summary>
    public class PaseDbException : DbException
    {
        /// <summary>
        /// The five digit error code given by DB2 for i.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The first two digits are the major class of the error, and the last three digits are the subclass of error.
        /// </para>
        /// <oara>
        /// This field is standard under X/Open SQL CAE, with IBM extensions.
        /// </oara>
        /// </remarks>
        string SqlState { get; }

        PaseDbException(string message, int nativeError, string sqlState) : base(message, nativeError)
        {
            SqlState = sqlState;
        }

        // we can only set message from the ctor, so work around
        internal static PaseDbException FromSqlError(PaseDbConnection dbc) =>
            FromSqlError(dbc?._henv ?? 0, dbc?._hdbc ?? 0, 0);

        internal static PaseDbException FromSqlError(PaseDbCommand command) =>
            FromSqlError(command?._dbc?._henv ?? 0, command?._dbc?._hdbc ?? 0, command?._stmt ?? 0);

        internal static PaseDbException FromSqlError(int henv, int hdbc) => FromSqlError(henv, hdbc, 0);

        internal static PaseDbException FromSqlError(int henv) => FromSqlError(henv, 0, 0);

        internal static PaseDbException FromSqlError(int henv, int hdbc, int hstmt)
        {
            var sqlState = new StringBuilder(6);
            int nativeError;
            const short len = Internals.SqlCli.SQL_MAX_MESSAGE_LENGTH + 1;
            var errorMessage = new StringBuilder(len);
            short outLen;

            Internals.SqlCli.SQLError(henv, hdbc, hstmt, sqlState, out nativeError, errorMessage, len, out outLen);

            return new Native.Data.PaseDbException(errorMessage.ToString(), nativeError, sqlState.ToString());
        }

    }
}
