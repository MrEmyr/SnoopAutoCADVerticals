// BookmarkService.cs - Service for managing bookmarks
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;

namespace UnifiedSnoop.Services
{
    /// <summary>
    /// Manages bookmarks for frequently accessed objects.
    /// </summary>
    public class BookmarkService
    {
        #region Fields

        private readonly string _bookmarkFilePath;
        private List<Bookmark> _bookmarks;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the BookmarkService.
        /// </summary>
        public BookmarkService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string unifiedSnoopPath = Path.Combine(appDataPath, "UnifiedSnoop");
            
            if (!Directory.Exists(unifiedSnoopPath))
            {
                Directory.CreateDirectory(unifiedSnoopPath);
            }

            _bookmarkFilePath = Path.Combine(unifiedSnoopPath, "bookmarks.txt");
            _bookmarks = new List<Bookmark>();
            LoadBookmarks();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a bookmark for an object.
        /// </summary>
        /// <param name="handle">The object handle.</param>
        /// <param name="name">The bookmark name.</param>
        /// <param name="typeName">The type name of the object.</param>
        public void AddBookmark(string handle, string name, string typeName)
        {
            // Check if bookmark already exists
            if (_bookmarks.Any(b => b.Handle == handle))
            {
                throw new InvalidOperationException("Bookmark already exists for this object.");
            }

            var bookmark = new Bookmark
            {
                Handle = handle,
                Name = name,
                TypeName = typeName,
                DateCreated = DateTime.Now
            };

            _bookmarks.Add(bookmark);
            SaveBookmarks();
        }

        /// <summary>
        /// Removes a bookmark.
        /// </summary>
        /// <param name="handle">The object handle.</param>
        public void RemoveBookmark(string handle)
        {
            _bookmarks.RemoveAll(b => b.Handle == handle);
            SaveBookmarks();
        }

        /// <summary>
        /// Gets all bookmarks.
        /// </summary>
        /// <returns>List of bookmarks.</returns>
        public List<Bookmark> GetAllBookmarks()
        {
            return _bookmarks.OrderBy(b => b.Name).ToList();
        }

        /// <summary>
        /// Checks if an object is bookmarked.
        /// </summary>
        /// <param name="handle">The object handle.</param>
        /// <returns>True if bookmarked; otherwise, false.</returns>
        public bool IsBookmarked(string handle)
        {
            return _bookmarks.Any(b => b.Handle == handle);
        }

        /// <summary>
        /// Gets a bookmark by handle.
        /// </summary>
        /// <param name="handle">The object handle.</param>
        /// <returns>The bookmark or null if not found.</returns>
        #if NET8_0_OR_GREATER
        public Bookmark? GetBookmark(string handle)
        #else
        public Bookmark GetBookmark(string handle)
        #endif
        {
            return _bookmarks.FirstOrDefault(b => b.Handle == handle);
        }

        /// <summary>
        /// Clears all bookmarks.
        /// </summary>
        public void ClearAll()
        {
            _bookmarks.Clear();
            SaveBookmarks();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads bookmarks from file.
        /// </summary>
        private void LoadBookmarks()
        {
            _bookmarks.Clear();

            if (!File.Exists(_bookmarkFilePath))
                return;

            try
            {
                var lines = File.ReadAllLines(_bookmarkFilePath);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var parts = line.Split('|');
                    if (parts.Length >= 4)
                    {
                        var bookmark = new Bookmark
                        {
                            Handle = parts[0],
                            Name = parts[1],
                            TypeName = parts[2],
                            DateCreated = DateTime.TryParse(parts[3], out var date) ? date : DateTime.Now
                        };
                        _bookmarks.Add(bookmark);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load bookmarks: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves bookmarks to file.
        /// </summary>
        private void SaveBookmarks()
        {
            try
            {
                var lines = _bookmarks.Select(b => $"{b.Handle}|{b.Name}|{b.TypeName}|{b.DateCreated:yyyy-MM-dd HH:mm:ss}");
                File.WriteAllLines(_bookmarkFilePath, lines);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save bookmarks: {ex.Message}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents a bookmarked object.
    /// </summary>
    public class Bookmark
    {
        public string Handle { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }

        public override string ToString()
        {
            return $"{Name} ({TypeName})";
        }
    }
}

