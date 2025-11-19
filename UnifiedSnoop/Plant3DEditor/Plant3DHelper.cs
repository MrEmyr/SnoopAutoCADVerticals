// Plant3DHelper.cs - Helper utilities for Plant 3D objects
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;

#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
#endif

namespace UnifiedSnoop.Plant3DEditor
{
    /// <summary>
    /// Helper class for working with Plant 3D objects without hard dependencies on Plant 3D DLLs.
    /// Uses reflection to access Plant 3D functionality when available.
    /// </summary>
    public static class Plant3DHelper
    {
        #region Plant 3D Detection

        /// <summary>
        /// Checks if Plant 3D is available in the current AutoCAD session.
        /// </summary>
        public static bool IsPlant3DAvailable()
        {
            try
            {
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
        /// Checks if an object is a Plant 3D object based on its type name.
        /// </summary>
        public static bool IsPlant3DObject(object obj)
        {
            if (obj == null) return false;

            string typeName = obj.GetType().FullName;
            return typeName != null && (
                typeName.StartsWith("Autodesk.ProcessPower") ||
                typeName.StartsWith("Autodesk.Plant3D") ||
                typeName.Contains("PnP3d") ||
                typeName.Contains("PnID") ||
                typeName.Contains("AcPp")
            );
        }

        /// <summary>
        /// Gets the Plant 3D class name of an object, if available.
        /// </summary>
        public static string GetPlant3DClassName(object obj)
        {
            if (obj == null) return string.Empty;

            try
            {
                // Try common Plant 3D class name properties
                var prop = obj.GetType().GetProperty("ClassName",
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (prop != null && prop.CanRead)
                {
                    var value = prop.GetValue(obj);
                    if (value != null)
                        return value.ToString();
                }

                prop = obj.GetType().GetProperty("AssetClassName",
                    BindingFlags.Public | BindingFlags.Instance);
                
                if (prop != null && prop.CanRead)
                {
                    var value = prop.GetValue(obj);
                    if (value != null)
                        return value.ToString();
                }
            }
            catch
            {
                // Ignore errors
            }

            return obj.GetType().Name;
        }

        #endregion

        #region Property Access

        /// <summary>
        /// Common Plant 3D property names that are frequently used.
        /// </summary>
        public static readonly string[] CommonPlant3DProperties = new string[]
        {
            "Tag",
            "Description",
            "Spec",
            "NominalDiameter",
            "Position",
            "StartPoint",
            "EndPoint",
            "OuterDiameter",
            "InnerDiameter",
            "WallThickness",
            "SymbolId",
            "ContentId",
            "PartSizeProperties",
            "Orientation",
            "ClassName",
            "AssetClassName"
        };

        /// <summary>
        /// Common DataLinks property names stored in the Plant 3D project database.
        /// </summary>
        public static readonly string[] CommonDataLinksProperties = new string[]
        {
            "Code",
            "Number",
            "Service",
            "InsulationType",
            "PaintCode",
            "FluidCode",
            "PressureClass",
            "TemperatureRating",
            "LineNumber",
            "PipeSpec",
            "Material"
        };

        /// <summary>
        /// Gets a property value from a Plant 3D object using reflection.
        /// Returns null if the property doesn't exist or can't be read.
        /// </summary>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return null;

            try
            {
                var prop = obj.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Instance);

                if (prop != null && prop.CanRead)
                {
                    return prop.GetValue(obj);
                }
            }
            catch
            {
                // Ignore errors
            }

            return null;
        }

        /// <summary>
        /// Gets all properties from a Plant 3D object.
        /// </summary>
        public static Dictionary<string, object> GetAllProperties(object obj)
        {
            var properties = new Dictionary<string, object>();
            
            if (obj == null) return properties;

            try
            {
                var type = obj.GetType();
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                foreach (var prop in props)
                {
                    if (!prop.CanRead) continue;

                    // Skip collection properties
                    if (typeof(System.Collections.IEnumerable).IsAssignableFrom(prop.PropertyType) &&
                        prop.PropertyType != typeof(string))
                        continue;

                    try
                    {
                        var value = prop.GetValue(obj);
                        properties[prop.Name] = value;
                    }
                    catch
                    {
                        // Skip properties that throw exceptions
                    }
                }
            }
            catch
            {
                // Return partial results on error
            }

            return properties;
        }

        #endregion

        #region DataLinks Access

        /// <summary>
        /// Notes about accessing DataLinks properties.
        /// These are informational strings for display purposes.
        /// </summary>
        public static class DataLinksInfo
        {
            public const string StorageMethod = "External Project Database (DataLinksManager)";
            
            public const string AccessInfo = 
                "DataLinks properties are stored in an external Plant 3D project database, " +
                "not embedded in the DWG file as XRecords. " +
                "They require an active Plant 3D project context to access.";
            
            public const string EditingInfo = 
                "To edit DataLinks properties, use:\n" +
                "1. Plant 3D Project Manager\n" +
                "2. PALETTES command (Data Manager palette)\n" +
                "3. Plant 3D toolset property palettes\n" +
                "4. AcPpDataLinksManager API (SetProperties/GetProperties)";
            
            public const string ApiInfo = 
                "API Access:\n" +
                "- AcPpDataLinksManager.SetProperties(ObjectId, names, values)\n" +
                "- AcPpDataLinksManager.GetProperties(ObjectId, names)\n" +
                "- Requires: using Autodesk.ProcessPower.DataLinks;";
        }

        #endregion

        #region Type Information

        /// <summary>
        /// Gets detailed type information about a Plant 3D object.
        /// </summary>
        public static string GetTypeInformation(object obj)
        {
            if (obj == null) return "Null object";

            var type = obj.GetType();
            var info = $"Type: {type.Name}\n";
            info += $"Full Name: {type.FullName}\n";
            info += $"Assembly: {type.Assembly.GetName().Name}\n";
            info += $"Namespace: {type.Namespace}\n";
            
            // Get base types
            var baseType = type.BaseType;
            if (baseType != null)
            {
                info += $"Base Type: {baseType.Name}\n";
            }

            // Get interfaces
            var interfaces = type.GetInterfaces();
            if (interfaces.Length > 0)
            {
                info += "Interfaces:\n";
                foreach (var iface in interfaces)
                {
                    if (iface.Namespace != null && 
                        (iface.Namespace.StartsWith("Autodesk") || 
                         iface.Namespace.StartsWith("System")))
                    {
                        info += $"  - {iface.Name}\n";
                    }
                }
            }

            return info;
        }

        #endregion

        #region Formatting Helpers

        /// <summary>
        /// Formats a value for display.
        /// </summary>
        public static string FormatValue(object value)
        {
            if (value == null)
                return "[null]";

            // Format common types
            if (value is double d)
                return $"{d:F6}";

            if (value is float f)
                return $"{f:F6}";

            if (value is Autodesk.AutoCAD.Geometry.Point3d pt)
                return $"({pt.X:F4}, {pt.Y:F4}, {pt.Z:F4})";

            if (value is Autodesk.AutoCAD.Geometry.Point2d pt2d)
                return $"({pt2d.X:F4}, {pt2d.Y:F4})";

            if (value is Autodesk.AutoCAD.Geometry.Vector3d vec)
                return $"({vec.X:F4}, {vec.Y:F4}, {vec.Z:F4})";

            if (value is ObjectId objId)
                return objId.IsNull ? "[Null ObjectId]" : $"Handle: {objId.Handle}";

            // Truncate very long strings
            string str = value.ToString();
            if (str != null && str.Length > 200)
                return str.Substring(0, 197) + "...";

            return str;
        }

        #endregion
    }
}

