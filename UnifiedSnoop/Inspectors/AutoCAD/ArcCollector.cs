// ArcCollector.cs - Specialized collector for AutoCAD Arc entities
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
    /// Specialized collector for AutoCAD Arc entities.
    /// </summary>
    public class ArcCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD Arc Collector";

        public bool CanCollect(object obj)
        {
            return obj is Arc;
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (!(obj is Arc arc))
                return new List<PropertyData>();

            var properties = new List<PropertyData>();

            try
            {
                // Arc Geometry
                properties.Add(new PropertyData
                {
                    Name = "Center",
                    Type = "Point3d",
                    Value = FormatPoint(arc.Center),
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Radius",
                    Type = "Double",
                    Value = $"{arc.Radius:F4}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Start Angle (Radians)",
                    Type = "Double",
                    Value = $"{arc.StartAngle:F6}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Start Angle (Degrees)",
                    Type = "Double",
                    Value = $"{arc.StartAngle * 180.0 / Math.PI:F2}°",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "End Angle (Radians)",
                    Type = "Double",
                    Value = $"{arc.EndAngle:F6}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "End Angle (Degrees)",
                    Type = "Double",
                    Value = $"{arc.EndAngle * 180.0 / Math.PI:F2}°",
                    Category = "Geometry"
                });

                double totalAngle = arc.EndAngle - arc.StartAngle;
                if (totalAngle < 0)
                    totalAngle += 2 * Math.PI;

                properties.Add(new PropertyData
                {
                    Name = "Total Angle (Radians)",
                    Type = "Double",
                    Value = $"{totalAngle:F6}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Total Angle (Degrees)",
                    Type = "Double",
                    Value = $"{totalAngle * 180.0 / Math.PI:F2}°",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Arc Length",
                    Type = "Double",
                    Value = $"{arc.Length:F4}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Start Point",
                    Type = "Point3d",
                    Value = FormatPoint(arc.StartPoint),
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "End Point",
                    Type = "Point3d",
                    Value = FormatPoint(arc.EndPoint),
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Normal",
                    Type = "Vector3d",
                    Value = FormatVector(arc.Normal),
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Thickness",
                    Type = "Double",
                    Value = $"{arc.Thickness:F4}",
                    Category = "Geometry"
                });

                // Entity Properties
                properties.Add(new PropertyData
                {
                    Name = "Layer",
                    Type = "String",
                    Value = arc.Layer ?? "[None]",
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Color",
                    Type = "Color",
                    Value = arc.Color.ToString(),
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Linetype",
                    Type = "String",
                    Value = arc.Linetype ?? "[None]",
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = arc.Handle.ToString(),
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Arc properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        public Dictionary<string, System.Collections.IEnumerable> GetCollections(object obj, Transaction trans)
        {
            return new Dictionary<string, System.Collections.IEnumerable>();
        }

        private string FormatPoint(Point3d point)
        {
            return $"({point.X:F4}, {point.Y:F4}, {point.Z:F4})";
        }

        private string FormatVector(Vector3d vector)
        {
            return $"({vector.X:F4}, {vector.Y:F4}, {vector.Z:F4})";
        }
    }
}

