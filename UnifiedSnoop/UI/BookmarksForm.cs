// BookmarksForm.cs - Bookmarks management form
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Drawing;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Services;

namespace UnifiedSnoop.UI
{
    /// <summary>
    /// Form for managing bookmarks.
    /// </summary>
    public partial class BookmarksForm : Form
    {
        #region Fields

        private readonly BookmarkService _bookmarkService;
        private readonly Database _database;
        private readonly Transaction _transaction;

        #if NET8_0_OR_GREATER
        private ListView _listView = null!;
        private Button _btnGo = null!;
        private Button _btnDelete = null!;
        private Button _btnClear = null!;
        private Button _btnClose = null!;
        #else
        private ListView _listView;
        private Button _btnGo;
        private Button _btnDelete;
        private Button _btnClear;
        private Button _btnClose;
        #endif

        #if NET8_0_OR_GREATER
        public Bookmark? SelectedBookmark { get; private set; }
        #else
        public Bookmark SelectedBookmark { get; private set; }
        #endif

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the BookmarksForm.
        /// </summary>
        public BookmarksForm(BookmarkService bookmarkService, Database database, Transaction transaction)
        {
            _bookmarkService = bookmarkService ?? throw new ArgumentNullException(nameof(bookmarkService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));

            InitializeComponent();
            InitializeForm();
            LoadBookmarks();
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
            this.Text = "Bookmarks";
            this.Size = new Size(700, 500);
            this.MinimumSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.ShowIcon = false;

            // Create ListView
            _listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            _listView.Columns.Add("Name", 250);
            _listView.Columns.Add("Type", 150);
            _listView.Columns.Add("Handle", 150);
            _listView.Columns.Add("Date", 150);

            _listView.DoubleClick += ListView_DoubleClick;
            _listView.SelectedIndexChanged += ListView_SelectedIndexChanged;

            // Create button panel
            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(10)
            };

            _btnGo = new Button
            {
                Text = "Go to Object",
                Location = new Point(10, 12),
                Size = new Size(120, 28),
                Enabled = false
            };
            _btnGo.Click += BtnGo_Click;

            _btnDelete = new Button
            {
                Text = "Delete",
                Location = new Point(140, 12),
                Size = new Size(100, 28),
                Enabled = false
            };
            _btnDelete.Click += BtnDelete_Click;

            _btnClear = new Button
            {
                Text = "Clear All",
                Location = new Point(250, 12),
                Size = new Size(100, 28)
            };
            _btnClear.Click += BtnClear_Click;

            _btnClose = new Button
            {
                Text = "Close",
                Location = new Point(360, 12),
                Size = new Size(100, 28),
                DialogResult = DialogResult.Cancel
            };

            buttonPanel.Controls.Add(_btnGo);
            buttonPanel.Controls.Add(_btnDelete);
            buttonPanel.Controls.Add(_btnClear);
            buttonPanel.Controls.Add(_btnClose);

            // Add controls to form
            this.Controls.Add(_listView);
            this.Controls.Add(buttonPanel);

            this.AcceptButton = _btnGo;
            this.CancelButton = _btnClose;
        }

        #endregion

        #region Data Loading

        /// <summary>
        /// Loads bookmarks into the ListView.
        /// </summary>
        private void LoadBookmarks()
        {
            _listView.Items.Clear();

            var bookmarks = _bookmarkService.GetAllBookmarks();

            foreach (var bookmark in bookmarks)
            {
                var item = new ListViewItem(bookmark.Name);
                item.SubItems.Add(bookmark.TypeName);
                item.SubItems.Add(bookmark.Handle);
                item.SubItems.Add(bookmark.DateCreated.ToString("yyyy-MM-dd HH:mm"));
                item.Tag = bookmark;

                _listView.Items.Add(item);
            }

            UpdateButtonStates();

            // Update title with count
            this.Text = $"Bookmarks ({bookmarks.Count})";
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles ListView selection changed.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void ListView_SelectedIndexChanged(object? sender, EventArgs e)
        #else
        private void ListView_SelectedIndexChanged(object sender, EventArgs e)
        #endif
        {
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles ListView double-click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void ListView_DoubleClick(object? sender, EventArgs e)
        #else
        private void ListView_DoubleClick(object sender, EventArgs e)
        #endif
        {
            if (_listView.SelectedItems.Count > 0)
            {
                BtnGo_Click(sender, e);
            }
        }

        /// <summary>
        /// Handles the Go button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnGo_Click(object? sender, EventArgs e)
        #else
        private void BtnGo_Click(object sender, EventArgs e)
        #endif
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            var item = _listView.SelectedItems[0];
            SelectedBookmark = item.Tag as Bookmark;

            if (SelectedBookmark != null)
            {
                // Verify object still exists
                try
                {
                    var handle = new Handle(Convert.ToInt64(SelectedBookmark.Handle, 16));
                    var objId = _database.GetObjectId(false, handle, 0);

                    if (objId.IsNull || objId.IsErased)
                    {
                        MessageBox.Show("The bookmarked object no longer exists or has been erased.",
                            "Object Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error accessing bookmarked object: {ex.Message}",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Handles the Delete button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnDelete_Click(object? sender, EventArgs e)
        #else
        private void BtnDelete_Click(object sender, EventArgs e)
        #endif
        {
            if (_listView.SelectedItems.Count == 0)
                return;

            var item = _listView.SelectedItems[0];
            var bookmark = item.Tag as Bookmark;

            if (bookmark != null)
            {
                var result = MessageBox.Show($"Delete bookmark '{bookmark.Name}'?",
                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _bookmarkService.RemoveBookmark(bookmark.Handle);
                    LoadBookmarks();
                }
            }
        }

        /// <summary>
        /// Handles the Clear All button click.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void BtnClear_Click(object? sender, EventArgs e)
        #else
        private void BtnClear_Click(object sender, EventArgs e)
        #endif
        {
            if (_listView.Items.Count == 0)
                return;

            var result = MessageBox.Show("Delete all bookmarks?",
                "Confirm Clear All", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                _bookmarkService.ClearAll();
                LoadBookmarks();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Updates button enabled states.
        /// </summary>
        private void UpdateButtonStates()
        {
            bool hasSelection = _listView.SelectedItems.Count > 0;
            _btnGo.Enabled = hasSelection;
            _btnDelete.Enabled = hasSelection;
        }

        #endregion
    }
}

