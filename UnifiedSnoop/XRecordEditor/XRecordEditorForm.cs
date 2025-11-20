// XRecordEditorForm.cs - Editor for managing XRecords in AutoCAD drawings
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Helpers;
using UnifiedSnoop.Services;

#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618
#endif

namespace UnifiedSnoop.XRecordEditor
{
    /// <summary>
    /// Form for editing and managing XRecords in the drawing database.
    /// </summary>
    public class XRecordEditorForm : Form
    {
        #region Fields

        private readonly Database _database;
        private readonly TransactionHelper _transactionHelper;
        
        // UI Controls
#if NET8_0_OR_GREATER
        private TreeView _treeView = null!;
        private Panel _detailsPanel = null!;
        private ListView _dataListView = null!;
        private Label _lblXRecordName = null!;
        private Label _lblEntryCount = null!;
        private Button _btnAddEntry = null!;
        private Button _btnEditEntry = null!;
        private Button _btnDeleteEntry = null!;
        private Button _btnDeleteXRecord = null!;
        private Button _btnRefresh = null!;
        private Button _btnClose = null!;
        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _statusLabel = null!;
#else
        private TreeView _treeView;
        private Panel _detailsPanel;
        private ListView _dataListView;
        private Label _lblXRecordName;
        private Label _lblEntryCount;
        private Button _btnAddEntry;
        private Button _btnEditEntry;
        private Button _btnDeleteEntry;
        private Button _btnDeleteXRecord;
        private Button _btnRefresh;
        private Button _btnClose;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
#endif

        #endregion

        #region Constructor

        public XRecordEditorForm(Database database, TransactionHelper transactionHelper)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));

            InitializeComponent();
            LoadXRecords();
        }

        #endregion

        #region Initialization

        private void InitializeComponent()
        {
            // Form properties
            this.Text = "XRecord Editor - UnifiedSnoop";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);

            // Create split container
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300,
                BorderStyle = BorderStyle.Fixed3D
            };

            // Left panel - TreeView for XRecords
            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                HideSelection = false,
                FullRowSelect = true,
                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = true
            };
            _treeView.AfterSelect += TreeView_AfterSelect;
            _treeView.MouseDown += TreeView_MouseDown;

            // Create context menu for TreeView
            var treeContextMenu = new ContextMenuStrip();
            var menuDeleteItem = new ToolStripMenuItem("Delete Selected Item", null, (s, e) => DeleteSelectedItem());
            menuDeleteItem.ForeColor = Color.Red;
            var menuRefreshTree = new ToolStripMenuItem("Refresh", null, (s, e) => BtnRefresh_Click(s, e));
            treeContextMenu.Items.Add(menuDeleteItem);
            treeContextMenu.Items.Add(new ToolStripSeparator());
            treeContextMenu.Items.Add(menuRefreshTree);
            treeContextMenu.Opening += (s, e) =>
            {
                // Enable delete if an XRecord or Dictionary is selected
                var tag = _treeView.SelectedNode?.Tag;
                menuDeleteItem.Enabled = tag is XRecordInfo || tag is DictionaryInfo;
                
                // Update menu text based on what's selected
                if (tag is XRecordInfo)
                    menuDeleteItem.Text = "Delete XRecord";
                else if (tag is DictionaryInfo)
                    menuDeleteItem.Text = "Delete Dictionary";
                else
                    menuDeleteItem.Text = "Delete Selected Item";
            };
            _treeView.ContextMenuStrip = treeContextMenu;

            splitContainer.Panel1.Controls.Add(_treeView);

            // Right panel - Details and data
            _detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // XRecord name label
            _lblXRecordName = new Label
            {
                Text = "Select an XRecord from the tree",
                Location = new Point(10, 10),
                Size = new Size(650, 25),
                Font = new System.Drawing.Font(this.Font.FontFamily, 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // Entry count label
            _lblEntryCount = new Label
            {
                Text = "",
                Location = new Point(10, 40),
                Size = new Size(650, 20),
                ForeColor = Color.Gray
            };

            // Data ListView
            _dataListView = new ListView
            {
                Location = new Point(10, 70),
                Size = new Size(650, 400),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _dataListView.Columns.Add("Index", 60);
            _dataListView.Columns.Add("DXF Code", 100);
            _dataListView.Columns.Add("Type", 150);
            _dataListView.Columns.Add("Value", 300);
            _dataListView.SelectedIndexChanged += DataListView_SelectedIndexChanged;
            _dataListView.DoubleClick += BtnEditEntry_Click;
            
            // Create context menu for ListView
            var listContextMenu = new ContextMenuStrip();
            var menuAddEntry = new ToolStripMenuItem("Add Entry...", null, (s, e) => BtnAddEntry_Click(s, e));
            var menuEditEntry = new ToolStripMenuItem("Edit Entry...", null, (s, e) => BtnEditEntry_Click(s, e));
            var menuDeleteEntry = new ToolStripMenuItem("Delete Entry", null, (s, e) => BtnDeleteEntry_Click(s, e));
            menuDeleteEntry.ForeColor = Color.Red;
            listContextMenu.Items.Add(menuAddEntry);
            listContextMenu.Items.Add(menuEditEntry);
            listContextMenu.Items.Add(new ToolStripSeparator());
            listContextMenu.Items.Add(menuDeleteEntry);
            listContextMenu.Opening += (s, e) =>
            {
                // Add is always available if an XRecord is selected
                menuAddEntry.Enabled = _treeView.SelectedNode?.Tag is XRecordInfo;
                // Edit and Delete require a selected entry
                bool hasSelection = _dataListView.SelectedItems.Count > 0;
                menuEditEntry.Enabled = hasSelection;
                menuDeleteEntry.Enabled = hasSelection;
            };
            _dataListView.ContextMenuStrip = listContextMenu;

            // Buttons panel
            var buttonsPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 480),
                Size = new Size(650, 40),
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            _btnAddEntry = new Button
            {
                Text = "Add Entry",
                Size = new Size(100, 30),
                Enabled = false
            };
            _btnAddEntry.Click += BtnAddEntry_Click;

            _btnEditEntry = new Button
            {
                Text = "Edit Entry",
                Size = new Size(100, 30),
                Enabled = false
            };
            _btnEditEntry.Click += BtnEditEntry_Click;

            _btnDeleteEntry = new Button
            {
                Text = "Delete Entry",
                Size = new Size(100, 30),
                Enabled = false
            };
            _btnDeleteEntry.Click += BtnDeleteEntry_Click;

            _btnDeleteXRecord = new Button
            {
                Text = "Delete Selected",
                Size = new Size(120, 30),
                Enabled = false,
                ForeColor = Color.Red
            };
            _btnDeleteXRecord.Click += (s, e) => DeleteSelectedItem();

            buttonsPanel.Controls.Add(_btnAddEntry);
            buttonsPanel.Controls.Add(_btnEditEntry);
            buttonsPanel.Controls.Add(_btnDeleteEntry);
            buttonsPanel.Controls.Add(_btnDeleteXRecord);

            _detailsPanel.Controls.Add(_lblXRecordName);
            _detailsPanel.Controls.Add(_lblEntryCount);
            _detailsPanel.Controls.Add(_dataListView);
            _detailsPanel.Controls.Add(buttonsPanel);

            splitContainer.Panel2.Controls.Add(_detailsPanel);

            // Bottom buttons
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            _btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(10, 10),
                Size = new Size(100, 30)
            };
            _btnRefresh.Click += BtnRefresh_Click;

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(120, 10),
                Size = new Size(100, 30)
            };
            _btnClose.Click += (s, e) => this.Close();

            bottomPanel.Controls.Add(_btnRefresh);
            bottomPanel.Controls.Add(_btnClose);

            // Status strip
            _statusStrip = new StatusStrip();
            _statusLabel = new ToolStripStatusLabel("Ready");
            _statusStrip.Items.Add(_statusLabel);

            // Add controls to form
            this.Controls.Add(splitContainer);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(_statusStrip);
        }

        #endregion

        #region XRecord Loading

        private void LoadXRecords()
        {
            try
            {
                _treeView.BeginUpdate();
                _treeView.Nodes.Clear();

                UpdateStatus("Loading XRecords...");

                // Create root nodes for different dictionary types
                var nodNode = new TreeNode("Named Objects Dictionary");
                LoadXRecordsFromDictionary(_database.NamedObjectsDictionaryId, nodNode);
                if (nodNode.Nodes.Count > 0)
                    _treeView.Nodes.Add(nodNode);

                // Load from all dictionaries
                var dictNode = new TreeNode("All Dictionaries");
                LoadAllXRecordsFromDatabase(dictNode);
                if (dictNode.Nodes.Count > 0)
                    _treeView.Nodes.Add(dictNode);

                UpdateStatus($"Loaded {CountTotalXRecords()} XRecords");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error loading XRecords: {ex.Message}");
                MessageBox.Show($"Error loading XRecords:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _treeView.EndUpdate();
            }
        }

        private void LoadXRecordsFromDictionary(ObjectId dictId, TreeNode parentNode)
        {
            try
            {
                var dict = _transactionHelper.Transaction.GetObject(dictId, OpenMode.ForRead) as DBDictionary;
                if (dict == null) return;

                foreach (DBDictionaryEntry entry in dict)
                {
                    var obj = _transactionHelper.Transaction.GetObject(entry.Value, OpenMode.ForRead);
                    
                    if (obj is Xrecord xrec)
                    {
                        var node = new TreeNode($"{entry.Key} [XRecord]")
                        {
                            Tag = new XRecordInfo
                            {
                                ObjectId = entry.Value,
                                Name = entry.Key,
                                ParentDictionaryId = dictId
                            }
                        };
                        parentNode.Nodes.Add(node);
                    }
                    else if (obj is DBDictionary)
                    {
                        var subDictNode = new TreeNode($"{entry.Key} [Dictionary]")
                        {
                            Tag = new DictionaryInfo
                            {
                                ObjectId = entry.Value,
                                Name = entry.Key
                            }
                        };
                        LoadXRecordsFromDictionary(entry.Value, subDictNode);
                        if (subDictNode.Nodes.Count > 0)
                            parentNode.Nodes.Add(subDictNode);
                    }
                }
            }
            catch (System.Exception ex)
            {
                parentNode.Nodes.Add(new TreeNode($"Error: {ex.Message}"));
            }
        }

        private void LoadAllXRecordsFromDatabase(TreeNode parentNode)
        {
            try
            {
                // 1. Scan entities in all block table records
                var entitiesNode = new TreeNode("Entity Extension Dictionaries");
                ScanEntitiesForXRecords(entitiesNode);
                if (entitiesNode.Nodes.Count > 0)
                    parentNode.Nodes.Add(entitiesNode);

                // 2. Scan symbol tables
                var symbolTablesNode = new TreeNode("Symbol Table Extension Dictionaries");
                ScanSymbolTablesForXRecords(symbolTablesNode);
                if (symbolTablesNode.Nodes.Count > 0)
                    parentNode.Nodes.Add(symbolTablesNode);

                // 3. Scan other dictionaries
                var otherDictsNode = new TreeNode("Other Dictionaries");
                ScanOtherDictionaries(otherDictsNode);
                if (otherDictsNode.Nodes.Count > 0)
                    parentNode.Nodes.Add(otherDictsNode);
            }
            catch (System.Exception ex)
            {
                parentNode.Nodes.Add(new TreeNode($"Error scanning database: {ex.Message}"));
            }
        }

        private void ScanEntitiesForXRecords(TreeNode parentNode)
        {
            try
            {
                using (var blockTable = _transactionHelper.Transaction.GetObject(_database.BlockTableId, OpenMode.ForRead) as BlockTable)
                {
                    if (blockTable == null) return;

                    foreach (ObjectId btrId in blockTable)
                    {
                        var btr = _transactionHelper.Transaction.GetObject(btrId, OpenMode.ForRead) as BlockTableRecord;
                        if (btr == null) continue;

                        foreach (ObjectId entId in btr)
                        {
                            try
                            {
                                var ent = _transactionHelper.Transaction.GetObject(entId, OpenMode.ForRead) as Entity;
                                if (ent != null && ent.ExtensionDictionary != ObjectId.Null)
                                {
                                    var extDictNode = new TreeNode($"{ent.GetType().Name} [{entId.Handle}]");
                                    LoadXRecordsFromDictionary(ent.ExtensionDictionary, extDictNode);
                                    if (extDictNode.Nodes.Count > 0)
                                        parentNode.Nodes.Add(extDictNode);
                                }
                            }
                            catch
                            {
                                // Skip entities that can't be opened
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors in entity scanning
            }
        }

        private void ScanSymbolTablesForXRecords(TreeNode parentNode)
        {
            try
            {
                // Scan Layer Table
                ScanSymbolTable(_database.LayerTableId, "Layers", parentNode);

                // Scan Block Table
                ScanSymbolTable(_database.BlockTableId, "Blocks", parentNode);

                // Scan Linetype Table
                ScanSymbolTable(_database.LinetypeTableId, "Linetypes", parentNode);

                // Scan Text Style Table
                ScanSymbolTable(_database.TextStyleTableId, "TextStyles", parentNode);

                // Scan Dimension Style Table
                ScanSymbolTable(_database.DimStyleTableId, "DimStyles", parentNode);

                // Scan UCS Table
                ScanSymbolTable(_database.UcsTableId, "UCSs", parentNode);

                // Scan View Table
                ScanSymbolTable(_database.ViewTableId, "Views", parentNode);

                // Scan Viewport Table
                ScanSymbolTable(_database.ViewportTableId, "Viewports", parentNode);

                // Scan RegApp Table
                ScanSymbolTable(_database.RegAppTableId, "RegApps", parentNode);
            }
            catch
            {
                // Ignore errors in symbol table scanning
            }
        }

        private void ScanSymbolTable(ObjectId tableId, string tableName, TreeNode parentNode)
        {
            try
            {
                var symbolTable = _transactionHelper.Transaction.GetObject(tableId, OpenMode.ForRead) as SymbolTable;
                if (symbolTable == null) return;

                var tableNode = new TreeNode(tableName);
                int recordCount = 0;

                foreach (ObjectId recordId in symbolTable)
                {
                    try
                    {
                        var record = _transactionHelper.Transaction.GetObject(recordId, OpenMode.ForRead) as SymbolTableRecord;
                        if (record != null && record.ExtensionDictionary != ObjectId.Null)
                        {
                            var extDictNode = new TreeNode($"{record.Name} [{recordId.Handle}]");
                            LoadXRecordsFromDictionary(record.ExtensionDictionary, extDictNode);
                            if (extDictNode.Nodes.Count > 0)
                            {
                                tableNode.Nodes.Add(extDictNode);
                                recordCount++;
                            }
                        }
                    }
                    catch
                    {
                        // Skip records that can't be opened
                    }
                }

                if (recordCount > 0)
                    parentNode.Nodes.Add(tableNode);
            }
            catch
            {
                // Ignore errors for individual tables
            }
        }

        private void ScanOtherDictionaries(TreeNode parentNode)
        {
            try
            {
                // Scan all dictionaries in Named Objects Dictionary recursively
                var nod = _transactionHelper.Transaction.GetObject(_database.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;
                if (nod == null) return;

                foreach (DBDictionaryEntry entry in nod)
                {
                    try
                    {
                        var obj = _transactionHelper.Transaction.GetObject(entry.Value, OpenMode.ForRead);
                        
                        // Skip if it's an XRecord (already shown in Named Objects Dictionary section)
                        if (obj is Xrecord) continue;

                        // If it's a dictionary, check if it has XRecords
                        if (obj is DBDictionary subDict)
                        {
                            var dictNode = new TreeNode($"{entry.Key}");
                            ScanDictionaryRecursive(subDict, dictNode);
                            if (dictNode.Nodes.Count > 0)
                                parentNode.Nodes.Add(dictNode);
                        }
                        
                        // Check for extension dictionary on the object itself
                        if (obj is DBObject dbObj && dbObj.ExtensionDictionary != ObjectId.Null)
                        {
                            var extDictNode = new TreeNode($"{entry.Key} [{entry.Value.Handle}] - Extension Dictionary");
                            LoadXRecordsFromDictionary(dbObj.ExtensionDictionary, extDictNode);
                            if (extDictNode.Nodes.Count > 0)
                                parentNode.Nodes.Add(extDictNode);
                        }
                    }
                    catch
                    {
                        // Skip objects that can't be opened
                    }
                }
            }
            catch
            {
                // Ignore errors in dictionary scanning
            }
        }

        private void ScanDictionaryRecursive(DBDictionary dict, TreeNode parentNode)
        {
            try
            {
                foreach (DBDictionaryEntry entry in dict)
                {
                    try
                    {
                        var obj = _transactionHelper.Transaction.GetObject(entry.Value, OpenMode.ForRead);
                        
                        if (obj is Xrecord)
                        {
                            var node = new TreeNode($"{entry.Key} [XRecord]")
                            {
                                Tag = new XRecordInfo
                                {
                                    ObjectId = entry.Value,
                                    Name = entry.Key,
                                    ParentDictionaryId = dict.ObjectId
                                }
                            };
                            parentNode.Nodes.Add(node);
                        }
                        else if (obj is DBDictionary subDict)
                        {
                            var subNode = new TreeNode($"{entry.Key} [Dictionary]");
                            ScanDictionaryRecursive(subDict, subNode);
                            if (subNode.Nodes.Count > 0)
                                parentNode.Nodes.Add(subNode);
                        }
                    }
                    catch
                    {
                        // Skip entries that can't be opened
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }

        private int CountTotalXRecords()
        {
            int count = 0;
            foreach (TreeNode node in _treeView.Nodes)
            {
                count += CountXRecordsInNode(node);
            }
            return count;
        }

        private int CountXRecordsInNode(TreeNode node)
        {
            int count = node.Tag is XRecordInfo ? 1 : 0;
            foreach (TreeNode child in node.Nodes)
            {
                count += CountXRecordsInNode(child);
            }
            return count;
        }

        #endregion

        #region Event Handlers

#if NET8_0_OR_GREATER
        private void TreeView_MouseDown(object? sender, MouseEventArgs e)
#else
        private void TreeView_MouseDown(object sender, MouseEventArgs e)
#endif
        {
            // Update selection on right-click BEFORE context menu opens
            if (e.Button == MouseButtons.Right)
            {
                var node = _treeView.GetNodeAt(e.X, e.Y);
                if (node != null)
                {
                    _treeView.SelectedNode = node;
                }
            }
        }

#if NET8_0_OR_GREATER
        private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
#else
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
#endif
        {
            if (e.Node?.Tag is XRecordInfo info)
            {
                LoadXRecordData(info);
            }
            else if (e.Node?.Tag is DictionaryInfo dictInfo)
            {
                LoadDictionaryInfo(dictInfo);
            }
            else
            {
                ClearDetails();
            }
        }

#if NET8_0_OR_GREATER
        private void DataListView_SelectedIndexChanged(object? sender, EventArgs e)
#else
        private void DataListView_SelectedIndexChanged(object sender, EventArgs e)
#endif
        {
            _btnEditEntry.Enabled = _dataListView.SelectedItems.Count > 0;
            _btnDeleteEntry.Enabled = _dataListView.SelectedItems.Count > 0;
        }

#if NET8_0_OR_GREATER
        private void BtnRefresh_Click(object? sender, EventArgs e)
#else
        private void BtnRefresh_Click(object sender, EventArgs e)
#endif
        {
            LoadXRecords();
            ClearDetails();
        }

#if NET8_0_OR_GREATER
        private void BtnAddEntry_Click(object? sender, EventArgs e)
#else
        private void BtnAddEntry_Click(object sender, EventArgs e)
#endif
        {
            if (_treeView.SelectedNode?.Tag is XRecordInfo info)
            {
                // Open dialog to add new entry
                using (var dialog = new XRecordValueEditorForm(null, _database, _transactionHelper))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        AddEntryToXRecord(info.ObjectId, dialog.TypedValue);
                    }
                }
            }
        }

#if NET8_0_OR_GREATER
        private void BtnEditEntry_Click(object? sender, EventArgs e)
#else
        private void BtnEditEntry_Click(object sender, EventArgs e)
#endif
        {
            if (_dataListView.SelectedItems.Count == 0) return;
            if (_treeView.SelectedNode?.Tag is not XRecordInfo info) return;

            var selectedItem = _dataListView.SelectedItems[0];
            if (selectedItem.Tag is not TypedValue currentValue) return;

            using (var dialog = new XRecordValueEditorForm(currentValue, _database, _transactionHelper))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int index = selectedItem.Index;
                    UpdateEntryInXRecord(info.ObjectId, index, dialog.TypedValue);
                }
            }
        }

#if NET8_0_OR_GREATER
        private void BtnDeleteEntry_Click(object? sender, EventArgs e)
#else
        private void BtnDeleteEntry_Click(object sender, EventArgs e)
#endif
        {
            if (_dataListView.SelectedItems.Count == 0) return;
            if (_treeView.SelectedNode?.Tag is not XRecordInfo info) return;

            var result = MessageBox.Show("Are you sure you want to delete this entry?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int index = _dataListView.SelectedItems[0].Index;
                DeleteEntryFromXRecord(info.ObjectId, index);
            }
        }

#if NET8_0_OR_GREATER
        private void BtnDeleteXRecord_Click(object? sender, EventArgs e)
#else
        private void BtnDeleteXRecord_Click(object sender, EventArgs e)
#endif
        {
            if (_treeView.SelectedNode?.Tag is not XRecordInfo info) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete the entire XRecord '{info.Name}'?\n\nThis action cannot be undone.",
                "Confirm Delete XRecord",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                DeleteXRecord(info);
            }
        }

        private void DeleteSelectedItem()
        {
            var tag = _treeView.SelectedNode?.Tag;
            
            if (tag is XRecordInfo xrecInfo)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the XRecord '{xrecInfo.Name}'?\n\nThis action cannot be undone.",
                    "Confirm Delete XRecord",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    DeleteXRecord(xrecInfo);
                }
            }
            else if (tag is DictionaryInfo dictInfo)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete the Dictionary '{dictInfo.Name}'?\n\nThis will delete ALL XRecords and sub-dictionaries inside it!\n\nThis action cannot be undone.",
                    "Confirm Delete Dictionary",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    DeleteDictionary(dictInfo);
                }
            }
        }

        #endregion

        #region XRecord Operations

        private void LoadXRecordData(XRecordInfo info)
        {
            try
            {
                var xrec = _transactionHelper.Transaction.GetObject(info.ObjectId, OpenMode.ForRead) as Xrecord;
                if (xrec == null)
                {
                    UpdateStatus("Error: XRecord not found");
                    return;
                }

                _lblXRecordName.Text = $"XRecord: {info.Name}";
                
                // Use BeginUpdate/EndUpdate for better performance with large datasets
                _dataListView.BeginUpdate();
                try
                {
                    _dataListView.Items.Clear();

                    var data = xrec.Data;
                    if (data != null)
                    {
                        int index = 0;
                        foreach (TypedValue tv in data)
                        {
                            var item = new ListViewItem(index.ToString());
                            item.SubItems.Add(tv.TypeCode.ToString());
                            item.SubItems.Add(GetDxfTypeName(tv.TypeCode));
                            item.SubItems.Add(FormatTypedValue(tv));
                            item.Tag = tv;
                            _dataListView.Items.Add(item);
                            index++;
                        }

                        _lblEntryCount.Text = $"{index} entries";
                    }
                    else
                    {
                        _lblEntryCount.Text = "Empty XRecord";
                    }
                }
                finally
                {
                    _dataListView.EndUpdate();
                }

                _btnAddEntry.Enabled = true;
                _btnDeleteXRecord.Enabled = true;
                _btnEditEntry.Enabled = _dataListView.SelectedItems.Count > 0;
                _btnDeleteEntry.Enabled = _dataListView.SelectedItems.Count > 0;

                UpdateStatus($"Loaded XRecord: {info.Name}");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error loading XRecord: {ex.Message}");
                MessageBox.Show($"Error loading XRecord:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDictionaryInfo(DictionaryInfo info)
        {
            try
            {
                var dict = _transactionHelper.Transaction.GetObject(info.ObjectId, OpenMode.ForRead) as DBDictionary;
                if (dict == null)
                {
                    UpdateStatus("Error: Dictionary not found");
                    return;
                }

                _lblXRecordName.Text = $"Dictionary: {info.Name}";
                
                _dataListView.BeginUpdate();
                try
                {
                    _dataListView.Items.Clear();

                    // Show dictionary contents
                    int xrecCount = 0;
                    int dictCount = 0;
                    int otherCount = 0;

                    foreach (var entry in dict)
                    {
                        var obj = _transactionHelper.Transaction.GetObject(entry.Value, OpenMode.ForRead);
                        if (obj is Xrecord)
                        {
                            var item = new ListViewItem(entry.Key);
                            item.SubItems.Add("XRecord");
                            item.SubItems.Add("XRecord");
                            item.SubItems.Add(entry.Value.Handle.ToString());
                            _dataListView.Items.Add(item);
                            xrecCount++;
                        }
                        else if (obj is DBDictionary)
                        {
                            var item = new ListViewItem(entry.Key);
                            item.SubItems.Add("Dictionary");
                            item.SubItems.Add("Sub-Dictionary");
                            item.SubItems.Add(entry.Value.Handle.ToString());
                            _dataListView.Items.Add(item);
                            dictCount++;
                        }
                        else
                        {
                            var item = new ListViewItem(entry.Key);
                            item.SubItems.Add("Other");
                            item.SubItems.Add(obj.GetType().Name);
                            item.SubItems.Add(entry.Value.Handle.ToString());
                            _dataListView.Items.Add(item);
                            otherCount++;
                        }
                    }

                    _lblEntryCount.Text = $"{xrecCount} XRecords, {dictCount} Sub-Dictionaries, {otherCount} Other entries";
                }
                finally
                {
                    _dataListView.EndUpdate();
                }

                _btnAddEntry.Enabled = false; // Can't add entries to dictionaries directly
                _btnDeleteXRecord.Enabled = true; // Can delete the dictionary
                _btnEditEntry.Enabled = false;
                _btnDeleteEntry.Enabled = false;

                UpdateStatus($"Loaded Dictionary: {info.Name}");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error loading Dictionary: {ex.Message}");
                MessageBox.Show($"Error loading Dictionary:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddEntryToXRecord(ObjectId xrecId, TypedValue newValue)
        {
            try
            {
                var xrec = _transactionHelper.Transaction.GetObject(xrecId, OpenMode.ForWrite) as Xrecord;
                if (xrec == null) return;

                var currentData = xrec.Data;
                var newList = new List<TypedValue>();

                if (currentData != null)
                {
                    foreach (TypedValue tv in currentData)
                    {
                        newList.Add(tv);
                    }
                }

                newList.Add(newValue);

                using (var rb = new ResultBuffer(newList.ToArray()))
                {
                    xrec.Data = rb;
                }

                // Reload display
                if (_treeView.SelectedNode?.Tag is XRecordInfo info)
                {
                    LoadXRecordData(info);
                }

                UpdateStatus("Entry added successfully");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error adding entry: {ex.Message}");
                MessageBox.Show($"Error adding entry:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateEntryInXRecord(ObjectId xrecId, int index, TypedValue newValue)
        {
            try
            {
                var xrec = _transactionHelper.Transaction.GetObject(xrecId, OpenMode.ForWrite) as Xrecord;
                if (xrec == null) return;

                var currentData = xrec.Data;
                if (currentData == null) return;

                var newList = new List<TypedValue>();
                int i = 0;
                foreach (TypedValue tv in currentData)
                {
                    newList.Add(i == index ? newValue : tv);
                    i++;
                }

                using (var rb = new ResultBuffer(newList.ToArray()))
                {
                    xrec.Data = rb;
                }

                // Reload display
                if (_treeView.SelectedNode?.Tag is XRecordInfo info)
                {
                    LoadXRecordData(info);
                }

                UpdateStatus("Entry updated successfully");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error updating entry: {ex.Message}");
                MessageBox.Show($"Error updating entry:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteEntryFromXRecord(ObjectId xrecId, int index)
        {
            try
            {
                var xrec = _transactionHelper.Transaction.GetObject(xrecId, OpenMode.ForWrite) as Xrecord;
                if (xrec == null) return;

                var currentData = xrec.Data;
                if (currentData == null) return;

                var newList = new List<TypedValue>();
                int i = 0;
                foreach (TypedValue tv in currentData)
                {
                    if (i != index)
                        newList.Add(tv);
                    i++;
                }

                using (var rb = new ResultBuffer(newList.ToArray()))
                {
                    xrec.Data = rb;
                }

                // Reload display
                if (_treeView.SelectedNode?.Tag is XRecordInfo info)
                {
                    LoadXRecordData(info);
                }

                UpdateStatus("Entry deleted successfully");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error deleting entry: {ex.Message}");
                MessageBox.Show($"Error deleting entry:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteXRecord(XRecordInfo info)
        {
            try
            {
                // Get parent dictionary
                var dict = _transactionHelper.Transaction.GetObject(info.ParentDictionaryId, OpenMode.ForWrite) as DBDictionary;
                if (dict == null)
                {
                    MessageBox.Show("Cannot find parent dictionary", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Remove from dictionary
                dict.Remove(info.Name);

                // Erase the XRecord
                var xrec = _transactionHelper.Transaction.GetObject(info.ObjectId, OpenMode.ForWrite) as Xrecord;
                if (xrec != null)
                {
                    xrec.Erase();
                }

                UpdateStatus($"XRecord '{info.Name}' deleted successfully");

                // Refresh tree
                LoadXRecords();
                ClearDetails();
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error deleting XRecord: {ex.Message}");
                MessageBox.Show($"Error deleting XRecord:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteDictionary(DictionaryInfo info)
        {
            try
            {
                // Open the dictionary to be deleted
                var dictToDelete = _transactionHelper.Transaction.GetObject(info.ObjectId, OpenMode.ForWrite) as DBDictionary;
                if (dictToDelete == null)
                {
                    MessageBox.Show("Cannot open dictionary for deletion", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Find and delete from parent dictionary
                // We need to search through all dictionaries to find where this one is referenced
                bool deleted = false;
                
                // Try Named Objects Dictionary first
                var nod = _transactionHelper.Transaction.GetObject(_database.NamedObjectsDictionaryId, OpenMode.ForWrite) as DBDictionary;
                if (nod != null && TryRemoveFromDictionary(nod, info.ObjectId))
                {
                    deleted = true;
                }
                
                // If not found, search recursively (for nested dictionaries)
                if (!deleted)
                {
                    deleted = SearchAndRemoveFromDictionary(_database.NamedObjectsDictionaryId, info.ObjectId);
                }

                if (deleted)
                {
                    // Erase the dictionary and all its contents
                    dictToDelete.Erase();
                    
                    UpdateStatus($"Dictionary '{info.Name}' deleted successfully");
                    
                    // Refresh tree
                    LoadXRecords();
                    ClearDetails();
                }
                else
                {
                    MessageBox.Show("Could not find dictionary in parent to remove reference", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error deleting Dictionary: {ex.Message}");
                MessageBox.Show($"Error deleting Dictionary:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryRemoveFromDictionary(DBDictionary dict, ObjectId targetId)
        {
            foreach (var entry in dict)
            {
                if (entry.Value == targetId)
                {
                    dict.Remove(entry.Key);
                    return true;
                }
            }
            return false;
        }

        private bool SearchAndRemoveFromDictionary(ObjectId dictId, ObjectId targetId)
        {
            try
            {
                var dict = _transactionHelper.Transaction.GetObject(dictId, OpenMode.ForWrite) as DBDictionary;
                if (dict == null) return false;

                // Check direct children
                if (TryRemoveFromDictionary(dict, targetId))
                    return true;

                // Check sub-dictionaries recursively
                foreach (var entry in dict)
                {
                    var obj = _transactionHelper.Transaction.GetObject(entry.Value, OpenMode.ForRead);
                    if (obj is DBDictionary)
                    {
                        if (SearchAndRemoveFromDictionary(entry.Value, targetId))
                            return true;
                    }
                }
            }
            catch
            {
                // Ignore errors during search
            }
            return false;
        }

        #endregion

        #region Helper Methods

        private void ClearDetails()
        {
            _lblXRecordName.Text = "Select an XRecord from the tree";
            _lblEntryCount.Text = "";
            _dataListView.Items.Clear();
            _btnAddEntry.Enabled = false;
            _btnEditEntry.Enabled = false;
            _btnDeleteEntry.Enabled = false;
            _btnDeleteXRecord.Enabled = false;
        }

        private void UpdateStatus(string message)
        {
            _statusLabel.Text = message;
            _statusStrip.Refresh();
        }

        private string FormatTypedValue(TypedValue tv)
        {
            if (tv.Value == null)
                return "[null]";

            return tv.Value.ToString();
        }

        private string GetDxfTypeName(short code)
        {
            // Based on DXF code ranges
            if (code == 0) return "String (Entity Type)";
            if (code >= 1 && code <= 9) return "String";
            if (code == 5 || code == 105) return "Handle";
            if (code >= 10 && code <= 18) return "Point3d";
            if (code >= 38 && code <= 59) return "Double";
            if (code >= 60 && code <= 79) return "Int16";
            if (code >= 90 && code <= 99) return "Int32";
            if (code >= 100 && code <= 102) return "String";
            if (code >= 140 && code <= 147) return "Double";
            if (code >= 170 && code <= 175) return "Int16";
            if (code >= 280 && code <= 289) return "Int8/Boolean";
            if (code >= 290 && code <= 293) return "Boolean";
            if (code >= 300 && code <= 319) return "String";
            if (code >= 320 && code <= 369) return "Handle";
            if (code >= 1000) return "XData";

            return "Unknown";
        }

        #endregion

        #region Helper Classes

        private class XRecordInfo
        {
            public ObjectId ObjectId { get; set; }
            public string Name { get; set; }
            public ObjectId ParentDictionaryId { get; set; }
        }

        private class DictionaryInfo
        {
            public ObjectId ObjectId { get; set; }
            public string Name { get; set; }
        }

        #endregion
    }
}

