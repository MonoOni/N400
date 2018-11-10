using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N400.Native.Data
{
    /// <summary>
    /// Represents a command to run in DB2 for i.
    /// </summary>
    public class PaseDbCommand : DbCommand, IDisposable
    {
        internal PaseDbConnection _dbc;
        internal int _stmt;
        bool _prepared;
        string _commandText;

        /// <summary>
        /// The text representing the SQL to run.
        /// </summary>
        public override string CommandText
        {
            get
            {
                return _commandText;
            }

            set
            {
                _commandText = value;
            }
        }

        public override int CommandTimeout
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override CommandType CommandType
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool DesignTimeVisible
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The DB2 for i connection being used.
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                return _dbc;
            }

            set
            {
                if (value is PaseDbConnection)
                    _dbc = (PaseDbConnection)value;
                else
                    throw new ArgumentException("The database connection is not a DB2 for i connection.");
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override DbTransaction DbTransaction
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Cancels the current command being run.
        /// </summary>
        public override void Cancel()
        {
            if (Internals.SqlCli.SQLCancel(_stmt) != Internals.SqlStatusReturnCode.Success)
                throw PaseDbException.FromSqlError(this);
        }

        void Execute()
        {
            if (CommandText == null)
                throw new InvalidOperationException("The command text cannot be null.");

            if (_prepared)
            {
                throw new NotImplementedException();
            }
            else
            {
                if (Internals.SqlCli.SQLExecDirect(_stmt, CommandText, Internals.SqlCli.SQL_NTS) != Internals.SqlStatusReturnCode.Success)
                    throw PaseDbException.FromSqlError(this);
            }
        }

        public override int ExecuteNonQuery()
        {
            Execute();

            int rowCount;
            if (Internals.SqlCli.SQLRowCount(_stmt, out rowCount) != Internals.SqlStatusReturnCode.Success)
                throw PaseDbException.FromSqlError(this);

            return rowCount;
        }

        public override object ExecuteScalar()
        {
            Execute();

            try
            {
                // Instead of binding, we'll fetch the first row and use GetCol.
                if (Internals.SqlCli.SQLFetch(_stmt) != Internals.SqlStatusReturnCode.Success)
                    throw PaseDbException.FromSqlError(this);

                // Get description
                var col = new Internals.Column(this, 1);
                return col.GetValue();
            }
            finally
            {
                // Cleanup
                if (Internals.SqlCli.SQLCloseCursor(_stmt) != Internals.SqlStatusReturnCode.Success)
                    throw PaseDbException.FromSqlError(this);
                // XXX: FreeSmt?
            }
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        protected override DbParameter CreateDbParameter()
        {
            throw new NotImplementedException();
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            throw new NotImplementedException();
        }

        internal PaseDbCommand(PaseDbConnection connection)
        {
            if (connection == null)
                throw new ArgumentNullException("The connection cannot be null.", nameof(connection));

            _dbc = connection;
            _prepared = false; // TODO

            // I wonder if it would make more sense to allocate the statement in Exec* functions, and do initialization that way?
            if (Internals.SqlCli.SQLAllocStmt(_dbc._hdbc, out _stmt) != Internals.SqlStatusReturnCode.Success)
                throw PaseDbException.FromSqlError(_dbc);
        }

        #region IDisposable Members

        /// <summary>
        /// Internal variable which checks if Dispose has already been called
        /// </summary>
        private Boolean disposed;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(Boolean disposing)
        {
            if (disposed)
            {
                return;
            }

            // XXX: is the order here right?
            if (disposing)
            {
                _dbc = null;
            }
            Internals.SqlCli.SQLFreeStmt(_stmt, Internals.SqlFreeStmtOptions.Drop);

            disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Call the private Dispose(bool) helper and indicate 
            // that we are explicitly disposing
            this.Dispose(true);

            // Tell the garbage collector that the object doesn't require any
            // cleanup when collected since Dispose was called explicitly.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The destructor for the class.
        /// </summary>
        ~PaseDbCommand()
        {
            this.Dispose(false);
        }


        #endregion

    }
}
