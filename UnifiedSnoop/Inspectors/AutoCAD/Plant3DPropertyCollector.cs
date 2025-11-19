// Plant3DPropertyCollector.cs - Specialized collector for Plant 3D objects
// Plant 3D objects store data using AcPpDataLinksManager rather than XRecords
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;

#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
#endif

namespace UnifiedSnoop.Inspectors.AutoCAD
{
    /// <summary>
    /// Specialized collector for Plant 3D objects.
    /// Plant 3D uses DataLinksManager to store properties in an external project database,
    /// rather than embedding data as XRecords within the DWG file.
    /// </summary>
    public class Plant3DPropertyCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Plant 3D Property Collector";

        public bool CanCollect(object obj)
        {
            if (obj == null) return false;

            // Check if the object is a Plant 3D object
            // Plant 3D objects are typically in the Autodesk.ProcessPower namespace
            string typeName = obj.GetType().FullName;
            
            return typeName != null && (
                typeName.StartsWith("Autodesk.ProcessPower") ||
                typeName.StartsWith("Autodesk.Plant3D") ||
                typeName.Contains("PnP3d") ||
                typeName.Contains("PnID") ||
                typeName.Contains("AcPp")
            );
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (obj == null)
                return new List<PropertyData>();

            var properties = new List<PropertyData>();

            try
            {
                // Add basic Plant 3D object information
                properties.Add(new PropertyData
                {
                    Name = "Object Type",
                    Type = "String",
                    Value = $"Plant 3D Object: {obj.GetType().Name}",
                    Category = "General"
                });

                // Try to get the ObjectId if it's a DBObject
                if (obj is DBObject dbObj)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Handle",
                        Type = "Handle",
                        Value = dbObj.Handle.ToString(),
                        Category = "General"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "ObjectId",
                        Type = "ObjectId",
                        Value = dbObj.ObjectId.ToString(),
                        Category = "General"
                    });
                }

                // Add information about Plant 3D data storage
                properties.Add(new PropertyData
                {
                    Name = "Data Storage Method",
                    Type = "String",
                    Value = "Plant 3D DataLinksManager (External Project Database)",
                    Category = "Plant 3D"
                });

                properties.Add(new PropertyData
                {
                    Name = "Note",
                    Type = "String",
                    Value = "Plant 3D properties are stored in an external project database, not as XRecords in the DWG file",
                    Category = "Plant 3D"
                });

                // Try to get Plant 3D specific properties using reflection
                // This avoids hard dependencies on Plant 3D DLLs
                TryCollectPlant3DProperties(obj, properties, trans);
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Plant 3D properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message,
                    Category = "Error"
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections from a Plant 3D object.
        /// </summary>
        public Dictionary<string, IEnumerable> GetCollections(object obj, Transaction trans)
        {
            var collections = new Dictionary<string, IEnumerable>();

            try
            {
                // Use reflection to find collection properties
                var type = obj.GetType();
                var props = type.GetProperties(
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance
                );

                foreach (var prop in props)
                {
                    if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && 
                        prop.PropertyType != typeof(string))
                    {
                        try
                        {
                            var value = prop.GetValue(obj);
                            if (value is IEnumerable enumerable)
                            {
                                collections[prop.Name] = enumerable;
                            }
                        }
                        catch
                        {
                            // Ignore properties that throw exceptions
                        }
                    }
                }
            }
            catch
            {
                // Return empty collections on error
            }

            return collections;
        }

        #region Private Helper Methods

        /// <summary>
        /// Tries to collect Plant 3D specific properties using reflection.
        /// This approach avoids hard dependencies on Plant 3D DLLs.
        /// </summary>
        private void TryCollectPlant3DProperties(object obj, List<PropertyData> properties, Transaction trans)
        {
            try
            {
                var type = obj.GetType();
                
                // Section header for Plant 3D properties
                properties.Add(new PropertyData
                {
                    Name = "--- Plant 3D Specific Properties ---",
                    Type = "Section",
                    Value = type.FullName,
                    Category = "Plant 3D"
                });

                // Try to get common Plant 3D properties
                TryAddProperty(obj, "PartSizeProperties", properties, "Plant 3D");
                TryAddProperty(obj, "NominalDiameter", properties, "Plant 3D");
                TryAddProperty(obj, "Spec", properties, "Plant 3D");
                TryAddProperty(obj, "Tag", properties, "Plant 3D");
                TryAddProperty(obj, "Description", properties, "Plant 3D");
                TryAddProperty(obj, "ClassName", properties, "Plant 3D");
                TryAddProperty(obj, "AssetClassName", properties, "Plant 3D");
                TryAddProperty(obj, "Position", properties, "Plant 3D");
                TryAddProperty(obj, "Orientation", properties, "Plant 3D");
                TryAddProperty(obj, "StartPoint", properties, "Plant 3D");
                TryAddProperty(obj, "EndPoint", properties, "Plant 3D");
                TryAddProperty(obj, "OuterDiameter", properties, "Plant 3D");
                TryAddProperty(obj, "InnerDiameter", properties, "Plant 3D");
                TryAddProperty(obj, "WallThickness", properties, "Plant 3D");
                TryAddProperty(obj, "SymbolId", properties, "Plant 3D");
                TryAddProperty(obj, "ContentId", properties, "Plant 3D");

                // Try to get DataLinks properties if available
                properties.Add(new PropertyData
                {
                    Name = "--- DataLinks Properties ---",
                    Type = "Section",
                    Value = "Properties stored in Plant 3D Project Database",
                    Category = "Plant 3D DataLinks"
                });

                // Note about accessing DataLinks properties
                properties.Add(new PropertyData
                {
                    Name = "DataLinks Access",
                    Type = "Info",
                    Value = "Use 'PLANT3DPROPEDIT' command to view/edit properties stored in the project database",
                    Category = "Plant 3D DataLinks"
                });

                properties.Add(new PropertyData
                {
                    Name = "DataLinks Note",
                    Type = "Info",
                    Value = "DataLinks properties require active Plant 3D project context. Properties include: Code, Number, Service, Insulation Type, etc.",
                    Category = "Plant 3D DataLinks"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Plant 3D Collection Error",
                    Type = "Error",
                    Value = $"Error collecting Plant 3D specific properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message,
                    Category = "Error"
                });
            }
        }

        /// <summary>
        /// Tries to get a property value using reflection and add it to the properties list.
        /// </summary>
        private void TryAddProperty(object obj, string propertyName, List<PropertyData> properties, string category)
        {
            try
            {
                var type = obj.GetType();
                var prop = type.GetProperty(propertyName,
                    System.Reflection.BindingFlags.Public | 
                    System.Reflection.BindingFlags.Instance
                );

                if (prop != null && prop.CanRead)
                {
                    try
                    {
                        var value = prop.GetValue(obj);
                        properties.Add(new PropertyData
                        {
                            Name = $"  {propertyName}",
                            Type = prop.PropertyType.Name,
                            Value = FormatValue(value),
                            Category = category,
                            DeclaringType = type.Name
                        });
                    }
                    catch (System.Exception ex)
                    {
                        properties.Add(new PropertyData
                        {
                            Name = $"  {propertyName}",
                            Type = prop.PropertyType.Name,
                            Value = $"[Error: {ex.Message}]",
                            Category = category,
                            HasError = true,
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }
            catch
            {
                // Property doesn't exist, skip it
            }
        }

        /// <summary>
        /// Formats a value for display.
        /// </summary>
        private string FormatValue(object value)
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
                return objId.IsNull ? "[Null ObjectId]" : objId.Handle.ToString();

            return value.ToString();
        }

        #endregion
    }
}

