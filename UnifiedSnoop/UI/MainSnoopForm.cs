// MainSnoopForm.cs - Main UI form for UnifiedSnoop
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)
// Based on Civil3DSnoop's UI pattern: TreeView for hierarchy, ListView for properties

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;
using UnifiedSnoop.Core.Helpers;
using UnifiedSnoop.Services;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#else
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

namespace UnifiedSnoop.UI
{
    /// <summary>
    /// Main form for the UnifiedSnoop application.
    /// Displays object hierarchy in a TreeView and properties in a ListView.
    /// </summary>
    public partial class MainSnoopForm : Form
    {
        #region Fields

        private readonly Database _database;
        private readonly TransactionHelper _transactionHelper;
        private readonly DatabaseService _databaseService;
        private readonly Services.BookmarkService _bookmarkService;
        
        // UI Controls (initialized in InitializeComponent)
        #if NET8_0_OR_GREATER
        private TreeView _treeView = null!;
        private ListView _listView = null!;
        private SplitContainer _splitContainer = null!;
        private Button _btnSelectObject = null!;
        private Button _btnRefresh = null!;
        private Button _btnExport = null!;
        private Button _btnCompare = null!;
        private Button _btnAddBookmark = null!;
        private Button _btnViewBookmarks = null!;
        private Label _lblPropertyCount = null!;
        private Label _lblStatus = null!;
        private Panel _topPanel = null!;
        private Panel _toolbarPanel = null!;
        private Panel _bottomPanel = null!;
        private FlowLayoutPanel _searchPanel = null!;
        private TextBox _txtSearch = null!;
        private Button _btnClearSearch = null!;
        private Button _btnCopyValue = null!;
        private Button _btnCopyAll = null!;
        private Label _lblSearch = null!;
        private ToolTip _toolTip = null!;
        #else
        private TreeView _treeView;
        private ListView _listView;
        private SplitContainer _splitContainer;
        private Button _btnSelectObject;
        private Button _btnRefresh;
        private Button _btnExport;
        private Button _btnCompare;
        private Button _btnAddBookmark;
        private Button _btnViewBookmarks;
        private Label _lblPropertyCount;
        private Label _lblStatus;
        private Panel _topPanel;
        private Panel _toolbarPanel;
        private Panel _bottomPanel;
        private FlowLayoutPanel _searchPanel;
        private TextBox _txtSearch;
        private Button _btnClearSearch;
        private Button _btnCopyValue;
        private Button _btnCopyAll;
        private Label _lblSearch;
        private ToolTip _toolTip;
        #endif

        #if NET8_0_OR_GREATER
        private object? _currentObject;
        private List<PropertyData>? _allProperties;
        #else
        private object _currentObject;
        private List<PropertyData> _allProperties;
        #endif

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainSnoopForm"/> class.
        /// </summary>
        /// <param name="database">The database to snoop.</param>
        /// <param name="transactionHelper">The transaction helper for database operations.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when database or transactionHelper is null.
        /// </exception>
        public MainSnoopForm(Database database, TransactionHelper transactionHelper)
        {
            try
            {
                _database = database ?? throw new ArgumentNullException(nameof(database));
                _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
                _databaseService = new DatabaseService(database, transactionHelper);
                _bookmarkService = new Services.BookmarkService();

                InitializeComponent();
                InitializeForm();
                SetupKeyboardShortcuts();
            }
            catch (System.Exception ex)
            {
                Services.ErrorLogService.Instance.LogError("Error initializing MainSnoopForm", ex, "MainSnoopForm constructor");
                throw; // Re-throw to prevent showing a broken form
            }
        }

        /// <summary>
        /// Initializes a new instance of the MainSnoopForm with a specific object to snoop.
        /// </summary>
        /// <param name="database">The AutoCAD database.</param>
        /// <param name="transactionHelper">The transaction helper.</param>
        /// <param name="targetObject">The specific object to snoop.</param>
        public MainSnoopForm(Database database, TransactionHelper transactionHelper, object targetObject) 
            : this(database, transactionHelper)
        {
            if (targetObject != null)
            {
                // Display the specific object immediately
                DisplayObjectProperties(targetObject);
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the form controls and layout.
        /// </summary>
        private void InitializeForm()
        {
            // Set form properties
            string hostApp = VersionHelper.IsCivil3DAvailable() ? "Civil 3D" : "AutoCAD";
            this.Text = $"UnifiedSnoop - Database Inspector ({hostApp})";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);

            // Create top panel with property count (per spec: 35px height)
            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35,
                BackColor = SystemColors.Control,
                Padding = new Padding(10, 8, 10, 8)
            };

            _lblPropertyCount = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.Blue,
                Font = new System.Drawing.Font(this.Font, FontStyle.Bold)
            };

            _topPanel.Controls.Add(_lblPropertyCount);

            // Create toolbar panel with buttons (40px height for comfort)
            _toolbarPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                Padding = new Padding(5)
            };

            _btnSelectObject = new Button
            {
                Text = "Select Object",
                Location = new Point(10, 8),
                Size = new Size(120, 25)
            };
            _btnSelectObject.Click += BtnSelectObject_Click;

            _btnRefresh = new Button
            {
                Text = "Refresh",
                Location = new Point(140, 8),
                Size = new Size(80, 25)
            };
            _btnRefresh.Click += BtnRefresh_Click;

            _btnExport = new Button
            {
                Text = "Export...",
                Location = new Point(230, 8),
                Size = new Size(80, 25)
            };
            _btnExport.Click += BtnExport_Click;

            _btnCompare = new Button
            {
                Text = "Compare...",
                Location = new Point(320, 8),
                Size = new Size(90, 25)
            };
            _btnCompare.Click += BtnCompare_Click;

            _btnAddBookmark = new Button
            {
                Text = "★ Add",
                Location = new Point(420, 8),
                Size = new Size(70, 25)
            };
            _btnAddBookmark.Click += BtnAddBookmark_Click;

            _btnViewBookmarks = new Button
            {
                Text = "Bookmarks",
                Location = new Point(500, 8),
                Size = new Size(90, 25)
            };
            _btnViewBookmarks.Click += BtnViewBookmarks_Click;

            _toolbarPanel.Controls.Add(_btnSelectObject);
            _toolbarPanel.Controls.Add(_btnRefresh);
            _toolbarPanel.Controls.Add(_btnExport);
            _toolbarPanel.Controls.Add(_btnCompare);
            _toolbarPanel.Controls.Add(_btnAddBookmark);
            _toolbarPanel.Controls.Add(_btnViewBookmarks);

            // Create split container
            // NOTE: DO NOT set SplitterDistance here - it will crash before form is sized!
            // SplitterDistance is set in OnLoad() after form layout is complete
            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                BorderStyle = BorderStyle.Fixed3D,
                IsSplitterFixed = false,
                SplitterWidth = 4,  // Per UI spec line 80
                Panel1MinSize = 150,  // Minimum width for TreeView panel (reduced from 200)
                Panel2MinSize = 250   // Minimum width for Property inspector panel (reduced from 400)
                // SplitterDistance will be set in OnLoad() - line ~437
            };

            // Create TreeView (left panel of split container)
            _treeView = new TreeView
            {
                Dock = DockStyle.Fill,
                HideSelection = false,
                FullRowSelect = true,
                ShowLines = true,
                ShowPlusMinus = true,
                ShowRootLines = true,
                Scrollable = true  // Ensure scrollbars are enabled
            };
            _treeView.AfterSelect += TreeView_AfterSelect;
            _treeView.BeforeExpand += TreeView_BeforeExpand;

            // Create search panel with FlowLayoutPanel for responsive button layout
            _searchPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 40,  // Increased from 38 to 40
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(5, 5, 5, 8),  // Increased bottom padding to 8px
                AutoScroll = true,
                BackColor = SystemColors.Control
            };

            _lblSearch = new Label
            {
                Text = "Search:",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 4, 5, 0)
            };

            _txtSearch = new TextBox
            {
                Width = 200,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 2, 10, 0)
            };
            _txtSearch.TextChanged += TxtSearch_TextChanged;

            _btnClearSearch = new Button
            {
                Text = "Clear",
                Size = new Size(70, 25),
                Margin = new Padding(0, 0, 5, 0)
            };
            _btnClearSearch.Click += BtnClearSearch_Click;

            _btnCopyValue = new Button
            {
                Text = "Copy Value",
                Size = new Size(90, 25),
                Margin = new Padding(0, 0, 5, 0),
                Enabled = false
            };
            _btnCopyValue.Click += BtnCopyValue_Click;

            _btnCopyAll = new Button
            {
                Text = "Copy All",
                Size = new Size(80, 25),
                Margin = new Padding(0, 0, 0, 0)
            };
            _btnCopyAll.Click += BtnCopyAll_Click;

            _searchPanel.Controls.Add(_lblSearch);
            _searchPanel.Controls.Add(_txtSearch);
            _searchPanel.Controls.Add(_btnClearSearch);
            _searchPanel.Controls.Add(_btnCopyValue);
            _searchPanel.Controls.Add(_btnCopyAll);

            // Create ListView (right panel of split container)
            // CRITICAL FIX: Use specific positioning to ensure headers are always visible
            _listView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = false,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                HeaderStyle = ColumnHeaderStyle.Clickable,
                Scrollable = true,
                // Use Anchor for proper resizing, not Dock
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(10, 50),  // Start well below search panel (40px) + spacing
                Size = new Size(100, 100)  // Will be resized in Panel2.Resize event
            };

            // Add columns to ListView with proper sizing
            _listView.Columns.Add("Property", 250);
            _listView.Columns.Add("Type", 180);
            _listView.Columns.Add("Value", -2);  // -2 = auto-size to fill remaining space

            _listView.SelectedIndexChanged += ListView_SelectedIndexChanged;
            _listView.DoubleClick += ListView_DoubleClick;
            _listView.MouseMove += ListView_MouseMove;

            // Add controls to split container in correct order
            // CRITICAL FIX: Add directly to Panel2 without intermediate container to avoid header hiding
            _splitContainer.Panel1.Controls.Add(_treeView);
            _splitContainer.Panel2.Controls.Add(_searchPanel);  // Docks to top (40px height)
            _splitContainer.Panel2.Controls.Add(_listView);     // Anchored, positioned below search
            
            // Handle Panel2 resize to properly size ListView and ensure headers stay visible
            _splitContainer.Panel2.Resize += (sender, e) =>
            {
                if (_listView != null && _splitContainer.Panel2 != null)
                {
                    int availableWidth = _splitContainer.Panel2.ClientSize.Width;
                    int availableHeight = _splitContainer.Panel2.ClientSize.Height;
                    int searchPanelHeight = _searchPanel.Height;
                    
                    // Position ListView below search panel with margins
                    _listView.Location = new Point(10, searchPanelHeight + 10);
                    
                    // Calculate dimensions ensuring non-negative values to prevent rendering issues
                    int listViewWidth = Math.Max(0, availableWidth - 20);  // 10px margin on each side
                    int listViewHeight = Math.Max(0, availableHeight - searchPanelHeight - 20);  // 10px top + 10px bottom margin
                    
                    _listView.Size = new Size(listViewWidth, listViewHeight);
                }
            };

            // Create bottom status panel (per spec: 25px height)
            _bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 25,
                BackColor = SystemColors.Control,
                Padding = new Padding(10, 4, 10, 4),
                BorderStyle = BorderStyle.FixedSingle
            };

            _lblStatus = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _bottomPanel.Controls.Add(_lblStatus);

            // Add controls to form (order matters for docking!)
            // CRITICAL: Add in specific order for proper docking behavior
            // Bottom docks first, then top elements, then fill
            this.Controls.Add(_bottomPanel);       // Dock to bottom first
            this.Controls.Add(_topPanel);          // Dock to top (occupies top position)
            this.Controls.Add(_toolbarPanel);      // Dock to top (below _topPanel)
            this.Controls.Add(_splitContainer);    // Fill remaining space last

            // Setup tooltips
            SetupTooltips();

            // Load initial data
            LoadDatabaseTree();
        }

        /// <summary>
        /// Required method for Designer support - do not modify.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Called when the form is loaded - sets up splitter distance after form is sized.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            
            try
            {
                // Calculate available width for split container
                int availableWidth = this.ClientSize.Width;
                
                // Set SplitterDistance per spec: 400px for left panel (TreeView)
                int desiredDistance = 400; // Per UI spec
                int minDistance = _splitContainer.Panel1MinSize; // 150
                int maxDistance = availableWidth - _splitContainer.Panel2MinSize - _splitContainer.SplitterWidth;
                
                // Only adjust if we have enough space, otherwise keep the initial 400px
                if (availableWidth >= 600) // Minimum sensible width (150 + 250 + 4 + margins = 404+)
                {
                    if (maxDistance > minDistance && desiredDistance > minDistance && desiredDistance < maxDistance)
                    {
                        _splitContainer.SplitterDistance = desiredDistance;
                    }
                    else if (maxDistance > minDistance)
                    {
                        // Use a proportion: 1/3 for tree, 2/3 for properties
                        _splitContainer.SplitterDistance = availableWidth / 3;
                    }
                }
                // If form is too small, keep the initial 400px and let user adjust
                
                // CRITICAL: Trigger initial ListView sizing to ensure headers are visible
                if (_listView != null && _splitContainer.Panel2 != null)
                {
                    int panelWidth = _splitContainer.Panel2.ClientSize.Width;
                    int panelHeight = _splitContainer.Panel2.ClientSize.Height;
                    int searchPanelHeight = _searchPanel.Height;
                    
                    _listView.Location = new Point(10, searchPanelHeight + 10);
                    
                    // Calculate dimensions ensuring non-negative values to prevent rendering issues
                    int listViewWidth = Math.Max(0, panelWidth - 20);
                    int listViewHeight = Math.Max(0, panelHeight - searchPanelHeight - 20);
                    
                    _listView.Size = new Size(listViewWidth, listViewHeight);
                }
                
                UpdateStatus($"Form loaded: {availableWidth}px wide, splitter at {_splitContainer.SplitterDistance}px");
            }
            catch (System.Exception ex)
            {
                Services.ErrorLogService.Instance.LogError("Error setting splitter distance", ex, "MainSnoopForm.OnLoad");
                UpdateStatus($"Warning: Splitter setup error - using defaults");
            }
        }

        /// <summary>
        /// Sets up keyboard shortcuts for the form.
        /// </summary>
        private void SetupKeyboardShortcuts()
        {
            // Enable key preview to capture keyboard events
            this.KeyPreview = true;
            this.KeyDown += MainSnoopForm_KeyDown;
        }

        /// <summary>
        /// Sets up tooltips for all UI controls.
        /// </summary>
        private void SetupTooltips()
        {
            _toolTip = new ToolTip
            {
                AutoPopDelay = 5000,
                InitialDelay = 500,
                ReshowDelay = 200,
                ShowAlways = true
            };

            // Top panel tooltips
            _toolTip.SetToolTip(_btnSelectObject, "Select an object from the drawing to inspect");
            _toolTip.SetToolTip(_btnRefresh, "Refresh the current view (F5)");
            _toolTip.SetToolTip(_btnExport, "Export object properties to CSV or Excel format");
            _toolTip.SetToolTip(_btnCompare, "Compare current object with another object");
            _toolTip.SetToolTip(_btnAddBookmark, "Add current object to bookmarks (Ctrl+B)");
            _toolTip.SetToolTip(_btnViewBookmarks, "View and manage bookmarks (Ctrl+Shift+B)");

            // Search panel tooltips
            _toolTip.SetToolTip(_txtSearch, "Search properties by name, type, or value. Press Ctrl+F to focus. Case-insensitive.");
            _toolTip.SetToolTip(_btnClearSearch, "Clear search filter (Esc)");
            _toolTip.SetToolTip(_btnCopyValue, "Copy selected property value to clipboard (Ctrl+C)");
            _toolTip.SetToolTip(_btnCopyAll, "Copy all visible properties to clipboard in Excel-ready format (Ctrl+Shift+C)");

            // TreeView and ListView tooltips
            _toolTip.SetToolTip(_treeView, "Object hierarchy. Click to select an object and view its properties. Press Ctrl+L to focus.");
            _toolTip.SetToolTip(_listView, "Properties of the selected object. Double-click on blue collection items to expand them. Press Ctrl+P to focus.");
        }

        #endregion

        #region Tree View Management

        /// <summary>
        /// Loads the database tree structure.
        /// </summary>
        private void LoadDatabaseTree()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadDatabaseTree: Starting...");
                
                if (_treeView == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDatabaseTree: ERROR - _treeView is null!");
                    UpdateStatus("Error: TreeView not initialized");
                    return;
                }

                if (_database == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDatabaseTree: ERROR - _database is null!");
                    UpdateStatus("Error: Database not initialized");
                    return;
                }

                if (_transactionHelper == null || _transactionHelper.Transaction == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadDatabaseTree: ERROR - Transaction not available!");
                    UpdateStatus("Error: Transaction not available");
                    return;
                }

                _treeView.BeginUpdate();
                _treeView.Nodes.Clear();
                System.Diagnostics.Debug.WriteLine("LoadDatabaseTree: TreeView cleared");

                // Create root node for database
                TreeNode rootNode = new TreeNode("Database")
                {
                    Tag = _database
                };
                System.Diagnostics.Debug.WriteLine("LoadDatabaseTree: Root node created");

                // Add AutoCAD collections
                TreeNode acadNode = new TreeNode("AutoCAD Collections");
                var acadCollections = _databaseService.GetDatabaseCollections(_transactionHelper.Transaction);
                System.Diagnostics.Debug.WriteLine($"LoadDatabaseTree: Got {acadCollections.Count} AutoCAD collections");
                
                foreach (var objNode in acadCollections)
                {
                    TreeNode treeNode = CreateTreeNode(objNode);
                    acadNode.Nodes.Add(treeNode);
                }

                rootNode.Nodes.Add(acadNode);
                System.Diagnostics.Debug.WriteLine($"LoadDatabaseTree: Added AutoCAD node with {acadNode.Nodes.Count} children");

                // Add Civil 3D collections if available
                if (VersionHelper.IsCivil3DAvailable())
                {
                    TreeNode civilNode = new TreeNode("Civil 3D Collections");
                    var civilCollections = _databaseService.GetCivil3DCollections(_transactionHelper.Transaction);
                    System.Diagnostics.Debug.WriteLine($"LoadDatabaseTree: Got {civilCollections.Count} Civil 3D collections");
                    
                    foreach (var objNode in civilCollections)
                    {
                        TreeNode treeNode = CreateTreeNode(objNode);
                        civilNode.Nodes.Add(treeNode);
                    }

                    // Always add Civil 3D node if detected, even if collections are empty
                    // (may contain diagnostic/info messages)
                    if (civilNode.Nodes.Count > 0)
                    {
                        rootNode.Nodes.Add(civilNode);
                    }
                    else
                    {
                        // Add a message if Civil 3D is detected but no collections found
                        civilNode.Nodes.Add(new TreeNode("No Civil 3D collections found in this drawing"));
                        rootNode.Nodes.Add(civilNode);
                    }
                    System.Diagnostics.Debug.WriteLine($"LoadDatabaseTree: Added Civil 3D node with {civilNode.Nodes.Count} children");
                }

                // Add root node and expand it
                _treeView.Nodes.Add(rootNode);
                rootNode.Expand();
                System.Diagnostics.Debug.WriteLine($"LoadDatabaseTree: Root node added with {rootNode.Nodes.Count} children. Tree now has {_treeView.Nodes.Count} root nodes");

                UpdateStatus($"Loaded database tree with {acadCollections.Count} AutoCAD collections. Civil 3D: {VersionHelper.IsCivil3DAvailable()}");
                System.Diagnostics.Debug.WriteLine("LoadDatabaseTree: Complete!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadDatabaseTree: EXCEPTION - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"LoadDatabaseTree: Stack trace - {ex.StackTrace}");
                
                MessageBox.Show($"Error loading database tree: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Error: {ex.Message}");
                
                Services.ErrorLogService.Instance.LogError("Error loading database tree", ex, "LoadDatabaseTree");
            }
            finally
            {
                _treeView.EndUpdate();
                System.Diagnostics.Debug.WriteLine("LoadDatabaseTree: EndUpdate called");
            }
        }

        /// <summary>
        /// Creates a TreeNode from an ObjectNode.
        /// </summary>
        private TreeNode CreateTreeNode(ObjectNode objNode)
        {
            TreeNode treeNode = new TreeNode(objNode.Name)
            {
                Tag = objNode
            };

            // Add dummy node for collections to show expand button
            if (objNode.IsCollection)
            {
                treeNode.Nodes.Add(new TreeNode("Loading..."));
            }

            return treeNode;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the AfterSelect event of the TreeView.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void TreeView_AfterSelect(object? sender, TreeViewEventArgs e)
        #else
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        #endif
        {
            #if NET8_0_OR_GREATER
            if (e.Node?.Tag == null)
            {
                UpdateStatus("TreeView: No node tag");
                return;
            }
            #else
            if (e.Node == null || e.Node.Tag == null)
            {
                UpdateStatus("TreeView: No node or tag");
                return;
            }
            #endif

            try
            {
                UpdateStatus($"TreeView: Selected node '{e.Node.Text}'");
                
                object selectedObj = null;

                if (e.Node.Tag is ObjectNode objNode)
                {
                    selectedObj = objNode.Object;
                    UpdateStatus($"TreeView: ObjectNode found, Object type = {selectedObj?.GetType().Name ?? "null"}");
                }
                else
                {
                    selectedObj = e.Node.Tag;
                    UpdateStatus($"TreeView: Direct tag, type = {selectedObj.GetType().Name}");
                }

                if (selectedObj != null)
                {
                    _currentObject = selectedObj;
                    UpdateStatus($"TreeView: Calling DisplayObjectProperties for {selectedObj.GetType().Name}");
                    DisplayObjectProperties(selectedObj);
                    UpdateBookmarkButton();
                }
                else
                {
                    UpdateStatus("TreeView: Selected object is null!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error selecting object: {ex.Message}\n\nStack: {ex.StackTrace}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"TreeView Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"TreeView_AfterSelect error: {ex}");
            }
        }

        /// <summary>
        /// Handles the BeforeExpand event of the TreeView.
        /// Lazy-loads child nodes for collections.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void TreeView_BeforeExpand(object? sender, TreeViewCancelEventArgs e)
        #else
        private void TreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        #endif
        {
            #if NET8_0_OR_GREATER
            if (e.Node?.Tag == null)
                return;
            #else
            if (e.Node == null || e.Node.Tag == null)
                return;
            #endif

            // Check if this is the first expansion (dummy node present)
            if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Text == "Loading...")
            {
                try
                {
                    e.Node.Nodes.Clear();
                    ExpandNode(e.Node);
                }
                catch (Exception ex)
                {
                    e.Node.Nodes.Clear();
                    e.Node.Nodes.Add($"Error: {ex.Message}");
                    UpdateStatus($"Error expanding node: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Expands a tree node by loading its children.
        /// </summary>
        private void ExpandNode(TreeNode treeNode)
        {
            object obj = null;

            if (treeNode.Tag is ObjectNode objNode)
            {
                obj = objNode.Object;
            }
            else
            {
                obj = treeNode.Tag;
            }

            if (obj == null)
                return;

            // Handle different collection types
            if (obj is SymbolTable symTable)
            {
                ExpandSymbolTable(treeNode, symTable);
            }
            else if (obj is DBDictionary dict)
            {
                ExpandDictionary(treeNode, dict);
            }
            else if (obj is BlockTableRecord btr)
            {
                ExpandBlockTableRecord(treeNode, btr);
            }
            else
            {
                // Try to get collections using reflection
                var collections = CollectorRegistry.Instance.GetCollections(obj, _transactionHelper.Transaction);
                
                if (collections.Count > 0)
                {
                    foreach (var kvp in collections)
                    {
                        TreeNode collNode = new TreeNode($"{kvp.Key} [Collection]");
                        collNode.Tag = kvp.Value;
                        collNode.Nodes.Add("Loading...");
                        treeNode.Nodes.Add(collNode);
                    }
                }
            }
        }

        /// <summary>
        /// Expands a SymbolTable node.
        /// </summary>
        private void ExpandSymbolTable(TreeNode parentNode, SymbolTable symTable)
        {
            foreach (ObjectId id in symTable)
            {
                try
                {
                    var record = _transactionHelper.Transaction.GetObject(id, OpenMode.ForRead) as SymbolTableRecord;
                    if (record != null)
                    {
                        #if NET8_0_OR_GREATER
                        string name = record.Name ?? "[Unnamed]";
                        #else
                        string name = string.IsNullOrEmpty(record.Name) ? "[Unnamed]" : record.Name;
                        #endif

                        TreeNode childNode = new TreeNode(name)
                        {
                            Tag = new ObjectNode(name, record) { ObjectId = id }
                        };
                        parentNode.Nodes.Add(childNode);
                    }
                }
                catch
                {
                    // Skip objects that can't be opened
                    continue;
                }
            }
        }

        /// <summary>
        /// Expands a DBDictionary node.
        /// </summary>
        private void ExpandDictionary(TreeNode parentNode, DBDictionary dict)
        {
            foreach (DBDictionaryEntry entry in dict)
            {
                try
                {
                    var obj = _transactionHelper.Transaction.GetObject(entry.Value, OpenMode.ForRead);
                    
                    TreeNode childNode = new TreeNode(entry.Key)
                    {
                        Tag = new ObjectNode(entry.Key, obj) { ObjectId = entry.Value }
                    };

                    // Add dummy node if this is also a dictionary
                    if (obj is DBDictionary)
                    {
                        childNode.Nodes.Add("Loading...");
                    }

                    parentNode.Nodes.Add(childNode);
                }
                catch
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// Expands a BlockTableRecord node.
        /// Shows all entities (no limit).
        /// </summary>
        private void ExpandBlockTableRecord(TreeNode parentNode, BlockTableRecord btr)
        {
            int count = 0;
            foreach (ObjectId id in btr)
            {
                try
                {
                    var entity = _transactionHelper.Transaction.GetObject(id, OpenMode.ForRead) as Entity;
                    if (entity != null)
                    {
                        string name = $"{entity.GetType().Name} [{id.Handle}]";
                        TreeNode childNode = new TreeNode(name)
                        {
                            Tag = new ObjectNode(name, entity) { ObjectId = id }
                        };
                        parentNode.Nodes.Add(childNode);
                        count++;
                    }
                }
                catch
                {
                    // Skip entities that can't be opened
                    continue;
                }
            }
            
            // Update parent node text to show count
            if (count > 0)
            {
                parentNode.Text = $"{parentNode.Text} ({count} entities)";
            }
        }

        /// <summary>
        /// Expands a collection property and adds it to the tree view.
        /// Handles various collection types including ObjectId collections, IEnumerable, etc.
        /// </summary>
        private void ExpandCollectionProperty(PropertyData propData)
        {
            try
            {
                if (propData.RawValue == null)
                {
                    MessageBox.Show("Collection is null.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Get the current tree node (if any)
                TreeNode currentNode = _treeView.SelectedNode;
                
                // Check if collection node already exists
                string collectionNodeName = $"{propData.Name} [Collection]";
                if (currentNode != null)
                {
                    foreach (TreeNode childNode in currentNode.Nodes)
                    {
                        if (childNode.Text == collectionNodeName)
                        {
                            // Already expanded, just select it
                            _treeView.SelectedNode = childNode;
                            childNode.Expand();
                            UpdateStatus($"Collection '{propData.Name}' already expanded");
                            return;
                        }
                    }
                }

                // Create a new tree node for this collection
                TreeNode collectionNode = new TreeNode(collectionNodeName)
                {
                    Tag = propData.RawValue,
                    ForeColor = Color.Blue
                };

                // Expand the collection based on its type
                int itemCount = ExpandCollectionItems(collectionNode, propData.RawValue);

                if (itemCount == 0)
                {
                    collectionNode.Nodes.Add("[Empty Collection]");
                    collectionNode.ForeColor = Color.Gray;
                }
                else
                {
                    collectionNode.Text = $"{propData.Name} [Collection: {itemCount} items]";
                }

                // Add to tree
                if (currentNode != null)
                {
                    currentNode.Nodes.Add(collectionNode);
                    currentNode.Expand();
                    _treeView.SelectedNode = collectionNode;
                    collectionNode.Expand();
                }
                else
                {
                    _treeView.Nodes.Add(collectionNode);
                    collectionNode.Expand();
                }

                UpdateStatus($"Expanded collection '{propData.Name}' with {itemCount} items");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error expanding collection '{propData.Name}': {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Error expanding collection: {ex.Message}");
            }
        }

        /// <summary>
        /// Expands collection items into tree nodes.
        /// Handles ObjectId collections, IEnumerable, arrays, and other collection types.
        /// </summary>
        private int ExpandCollectionItems(TreeNode parentNode, object collection)
        {
            int count = 0;

            try
            {
                // Handle ObjectId collections (need transaction)
                if (collection is IEnumerable)
                {
                    var enumerable = collection as IEnumerable;
                    
                    foreach (var item in enumerable)
                    {
                        try
                        {
                            object actualItem = item;
                            string itemName = $"[Item {count}]";

                            // If it's an ObjectId, open the object
                            if (item is ObjectId objectId)
                            {
                                if (!objectId.IsNull && !objectId.IsErased)
                                {
                                    actualItem = _transactionHelper.Transaction.GetObject(objectId, OpenMode.ForRead);
                                    itemName = $"[Item {count}] {actualItem.GetType().Name} [{objectId.Handle}]";
                                }
                                else
                                {
                                    itemName = $"[Item {count}] [Null or Erased ObjectId]";
                                    actualItem = null;
                                }
                            }
                            // Get name from object if it has a Name property
                            else if (actualItem != null)
                            {
                                var nameProp = actualItem.GetType().GetProperty("Name");
                                if (nameProp != null && nameProp.CanRead)
                                {
                                    try
                                    {
                                        var nameValue = nameProp.GetValue(actualItem, null);
                                        if (nameValue != null)
                                        {
                                            itemName = $"[Item {count}] {nameValue}";
                                        }
                                        else
                                        {
                                            itemName = $"[Item {count}] {actualItem.GetType().Name}";
                                        }
                                    }
                                    catch
                                    {
                                        itemName = $"[Item {count}] {actualItem.GetType().Name}";
                                    }
                                }
                                else if (IsSimpleType(actualItem))
                                {
                                    // For simple types (string, number, etc.), show the value
                                    itemName = $"[Item {count}] {actualItem}";
                                }
                                else
                                {
                                    itemName = $"[Item {count}] {actualItem.GetType().Name}";
                                }
                            }

                            // Create tree node for this item
                            TreeNode itemNode = new TreeNode(itemName);
                            
                            if (actualItem != null)
                            {
                                itemNode.Tag = new ObjectNode(itemName, actualItem);
                                
                                // Add dummy node if this object might have children
                                if (!IsSimpleType(actualItem) && HasPotentialChildren(actualItem))
                                {
                                    itemNode.Nodes.Add("Loading...");
                                }
                            }
                            else
                            {
                                itemNode.ForeColor = Color.Gray;
                            }

                            parentNode.Nodes.Add(itemNode);
                            count++;
                        }
                        catch (Exception ex)
                        {
                            // Add error node but continue processing
                            TreeNode errorNode = new TreeNode($"[Item {count}] Error: {ex.Message}")
                            {
                                ForeColor = Color.Red
                            };
                            parentNode.Nodes.Add(errorNode);
                            count++;
                        }
                    }
                }
                else
                {
                    // Not a recognized collection type
                    parentNode.Nodes.Add($"[Unsupported collection type: {collection.GetType().Name}]");
                }
            }
            catch (Exception ex)
            {
                parentNode.Nodes.Add($"[Error iterating collection: {ex.Message}]");
            }

            return count;
        }

        /// <summary>
        /// Determines if an object is a simple type that shouldn't be expanded.
        /// </summary>
        private bool IsSimpleType(object obj)
        {
            if (obj == null)
                return true;

            Type type = obj.GetType();
            
            return type.IsPrimitive
                || type.IsEnum
                || type == typeof(string)
                || type == typeof(decimal)
                || type == typeof(DateTime)
                || type == typeof(DateTimeOffset)
                || type == typeof(TimeSpan)
                || type == typeof(Guid);
        }

        /// <summary>
        /// Determines if an object potentially has child properties/collections.
        /// </summary>
        private bool HasPotentialChildren(object obj)
        {
            if (obj == null || IsSimpleType(obj))
                return false;

            Type type = obj.GetType();
            
            // AutoCAD objects typically have properties
            if (typeof(DBObject).IsAssignableFrom(type))
                return true;

            // Check if it has any public readable properties
            var properties = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            return properties.Length > 0;
        }

        /// <summary>
        /// Handles ListView item selection.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void ListView_SelectedIndexChanged(object? sender, EventArgs e)
        #else
        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        #endif
        {
            // Enable/disable Copy Value button based on selection
            _btnCopyValue.Enabled = _listView.SelectedItems.Count > 0;
        }

        /// <summary>
        /// Handles double-click on ListView items.
        /// If the item is a collection, it expands to show the collection contents.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void ListView_DoubleClick(object? sender, EventArgs e)
        #else
        private void ListView_DoubleClick(object sender, EventArgs e)
        #endif
        {
            try
            {
                if (_listView.SelectedItems.Count == 0)
                    return;

                ListViewItem selectedItem = _listView.SelectedItems[0];
                
                // Check if this is a collection property
                if (selectedItem.Tag is PropertyData propData && propData.IsCollection && propData.RawValue != null)
                {
                    // Expand the collection
                    ExpandCollectionProperty(propData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error expanding collection: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Error expanding collection: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles mouse move over ListView items.
        /// Changes cursor to hand pointer when hovering over collection items.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void ListView_MouseMove(object? sender, MouseEventArgs e)
        #else
        private void ListView_MouseMove(object sender, MouseEventArgs e)
        #endif
        {
            try
            {
                // Get the item at the mouse position
                ListViewItem item = _listView.GetItemAt(e.X, e.Y);
                
                if (item != null && item.Tag is PropertyData propData && propData.IsCollection && propData.RawValue != null)
                {
                    // Change cursor to hand pointer for clickable collection items
                    if (_listView.Cursor != Cursors.Hand)
                    {
                        _listView.Cursor = Cursors.Hand;
                    }
                }
                else
                {
                    // Reset cursor to default
                    if (_listView.Cursor != Cursors.Default)
                    {
                        _listView.Cursor = Cursors.Default;
                    }
                }
            }
            catch
            {
                // Silently handle any errors in mouse move
                _listView.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Handles search text changes for property filtering.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        #else
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        #endif
        {
            FilterProperties();
        }

        /// <summary>
        /// Handles the Clear Search button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnClearSearch_Click(object? sender, EventArgs e)
        #else
        private void BtnClearSearch_Click(object sender, EventArgs e)
        #endif
        {
            _txtSearch.Text = string.Empty;
            _txtSearch.Focus();
        }

        /// <summary>
        /// Handles the Copy Value button click.
        /// Copies the selected property value to the clipboard.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnCopyValue_Click(object? sender, EventArgs e)
        #else
        private void BtnCopyValue_Click(object sender, EventArgs e)
        #endif
        {
            try
            {
                if (_listView.SelectedItems.Count == 0)
                    return;

                ListViewItem selectedItem = _listView.SelectedItems[0];
                
                // Get the property data
                if (selectedItem.Tag is PropertyData propData)
                {
                    string textToCopy = propData.Value ?? "[null]";
                    Clipboard.SetText(textToCopy);
                    UpdateStatus($"Copied: {propData.Name} = {textToCopy}");
                }
                else
                {
                    // Fallback: copy the value column text
                    string value = selectedItem.SubItems[2].Text;
                    Clipboard.SetText(value);
                    UpdateStatus($"Copied value to clipboard");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Copy All button click.
        /// Copies all visible properties to the clipboard as tab-delimited text.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnCopyAll_Click(object? sender, EventArgs e)
        #else
        private void BtnCopyAll_Click(object sender, EventArgs e)
        #endif
        {
            try
            {
                if (_listView.Items.Count == 0)
                {
                    MessageBox.Show("No properties to copy.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var sb = new System.Text.StringBuilder();
                
                // Add header
                sb.AppendLine("Property\tType\tValue");
                sb.AppendLine(new string('-', 80));

                // Add all visible items
                foreach (ListViewItem item in _listView.Items)
                {
                    string propName = item.SubItems[0].Text;
                    string propType = item.SubItems[1].Text;
                    string propValue = item.SubItems[2].Text;
                    
                    sb.AppendLine($"{propName}\t{propType}\t{propValue}");
                }

                string textToCopy = sb.ToString();
                Clipboard.SetText(textToCopy);
                
                int visibleCount = _listView.Items.Count;
                UpdateStatus($"Copied {visibleCount} properties to clipboard");

                MessageBox.Show($"Copied {visibleCount} properties to clipboard.\n\nYou can paste this into Excel, Notepad, or any text editor.",
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying to clipboard: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Select Object button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnSelectObject_Click(object? sender, EventArgs e)
        #else
        private void BtnSelectObject_Click(object sender, EventArgs e)
        #endif
        {
            try
            {
                // Hide form temporarily
                this.Hide();

                var doc = AcadApp.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    this.Show();
                    return;
                }

                Editor ed = doc.Editor;
                PromptEntityOptions peo = new PromptEntityOptions("\nSelect an entity: ");
                PromptEntityResult per = ed.GetEntity(peo);

                this.Show();

                if (per.Status == PromptStatus.OK)
                {
                    var obj = _transactionHelper.Transaction.GetObject(per.ObjectId, OpenMode.ForRead);
                    _currentObject = obj;
                    DisplayObjectProperties(obj);
                    UpdateStatus($"Selected from drawing: {obj.GetType().Name}");
                }
            }
            catch (Exception ex)
            {
                this.Show();
                MessageBox.Show($"Error selecting object: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the Refresh button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnRefresh_Click(object? sender, EventArgs e)
        #else
        private void BtnRefresh_Click(object sender, EventArgs e)
        #endif
        {
            if (_currentObject != null)
            {
                DisplayObjectProperties(_currentObject);
                UpdateStatus("Refreshed properties");
            }
            else
            {
                LoadDatabaseTree();
                UpdateStatus("Refreshed tree");
            }
        }

        /// <summary>
        /// Handles the Export button click.
        /// Shows a menu with export options.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnExport_Click(object? sender, EventArgs e)
        #else
        private void BtnExport_Click(object sender, EventArgs e)
        #endif
        {
            if (_currentObject == null)
            {
                MessageBox.Show("Please select an object to export.", "No Object Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Create context menu for export options
            ContextMenuStrip exportMenu = new ContextMenuStrip();
            
            ToolStripMenuItem csvItem = new ToolStripMenuItem("Export to CSV...");
            csvItem.Click += (s, args) => ExportToCSV();
            exportMenu.Items.Add(csvItem);

            ToolStripMenuItem excelItem = new ToolStripMenuItem("Export to Excel (Tab-delimited)...");
            excelItem.Click += (s, args) => ExportToExcel();
            exportMenu.Items.Add(excelItem);

            ToolStripMenuItem jsonItem = new ToolStripMenuItem("Export to JSON...");
            jsonItem.Click += (s, args) => ExportToJSON();
            exportMenu.Items.Add(jsonItem);

            // Show menu below the Export button
            exportMenu.Show(_btnExport, new Point(0, _btnExport.Height));
        }

        /// <summary>
        /// Exports the current object to CSV.
        /// </summary>
        private void ExportToCSV()
        {
            if (_currentObject == null)
                return;

            try
            {
                var exportService = new Services.ExportService();
                bool success = exportService.ExportToCSV(_currentObject, _transactionHelper.Transaction);
                
                if (success)
                {
                    MessageBox.Show("Export completed successfully!", "Export Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus("Exported to CSV successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Export failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the current object to Excel format.
        /// </summary>
        private void ExportToExcel()
        {
            if (_currentObject == null)
                return;

            try
            {
                var exportService = new Services.ExportService();
                bool success = exportService.ExportToExcel(_currentObject, _transactionHelper.Transaction);
                
                if (success)
                {
                    MessageBox.Show("Export completed successfully!\n\nYou can open the file in Excel or any text editor.",
                        "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus("Exported to Excel format successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Export failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Exports the current object to JSON format.
        /// </summary>
        private void ExportToJSON()
        {
            if (_currentObject == null)
                return;

            try
            {
                var exportService = new Services.ExportService();
                bool success = exportService.ExportToJSON(_currentObject, _transactionHelper.Transaction);
                
                if (success)
                {
                    MessageBox.Show("Export completed successfully!\n\nJSON file can be opened in any text editor.",
                        "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateStatus("Exported to JSON format successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Export failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Compare button click.
        /// Opens a dialog to select another object and compares them.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnCompare_Click(object? sender, EventArgs e)
        #else
        private void BtnCompare_Click(object sender, EventArgs e)
        #endif
        {
            if (_currentObject == null)
            {
                MessageBox.Show("Please select an object to compare.", "No Object Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Hide the form temporarily
            this.Hide();

            try
            {
                #if NET8_0_OR_GREATER
                var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
                #else
                var ed = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument.Editor;
                #endif

                var peo = new Autodesk.AutoCAD.EditorInput.PromptEntityOptions("\nSelect second object to compare: ");
                var per = ed.GetEntity(peo);

                if (per.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                {
                    using (var obj2 = _transactionHelper.Transaction.GetObject(per.ObjectId, OpenMode.ForRead))
                    {
                        // Open comparison form
                        var comparisonForm = new ComparisonForm(_currentObject, obj2, _database, _transactionHelper);
                        
                        #if NET8_0_OR_GREATER
                        comparisonForm.ShowDialog();
                        #else
                        AcadApp.ShowModalDialog(comparisonForm);
                        #endif

                        UpdateStatus("Comparison complete");
                    }
                }
                else
                {
                    UpdateStatus("Comparison cancelled");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error comparing objects: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"Comparison failed: {ex.Message}");
            }
            finally
            {
                this.Show();
            }
        }

        /// <summary>
        /// Handles keyboard shortcuts for the form.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void MainSnoopForm_KeyDown(object? sender, KeyEventArgs e)
        #else
        private void MainSnoopForm_KeyDown(object sender, KeyEventArgs e)
        #endif
        {
            // F5 - Refresh
            if (e.KeyCode == Keys.F5)
            {
                BtnRefresh_Click(this, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Ctrl+F - Focus search box
            else if (e.Control && e.KeyCode == Keys.F)
            {
                _txtSearch.Focus();
                _txtSearch.SelectAll();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Ctrl+C - Copy selected value
            else if (e.Control && e.KeyCode == Keys.C)
            {
                // Only if ListView has focus and an item is selected
                if (_listView.Focused && _listView.SelectedItems.Count > 0)
                {
                    BtnCopyValue_Click(this, EventArgs.Empty);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
            // Ctrl+Shift+C - Copy all properties
            else if (e.Control && e.Shift && e.KeyCode == Keys.C)
            {
                BtnCopyAll_Click(this, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Escape - Clear search or close form
            else if (e.KeyCode == Keys.Escape)
            {
                if (!string.IsNullOrEmpty(_txtSearch.Text))
                {
                    // Clear search first
                    _txtSearch.Text = string.Empty;
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
                else
                {
                    // Close form
                    this.Close();
                }
            }
            // Ctrl+L - Focus tree view
            else if (e.Control && e.KeyCode == Keys.L)
            {
                _treeView.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Ctrl+P - Focus property list
            else if (e.Control && e.KeyCode == Keys.P)
            {
                _listView.Focus();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Ctrl+B - Add bookmark
            else if (e.Control && !e.Shift && e.KeyCode == Keys.B)
            {
                BtnAddBookmark_Click(this, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            // Ctrl+Shift+B - View bookmarks
            else if (e.Control && e.Shift && e.KeyCode == Keys.B)
            {
                BtnViewBookmarks_Click(this, EventArgs.Empty);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        /// <summary>
        /// Handles the Add Bookmark button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnAddBookmark_Click(object? sender, EventArgs e)
        #else
        private void BtnAddBookmark_Click(object sender, EventArgs e)
        #endif
        {
            if (_currentObject == null)
            {
                MessageBox.Show("Please select an object to bookmark.", "No Object Selected",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                string handle = "";
                string name = "";
                string typeName = _currentObject.GetType().Name;

                // Get handle
                if (_currentObject is DBObject dbObj)
                {
                    handle = dbObj.Handle.ToString();
                    var nameProp = dbObj.GetType().GetProperty("Name");
                    if (nameProp != null)
                    {
                        name = nameProp.GetValue(dbObj) as string ?? typeName;
                    }
                    else
                    {
                        name = $"{typeName} [{handle}]";
                    }
                }
                else
                {
                    MessageBox.Show("Cannot bookmark this object type.", "Unsupported",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Check if already bookmarked
                if (_bookmarkService.IsBookmarked(handle))
                {
                    MessageBox.Show("This object is already bookmarked.", "Already Bookmarked",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateBookmarkButton();
                    return;
                }

                // Prompt for name
                string bookmarkName = PromptForBookmarkName(name);
                if (string.IsNullOrWhiteSpace(bookmarkName))
                {
                    return; // User cancelled
                }

                // Add bookmark
                _bookmarkService.AddBookmark(handle, bookmarkName, typeName);
                MessageBox.Show($"Bookmark '{bookmarkName}' added successfully!", "Bookmark Added",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateStatus($"Added bookmark: {bookmarkName}");
                UpdateBookmarkButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding bookmark: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Handles the View Bookmarks button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnViewBookmarks_Click(object? sender, EventArgs e)
        #else
        private void BtnViewBookmarks_Click(object sender, EventArgs e)
        #endif
        {
            try
            {
                var bookmarksForm = new BookmarksForm(_bookmarkService, _database, _transactionHelper.Transaction);
                
                #if NET8_0_OR_GREATER
                if (bookmarksForm.ShowDialog() == DialogResult.OK)
                #else
                if (AcadApp.ShowModalDialog(bookmarksForm) == DialogResult.OK)
                #endif
                {
                    // User selected a bookmark - navigate to it
                    if (bookmarksForm.SelectedBookmark != null)
                    {
                        NavigateToBookmark(bookmarksForm.SelectedBookmark);
                    }
                }

                UpdateBookmarkButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error viewing bookmarks: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Property Display

        /// <summary>
        /// Displays properties of an object in the ListView.
        /// </summary>
        private void DisplayObjectProperties(object obj)
        {
            try
            {
                _listView.BeginUpdate();
                _listView.Items.Clear();

                if (obj == null)
                {
                    _allProperties = null;
                    UpdatePropertyCount("No object selected");
                    UpdateStatus("Ready");
                    return;
                }

                UpdateStatus($"Loading properties for {obj.GetType().Name}...");

                // Collect properties
                _allProperties = _databaseService.GetObjectProperties(obj);

                if (_allProperties == null || _allProperties.Count == 0)
                {
                    UpdatePropertyCount($"No properties found for {obj.GetType().Name}");
                    UpdateStatus("Ready");
                    
                    // Add a message to the ListView
                    var item = new ListViewItem("No properties available");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.ForeColor = Color.Gray;
                    _listView.Items.Add(item);
                    return;
                }

                // Display properties (filtered if search text is present)
                FilterProperties();
                
                // Update property count in top panel
                UpdatePropertyCount($"Loaded {_allProperties.Count} properties for {obj.GetType().Name}");
                UpdateStatus("Ready");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error: {ex.Message}");
                
                // Add error to ListView
                var item = new ListViewItem($"ERROR: {ex.Message}");
                item.SubItems.Add(ex.GetType().Name);
                item.SubItems.Add(ex.StackTrace ?? "");
                item.ForeColor = Color.Red;
                _listView.Items.Add(item);
                
                System.Diagnostics.Debug.WriteLine($"DisplayObjectProperties error: {ex}");
            }
            finally
            {
                _listView.EndUpdate();
            }
        }

        /// <summary>
        /// Filters properties based on search text.
        /// </summary>
        private void FilterProperties()
        {
            try
            {
                _listView.BeginUpdate();
                _listView.Items.Clear();

                if (_allProperties == null || _allProperties.Count == 0)
                {
                    return;
                }

                string searchText = _txtSearch.Text.Trim();
                bool hasSearch = !string.IsNullOrEmpty(searchText);

                int displayCount = 0;
                int totalCount = _allProperties.Count;

                foreach (var prop in _allProperties)
                {
                    // Apply filter if search text is present
                    if (hasSearch)
                    {
                        bool matchesName = prop.Name != null && 
                            prop.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool matchesType = prop.Type != null && 
                            prop.Type.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                        bool matchesValue = prop.Value != null && 
                            prop.Value.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!matchesName && !matchesType && !matchesValue)
                        {
                            continue; // Skip this property
                        }
                    }

                    ListViewItem item = new ListViewItem(prop.Name ?? "[null]");
                    item.SubItems.Add(prop.Type ?? "[null]");
                    
                    if (prop.HasError)
                    {
                        item.SubItems.Add($"[Error: {prop.ErrorMessage}]");
                        item.ForeColor = Color.Red;
                    }
                    else
                    {
                        #if NET8_0_OR_GREATER
                        string valueText = prop.Value ?? "[null]";
                        #else
                        string valueText = prop.Value ?? "[null]";
                        #endif
                        
                        item.SubItems.Add(valueText);
                        
                        // Color coding for different property states
                        if (prop.IsCollection)
                        {
                            item.ForeColor = Color.Blue;
                        }
                        else if (valueText == "[Not Applicable]")
                        {
                            item.ForeColor = Color.Gray;
                        }
                    }

                    item.Tag = prop;
                    _listView.Items.Add(item);
                    displayCount++;
                }

                // Update status with filter info
                if (hasSearch)
                {
                    UpdateStatus($"Showing {displayCount} of {totalCount} properties (filtered by '{searchText}')");
                }
                else
                {
                    UpdateStatus($"Displaying {totalCount} properties");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error filtering: {ex.Message}");
            }
            finally
            {
                _listView.EndUpdate();
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Updates the status label in the bottom panel.
        /// </summary>
        private void UpdateStatus(string message)
        {
            if (_lblStatus != null)
            {
                _lblStatus.Text = message;
                _lblStatus.Refresh();
            }
        }

        /// <summary>
        /// Updates the property count label in the top panel.
        /// </summary>
        private void UpdatePropertyCount(string message)
        {
            if (_lblPropertyCount != null)
            {
                _lblPropertyCount.Text = message;
                _lblPropertyCount.Refresh();
            }
        }

        /// <summary>
        /// Navigates to a bookmarked object.
        /// </summary>
        private void NavigateToBookmark(Services.Bookmark bookmark)
        {
            try
            {
                var handle = new Handle(Convert.ToInt64(bookmark.Handle, 16));
                var objId = _database.GetObjectId(false, handle, 0);

                if (objId.IsNull || objId.IsErased)
                {
                    MessageBox.Show("The bookmarked object no longer exists.", "Object Not Found",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using (DBObject obj = _transactionHelper.GetObject<DBObject>(objId))
                {
                    // Clear tree and show just this object
                    _treeView.Nodes.Clear();
                    var objNode = new Core.Data.ObjectNode(Core.Helpers.ReflectionHelper.GetObjectName(obj), obj);
                    TreeNode treeNode = new TreeNode(objNode.Name) { Tag = objNode };
                    _treeView.Nodes.Add(treeNode);

                    // Add dummy for expansion if needed
                    if (_databaseService.GetObjectChildNodes(obj).Any())
                    {
                        treeNode.Nodes.Add("Loading...");
                    }

                    _treeView.SelectedNode = treeNode;
                    UpdateStatus($"Navigated to bookmark: {bookmark.Name}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to bookmark: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Prompts the user for a bookmark name.
        /// </summary>
        private string PromptForBookmarkName(string defaultName)
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 450;
                prompt.Height = 150;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = "Add Bookmark";
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MaximizeBox = false;
                prompt.MinimizeBox = false;

                Label textLabel = new Label() { Left = 20, Top = 20, Width = 400, Text = "Bookmark name:" };
                TextBox textBox = new TextBox() { Left = 20, Top = 45, Width = 400, Text = defaultName };
                textBox.SelectAll();

                Button confirmation = new Button() { Text = "OK", Left = 260, Width = 80, Top = 80, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Cancel", Left = 350, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };

                confirmation.Click += (sender, e) => { prompt.Close(); };
                cancel.Click += (sender, e) => { prompt.Close(); };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
            }
        }

        /// <summary>
        /// Updates the Add Bookmark button text based on current object's bookmark status.
        /// </summary>
        private void UpdateBookmarkButton()
        {
            if (_currentObject is DBObject dbObj)
            {
                string handle = dbObj.Handle.ToString();
                if (_bookmarkService.IsBookmarked(handle))
                {
                    _btnAddBookmark.Text = "★ Added";
                    _btnAddBookmark.ForeColor = Color.Gold;
                }
                else
                {
                    _btnAddBookmark.Text = "★ Add";
                    _btnAddBookmark.ForeColor = SystemColors.ControlText;
                }
            }
        }

        #endregion

        #region Form Overrides

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion
    }
}

