// ComparisonForm.cs - Object comparison form for UnifiedSnoop
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;


#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
#endif

namespace UnifiedSnoop.UI
{
    /// <summary>
    /// Form for comparing two objects side-by-side.
    /// </summary>
    public partial class ComparisonForm : Form
    {
        #region Fields

        private readonly object _object1;
        private readonly object _object2;
        private readonly Database _database;
        private readonly Core.Helpers.TransactionHelper _transHelper;

        // UI Controls (initialized in InitializeComponent)
        #if NET8_0_OR_GREATER
        private Panel _topPanel = null!;
        private Label _lblObject1 = null!;
        private Label _lblObject2 = null!;
        private ListView _listView = null!;
        private Button _btnExport = null!;
        private Button _btnClose = null!;
        private CheckBox _chkShowDifferencesOnly = null!;
        #else
        private Panel _topPanel;
        private Label _lblObject1;
        private Label _lblObject2;
        private ListView _listView;
        private Button _btnExport;
        private Button _btnClose;
        private CheckBox _chkShowDifferencesOnly;
        #endif

        #if NET8_0_OR_GREATER
        private List<PropertyData>? _properties1;
        private List<PropertyData>? _properties2;
        #else
        private List<PropertyData> _properties1;
        private List<PropertyData> _properties2;
        #endif

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the ComparisonForm.
        /// </summary>
        public ComparisonForm(object object1, object object2, Database database, Core.Helpers.TransactionHelper transHelper)
        {
            _object1 = object1 ?? throw new ArgumentNullException(nameof(object1));
            _object2 = object2 ?? throw new ArgumentNullException(nameof(object2));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _transHelper = transHelper ?? throw new ArgumentNullException(nameof(transHelper));

            InitializeComponent();
            InitializeForm();
            LoadComparison();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Required method for Designer support.
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }

        /// <summary>
        /// Initializes the form controls.
        /// </summary>
        private void InitializeForm()
        {
            // Set form properties
            this.Text = "Object Comparison";
            this.Size = new Size(1200, 700);
            this.MinimumSize = new Size(900, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowIcon = false;

            // Create top panel
            _topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(10)
            };

            // Object 1 label
            _lblObject1 = new Label
            {
                Location = new Point(10, 10),
                Size = new Size(550, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "Object 1: Loading..."
            };

            // Object 2 label
            _lblObject2 = new Label
            {
                Location = new Point(10, 40),
                Size = new Size(550, 25),
                Font = new System.Drawing.Font("Segoe UI", 10F, FontStyle.Bold),
                Text = "Object 2: Loading..."
            };

            // Show differences only checkbox
            _chkShowDifferencesOnly = new CheckBox
            {
                Text = "Show differences only",
                Location = new Point(580, 10),
                Size = new Size(180, 25),
                Checked = false
            };
            _chkShowDifferencesOnly.CheckedChanged += ChkShowDifferencesOnly_CheckedChanged;

            // Export button
            _btnExport = new Button
            {
                Text = "Export Comparison",
                Location = new Point(580, 40),
                Size = new Size(140, 25)
            };
            _btnExport.Click += BtnExport_Click;

            // Close button
            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(730, 40),
                Size = new Size(80, 25)
            };
            _btnClose.Click += (s, e) => this.Close();

            _topPanel.Controls.Add(_lblObject1);
            _topPanel.Controls.Add(_lblObject2);
            _topPanel.Controls.Add(_chkShowDifferencesOnly);
            _topPanel.Controls.Add(_btnExport);
            _topPanel.Controls.Add(_btnClose);

            // Create ListView
            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false
            };

            // Add columns
            _listView.Columns.Add("Property", 200);
            _listView.Columns.Add("Object 1 Value", 400);
            _listView.Columns.Add("Object 2 Value", 400);
            _listView.Columns.Add("Status", 100);

            // Add controls to form
            this.Controls.Add(_listView);
            this.Controls.Add(_topPanel);
        }

        #endregion

        #region Comparison Logic

        /// <summary>
        /// Loads and displays the comparison.
        /// </summary>
        private void LoadComparison()
        {
            try
            {
                // Collect properties from both objects using DatabaseService
                var dbService = new Services.DatabaseService(_database, _transHelper);
                
                _properties1 = dbService.GetProperties(_object1);
                _properties2 = dbService.GetProperties(_object2);

                // Update labels
                _lblObject1.Text = $"Object 1: {GetObjectName(_object1)} ({_object1.GetType().Name})";
                _lblObject2.Text = $"Object 2: {GetObjectName(_object2)} ({_object2.GetType().Name})";

                // Display comparison
                DisplayComparison();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading comparison: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Displays the property comparison in the ListView.
        /// </summary>
        private void DisplayComparison()
        {
            _listView.BeginUpdate();
            _listView.Items.Clear();

            if (_properties1 == null || _properties2 == null)
                return;

            try
            {
                // Create a dictionary for quick lookup of properties from object 2
                var props2Dict = _properties2.ToDictionary(p => p.Name ?? "", p => p);

                // Compare properties
                foreach (var prop1 in _properties1)
                {
                    if (prop1.Name == null)
                        continue;

                    bool exists2 = props2Dict.TryGetValue(prop1.Name, out var prop2);

                    string value1 = prop1.HasError ? $"[Error: {prop1.ErrorMessage}]" : (prop1.Value ?? "[null]");
                    string value2 = exists2 ? (prop2.HasError ? $"[Error: {prop2.ErrorMessage}]" : (prop2.Value ?? "[null]")) : "[Not Present]";

                    bool isDifferent = value1 != value2;

                    // Filter if "show differences only" is checked
                    if (_chkShowDifferencesOnly.Checked && !isDifferent)
                        continue;

                    string status = isDifferent ? "Different" : "Same";

                    ListViewItem item = new ListViewItem(prop1.Name);
                    item.SubItems.Add(value1);
                    item.SubItems.Add(value2);
                    item.SubItems.Add(status);

                    // Color code differences
                    if (isDifferent)
                    {
                        item.BackColor = Color.LightYellow;
                        item.Font = new System.Drawing.Font(item.Font, FontStyle.Bold);
                    }

                    _listView.Items.Add(item);

                    // Remove from dict so we can find properties unique to object 2
                    if (exists2)
                        props2Dict.Remove(prop1.Name);
                }

                // Add properties unique to object 2
                foreach (var prop2 in props2Dict.Values)
                {
                    if (_chkShowDifferencesOnly.Checked || true) // Always show these as they're different
                    {
                        string value2 = prop2.HasError ? $"[Error: {prop2.ErrorMessage}]" : (prop2.Value ?? "[null]");

                        ListViewItem item = new ListViewItem(prop2.Name ?? "[Unknown]");
                        item.SubItems.Add("[Not Present]");
                        item.SubItems.Add(value2);
                        item.SubItems.Add("Different");
                        item.BackColor = Color.LightCoral;
                        item.Font = new System.Drawing.Font(item.Font, FontStyle.Bold);

                        _listView.Items.Add(item);
                    }
                }

                // Update title with counts
                int totalProps = _listView.Items.Count;
                int differences = _listView.Items.Cast<ListViewItem>().Count(i => i.SubItems[3].Text == "Different");
                this.Text = $"Object Comparison - {differences} difference(s) out of {totalProps} properties";
            }
            finally
            {
                _listView.EndUpdate();
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the "Show differences only" checkbox change.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void ChkShowDifferencesOnly_CheckedChanged(object? sender, EventArgs e)
        #else
        private void ChkShowDifferencesOnly_CheckedChanged(object sender, EventArgs e)
        #endif
        {
            DisplayComparison();
        }

        /// <summary>
        /// Handles the Export button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnExport_Click(object? sender, EventArgs e)
        #else
        private void BtnExport_Click(object sender, EventArgs e)
        #endif
        {
            try
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV Files|*.csv|Text Files|*.txt";
                    saveDialog.Title = "Export Comparison";
                    saveDialog.FileName = $"Comparison_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        var content = new System.Text.StringBuilder();
                        content.AppendLine($"Object 1,{GetObjectName(_object1)},{_object1.GetType().Name}");
                        content.AppendLine($"Object 2,{GetObjectName(_object2)},{_object2.GetType().Name}");
                        content.AppendLine();
                        content.AppendLine("Property,Object 1 Value,Object 2 Value,Status");

                        foreach (ListViewItem item in _listView.Items)
                        {
                            content.AppendLine($"{EscapeCSV(item.SubItems[0].Text)},{EscapeCSV(item.SubItems[1].Text)},{EscapeCSV(item.SubItems[2].Text)},{item.SubItems[3].Text}");
                        }

                        System.IO.File.WriteAllText(saveDialog.FileName, content.ToString());

                        MessageBox.Show("Comparison exported successfully!", "Export Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets the display name of an object.
        /// </summary>
        private string GetObjectName(object obj)
        {
            if (obj == null)
                return "[null]";

            var nameProp = obj.GetType().GetProperty("Name");
            if (nameProp != null)
            {
                var name = nameProp.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(name))
                    return name;
            }

            return obj.GetType().Name;
        }

        /// <summary>
        /// Escapes a string for CSV format.
        /// </summary>
        private string EscapeCSV(string value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            if (value.Contains(",") || value.Contains("\n") || value.Contains("\""))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        #endregion
    }
}

