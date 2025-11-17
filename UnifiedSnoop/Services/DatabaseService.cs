// DatabaseService.cs - Service for database-level operations
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Data;
using UnifiedSnoop.Core.Helpers;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
#endif

namespace UnifiedSnoop.Services
{
    /// <summary>
    /// Service class for database-level operations and object inspection.
    /// Provides high-level methods for snooping database objects.
    /// </summary>
    public class DatabaseService
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private readonly Database? _database;
        private readonly TransactionHelper? _transHelper;
        #else
        private readonly Database _database;
        private readonly TransactionHelper _transHelper;
        #endif

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class
        /// using the current working database.
        /// </summary>
        public DatabaseService()
        {
            _database = HostApplicationServices.WorkingDatabase;
            if (_database == null)
            {
                throw new InvalidOperationException(
                    "No working database available. Ensure AutoCAD has an active document.");
            }
            _transHelper = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class
        /// with a specific database.
        /// </summary>
        /// <param name="database">The database to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when database is null.</exception>
        public DatabaseService(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _transHelper = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseService"/> class
        /// with a specific database and transaction helper.
        /// </summary>
        /// <param name="database">The database to inspect.</param>
        /// <param name="transHelper">The transaction helper for database operations.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="database"/> or <paramref name="transHelper"/> is null.
        /// </exception>
        public DatabaseService(Database database, TransactionHelper transHelper)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _transHelper = transHelper ?? throw new ArgumentNullException(nameof(transHelper));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the database managed by this service.
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a root node for the database, suitable for displaying in a tree view.
        /// </summary>
        /// <returns>An ObjectNode representing the database.</returns>
        public ObjectNode CreateDatabaseRootNode()
        {
            var rootNode = new ObjectNode("Database", _database)
            {
                Tag = _database
            };

            return rootNode;
        }

        /// <summary>
        /// Gets the main database collections (symbol tables and dictionaries).
        /// </summary>
        /// <param name="trans">The active transaction.</param>
        /// <returns>A list of ObjectNodes representing the main database collections.</returns>
        /// <exception cref="ArgumentNullException">Thrown when trans is null.</exception>
        public List<ObjectNode> GetDatabaseCollections(Transaction trans)
        {
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            var collections = new List<ObjectNode>();

            try
            {
                // Add symbol tables
                collections.Add(CreateSymbolTableNode("Block Table", _database.BlockTableId, trans));
                collections.Add(CreateSymbolTableNode("Layer Table", _database.LayerTableId, trans));
                collections.Add(CreateSymbolTableNode("Linetype Table", _database.LinetypeTableId, trans));
                collections.Add(CreateSymbolTableNode("Text Style Table", _database.TextStyleTableId, trans));
                collections.Add(CreateSymbolTableNode("DimStyle Table", _database.DimStyleTableId, trans));
                collections.Add(CreateSymbolTableNode("UCS Table", _database.UcsTableId, trans));
                collections.Add(CreateSymbolTableNode("View Table", _database.ViewTableId, trans));
                collections.Add(CreateSymbolTableNode("Viewport Table", _database.ViewportTableId, trans));
                collections.Add(CreateSymbolTableNode("RegApp Table", _database.RegAppTableId, trans));

                // Add named objects dictionary
                collections.Add(CreateDictionaryNode("Named Objects Dictionary", 
                    _database.NamedObjectsDictionaryId, trans));

                // Add model space and paper space
                var blockTable = trans.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (blockTable != null)
                {
                    if (blockTable.Has(BlockTableRecord.ModelSpace))
                    {
                        var msId = blockTable[BlockTableRecord.ModelSpace];
                        collections.Add(CreateBlockNode("Model Space", msId, trans));
                    }

                    if (blockTable.Has(BlockTableRecord.PaperSpace))
                    {
                        var psId = blockTable[BlockTableRecord.PaperSpace];
                        collections.Add(CreateBlockNode("Paper Space", psId, trans));
                    }
                }
            }
            catch (Exception ex)
            {
                // Add error node if we fail to get collections
                var errorNode = new ObjectNode($"Error: {ex.Message}")
                {
                    Tag = ex
                };
                collections.Add(errorNode);
            }

            return collections;
        }

        /// <summary>
        /// Gets Civil 3D collections if Civil 3D is available.
        /// </summary>
        /// <param name="trans">The active transaction.</param>
        /// <returns>A list of ObjectNodes representing Civil 3D collections, or an empty list if Civil 3D is not available.</returns>
        public List<ObjectNode> GetCivil3DCollections(Transaction trans)
        {
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            var collections = new List<ObjectNode>();

            if (!VersionHelper.IsCivil3DAvailable())
            {
                return collections;
            }

            try
            {
                // Use reflection to access Civil 3D objects (for version compatibility)
                var civilDocType = Type.GetType("Autodesk.Civil.ApplicationServices.CivilDocument, AecBaseMgd");
                if (civilDocType != null)
                {
                    var getDocMethod = civilDocType.GetMethod("GetCivilDocument", 
                        new[] { typeof(Database) });
                    
                    if (getDocMethod != null)
                    {
                        var civilDoc = getDocMethod.Invoke(null, new object[] { _database });
                        
                        if (civilDoc != null)
                        {
                            // Add Civil 3D node
                            var civil3DNode = new ObjectNode("Civil 3D Objects", civilDoc)
                            {
                                Tag = civilDoc
                            };
                            collections.Add(civil3DNode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Add error node if Civil 3D access fails
                var errorNode = new ObjectNode($"Civil 3D Error: {ex.Message}")
                {
                    Tag = ex
                };
                collections.Add(errorNode);
            }

            return collections;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Creates a node for a symbol table.
        /// </summary>
        private ObjectNode CreateSymbolTableNode(string name, ObjectId tableId, Transaction trans)
        {
            try
            {
                var table = trans.GetObject(tableId, OpenMode.ForRead) as SymbolTable;
                var node = new ObjectNode(name, table)
                {
                    ObjectId = tableId,
                    IsCollection = true,
                    Tag = table
                };
                return node;
            }
            catch (Exception ex)
            {
                return new ObjectNode($"{name} [Error: {ex.Message}]")
                {
                    Tag = ex
                };
            }
        }

        /// <summary>
        /// Creates a node for a dictionary.
        /// </summary>
        private ObjectNode CreateDictionaryNode(string name, ObjectId dictId, Transaction trans)
        {
            try
            {
                var dict = trans.GetObject(dictId, OpenMode.ForRead) as DBDictionary;
                var node = new ObjectNode(name, dict)
                {
                    ObjectId = dictId,
                    IsCollection = true,
                    Tag = dict
                };
                return node;
            }
            catch (Exception ex)
            {
                return new ObjectNode($"{name} [Error: {ex.Message}]")
                {
                    Tag = ex
                };
            }
        }

        /// <summary>
        /// Creates a node for a block table record.
        /// </summary>
        private ObjectNode CreateBlockNode(string name, ObjectId blockId, Transaction trans)
        {
            try
            {
                var block = trans.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                var node = new ObjectNode(name, block)
                {
                    ObjectId = blockId,
                    IsCollection = true,
                    Tag = block
                };
                return node;
            }
            catch (Exception ex)
            {
                return new ObjectNode($"{name} [Error: {ex.Message}]")
                {
                    Tag = ex
                };
            }
        }

        /// <summary>
        /// Gets properties of an object using the appropriate collector.
        /// </summary>
        /// <param name="obj">The object to inspect.</param>
        /// <returns>A list of PropertyData.</returns>
        public List<PropertyData> GetProperties(object obj)
        {
            if (obj == null || _transHelper == null)
                return new List<PropertyData>();

            var collector = Core.Collectors.CollectorRegistry.Instance.GetCollector(obj);
            return collector.Collect(obj, _transHelper.Transaction);
        }

        /// <summary>
        /// Alias for GetProperties for backward compatibility.
        /// </summary>
        public List<PropertyData> GetObjectProperties(object obj)
        {
            return GetProperties(obj);
        }

        /// <summary>
        /// Gets child nodes (collections) of an object using the appropriate collector.
        /// </summary>
        /// <param name="obj">The parent object.</param>
        /// <returns>A list of ObjectNode representing collections.</returns>
        public List<ObjectNode> GetObjectChildNodes(object obj)
        {
            if (obj == null || _transHelper == null)
                return new List<ObjectNode>();

            var childNodes = new List<ObjectNode>();
            
            try
            {
                var collector = Core.Collectors.CollectorRegistry.Instance.GetCollector(obj);
                var collections = collector.GetCollections(obj, _transHelper.Transaction);

                foreach (var kvp in collections)
                {
                    var node = new ObjectNode(kvp.Key, kvp.Value);
                    node.IsCollection = true;
                    childNodes.Add(node);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting child nodes: {ex.Message}");
            }

            return childNodes;
        }

        #endregion
    }
}

