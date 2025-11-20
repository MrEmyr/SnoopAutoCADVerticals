// XRecordValueEditorForm.cs - Dialog for editing individual TypedValue entries
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using UnifiedSnoop.Core.Helpers;

#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604, CS8618
#endif

namespace UnifiedSnoop.XRecordEditor
{
    /// <summary>
    /// Dialog for editing or creating a TypedValue entry in an XRecord.
    /// </summary>
    public class XRecordValueEditorForm : Form
    {
        #region Fields

        private readonly Database _database;
        private readonly TransactionHelper _transactionHelper;
        private TypedValue? _currentValue;
        
        // UI Controls
#if NET8_0_OR_GREATER
        private ComboBox _cmbDxfCode = null!;
        private TextBox _txtValue = null!;
        private Label _lblTypeDescription = null!;
        private Label _lblValueFormat = null!;
        private Button _btnOK = null!;
        private Button _btnCancel = null!;
#else
        private ComboBox _cmbDxfCode;
        private TextBox _txtValue;
        private Label _lblTypeDescription;
        private Label _lblValueFormat;
        private Button _btnOK;
        private Button _btnCancel;
#endif

        public TypedValue TypedValue { get; private set; }

        #endregion

        #region Constructor

        public XRecordValueEditorForm(TypedValue? currentValue, Database database, TransactionHelper transactionHelper)
        {
            _currentValue = currentValue;
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));

            InitializeComponent();
            PopulateDxfCodes();

            if (currentValue.HasValue)
            {
                LoadCurrentValue(currentValue.Value);
            }
        }

        #endregion

        #region Initialization

        private void InitializeComponent()
        {
            this.Text = "Edit XRecord Entry";
            this.Size = new Size(500, 300);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // DXF Code label and combobox
            var lblDxfCode = new Label
            {
                Text = "DXF Code:",
                Location = new Point(20, 20),
                Size = new Size(100, 20)
            };

            _cmbDxfCode = new ComboBox
            {
                Location = new Point(130, 18),
                Size = new Size(320, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbDxfCode.SelectedIndexChanged += CmbDxfCode_SelectedIndexChanged;

            // Type description
            _lblTypeDescription = new Label
            {
                Text = "",
                Location = new Point(130, 48),
                Size = new Size(320, 20),
                ForeColor = Color.Gray
            };

            // Value label and textbox
            var lblValue = new Label
            {
                Text = "Value:",
                Location = new Point(20, 80),
                Size = new Size(100, 20)
            };

            _txtValue = new TextBox
            {
                Location = new Point(130, 78),
                Size = new Size(320, 25),
                Multiline = false
            };

            // Value format hint
            _lblValueFormat = new Label
            {
                Text = "",
                Location = new Point(130, 108),
                Size = new Size(320, 40),
                ForeColor = Color.Blue
            };

            // Buttons
            _btnOK = new Button
            {
                Text = "OK",
                Location = new Point(250, 220),
                Size = new Size(90, 30),
                DialogResult = DialogResult.OK
            };
            _btnOK.Click += BtnOK_Click;

            _btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(350, 220),
                Size = new Size(90, 30),
                DialogResult = DialogResult.Cancel
            };

            this.Controls.Add(lblDxfCode);
            this.Controls.Add(_cmbDxfCode);
            this.Controls.Add(_lblTypeDescription);
            this.Controls.Add(lblValue);
            this.Controls.Add(_txtValue);
            this.Controls.Add(_lblValueFormat);
            this.Controls.Add(_btnOK);
            this.Controls.Add(_btnCancel);

            this.AcceptButton = _btnOK;
            this.CancelButton = _btnCancel;
        }

        private void PopulateDxfCodes()
        {
            // Common DXF codes for XRecord data
            var codes = new[]
            {
                new DxfCodeInfo(1, "String"),
                new DxfCodeInfo(5, "Handle"),
                new DxfCodeInfo(10, "Point3d (X, Y, Z)"),
                new DxfCodeInfo(40, "Double"),
                new DxfCodeInfo(70, "Int16"),
                new DxfCodeInfo(90, "Int32"),
                new DxfCodeInfo(280, "Int8/Boolean"),
                new DxfCodeInfo(290, "Boolean"),
                new DxfCodeInfo(300, "String (Arbitrary)"),
                new DxfCodeInfo(330, "Handle (Soft Pointer)"),
                new DxfCodeInfo(1000, "String (XData)"),
                new DxfCodeInfo(1010, "Point3d (XData)"),
                new DxfCodeInfo(1040, "Double (XData)"),
                new DxfCodeInfo(1070, "Int16 (XData)")
            };

            foreach (var code in codes)
            {
                _cmbDxfCode.Items.Add(code);
            }

            if (_cmbDxfCode.Items.Count > 0)
                _cmbDxfCode.SelectedIndex = 0;
        }

        private void LoadCurrentValue(TypedValue tv)
        {
            // Find matching DXF code in combobox
            for (int i = 0; i < _cmbDxfCode.Items.Count; i++)
            {
                if (_cmbDxfCode.Items[i] is DxfCodeInfo info && info.Code == tv.TypeCode)
                {
                    _cmbDxfCode.SelectedIndex = i;
                    break;
                }
            }

            // Set value text
            if (tv.Value != null)
            {
                if (tv.Value is Point3d pt)
                {
                    _txtValue.Text = $"{pt.X},{pt.Y},{pt.Z}";
                }
                else if (tv.Value is Point2d pt2d)
                {
                    _txtValue.Text = $"{pt2d.X},{pt2d.Y}";
                }
                else
                {
                    _txtValue.Text = tv.Value.ToString();
                }
            }
        }

        #endregion

        #region Event Handlers

#if NET8_0_OR_GREATER
        private void CmbDxfCode_SelectedIndexChanged(object? sender, EventArgs e)
#else
        private void CmbDxfCode_SelectedIndexChanged(object sender, EventArgs e)
#endif
        {
            if (_cmbDxfCode.SelectedItem is DxfCodeInfo info)
            {
                _lblTypeDescription.Text = info.Description;
                UpdateValueFormatHint(info.Code);
            }
        }

        private void UpdateValueFormatHint(short code)
        {
            string hint = "";

            if (code == 1 || (code >= 300 && code <= 319) || code == 1000)
            {
                hint = "Enter a text string";
            }
            else if (code == 5 || code == 105 || (code >= 320 && code <= 369))
            {
                hint = "Enter a handle (e.g., 1F4A)";
            }
            else if (code >= 10 && code <= 18 || code == 1010)
            {
                hint = "Enter X,Y,Z coordinates\n(e.g., 100.5,200.3,0)";
            }
            else if (code >= 38 && code <= 59 || code >= 140 && code <= 147 || code == 40 || code == 1040)
            {
                hint = "Enter a decimal number\n(e.g., 3.14159)";
            }
            else if (code >= 60 && code <= 79 || code >= 170 && code <= 175 || code == 70 || code == 1070)
            {
                hint = "Enter an integer\n(e.g., 42)";
            }
            else if (code >= 90 && code <= 99)
            {
                hint = "Enter an integer\n(e.g., 1000)";
            }
            else if (code >= 280 && code <= 293)
            {
                hint = "Enter 0 or 1\n(0 = false, 1 = true)";
            }

            _lblValueFormat.Text = hint;
        }

#if NET8_0_OR_GREATER
        private void BtnOK_Click(object? sender, EventArgs e)
#else
        private void BtnOK_Click(object sender, EventArgs e)
#endif
        {
            try
            {
                if (_cmbDxfCode.SelectedItem is not DxfCodeInfo info)
                {
                    MessageBox.Show("Please select a DXF code", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                if (string.IsNullOrWhiteSpace(_txtValue.Text))
                {
                    MessageBox.Show("Please enter a value", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.None;
                    return;
                }

                object value = ParseValue(info.Code, _txtValue.Text);
                TypedValue = new TypedValue((int)info.Code, value);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error parsing value:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.None;
            }
        }

        #endregion

        #region Value Parsing

        private object ParseValue(short code, string text)
        {
            text = text.Trim();

            // String types
            if (code == 1 || (code >= 300 && code <= 319) || code == 1000)
            {
                return text;
            }

            // Handle types
            if (code == 5 || code == 105 || (code >= 320 && code <= 369))
            {
                return text;
            }

            // Point3d types
            if ((code >= 10 && code <= 18) || code == 1010)
            {
                var parts = text.Split(',');
                if (parts.Length != 3)
                    throw new FormatException("Point3d requires 3 coordinates separated by commas (X,Y,Z)");

                double x = double.Parse(parts[0].Trim());
                double y = double.Parse(parts[1].Trim());
                double z = double.Parse(parts[2].Trim());

                return new Point3d(x, y, z);
            }

            // Double types
            if ((code >= 38 && code <= 59) || (code >= 140 && code <= 147) || code == 40 || code == 1040)
            {
                return double.Parse(text);
            }

            // Int16 types
            if ((code >= 60 && code <= 79) || (code >= 170 && code <= 175) || code == 70 || code == 1070)
            {
                return short.Parse(text);
            }

            // Int32 types
            if (code >= 90 && code <= 99)
            {
                return int.Parse(text);
            }

            // Boolean types
            if (code >= 280 && code <= 293)
            {
                int val = int.Parse(text);
                if (val != 0 && val != 1)
                    throw new FormatException("Boolean values must be 0 or 1");
                return val != 0;
            }

            // Default to string
            return text;
        }

        #endregion

        #region Helper Classes

        private class DxfCodeInfo
        {
            public short Code { get; }
            public string Description { get; }

            public DxfCodeInfo(short code, string description)
            {
                Code = code;
                Description = description;
            }

            public override string ToString()
            {
                return $"{Code} - {Description}";
            }
        }

        #endregion
    }
}

