// XRecordEditorCommand.cs - AutoCAD command for XRecord Editor
using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;

namespace UnifiedSnoop.XRecordEditor
{
    /// <summary>
    /// Provides AutoCAD commands for the XRecord Editor.
    /// </summary>
    public class XRecordEditorCommand
    {
        /// <summary>
        /// Opens the XRecord Editor - a dedicated tool for viewing, editing, and managing XRecords.
        /// </summary>
        [CommandMethod("XRECORDEDIT", CommandFlags.Modal)]
        public void XRecordEdit()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                System.Diagnostics.Debug.WriteLine("No active document.");
                return;
            }

            Database db = doc.Database;
            var ed = doc.Editor;

            try
            {
                ed.WriteMessage("\nOpening XRecord Editor...\n");

                using (TransactionHelper trHelper = new TransactionHelper(db))
                {
                    trHelper.Start();

                    // Open the XRecord Editor
                    var xrecordEditor = new XRecordEditorForm(db, trHelper);
                    Application.ShowModalDialog(xrecordEditor);

                    trHelper.Commit();
                }

                ed.WriteMessage("\nXRecord Editor closed.\n");
            }
            catch (System.Exception ex)
            {
                ErrorLogService.Instance.LogError("Error opening XRecord Editor", ex, "XRECORDEDIT command");
                ed.WriteMessage($"\nError: {ex.Message}\n");
                ed.WriteMessage($"Log file: {ErrorLogService.Instance.GetLogFilePath()}\n");
            }
        }
    }
}

