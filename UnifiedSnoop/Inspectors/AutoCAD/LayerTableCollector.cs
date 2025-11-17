// LayerTableCollector.cs - Specialized collector for AutoCAD Layer table records
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Colors;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;

namespace UnifiedSnoop.Inspectors.AutoCAD
{
    /// <summary>
    /// Specialized collector for AutoCAD LayerTableRecord entities.
    /// </summary>
    public class LayerTableCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD Layer Collector";

        public bool CanCollect(object obj)
        {
            return obj is LayerTableRecord;
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (!(obj is LayerTableRecord layer))
                return new List<PropertyData>();

            var properties = new List<PropertyData>();

            try
            {
                // Layer Identity
                properties.Add(new PropertyData
                {
                    Name = "Layer Name",
                    Type = "String",
                    Value = layer.Name ?? "[Unnamed]",
                    Category = "Identity"
                });

                // Layer States
                properties.Add(new PropertyData
                {
                    Name = "Is Off",
                    Type = "Boolean",
                    Value = layer.IsOff.ToString(),
                    Category = "State"
                });

                properties.Add(new PropertyData
                {
                    Name = "Is Frozen",
                    Type = "Boolean",
                    Value = layer.IsFrozen.ToString(),
                    Category = "State"
                });

                properties.Add(new PropertyData
                {
                    Name = "Is Locked",
                    Type = "Boolean",
                    Value = layer.IsLocked.ToString(),
                    Category = "State"
                });

                properties.Add(new PropertyData
                {
                    Name = "Is Plottable",
                    Type = "Boolean",
                    Value = layer.IsPlottable.ToString(),
                    Category = "State"
                });

                properties.Add(new PropertyData
                {
                    Name = "Is Hidden",
                    Type = "Boolean",
                    Value = layer.IsHidden.ToString(),
                    Category = "State"
                });

                // Layer Properties
                properties.Add(new PropertyData
                {
                    Name = "Color",
                    Type = "Color",
                    Value = layer.Color.ToString(),
                    Category = "Appearance"
                });

                properties.Add(new PropertyData
                {
                    Name = "Color Index",
                    Type = "Int16",
                    Value = layer.Color.ColorIndex.ToString(),
                    Category = "Appearance"
                });

                properties.Add(new PropertyData
                {
                    Name = "Linetype",
                    Type = "ObjectId",
                    Value = layer.LinetypeObjectId.ToString(),
                    Category = "Appearance"
                });

                properties.Add(new PropertyData
                {
                    Name = "LineWeight",
                    Type = "LineWeight",
                    Value = layer.LineWeight.ToString(),
                    Category = "Appearance"
                });

                properties.Add(new PropertyData
                {
                    Name = "Plot Style Name",
                    Type = "String",
                    Value = layer.PlotStyleName ?? "[Default]",
                    Category = "Plotting"
                });

                properties.Add(new PropertyData
                {
                    Name = "Plot Style Name ObjectId",
                    Type = "ObjectId",
                    Value = layer.PlotStyleNameId.ToString(),
                    Category = "Plotting"
                });

                // Advanced Properties
                properties.Add(new PropertyData
                {
                    Name = "Transparency",
                    Type = "Transparency",
                    Value = layer.Transparency.ToString(),
                    Category = "Appearance"
                });

                properties.Add(new PropertyData
                {
                    Name = "Material",
                    Type = "ObjectId",
                    Value = layer.MaterialId.ToString(),
                    Category = "Appearance"
                });

                properties.Add(new PropertyData
                {
                    Name = "Description",
                    Type = "String",
                    Value = layer.Description ?? "[None]",
                    Category = "General"
                });

                properties.Add(new PropertyData
                {
                    Name = "Is Dependent",
                    Type = "Boolean",
                    Value = layer.IsDependent.ToString(),
                    Category = "XREF"
                });

                properties.Add(new PropertyData
                {
                    Name = "Is Resolved",
                    Type = "Boolean",
                    Value = layer.IsResolved.ToString(),
                    Category = "XREF"
                });

                // Viewport Overrides
                properties.Add(new PropertyData
                {
                    Name = "Has Viewport Overrides",
                    Type = "Boolean",
                    Value = layer.HasOverrides.ToString(),
                    Category = "Viewport"
                });

                // Object Properties
                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = layer.Handle.ToString(),
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "ObjectId",
                    Type = "ObjectId",
                    Value = layer.ObjectId.ToString(),
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "Owner (Layer Table)",
                    Type = "ObjectId",
                    Value = layer.OwnerId.ToString(),
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Layer properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        public Dictionary<string, System.Collections.IEnumerable> GetCollections(object obj, Transaction trans)
        {
            // Layers don't have sub-collections in this context
            return new Dictionary<string, System.Collections.IEnumerable>();
        }
    }
}

