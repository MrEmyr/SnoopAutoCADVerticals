// PolylineCollector.cs - Specialized collector for AutoCAD Polyline entities
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;

namespace UnifiedSnoop.Inspectors.AutoCAD
{
    /// <summary>
    /// Specialized collector for AutoCAD Polyline entities.
    /// </summary>
    public class PolylineCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD Polyline Collector";

        public bool CanCollect(object obj)
        {
            return obj is Polyline || obj is Polyline2d || obj is Polyline3d;
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            var properties = new List<PropertyData>();

            try
            {
                if (obj is Polyline pline)
                {
                    CollectPolylineProperties(pline, properties);
                }
                else if (obj is Polyline2d pline2d)
                {
                    CollectPolyline2dProperties(pline2d, properties, trans);
                }
                else if (obj is Polyline3d pline3d)
                {
                    CollectPolyline3dProperties(pline3d, properties, trans);
                }
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Polyline properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        private void CollectPolylineProperties(Polyline pline, List<PropertyData> properties)
        {
            properties.Add(new PropertyData
            {
                Name = "Type",
                Type = "String",
                Value = "Lightweight Polyline",
                Category = "General"
            });

            properties.Add(new PropertyData
            {
                Name = "Number of Vertices",
                Type = "Int32",
                Value = pline.NumberOfVertices.ToString(),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Length",
                Type = "Double",
                Value = $"{pline.Length:F4}",
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Is Closed",
                Type = "Boolean",
                Value = pline.Closed.ToString(),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Has Bulges",
                Type = "Boolean",
                Value = pline.HasBulges.ToString(),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Has Width",
                Type = "Boolean",
                Value = pline.HasWidth.ToString(),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Constant Width",
                Type = "Double",
                Value = $"{pline.ConstantWidth:F4}",
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Thickness",
                Type = "Double",
                Value = $"{pline.Thickness:F4}",
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Elevation",
                Type = "Double",
                Value = $"{pline.Elevation:F4}",
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Normal",
                Type = "Vector3d",
                Value = $"({pline.Normal.X:F4}, {pline.Normal.Y:F4}, {pline.Normal.Z:F4})",
                Category = "Geometry"
            });

            // Entity Properties
            properties.Add(new PropertyData
            {
                Name = "Layer",
                Type = "String",
                Value = pline.Layer ?? "[None]",
                Category = "Entity"
            });

            properties.Add(new PropertyData
            {
                Name = "Handle",
                Type = "Handle",
                Value = pline.Handle.ToString(),
                Category = "Object"
            });
        }

        private void CollectPolyline2dProperties(Polyline2d pline2d, List<PropertyData> properties, Transaction trans)
        {
            properties.Add(new PropertyData
            {
                Name = "Type",
                Type = "String",
                Value = "2D Polyline",
                Category = "General"
            });

            properties.Add(new PropertyData
            {
                Name = "Polyline Type",
                Type = "Poly2dType",
                Value = pline2d.PolyType.ToString(),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Length",
                Type = "Double",
                Value = $"{pline2d.Length:F4}",
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Is Closed",
                Type = "Boolean",
                Value = pline2d.Closed.ToString(),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Layer",
                Type = "String",
                Value = pline2d.Layer ?? "[None]",
                Category = "Entity"
            });

            properties.Add(new PropertyData
            {
                Name = "Handle",
                Type = "Handle",
                Value = pline2d.Handle.ToString(),
                Category = "Object"
            });
        }

        private void CollectPolyline3dProperties(Polyline3d pline3d, List<PropertyData> properties, Transaction trans)
        {
            properties.Add(new PropertyData
            {
                Name = "Type",
                Type = "String",
                Value = "3D Polyline",
                Category = "General"
            });

            properties.Add(new PropertyData
            {
                Name = "Polyline Type",
                Type = "Poly3dType",
                Value = pline3d.PolyType.ToString(),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Length",
                Type = "Double",
                Value = $"{pline3d.Length:F4}",
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Is Closed",
                Type = "Boolean",
                Value = pline3d.Closed.ToString(),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Layer",
                Type = "String",
                Value = pline3d.Layer ?? "[None]",
                Category = "Entity"
            });

            properties.Add(new PropertyData
            {
                Name = "Handle",
                Type = "Handle",
                Value = pline3d.Handle.ToString(),
                Category = "Object"
            });
        }

        public Dictionary<string, System.Collections.IEnumerable> GetCollections(object obj, Transaction trans)
        {
            var collections = new Dictionary<string, System.Collections.IEnumerable>();

            if (obj is Polyline2d pline2d)
            {
                try
                {
                    var vertices = new List<object>();
                    foreach (ObjectId vertexId in pline2d)
                    {
                        var vertex = trans.GetObject(vertexId, OpenMode.ForRead);
                        if (vertex != null)
                            vertices.Add(vertex);
                    }
                    if (vertices.Count > 0)
                        collections.Add("Vertices", vertices);
                }
                catch { }
            }
            else if (obj is Polyline3d pline3d)
            {
                try
                {
                    var vertices = new List<object>();
                    foreach (ObjectId vertexId in pline3d)
                    {
                        var vertex = trans.GetObject(vertexId, OpenMode.ForRead);
                        if (vertex != null)
                            vertices.Add(vertex);
                    }
                    if (vertices.Count > 0)
                        collections.Add("Vertices", vertices);
                }
                catch { }
            }

            return collections;
        }
    }
}

