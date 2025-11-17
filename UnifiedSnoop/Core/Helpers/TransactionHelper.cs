// TransactionHelper.cs - Helper class for managing AutoCAD transactions
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)
// Based on MgdDbg's excellent transaction management pattern

using System;
using System.Diagnostics;
using Autodesk.AutoCAD.DatabaseServices;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace UnifiedSnoop.Core.Helpers
{
    /// <summary>
    /// Helper class for managing AutoCAD transactions.
    /// Implements IDisposable to ensure proper cleanup of transactions.
    /// Based on MgdDbg's TransactionHelper pattern.
    /// </summary>
    /// <remarks>
    /// This class simplifies transaction management and ensures proper disposal.
    /// Always use within a using statement to guarantee cleanup.
    /// </remarks>
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
        /// <exception cref="InvalidOperationException">
        /// Thrown when accessed before Start() is called.
        /// </exception>
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
        /// using the current working database.
        /// </summary>
        public TransactionHelper()
        {
            _database = HostApplicationServices.WorkingDatabase;
            
            if (_database == null)
            {
                throw new InvalidOperationException(
                    "No working database is available. Ensure AutoCAD is running with an active document.");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionHelper"/> class
        /// with a specific database.
        /// </summary>
        /// <param name="database">The database to use for transactions.</param>
        /// <exception cref="ArgumentNullException">Thrown when database is null.</exception>
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
        /// <exception cref="InvalidOperationException">
        /// Thrown when a transaction is already active.
        /// </exception>
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
        /// <exception cref="InvalidOperationException">
        /// Thrown when no transaction is active.
        /// </exception>
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
        /// <exception cref="InvalidOperationException">
        /// Thrown when no transaction is active.
        /// </exception>
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

        /// <summary>
        /// Gets an object from the database using the current transaction.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve.</typeparam>
        /// <param name="objectId">The ObjectId of the object to retrieve.</param>
        /// <param name="mode">The mode to open the object in. Use OpenMode.ForRead for inspection.</param>
        /// <returns>The requested object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no transaction is active.</exception>
        public T GetObject<T>(ObjectId objectId, OpenMode mode) where T : DBObject
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException(
                    "No active transaction. Call Start() before GetObject().");
            }

            return (T)_transaction.GetObject(objectId, mode);
        }

        /// <summary>
        /// Gets an object from the database using the current transaction with OpenMode.ForRead.
        /// This is the recommended method for inspection operations.
        /// </summary>
        /// <typeparam name="T">The type of object to retrieve.</typeparam>
        /// <param name="objectId">The ObjectId of the object to retrieve.</param>
        /// <returns>The requested object opened for reading.</returns>
        /// <remarks>
        /// IMPORTANT: This method uses OpenMode.ForRead as per development rules.
        /// Never use ForWrite for inspection operations.
        /// </remarks>
        public T GetObject<T>(ObjectId objectId) where T : DBObject
        {
            // Always use ForRead for inspection - RULE 3.1 from DEVELOPMENT_RULES.md
            return GetObject<T>(objectId, OpenMode.ForRead);
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
        /// <param name="disposing">
        /// true to release both managed and unmanaged resources;
        /// false to release only unmanaged resources.
        /// </param>
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

