using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Native.Data.Internals
{
    // this is an internal implementation of Column, unsure if it could get just used as DbColumn or stay a backend thing
    internal class Column
    {
        internal PaseDbCommand _command;

        internal string Name { get; }
        internal short Index { get; }
        internal SqlType Type { get; }
        internal bool Nullable { get; }
        // digits/bytes/characters?
        internal int Precision { get; }
        // decimal places, it seems
        internal short Scale { get; set; }

        internal Column(PaseDbCommand dbCommand, short index)
        {
            if (dbCommand == null)
                throw new ArgumentNullException(nameof(dbCommand), "The DB2 for i command cannot be null.");

            _command = dbCommand;

            // use Describe to get core attributes
            var name = new StringBuilder(129);
            short bytesReturned, scale, nullable;
            int precision;
            SqlType type;
            if (SqlCli.SQLDescribeCol(_command._stmt, index, name, 129, out bytesReturned, out type, out precision, out scale, out nullable) != SqlStatusReturnCode.Success)
                throw PaseDbException.FromSqlError(_command);

            Index = index;
            Name = name.ToString();
            Type = type;
            Nullable = nullable != 0;
            Precision = precision;
            Scale = scale;
        }

        // these return DBNull OR their supposed value
        internal object GetString()
        {
            // XXX: Use description precision instead
            int bytesReturned, capacity = Precision + 1;
            var outputString = new StringBuilder(capacity);
            if (SqlCli.SQLGetCol(_command._stmt, Index, SqlType.CHAR, outputString, capacity, out bytesReturned) != SqlStatusReturnCode.Success)
                throw PaseDbException.FromSqlError(_command);

            if (Nullable && bytesReturned == -1)
                return DBNull.Value;

            return outputString.ToString();
        }

        internal object GetInt32()
        {
            Debug.Assert(Precision == 4);
            int bytesReturned, outputInt;
            if (SqlCli.SQLGetCol(_command._stmt, Index, SqlType.INTEGER, out outputInt, 4, out bytesReturned) != SqlStatusReturnCode.Success)
                throw PaseDbException.FromSqlError(_command);

            if (Nullable && bytesReturned == -1)
                return DBNull.Value;

            return outputInt;
        }

        // preferred type, no conversions.
        internal object GetValue()
        {
            switch (Type)
            {
                case SqlType.CHAR:
                case SqlType.VARCHAR:
                // These are weird candidates for stringifcation, but its what the header says to do?
                case SqlType.NUMERIC:
                case SqlType.DECIMAL:
                    return GetString();
                case SqlType.INTEGER:
                    return GetInt32();
                default:
                    // Could we just get a stringy representation?
                    throw new NotImplementedException($"Don't know how to handle the type {Type}.");
            }
        }
    }
}
