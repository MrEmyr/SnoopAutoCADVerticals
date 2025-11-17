// LineCollector.cs - Specialized collector for AutoCAD Line entities
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;

namespace UnifiedSnoop.Inspectors.AutoCAD
{
    /// <summary>
    /// Specialized collector for AutoCAD Line entities.
    /// Provides enhanced property information beyond basic reflection.
    /// </summary>
    public class LineCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD Line Collector";

        /// <summary>
        /// Determines if this collector can handle the given object.
        /// </summary>
        public bool CanCollect(object obj)
        {
            return obj is Line;
        }

        /// <summary>
        /// Collects enhanced properties from a Line entity.
        /// </summary>
        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (!(obj is Line line))
                return new List<PropertyData>();

            var properties = new List<PropertyData>();

            try
            {
                // Basic Line Properties
                properties.Add(new PropertyData
                {
                    Name = "Start Point",
                    Type = "Point3d",
                    Value = FormatPoint(line.StartPoint),
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "End Point",
                    Type = "Point3d",
                    Value = FormatPoint(line.EndPoint),
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Length",
                    Type = "Double",
                    Value = $"{line.Length:F4}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Angle (Radians)",
                    Type = "Double",
                    Value = $"{line.Angle:F6}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Angle (Degrees)",
                    Type = "Double",
                    Value = $"{line.Angle * 180.0 / Math.PI:F2}°",
                    Category = "Geometry"
                });

                // Delta (directional vector)
                properties.Add(new PropertyData
                {
                    Name = "Delta (Vector)",
                    Type = "Vector3d",
                    Value = FormatVector(line.Delta),
                    Category = "Geometry"
                });

                // Normal vector
                properties.Add(new PropertyData
                {
                    Name = "Normal",
                    Type = "Vector3d",
                    Value = FormatVector(line.Normal),
                    Category = "Geometry"
                });

                // Thickness
                properties.Add(new PropertyData
                {
                    Name = "Thickness",
                    Type = "Double",
                    Value = $"{line.Thickness:F4}",
                    Category = "Geometry"
                });

                // Common Entity Properties
                properties.Add(new PropertyData
                {
                    Name = "Layer",
                    Type = "String",
                    Value = line.Layer ?? "[None]",
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Color",
                    Type = "Color",
                    Value = line.Color.ToString(),
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Linetype",
                    Type = "String",
                    Value = line.Linetype ?? "[None]",
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Linetype Scale",
                    Type = "Double",
                    Value = $"{line.LinetypeScale:F2}",
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Lineweight",
                    Type = "LineWeight",
                    Value = line.LineWeight.ToString(),
                    Category = "Entity"
                });

                // Object Properties
                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = line.Handle.ToString(),
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "ObjectId",
                    Type = "ObjectId",
                    Value = line.ObjectId.ToString(),
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "Class Name",
                    Type = "String",
                    Value = line.GetRXClass().Name,
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Line properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections of related objects (Lines don't have sub-collections).
        /// </summary>
        public Dictionary<string, System.Collections.IEnumerable> GetCollections(object obj, Transaction trans)
        {
            // Lines are simple entities with no sub-collections
            return new Dictionary<string, System.Collections.IEnumerable>();
        }

        #region Helper Methods

        private string FormatPoint(Point3d point)
        {
            return $"({point.X:F4}, {point.Y:F4}, {point.Z:F4})";
        }

        private string FormatVector(Vector3d vector)
        {
            return $"({vector.X:F4}, {vector.Y:F4}, {vector.Z:F4})";
        }

        #endregion
    }
}

