// DimensionCollector.cs - Specialized collector for AutoCAD Dimension entities
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
    /// Specialized collector for AutoCAD Dimension entities.
    /// </summary>
    public class DimensionCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD Dimension Collector";

        public bool CanCollect(object obj)
        {
            return obj is Dimension;
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (!(obj is Dimension dim))
                return new List<PropertyData>();

            var properties = new List<PropertyData>();

            try
            {
                // Dimension Type
                properties.Add(new PropertyData
                {
                    Name = "Dimension Type",
                    Type = "String",
                    Value = GetDimensionType(dim),
                    Category = "General"
                });

                // Measurement
                properties.Add(new PropertyData
                {
                    Name = "Measurement",
                    Type = "Double",
                    Value = $"{dim.Measurement:F4}",
                    Category = "Measurement"
                });

                // Dimension Text
                properties.Add(new PropertyData
                {
                    Name = "Dimension Text",
                    Type = "String",
                    Value = dim.DimensionText ?? "[Default]",
                    Category = "Text"
                });

                properties.Add(new PropertyData
                {
                    Name = "Dimension Style",
                    Type = "String",
                    Value = dim.DimensionStyleName ?? "[Default]",
                    Category = "Style"
                });

                properties.Add(new PropertyData
                {
                    Name = "Text Position",
                    Type = "Point3d",
                    Value = $"({dim.TextPosition.X:F4}, {dim.TextPosition.Y:F4}, {dim.TextPosition.Z:F4})",
                    Category = "Text"
                });

                properties.Add(new PropertyData
                {
                    Name = "Text Rotation",
                    Type = "Double",
                    Value = $"{dim.TextRotation * 180.0 / Math.PI:F2}°",
                    Category = "Text"
                });

                properties.Add(new PropertyData
                {
                    Name = "Horizontal Rotation",
                    Type = "Double",
                    Value = $"{dim.HorizontalRotation * 180.0 / Math.PI:F2}°",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Elevation",
                    Type = "Double",
                    Value = $"{dim.Elevation:F4}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "Normal",
                    Type = "Vector3d",
                    Value = $"({dim.Normal.X:F4}, {dim.Normal.Y:F4}, {dim.Normal.Z:F4})",
                    Category = "Geometry"
                });

                // Specific dimension type properties
                if (dim is AlignedDimension alignedDim)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "XLine1 Point",
                        Type = "Point3d",
                        Value = $"({alignedDim.XLine1Point.X:F4}, {alignedDim.XLine1Point.Y:F4}, {alignedDim.XLine1Point.Z:F4})",
                        Category = "Aligned Dimension"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "XLine2 Point",
                        Type = "Point3d",
                        Value = $"({alignedDim.XLine2Point.X:F4}, {alignedDim.XLine2Point.Y:F4}, {alignedDim.XLine2Point.Z:F4})",
                        Category = "Aligned Dimension"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "Dim Line Point",
                        Type = "Point3d",
                        Value = $"({alignedDim.DimLinePoint.X:F4}, {alignedDim.DimLinePoint.Y:F4}, {alignedDim.DimLinePoint.Z:F4})",
                        Category = "Aligned Dimension"
                    });
                }
                else if (dim is RotatedDimension rotatedDim)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Rotation Angle",
                        Type = "Double",
                        Value = $"{rotatedDim.Rotation * 180.0 / Math.PI:F2}°",
                        Category = "Rotated Dimension"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "XLine1 Point",
                        Type = "Point3d",
                        Value = $"({rotatedDim.XLine1Point.X:F4}, {rotatedDim.XLine1Point.Y:F4}, {rotatedDim.XLine1Point.Z:F4})",
                        Category = "Rotated Dimension"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "XLine2 Point",
                        Type = "Point3d",
                        Value = $"({rotatedDim.XLine2Point.X:F4}, {rotatedDim.XLine2Point.Y:F4}, {rotatedDim.XLine2Point.Z:F4})",
                        Category = "Rotated Dimension"
                    });
                }
                else if (dim is RadialDimension radialDim)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Center",
                        Type = "Point3d",
                        Value = $"({radialDim.Center.X:F4}, {radialDim.Center.Y:F4}, {radialDim.Center.Z:F4})",
                        Category = "Radial Dimension"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "Chord Point",
                        Type = "Point3d",
                        Value = $"({radialDim.ChordPoint.X:F4}, {radialDim.ChordPoint.Y:F4}, {radialDim.ChordPoint.Z:F4})",
                        Category = "Radial Dimension"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "Leader Length",
                        Type = "Double",
                        Value = $"{radialDim.LeaderLength:F4}",
                        Category = "Radial Dimension"
                    });
                }
                else if (dim is DiametricDimension diamDim)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Chord Point",
                        Type = "Point3d",
                        Value = $"({diamDim.ChordPoint.X:F4}, {diamDim.ChordPoint.Y:F4}, {diamDim.ChordPoint.Z:F4})",
                        Category = "Diametric Dimension"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "Far Chord Point",
                        Type = "Point3d",
                        Value = $"({diamDim.FarChordPoint.X:F4}, {diamDim.FarChordPoint.Y:F4}, {diamDim.FarChordPoint.Z:F4})",
                        Category = "Diametric Dimension"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "Leader Length",
                        Type = "Double",
                        Value = $"{diamDim.LeaderLength:F4}",
                        Category = "Diametric Dimension"
                    });
                }

                // Entity Properties
                properties.Add(new PropertyData
                {
                    Name = "Layer",
                    Type = "String",
                    Value = dim.Layer ?? "[None]",
                    Category = "Entity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = dim.Handle.ToString(),
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Dimension properties: {ex.Message}",
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

        private string GetDimensionType(Dimension dim)
        {
            if (dim is AlignedDimension)
                return "Aligned Dimension";
            else if (dim is RotatedDimension)
                return "Rotated/Linear Dimension";
            else if (dim is RadialDimension)
                return "Radial Dimension";
            else if (dim is DiametricDimension)
                return "Diametric Dimension";
            else if (dim is ArcDimension)
                return "Arc Dimension";
            else if (dim is OrdinateDimension)
                return "Ordinate Dimension";
            else
                return dim.GetType().Name;
        }
    }
}

