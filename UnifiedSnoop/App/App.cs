// App.cs - Main application entry point for UnifiedSnoop
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Helpers;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

[assembly: ExtensionApplication(typeof(UnifiedSnoop.App.App))]
[assembly: CommandClass(typeof(UnifiedSnoop.App.SnoopCommands))]

namespace UnifiedSnoop.App
{
    /// <summary>
    /// Main application class for UnifiedSnoop.
    /// Implements IExtensionApplication to initialize and terminate with AutoCAD.
    /// </summary>
    public class App : IExtensionApplication
    {
        #region Fields

        private const string APP_NAME = "UnifiedSnoop";
        private const string VERSION = "2.0.0";

#if NET8_0_OR_GREATER
        private ContextMenuHandler? _contextMenuHandler;
#else
        private ContextMenuHandler _contextMenuHandler;
#endif

        #endregion

        #region IExtensionApplication Implementation

        /// <summary>
        /// Called when the application is loaded into AutoCAD.
        /// </summary>
        public void Initialize()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                
                // Write startup message
                WriteStartupMessage(doc);

                // Validate version compatibility
                if (!VersionHelper.ValidateVersion(out string errorMessage))
                {
                    if (doc != null)
                    {
                        doc.Editor.WriteMessage($"\n*** WARNING: {errorMessage} ***\n");
                    }
                }

                // Initialize the collector registry
                InitializeCollectors();

                // Add context menu
                _contextMenuHandler = new ContextMenuHandler();
                _contextMenuHandler.Register();

                if (doc != null)
                {
                    doc.Editor.WriteMessage($"\n{APP_NAME} initialized successfully.\n");
                    doc.Editor.WriteMessage("Type 'SNOOP' to start snooping!\n");
                }
            }
            catch (System.Exception ex)
            {
                // Log initialization errors
                System.Diagnostics.Debug.WriteLine($"UnifiedSnoop initialization error: {ex.Message}");
                
                try
                {
                    var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                    if (doc != null)
                    {
                        doc.Editor.WriteMessage($"\n*** ERROR: UnifiedSnoop failed to initialize: {ex.Message} ***\n");
                    }
                }
                catch
                {
                    // Ignore errors in error handling
                }
            }
        }

        /// <summary>
        /// Called when the application is unloaded from AutoCAD.
        /// </summary>
        public void Terminate()
        {
            try
            {
                // Unregister context menu
                _contextMenuHandler?.Unregister();

                // Clean up resources
                CollectorRegistry.Instance.ClearCollectors();

                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.Editor.WriteMessage($"\n{APP_NAME} terminated.\n");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UnifiedSnoop termination error: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Writes the startup message to the command line.
        /// </summary>
        /// <param name="doc">The active document.</param>
        private void WriteStartupMessage(Document doc)
        {
            #if NET8_0_OR_GREATER
            if (doc?.Editor == null)
                return;
            #else
            if (doc == null || doc.Editor == null)
                return;
            #endif

            var ed = doc.Editor;
            
            ed.WriteMessage("\n========================================");
            ed.WriteMessage($"\n{APP_NAME} v{VERSION}");
            ed.WriteMessage("\n========================================");
            ed.WriteMessage($"\nTarget: {VersionHelper.TargetFramework}");
            ed.WriteMessage($"\nExpected Version: {VersionHelper.ExpectedVersionRange}");
            ed.WriteMessage($"\nAutoCAD Version: {VersionHelper.GetAcadVersionString()}");
            ed.WriteMessage($"\nCivil 3D Available: {VersionHelper.IsCivil3DAvailable()}");
            ed.WriteMessage("\n========================================");
        }

        /// <summary>
        /// Initializes and registers all collectors.
        /// </summary>
        private void InitializeCollectors()
        {
            var registry = CollectorRegistry.Instance;

            // Register specialized AutoCAD collectors
            try
            {
                // Entity Collectors
                registry.RegisterCollector(new Inspectors.AutoCAD.LineCollector());
                registry.RegisterCollector(new Inspectors.AutoCAD.ArcCollector());
                registry.RegisterCollector(new Inspectors.AutoCAD.CircleCollector());
                registry.RegisterCollector(new Inspectors.AutoCAD.PolylineCollector());
                registry.RegisterCollector(new Inspectors.AutoCAD.TextCollector());
                registry.RegisterCollector(new Inspectors.AutoCAD.DimensionCollector());
                registry.RegisterCollector(new Inspectors.AutoCAD.BlockReferenceCollector());
                
                // Symbol Table Collectors
                registry.RegisterCollector(new Inspectors.AutoCAD.LayerTableCollector());
                
                // Database Object Collectors
                registry.RegisterCollector(new Inspectors.AutoCAD.XRecordCollector());
                
                // Plant 3D Object Collectors (optional - works only if Plant 3D is available)
                try
                {
                    registry.RegisterCollector(new Inspectors.AutoCAD.Plant3DPropertyCollector());
                }
                catch (System.Exception)
                {
                    // Plant 3D DLLs not available - this is OK, just skip it
                    System.Diagnostics.Debug.WriteLine("Plant 3D collector not registered - Plant 3D DLLs not available");
                }

                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    doc.Editor.WriteMessage($"\nRegistered {registry.CollectorCount} specialized AutoCAD collectors.");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to register AutoCAD collectors: {ex.Message}");
            }

            // Register specialized Civil 3D collectors if Civil 3D is available
            if (VersionHelper.IsCivil3DAvailable())
            {
                try
                {
                    // Original Civil 3D Collectors
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DDocumentCollector());
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DAlignmentCollector());
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DSurfaceCollector());
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DCorridorCollector());
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DPipeNetworkCollector());
                    
                    // New Civil 3D Collectors
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DProfileCollector());
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DProfileViewCollector());
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DAssemblyCollector());
                    registry.RegisterCollector(new Inspectors.Civil3D.Civil3DPointGroupCollector());

                    var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                    if (doc != null)
                    {
                        doc.Editor.WriteMessage($"\nRegistered {registry.CollectorCount} total specialized collectors (AutoCAD + Civil 3D).");
                    }
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to register Civil 3D collectors: {ex.Message}");
                }
            }

            // The reflection collector is always available as a fallback
            // No need to register it explicitly
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Gets the application name.
        /// </summary>
        public static string ApplicationName => APP_NAME;

        /// <summary>
        /// Gets the application version.
        /// </summary>
        public static string ApplicationVersion => VERSION;

        #endregion
    }
}

