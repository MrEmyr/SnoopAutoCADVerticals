// ContextMenuHandler.cs - Context menu integration for UnifiedSnoop
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using UnifiedSnoop.Core.Helpers;

#if NET8_0_OR_GREATER
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#else
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

namespace UnifiedSnoop.App
{
    /// <summary>
    /// Handles context menu integration for UnifiedSnoop.
    /// Adds right-click menu items to snoop objects directly from the drawing.
    /// </summary>
    public class ContextMenuHandler
    {
        #region Fields

#if NET8_0_OR_GREATER
        private ContextMenuExtension? _contextMenu;
#else
        private ContextMenuExtension _contextMenu;
#endif
        private bool _isRegistered = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers the context menu with AutoCAD.
        /// </summary>
        public void Register()
        {
            if (_isRegistered)
                return;

            try
            {
                // Create context menu extension
                _contextMenu = new ContextMenuExtension();
                _contextMenu.Title = "UnifiedSnoop";

                // Add "Snoop This Object" menu item
                var snoopItem = new MenuItem("Snoop This Object");
                snoopItem.Click += SnoopObject_Click;
                _contextMenu.MenuItems.Add(snoopItem);

                // Add separator
                var separator = new MenuItem("-");
                _contextMenu.MenuItems.Add(separator);

                // Add "Snoop Properties" menu item (opens in command line)
                var propertiesItem = new MenuItem("Show Properties (Command Line)");
                propertiesItem.Click += ShowProperties_Click;
                _contextMenu.MenuItems.Add(propertiesItem);

                // Register the context menu
                Autodesk.AutoCAD.ApplicationServices.Application.AddDefaultContextMenuExtension(_contextMenu);
                _isRegistered = true;

                System.Diagnostics.Debug.WriteLine("UnifiedSnoop context menu registered successfully.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to register context menu: {ex.Message}");
            }
        }

        /// <summary>
        /// Unregisters the context menu from AutoCAD.
        /// </summary>
        public void Unregister()
        {
            if (!_isRegistered || _contextMenu == null)
                return;

            try
            {
                Autodesk.AutoCAD.ApplicationServices.Application.RemoveDefaultContextMenuExtension(_contextMenu);
                _isRegistered = false;

                System.Diagnostics.Debug.WriteLine("UnifiedSnoop context menu unregistered.");
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to unregister context menu: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the "Snoop This Object" menu item click.
        /// Opens the main snoop form with the selected object.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void SnoopObject_Click(object? sender, EventArgs e)
        #else
        private void SnoopObject_Click(object sender, EventArgs e)
        #endif
        {
            #if NET8_0_OR_GREATER
            Document? doc = AcadApp.DocumentManager.MdiActiveDocument;
            #else
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            #endif
            if (doc == null)
                return;

            Database db = doc.Database;
            Editor ed = doc.Editor;

            try
            {
                // Get the object IDs from the current selection
                PromptSelectionResult psr = ed.SelectImplied();
                
                if (psr.Status != PromptStatus.OK)
                {
                    // If no implied selection, prompt user to select
                    ed.WriteMessage("\nNo object selected. Please select an object first.\n");
                    return;
                }

                SelectionSet ss = psr.Value;
                if (ss.Count == 0)
                {
                    ed.WriteMessage("\nNo objects in selection.\n");
                    return;
                }

                // Get the first selected object
                ObjectId objId = ss[0].ObjectId;

                using (TransactionHelper trHelper = new TransactionHelper(db))
                {
                    trHelper.Start();

                    // Get the object
                    DBObject obj = trHelper.GetObject<DBObject>(objId);

                    if (obj == null)
                    {
                        ed.WriteMessage("\nFailed to retrieve object.\n");
                        trHelper.Abort();
                        return;
                    }

                    ed.WriteMessage($"\nOpening UnifiedSnoop for {obj.GetType().Name}...\n");

                    // Open the main snoop form
                    var mainForm = new UI.MainSnoopForm(db, trHelper);
                    
                    // Form opens with the database tree loaded
                    
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(mainForm);

                    trHelper.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError opening snoop form: {ex.Message}\n");
                System.Diagnostics.Debug.WriteLine($"SnoopObject_Click error: {ex}");
            }
        }

        /// <summary>
        /// Handles the "Show Properties" menu item click.
        /// Displays properties in the command line.
        /// </summary>
        #if NET8_0_OR_GREATER
        private void ShowProperties_Click(object? sender, EventArgs e)
        #else
        private void ShowProperties_Click(object sender, EventArgs e)
        #endif
        {
            #if NET8_0_OR_GREATER
            Document? doc = AcadApp.DocumentManager.MdiActiveDocument;
            #else
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            #endif
            if (doc == null)
                return;

            Database db = doc.Database;
            Editor ed = doc.Editor;

            try
            {
                // Get the object IDs from the current selection
                PromptSelectionResult psr = ed.SelectImplied();
                
                if (psr.Status != PromptStatus.OK)
                {
                    ed.WriteMessage("\nNo object selected. Please select an object first.\n");
                    return;
                }

                SelectionSet ss = psr.Value;
                if (ss.Count == 0)
                {
                    ed.WriteMessage("\nNo objects in selection.\n");
                    return;
                }

                // Get the first selected object
                ObjectId objId = ss[0].ObjectId;

                using (TransactionHelper trHelper = new TransactionHelper(db))
                {
                    trHelper.Start();

                    // Get the object
                    DBObject obj = trHelper.GetObject<DBObject>(objId);

                    if (obj == null)
                    {
                        ed.WriteMessage("\nFailed to retrieve object.\n");
                        trHelper.Abort();
                        return;
                    }

                    // Display basic information
                    ed.WriteMessage("\n========================================");
                    ed.WriteMessage($"\nObject Type: {obj.GetType().Name}");
                    ed.WriteMessage($"\nObject Id: {obj.ObjectId}");
                    ed.WriteMessage($"\nHandle: {obj.ObjectId.Handle}");
                    ed.WriteMessage("\n========================================");

                    // Collect properties
                    var properties = Core.Collectors.CollectorRegistry.Instance.Collect(obj, trHelper.Transaction);

                    ed.WriteMessage($"\nFound {properties.Count} properties:");
                    ed.WriteMessage("\n========================================");

                    // Display first 15 properties
                    int displayCount = Math.Min(15, properties.Count);
                    for (int i = 0; i < displayCount; i++)
                    {
                        var prop = properties[i];
                        if (!prop.HasError)
                        {
                            ed.WriteMessage($"\n  {prop.Name} ({prop.Type}): {prop.Value}");
                        }
                        else
                        {
                            ed.WriteMessage($"\n  {prop.Name}: [Error: {prop.ErrorMessage}]");
                        }
                    }

                    if (properties.Count > 15)
                    {
                        ed.WriteMessage($"\n  ... and {properties.Count - 15} more properties");
                    }

                    ed.WriteMessage("\n========================================");
                    ed.WriteMessage("\nTip: Use 'SNOOP' for full UI or right-click → 'Snoop This Object'\n");

                    trHelper.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError displaying properties: {ex.Message}\n");
                System.Diagnostics.Debug.WriteLine($"ShowProperties_Click error: {ex}");
            }
        }

        #endregion
    }
}

