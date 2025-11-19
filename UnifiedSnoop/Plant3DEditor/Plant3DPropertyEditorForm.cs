// Plant3DPropertyEditorForm.cs - Editor for managing Plant 3D object properties
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618
#endif

namespace UnifiedSnoop.Plant3DEditor
{
    /// <summary>
    /// Form for viewing and editing Plant 3D object properties.
    /// Plant 3D stores properties using DataLinksManager in an external project database,
    /// unlike standard AutoCAD objects that use XRecords.
    /// </summary>
    public class Plant3DPropertyEditorForm : Form
    {
        #region Fields

        private readonly Database _database;
        private readonly ObjectId _objectId;
        
        // UI Controls
#if NET8_0_OR_GREATER
        private ListView _propertyListView = null!;
        private Label _lblObjectInfo = null!;
        private Label _lblPropertyCount = null!;
        private Button _btnRefresh = null!;
        private Button _btnClose = null!;
        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _statusLabel = null!;
        private Panel _infoPanel = null!;
        private Label _lblDataLinksInfo = null!;
#else
        private ListView _propertyListView;
        private Label _lblObjectInfo;
        private Label _lblPropertyCount;
        private Button _btnRefresh;
        private Button _btnClose;
        private StatusStrip _statusStrip;
        private ToolStripStatusLabel _statusLabel;
        private Panel _infoPanel;
        private Label _lblDataLinksInfo;
#endif

        #endregion

        #region Constructor

        public Plant3DPropertyEditorForm(Database database, ObjectId objectId)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _objectId = objectId;

            if (_objectId.IsNull)
                throw new ArgumentException("ObjectId cannot be null", nameof(objectId));

            InitializeComponent();
            LoadProperties();
        }

        #endregion

        #region Initialization

        private void InitializeComponent()
        {
            // Form properties
            this.Text = "Plant 3D Property Editor - UnifiedSnoop";
            this.Size = new Size(900, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(700, 500);

            // Info Panel at top
            _infoPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 120,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(240, 248, 255) // Light blue background
            };

            // Object info label
            _lblObjectInfo = new Label
            {
                Text = "Plant 3D Object Information",
                Location = new Point(10, 10),
                Size = new Size(860, 25),
                Font = new System.Drawing.Font(this.Font.FontFamily, 12, FontStyle.Bold),
                ForeColor = Color.DarkBlue
            };

            // DataLinks info label
            _lblDataLinksInfo = new Label
            {
                Text = "Plant 3D properties are stored in the project database using DataLinksManager.\n" +
                       "These properties are not embedded in the DWG file as XRecords.\n" +
                       "To edit properties, use the Plant 3D Project Manager or PALETTES command.",
                Location = new Point(10, 40),
                Size = new Size(860, 60),
                ForeColor = Color.DarkRed,
                Font = new System.Drawing.Font(this.Font.FontFamily, 9, FontStyle.Italic)
            };

            _infoPanel.Controls.Add(_lblObjectInfo);
            _infoPanel.Controls.Add(_lblDataLinksInfo);

            // Property count label
            _lblPropertyCount = new Label
            {
                Text = "",
                Location = new Point(10, 130),
                Size = new Size(860, 20),
                ForeColor = Color.Gray
            };

            // Property ListView
            _propertyListView = new ListView
            {
                Location = new Point(10, 160),
                Size = new Size(860, 450),
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _propertyListView.Columns.Add("Property Name", 250);
            _propertyListView.Columns.Add("Type", 150);
            _propertyListView.Columns.Add("Value", 400);

            // Create context menu for ListView
            var listContextMenu = new ContextMenuStrip();
            var menuCopyName = new ToolStripMenuItem("Copy Property Name", null, (s, e) => CopySelectedPropertyName());
            var menuCopyValue = new ToolStripMenuItem("Copy Property Value", null, (s, e) => CopySelectedPropertyValue());
            var menuCopyAll = new ToolStripMenuItem("Copy Property (Name = Value)", null, (s, e) => CopySelectedProperty());
            listContextMenu.Items.Add(menuCopyName);
            listContextMenu.Items.Add(menuCopyValue);
            listContextMenu.Items.Add(menuCopyAll);
            listContextMenu.Opening += (s, e) =>
            {
                bool hasSelection = _propertyListView.SelectedItems.Count > 0;
                menuCopyName.Enabled = hasSelection;
                menuCopyValue.Enabled = hasSelection;
                menuCopyAll.Enabled = hasSelection;
            };
            _propertyListView.ContextMenuStrip = listContextMenu;

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
            this.Controls.Add(_infoPanel);
            this.Controls.Add(_lblPropertyCount);
            this.Controls.Add(_propertyListView);
            this.Controls.Add(bottomPanel);
            this.Controls.Add(_statusStrip);
        }

        #endregion

        #region Property Loading

        private void LoadProperties()
        {
            try
            {
                _propertyListView.BeginUpdate();
                _propertyListView.Items.Clear();

                UpdateStatus("Loading Plant 3D properties...");

                using (Transaction tr = _database.TransactionManager.StartTransaction())
                {
                    DBObject obj = tr.GetObject(_objectId, OpenMode.ForRead);
                    
                    // Update object info
                    _lblObjectInfo.Text = $"Plant 3D Object: {obj.GetType().Name} (Handle: {obj.Handle})";

                    // Add basic properties
                    AddPropertyGroup("Basic Properties");
                    AddProperty("Type", obj.GetType().Name, obj.GetType().FullName);
                    AddProperty("Handle", "Handle", obj.Handle.ToString());
                    AddProperty("ObjectId", "ObjectId", _objectId.ToString());
                    AddProperty("IsErased", "Boolean", obj.IsErased.ToString());
                    AddProperty("IsReadEnabled", "Boolean", obj.IsReadEnabled.ToString());
                    AddProperty("IsWriteEnabled", "Boolean", obj.IsWriteEnabled.ToString());

                    // Add Plant 3D specific properties using reflection
                    AddPropertyGroup("Plant 3D Properties");
                    
                    var type = obj.GetType();
                    var properties = type.GetProperties(
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.Instance
                    );

                    int plant3DPropCount = 0;
                    foreach (var prop in properties)
                    {
                        if (!prop.CanRead) continue;
                        
                        // Skip collection properties for now
                        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) && 
                            prop.PropertyType != typeof(string))
                            continue;

                        try
                        {
                            var value = prop.GetValue(obj);
                            AddProperty(prop.Name, prop.PropertyType.Name, FormatValue(value));
                            plant3DPropCount++;
                        }
                        catch (System.Exception ex)
                        {
                            AddProperty(prop.Name, prop.PropertyType.Name, $"[Error: {ex.Message}]");
                        }
                    }

                    // Add DataLinks information section
                    AddPropertyGroup("DataLinks Information");
                    AddProperty("Storage Method", "Info", "External Project Database (DataLinksManager)");
                    AddProperty("Note", "Info", "Use Plant 3D tools to edit DataLinks properties");
                    AddProperty("Common DataLinks Props", "Info", "Code, Number, Service, InsulationType, PaintCode, etc.");
                    AddProperty("Access Method", "Info", "AcPpDataLinksManager.SetProperties() / GetProperties()");

                    _lblPropertyCount.Text = $"{plant3DPropCount} Plant 3D properties found";
                    
                    tr.Commit();
                }

                UpdateStatus($"Loaded properties for {_lblObjectInfo.Text}");
            }
            catch (System.Exception ex)
            {
                UpdateStatus($"Error loading properties: {ex.Message}");
                MessageBox.Show($"Error loading properties:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _propertyListView.EndUpdate();
            }
        }

        private void AddPropertyGroup(string groupName)
        {
            var item = new ListViewItem(groupName);
            item.Font = new System.Drawing.Font(this.Font, FontStyle.Bold);
            item.ForeColor = Color.DarkBlue;
            item.SubItems.Add("");
            item.SubItems.Add("");
            _propertyListView.Items.Add(item);
        }

        private void AddProperty(string name, string type, string value)
        {
            var item = new ListViewItem($"  {name}");
            item.SubItems.Add(type);
            item.SubItems.Add(value);
            _propertyListView.Items.Add(item);
        }

        private string FormatValue(object value)
        {
            if (value == null)
                return "[null]";

            // Format common types
            if (value is double d)
                return $"{d:F6}";
            
            if (value is float f)
                return $"{f:F6}";
            
            if (value is Point3d pt)
                return $"({pt.X:F4}, {pt.Y:F4}, {pt.Z:F4})";
            
            if (value is Point2d pt2d)
                return $"({pt2d.X:F4}, {pt2d.Y:F4})";
            
            if (value is Vector3d vec)
                return $"({vec.X:F4}, {vec.Y:F4}, {vec.Z:F4})";
            
            if (value is ObjectId objId)
                return objId.IsNull ? "[Null ObjectId]" : $"Handle: {objId.Handle}";

            // Truncate very long strings
            string str = value.ToString();
            if (str != null && str.Length > 200)
                return str.Substring(0, 197) + "...";

            return str;
        }

        #endregion

        #region Event Handlers

#if NET8_0_OR_GREATER
        private void BtnRefresh_Click(object? sender, EventArgs e)
#else
        private void BtnRefresh_Click(object sender, EventArgs e)
#endif
        {
            LoadProperties();
        }

        private void CopySelectedPropertyName()
        {
            if (_propertyListView.SelectedItems.Count > 0)
            {
                string name = _propertyListView.SelectedItems[0].Text.Trim();
                Clipboard.SetText(name);
                UpdateStatus($"Copied property name: {name}");
            }
        }

        private void CopySelectedPropertyValue()
        {
            if (_propertyListView.SelectedItems.Count > 0)
            {
                string value = _propertyListView.SelectedItems[0].SubItems[2].Text;
                Clipboard.SetText(value);
                UpdateStatus($"Copied property value");
            }
        }

        private void CopySelectedProperty()
        {
            if (_propertyListView.SelectedItems.Count > 0)
            {
                var item = _propertyListView.SelectedItems[0];
                string text = $"{item.Text.Trim()} = {item.SubItems[2].Text}";
                Clipboard.SetText(text);
                UpdateStatus($"Copied property: {item.Text.Trim()}");
            }
        }

        #endregion

        #region Helper Methods

        private void UpdateStatus(string message)
        {
            _statusLabel.Text = message;
            _statusStrip.Refresh();
        }

        #endregion
    }
}

