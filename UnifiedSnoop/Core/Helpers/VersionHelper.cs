// VersionHelper.cs - Helper class for version detection and compatibility
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;

namespace UnifiedSnoop.Core.Helpers
{
    /// <summary>
    /// Helper class for detecting AutoCAD/Civil 3D versions and managing version-specific code.
    /// </summary>
    public static class VersionHelper
    {
        #region Properties

        /// <summary>
        /// Gets the target framework this assembly was built for.
        /// </summary>
        public static string TargetFramework
        {
            get
            {
#if NET48
                return ".NET Framework 4.8";
#elif NET8_0_OR_GREATER
                return ".NET 8.0";
#else
                return "Unknown";
#endif
            }
        }

        /// <summary>
        /// Gets the expected AutoCAD/Civil 3D version range for this build.
        /// </summary>
        public static string ExpectedVersionRange
        {
            get
            {
#if ACAD2024
                return "AutoCAD/Civil 3D 2024";
#elif ACAD2025_OR_GREATER
                return "AutoCAD/Civil 3D 2025+";
#else
                return "Unknown";
#endif
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is a .NET Framework 4.8 build.
        /// </summary>
        public static bool IsNet48
        {
            get
            {
#if NET48
                return true;
#else
                return false;
#endif
            }
        }

        /// <summary>
        /// Gets a value indicating whether this is a .NET 8.0 or greater build.
        /// </summary>
        public static bool IsNet80OrGreater
        {
            get
            {
#if NET8_0_OR_GREATER
                return true;
#else
                return false;
#endif
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a formatted version string for display.
        /// </summary>
        /// <returns>A string containing build and version information.</returns>
        public static string GetVersionString()
        {
            return $"UnifiedSnoop - Target: {TargetFramework} ({ExpectedVersionRange})";
        }

        /// <summary>
        /// Writes version information to the AutoCAD command line.
        /// </summary>
        public static void ShowVersionInfo()
        {
            try
            {
                var doc = Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument;
                if (doc != null)
                {
                    var ed = doc.Editor;
                    
                    // Read version from version.json if available
                    string version = "Unknown";
                    string buildDate = "Unknown";
                    string usVersion = "Unknown";
                    string xrecVersion = "Unknown";
                    
                    try
                    {
                        var assemblyLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        var assemblyDir = System.IO.Path.GetDirectoryName(assemblyLocation);
                        var bundleDir = System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(assemblyDir));
                        var versionFile = System.IO.Path.Combine(bundleDir, "version.json");
                        
                        if (System.IO.File.Exists(versionFile))
                        {
                            var jsonContent = System.IO.File.ReadAllText(versionFile);
                            // Simple JSON parsing (avoiding dependency on JSON library)
                            if (jsonContent.Contains("\"version\""))
                            {
                                var versionMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"version\"\\s*:\\s*\"([^\"]+)\"");
                                if (versionMatch.Success) version = versionMatch.Groups[1].Value;
                            }
                            if (jsonContent.Contains("\"buildDate\""))
                            {
                                var buildMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"buildDate\"\\s*:\\s*\"([^\"]+)\"");
                                if (buildMatch.Success) buildDate = buildMatch.Groups[1].Value;
                            }
                            // Parse component versions
                            var usMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"UnifiedSnoop\"\\s*:\\s*\"([^\"]+)\"");
                            if (usMatch.Success) usVersion = usMatch.Groups[1].Value;
                            var xrecMatch = System.Text.RegularExpressions.Regex.Match(jsonContent, "\"XRecordEditor\"\\s*:\\s*\"([^\"]+)\"");
                            if (xrecMatch.Success) xrecVersion = xrecMatch.Groups[1].Value;
                        }
                    }
                    catch
                    {
                        // If version file reading fails, use defaults
                    }
                    
                    ed.WriteMessage("\n╔════════════════════════════════════════════════════════════╗");
                    ed.WriteMessage("\n║                                                            ║");
                    ed.WriteMessage("\n║          UnifiedSnoop Version Information                  ║");
                    ed.WriteMessage("\n║                                                            ║");
                    ed.WriteMessage("\n╚════════════════════════════════════════════════════════════╝");
                    ed.WriteMessage($"\n\n📦 Version: {version}");
                    ed.WriteMessage($"\n📅 Build Date: {buildDate}");
                    ed.WriteMessage("\n\n🔧 Components:");
                    ed.WriteMessage($"\n   • UnifiedSnoop.dll: v{usVersion}");
                    ed.WriteMessage($"\n   • XRecordEditor.dll: v{xrecVersion}");
                    ed.WriteMessage("\n\n💻 Runtime Environment:");
                    ed.WriteMessage($"\n   • Target Framework: {TargetFramework}");
                    ed.WriteMessage($"\n   • Expected Version: {ExpectedVersionRange}");
                    ed.WriteMessage($"\n   • AutoCAD Version: {GetAcadVersionString()}");
                    ed.WriteMessage($"\n   • Civil 3D Available: {IsCivil3DAvailable()}");
                    
                    // Validate version
                    if (ValidateVersion(out string errorMsg))
                    {
                        ed.WriteMessage("\n   • Version Check: ✅ Compatible");
                    }
                    else
                    {
                        ed.WriteMessage($"\n   • Version Check: ❌ {errorMsg}");
                    }
                    
                    ed.WriteMessage("\n\n════════════════════════════════════════════════════════════\n");
                }
            }
            catch (System.Exception ex)
            {
                // If we can't write to the editor, fail silently
                System.Diagnostics.Debug.WriteLine($"Failed to show version info: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the AutoCAD version string.
        /// </summary>
        /// <returns>The AutoCAD version string or "Unknown" if not available.</returns>
        public static string GetAcadVersionString()
        {
            try
            {
                var acadVer = Autodesk.AutoCAD.ApplicationServices.Application.Version;
                return $"{acadVer.Major}.{acadVer.Minor}.{acadVer.Build}.{acadVer.Revision}";
            }
            catch
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Determines whether Civil 3D is available in the current AutoCAD session.
        /// </summary>
        /// <returns>true if Civil 3D is available; otherwise, false.</returns>
        public static bool IsCivil3DAvailable()
        {
            try
            {
                // Try to check if Civil 3D assemblies are loaded
                // Try multiple assembly names for different Civil 3D versions
                Type civilType = null;
                
                // Try AeccDbMgd first (Civil 3D 2021+)
                civilType = Type.GetType("Autodesk.Civil.ApplicationServices.CivilApplication, AeccDbMgd");
                
                // Fallback to AecBaseMgd (older versions)
                if (civilType == null)
                {
                    civilType = Type.GetType("Autodesk.Civil.ApplicationServices.CivilApplication, AecBaseMgd");
                }
                
                // Try CivilDocument as alternative
                if (civilType == null)
                {
                    civilType = Type.GetType("Autodesk.Civil.ApplicationServices.CivilDocument, AeccDbMgd");
                }
                
                if (civilType == null)
                {
                    civilType = Type.GetType("Autodesk.Civil.ApplicationServices.CivilDocument, AecBaseMgd");
                }
                
                // Try without assembly specification
                if (civilType == null)
                {
                    civilType = Type.GetType("Autodesk.Civil.ApplicationServices.CivilApplication");
                }
                
                return civilType != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates that the DLL is running in a compatible AutoCAD/Civil 3D version.
        /// </summary>
        /// <param name="errorMessage">Contains the error message if validation fails.</param>
        /// <returns>true if the version is compatible; otherwise, false.</returns>
        public static bool ValidateVersion(out string errorMessage)
        {
            errorMessage = string.Empty;

            try
            {
                var acadVer = Autodesk.AutoCAD.ApplicationServices.Application.Version;
                int majorVersion = acadVer.Major;

#if ACAD2024
                // For .NET Framework 4.8 build, we expect AutoCAD 2024 (major version 24)
                if (majorVersion != 24)
                {
                    errorMessage = $"This DLL was built for AutoCAD/Civil 3D 2024 (.NET Framework 4.8) " +
                                 $"but is running in AutoCAD version {majorVersion}. " +
                                 $"Please use the correct DLL version.";
                    return false;
                }
#elif ACAD2025_OR_GREATER
                // For .NET 8.0 build, we expect AutoCAD 2025+ (major version 25+)
                if (majorVersion < 25)
                {
                    errorMessage = $"This DLL was built for AutoCAD/Civil 3D 2025+ (.NET 8.0) " +
                                 $"but is running in AutoCAD version {majorVersion}. " +
                                 $"Please use the .NET Framework 4.8 build for AutoCAD 2024.";
                    return false;
                }
#endif

                return true;
            }
            catch (System.Exception ex)
            {
                errorMessage = $"Failed to validate version: {ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Gets a descriptive string about the current runtime environment.
        /// </summary>
        /// <returns>A multi-line string describing the environment.</returns>
        public static string GetEnvironmentInfo()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("UnifiedSnoop Environment Information:");
            sb.AppendLine($"  Target Framework: {TargetFramework}");
            sb.AppendLine($"  Expected Version: {ExpectedVersionRange}");
            sb.AppendLine($"  AutoCAD Version: {GetAcadVersionString()}");
            sb.AppendLine($"  Civil 3D Available: {IsCivil3DAvailable()}");
            sb.AppendLine($"  .NET Framework 4.8 Build: {IsNet48}");
            sb.AppendLine($"  .NET 8.0+ Build: {IsNet80OrGreater}");
            
            // Validate version
            if (ValidateVersion(out string errorMsg))
            {
                sb.AppendLine("  Version Check: ✓ Compatible");
            }
            else
            {
                sb.AppendLine($"  Version Check: ✗ {errorMsg}");
            }

            return sb.ToString();
        }

        #endregion
    }
}

