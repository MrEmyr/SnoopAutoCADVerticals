// Plant3DPropertyEditorCommand.cs - AutoCAD command for Plant 3D Property Editor
using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.EditorInput;

namespace UnifiedSnoop.Plant3DEditor
{
    /// <summary>
    /// Provides AutoCAD commands for the Plant 3D Property Editor.
    /// </summary>
    public class Plant3DPropertyEditorCommand
    {
        /// <summary>
        /// Opens the Plant 3D Property Editor - a dedicated tool for viewing and editing Plant 3D object properties.
        /// </summary>
        [CommandMethod("PLANT3DPROPEDIT", CommandFlags.Modal)]
        public void Plant3DPropertyEdit()
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
                // Check if Plant 3D is available
                if (!IsPlant3DAvailable())
                {
                    ed.WriteMessage("\nPlant 3D is not available in this AutoCAD session.\n");
                    ed.WriteMessage("This command requires AutoCAD Plant 3D to be loaded.\n");
                    return;
                }

                ed.WriteMessage("\nOpening Plant 3D Property Editor...\n");

                // Prompt user to select a Plant 3D object
                PromptEntityOptions peo = new PromptEntityOptions("\nSelect a Plant 3D object: ");
                peo.SetRejectMessage("\nObject must be a Plant 3D object.");
                peo.AddAllowedClass(typeof(DBObject), true);

                PromptEntityResult per = ed.GetEntity(peo);
                
                if (per.Status != PromptStatus.OK)
                {
                    ed.WriteMessage("\nCommand cancelled.\n");
                    return;
                }

                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    try
                    {
                        DBObject obj = tr.GetObject(per.ObjectId, OpenMode.ForRead);
                        
                        // Check if it's a Plant 3D object
                        string typeName = obj.GetType().FullName;
                        bool isPlant3D = typeName != null && (
                            typeName.StartsWith("Autodesk.ProcessPower") ||
                            typeName.StartsWith("Autodesk.Plant3D") ||
                            typeName.Contains("PnP3d") ||
                            typeName.Contains("PnID") ||
                            typeName.Contains("AcPp")
                        );

                        if (!isPlant3D)
                        {
                            ed.WriteMessage("\nSelected object is not a Plant 3D object.\n");
                            return;
                        }

                        // Open the Plant 3D Property Editor
                        var editor = new Plant3DPropertyEditorForm(db, per.ObjectId);
                        Application.ShowModalDialog(editor);

                        tr.Commit();
                    }
                    catch (System.Exception ex)
                    {
                        ed.WriteMessage($"\nError: {ex.Message}\n");
                    }
                }

                ed.WriteMessage("\nPlant 3D Property Editor closed.\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError opening Plant 3D Property Editor: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Lists all Plant 3D objects in the current drawing.
        /// </summary>
        [CommandMethod("PLANT3DLIST", CommandFlags.Modal)]
        public void Plant3DList()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;

            Database db = doc.Database;
            var ed = doc.Editor;

            try
            {
                ed.WriteMessage("\n=== Plant 3D Objects in Drawing ===\n");

                int count = 0;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (bt == null) return;

                    BlockTableRecord modelSpace = tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
                    if (modelSpace == null) return;

                    foreach (ObjectId objId in modelSpace)
                    {
                        DBObject obj = tr.GetObject(objId, OpenMode.ForRead);
                        string typeName = obj.GetType().FullName;
                        
                        bool isPlant3D = typeName != null && (
                            typeName.StartsWith("Autodesk.ProcessPower") ||
                            typeName.StartsWith("Autodesk.Plant3D") ||
                            typeName.Contains("PnP3d") ||
                            typeName.Contains("PnID") ||
                            typeName.Contains("AcPp")
                        );

                        if (isPlant3D)
                        {
                            count++;
                            ed.WriteMessage($"\n{count}. {obj.GetType().Name} (Handle: {obj.Handle})");
                            
                            // Try to get some common properties
                            TryPrintProperty(obj, "Tag");
                            TryPrintProperty(obj, "Description");
                            TryPrintProperty(obj, "Spec");
                        }
                    }

                    tr.Commit();
                }

                ed.WriteMessage($"\n\nTotal Plant 3D objects found: {count}\n");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage($"\nError listing Plant 3D objects: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Checks if Plant 3D is available in the current AutoCAD session.
        /// </summary>
        private bool IsPlant3DAvailable()
        {
            try
            {
                // Try to check for Plant 3D specific assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var asm in assemblies)
                {
                    string name = asm.GetName().Name;
                    if (name.Contains("ProcessPower") || 
                        name.Contains("Plant3D") ||
                        name.Contains("PnP") ||
                        name.Contains("AcPp"))
                    {
                        return true;
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Tries to print a property value to the command line.
        /// </summary>
        private void TryPrintProperty(DBObject obj, string propertyName)
        {
            try
            {
                var prop = obj.GetType().GetProperty(propertyName,
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance
                );

                if (prop != null && prop.CanRead)
                {
                    var value = prop.GetValue(obj);
                    if (value != null)
                    {
                        var ed = Application.DocumentManager.MdiActiveDocument.Editor;
                        ed.WriteMessage($" | {propertyName}: {value}");
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}

