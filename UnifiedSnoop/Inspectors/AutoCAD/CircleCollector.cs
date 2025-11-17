// CircleCollector.cs - Specialized collector for AutoCAD Circle entities
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
    /// Specialized collector for AutoCAD Circle entities.
    /// </summary>
    public class CircleCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD Circle Collector";

        public bool CanCollect(object obj)
        {
            return obj is Circle;
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (!(obj is Circle circle))
                return new List<PropertyData>();

            var properties = new List<PropertyData>();

            try
            {
                // Circle Geometry
                properties.Add(new PropertyData
                {
                    Name = "Center",
                    Type = "Point3d",
                    Value = $"({circle.Center.X:F4}, {circle.Center.Y:F4}, {circle.Center.Z:F4})",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Radius",
                    Type = "Double",
                    Value = $"{circle.Radius:F4}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Diameter",
                    Type = "Double",
                    Value = $"{circle.Diameter:F4}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Circumference",
                    Type = "Double",
                    Value = $"{circle.Circumference:F4}",
                    Category = "Geometry"
                });

                double area = Math.PI * circle.Radius * circle.Radius;
                properties.Add(new PropertyData
                {
                    Name = "Area",
                    Type = "Double",
                    Value = $"{area:F4}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Normal",
                    Type = "Vector3d",
                    Value = $"({circle.Normal.X:F4}, {circle.Normal.Y:F4}, {circle.Normal.Z:F4})",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Thickness",
                    Type = "Double",
                    Value = $"{circle.Thickness:F4}",
                    Category = "Geometry"
                });

                // Entity Properties
                properties.Add(new PropertyData
                {
                    Name = "Layer",
                    Type = "String",
                    Value = circle.Layer ?? "[None]",
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Color",
                    Type = "Color",
                    Value = circle.Color.ToString(),
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Linetype",
                    Type = "String",
                    Value = circle.Linetype ?? "[None]",
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = circle.Handle.ToString(),
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Circle properties: {ex.Message}",
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
    }
}

