// TransactionHelper.cs - Helper class for managing AutoCAD transactions
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Diagnostics;
using Autodesk.AutoCAD.DatabaseServices;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace UnifiedSnoop.XRecordEditor
{
    /// <summary>
    /// Helper class for managing AutoCAD transactions.
    /// Implements IDisposable to ensure proper cleanup of transactions.
    /// </summary>
    public class TransactionHelper : IDisposable
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private Database? _database;
        private Transaction? _transaction;
        #else
        private Database _database;
        private Transaction _transaction;
        #endif
        private bool _disposed = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the transaction managed by this helper.
        /// </summary>
        public Transaction Transaction
        {
            get
            {
                if (_transaction == null)
                {
                    throw new InvalidOperationException(
                        "Transaction has not been started. Call Start() before accessing the Transaction property.");
                }
                return _transaction;
            }
        }

        /// <summary>
        /// Gets the database associated with this transaction helper.
        /// </summary>
        public Database Database
        {
            get
            {
                #if NET8_0_OR_GREATER
                return _database ?? throw new InvalidOperationException("Database is not initialized.");
                #else
                if (_database == null)
                {
                    throw new InvalidOperationException("Database is not initialized.");
                }
                return _database;
                #endif
            }
        }

        /// <summary>
        /// Gets a value indicating whether a transaction is currently active.
        /// </summary>
        public bool IsActive
        {
            get { return _transaction != null; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHelper"/> class
        /// with a specific database.
        /// </summary>
        public TransactionHelper(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        /// <summary>
        /// Finalizer for TransactionHelper.
        /// </summary>
        ~TransactionHelper()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts a new transaction.
        /// </summary>
        public void Start()
        {
            if (_transaction != null)
            {
                throw new InvalidOperationException(
                    "A transaction is already active. Commit or abort the current transaction before starting a new one.");
            }

            Debug.Assert(_database != null, "Database should not be null");
            _transaction = _database.TransactionManager.StartTransaction();
        }

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        public void Commit()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(
                    "No active transaction to commit. Call Start() before Commit().");
            }

            _transaction.Commit();
            _transaction = null;
        }

        /// <summary>
        /// Aborts the current transaction.
        /// </summary>
        public void Abort()
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(
                    "No active transaction to abort. Call Start() before Abort().");
            }

            _transaction.Abort();
            _transaction = null;
        }

        #endregion

        #region IDisposable Implementation

        /// <summary>
        /// Releases all resources used by the TransactionHelper.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the TransactionHelper
        /// and optionally releases the managed resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (_transaction != null)
                    {
                        // If transaction is still active, abort it
                        try
                        {
                            _transaction.Abort();
                        }
                        catch
                        {
                            // Ignore errors during cleanup
                        }
                        finally
                        {
                            _transaction.Dispose();
                            _transaction = null;
                        }
                    }
                }

                _disposed = true;
            }
        }

        #endregion
    }
}

