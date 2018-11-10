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
    /// Represents a local connection to DB2 for i via libdb400. This class will only function under i.
    /// </summary>
    public class PaseDbConnection : DbConnection, IDisposable
    {
        const short stringLen = Internals.SqlCli.SQL_MAX_MESSAGE_LENGTH + 1;

        bool _connected;
        internal int _hdbc;
        internal int _henv; // XXX: See ctor if this should be there
        string _connectionString;

        /// <summary>
        /// Not currently used by DB2 for i.
        /// </summary>
        public override string ConnectionString
        {
            get
            {
                return _connectionString;
            }

            set
            {
                if (_connected)
                {
                    throw new InvalidOperationException("The connection string can only be set when the connection is closed.");
                }
                else
                {
                    _connectionString = value;
                }
            }
        }


        public override string Database
        {
            get
            {
                if (_connected)
                {
                    var versionSb = new StringBuilder(stringLen);
                    short outLen;
                    Internals.SqlCli.SQLGetInfo(_hdbc, 204 /* dn name */, versionSb, stringLen, out outLen);
                    return versionSb.ToString();
                }
                else
                {
                    throw new InvalidOperationException("Must be connected to get the database name.");
                }
            }
        }

        public override string DataSource
        {
            get
            {
                if (_connected)
                {
                    var versionSb = new StringBuilder(stringLen);
                    short outLen;
                    Internals.SqlCli.SQLGetInfo(_hdbc, 42 /* ds name */, versionSb, stringLen, out outLen);
                    return versionSb.ToString();
                }
                else
                {
                    throw new InvalidOperationException("Must be connected to get the data source name.");
                }
            }
        }

        /// <summary>
        /// Gets the current version of DB2 for i.
        /// </summary>
        // looks to be in the format 07030
        public override string ServerVersion
        {
            get
            {
                if (_connected)
                {
                    var versionSb = new StringBuilder(stringLen);
                    short outLen;
                    Internals.SqlCli.SQLGetInfo(_hdbc, 18 /* version */, versionSb, stringLen, out outLen);
                    return versionSb.ToString();
                }
                else
                {
                    throw new InvalidOperationException("Must be connected to get the server version.");
                }
            }
        }

        // TODO: The connected state will need to determine the execution state.
        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        /// <remarks>
        /// Local database connections on IBM i are not network based, and
        /// internally use ILE to PASE IPC. As such, connections don't break.
        /// </remarks>
        public override ConnectionState State
        {
            get
            {
                if (_connected)
                {
                    return ConnectionState.Open;
                }
                else
                {
                    return ConnectionState.Closed;
                }
            }
        }

        public override void ChangeDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        public override void Close()
        {
            if (_connected)
            {
                if (Internals.SqlCli.SQLDisconnect(_hdbc) != Internals.SqlStatusReturnCode.Success)
                    throw PaseDbException.FromSqlError(this);
                _connected = false;
            }
        }

        public override void Open()
        {
            if (!_connected)
            {
                // We may be able to act on these params in the future.
                if (Internals.SqlCli.SQLConnect(_hdbc, null, 0, null, 0, null, 0) != Internals.SqlStatusReturnCode.Success)
                    throw PaseDbException.FromSqlError(this);
                _connected = true;
            }
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            throw new NotImplementedException();
        }

        protected override DbCommand CreateDbCommand()
        {
            return new PaseDbCommand(this);
        }

        // XXX: uname
        static bool IsPase()
        {
            return Environment.OSVersion.Platform == PlatformID.Unix;
        }

        void FreeEnvironment ()
        {
            Internals.SqlCli.SQLFreeEnv(_henv);
            Internals.SqlCli.SQLReleaseEnv(_henv);
        }

        public PaseDbConnection(string connectionString) : base()
        {
            if (!IsPase())
                throw new PlatformNotSupportedException("This database provider is only supported when running on IBM i.");

            // Set the CCSID to UTF-8, so we can use Unicode without having to use wide functions.
            Internals.SqlCli.SQLOverrideCCSID400(1208);

            ConnectionString = connectionString;

            // Acquire an environment.
            // TODO: Is this better done by a facftory of some sort?
            if (Internals.SqlCli.SQLAllocEnv(out _henv) != Internals.SqlStatusReturnCode.Success)
            {
                if (_henv != 0)
                {
                    var pde = PaseDbException.FromSqlError(_henv);
                    // PaseDbException doesn't hold onto anything
                    FreeEnvironment();
                    throw pde;
                }
                else
                {
                    throw new InvalidOperationException("Couldn't acquire an SQL environment handle.");
                }
            }
            if (Internals.SqlCli.SQLAllocConnect(_henv, out _hdbc) != Internals.SqlStatusReturnCode.Success)
            {
                var pde = PaseDbException.FromSqlError(_henv);
                // PaseDbException doesn't hold onto anything
                FreeEnvironment();
                throw pde;
            }
        }
        
        #region IDisposable Members

        /// <summary>
        /// Internal variable which checks if Dispose has already been called
        /// </summary>
        private bool disposed;

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

            if (disposing)
            {
                if (_connected)
                    Close();
            }

            FreeEnvironment();

            disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public new void Dispose()
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
        ~PaseDbConnection()
        {
            this.Dispose(false);
        }


        #endregion

    }
}
