// SnoopCommands.cs - AutoCAD commands for UnifiedSnoop
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using UnifiedSnoop.Core.Helpers;
using UnifiedSnoop.Services;

#if NET8_0_OR_GREATER
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#else
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;
#endif

namespace UnifiedSnoop.App
{
    /// <summary>
    /// Defines AutoCAD commands for the UnifiedSnoop application.
    /// All functionality is accessed through the single SNOOP command UI.
    /// </summary>
    public class SnoopCommands
    {
        /// <summary>
        /// Opens the UnifiedSnoop UI - the single entry point for all inspection functionality.
        /// </summary>
        [CommandMethod("SNOOP", CommandFlags.Modal)]
        public void Snoop()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                System.Diagnostics.Debug.WriteLine("No active document.");
                return;
            }

            Database db = doc.Database;
            var ed = doc.Editor;

            try
            {
                ed.WriteMessage("\nOpening UnifiedSnoop...\n");

                using (TransactionHelper trHelper = new TransactionHelper(db))
                {
                    trHelper.Start();

                    // Open the main snoop UI - all functionality is in the UI
                    var mainForm = new UI.MainSnoopForm(db, trHelper);
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(mainForm);

                    trHelper.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogService.Instance.LogError("Error opening UnifiedSnoop UI", ex, "SNOOP command");
                ed.WriteMessage($"\nError: {ex.Message}\n");
                ed.WriteMessage($"Log file: {ErrorLogService.Instance.GetLogFilePath()}\n");
            }
        }

        /// <summary>
        /// Snoops a specific object by its handle.
        /// </summary>
        [CommandMethod("SNOOPHANDLE", CommandFlags.Modal)]
        public void SnoopHandle()
        {
            Document doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                System.Diagnostics.Debug.WriteLine("No active document.");
                return;
            }

            Database db = doc.Database;
            var ed = doc.Editor;

            try
            {
                // Prompt for handle
                var handleResult = ed.GetString("\nEnter object handle: ");
                if (handleResult.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                    return;

                string handleString = handleResult.StringResult.Trim();
                if (string.IsNullOrEmpty(handleString))
                {
                    ed.WriteMessage("\nInvalid handle.\n");
                    return;
                }

                // Parse handle
                long handleValue;
                if (!long.TryParse(handleString, System.Globalization.NumberStyles.HexNumber, null, out handleValue))
                {
                    ed.WriteMessage("\nInvalid handle format. Handle must be a hexadecimal value.\n");
                    return;
                }

                using (TransactionHelper trHelper = new TransactionHelper(db))
                {
                    trHelper.Start();

                    // Try to get the object
                    Handle handle = new Handle(handleValue);
                    ObjectId objId;
                    
                    if (!db.TryGetObjectId(handle, out objId) || objId.IsNull)
                    {
                        ed.WriteMessage($"\nObject with handle {handleString} not found.\n");
                        return;
                    }

                    // Get the object
                    var obj = trHelper.Transaction.GetObject(objId, OpenMode.ForRead);
                    if (obj == null)
                    {
                        ed.WriteMessage($"\nCould not open object with handle {handleString}.\n");
                        return;
                    }

                    ed.WriteMessage($"\nOpening UnifiedSnoop for object: {obj.GetType().Name} (Handle: {handleString})...\n");

                    // Open the main snoop UI with the specific object
                    var mainForm = new UI.MainSnoopForm(db, trHelper, obj);
                    Autodesk.AutoCAD.ApplicationServices.Application.ShowModalDialog(mainForm);

                    trHelper.Commit();
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogService.Instance.LogError("Error in SNOOPHANDLE command", ex, "SNOOPHANDLE command");
                ed.WriteMessage($"\nError: {ex.Message}\n");
                ed.WriteMessage($"Log file: {ErrorLogService.Instance.GetLogFilePath()}\n");
            }
        }

        /// <summary>
        /// Hidden command for version diagnostics (not advertised to users).
        /// </summary>
        [CommandMethod("SNOOPVERSION", CommandFlags.Modal)]
        internal void SnoopVersion()
        {
            try
            {
                VersionHelper.ShowVersionInfo();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SnoopVersion error: {ex.Message}");
            }
        }
    }
}

