// TextCollector.cs - Specialized collector for AutoCAD Text entities
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
    /// Specialized collector for AutoCAD Text entities (DBText and MText).
    /// </summary>
    public class TextCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD Text Collector";

        public bool CanCollect(object obj)
        {
            return obj is DBText || obj is MText;
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            var properties = new List<PropertyData>();

            try
            {
                if (obj is DBText dbText)
                {
                    CollectDBTextProperties(dbText, properties);
                }
                else if (obj is MText mText)
                {
                    CollectMTextProperties(mText, properties);
                }
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Text properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        private void CollectDBTextProperties(DBText dbText, List<PropertyData> properties)
        {
            properties.Add(new PropertyData
            {
                Name = "Type",
                Type = "String",
                Value = "Single-Line Text (DBText)",
                Category = "General"
            });

            properties.Add(new PropertyData
            {
                Name = "Text String",
                Type = "String",
                Value = dbText.TextString ?? "[Empty]",
                Category = "Content"
            });

            properties.Add(new PropertyData
            {
                Name = "Position",
                Type = "Point3d",
                Value = FormatPoint(dbText.Position),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Alignment Point",
                Type = "Point3d",
                Value = FormatPoint(dbText.AlignmentPoint),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Height",
                Type = "Double",
                Value = $"{dbText.Height:F4}",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Width Factor",
                Type = "Double",
                Value = $"{dbText.WidthFactor:F2}",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Oblique Angle",
                Type = "Double",
                Value = $"{dbText.Oblique * 180.0 / Math.PI:F2}°",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Rotation",
                Type = "Double",
                Value = $"{dbText.Rotation * 180.0 / Math.PI:F2}°",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Text Style",
                Type = "String",
                Value = dbText.TextStyleName ?? "[Default]",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Horizontal Mode",
                Type = "TextHorizontalMode",
                Value = dbText.HorizontalMode.ToString(),
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Vertical Mode",
                Type = "TextVerticalMode",
                Value = dbText.VerticalMode.ToString(),
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Is Mirrored in X",
                Type = "Boolean",
                Value = dbText.IsMirroredInX.ToString(),
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Is Mirrored in Y",
                Type = "Boolean",
                Value = dbText.IsMirroredInY.ToString(),
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Layer",
                Type = "String",
                Value = dbText.Layer ?? "[None]",
                Category = "Entity"
            });

            properties.Add(new PropertyData
            {
                Name = "Handle",
                Type = "Handle",
                Value = dbText.Handle.ToString(),
                Category = "Object"
            });
        }

        private void CollectMTextProperties(MText mText, List<PropertyData> properties)
        {
            properties.Add(new PropertyData
            {
                Name = "Type",
                Type = "String",
                Value = "Multi-Line Text (MText)",
                Category = "General"
            });

            properties.Add(new PropertyData
            {
                Name = "Text String",
                Type = "String",
                Value = mText.Contents ?? "[Empty]",
                Category = "Content"
            });

            properties.Add(new PropertyData
            {
                Name = "Plain Text",
                Type = "String",
                Value = mText.Text ?? "[Empty]",
                Category = "Content"
            });

            properties.Add(new PropertyData
            {
                Name = "Location",
                Type = "Point3d",
                Value = FormatPoint(mText.Location),
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Width",
                Type = "Double",
                Value = $"{mText.Width:F4}",
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Height",
                Type = "Double",
                Value = $"{mText.Height:F4}",
                Category = "Geometry"
            });

            properties.Add(new PropertyData
            {
                Name = "Text Height",
                Type = "Double",
                Value = $"{mText.TextHeight:F4}",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Rotation",
                Type = "Double",
                Value = $"{mText.Rotation * 180.0 / Math.PI:F2}°",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Attachment",
                Type = "AttachmentPoint",
                Value = mText.Attachment.ToString(),
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Flow Direction",
                Type = "FlowDirection",
                Value = mText.FlowDirection.ToString(),
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Text Style",
                Type = "String",
                Value = mText.TextStyleName ?? "[Default]",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Line Spacing Factor",
                Type = "Double",
                Value = $"{mText.LineSpacingFactor:F2}",
                Category = "Text"
            });

            properties.Add(new PropertyData
            {
                Name = "Layer",
                Type = "String",
                Value = mText.Layer ?? "[None]",
                Category = "Entity"
            });

            properties.Add(new PropertyData
            {
                Name = "Handle",
                Type = "Handle",
                Value = mText.Handle.ToString(),
                Category = "Object"
            });
        }

        public Dictionary<string, System.Collections.IEnumerable> GetCollections(object obj, Transaction trans)
        {
            return new Dictionary<string, System.Collections.IEnumerable>();
        }

        private string FormatPoint(Point3d point)
        {
            return $"({point.X:F4}, {point.Y:F4}, {point.Z:F4})";
        }
    }
}

