// BlockReferenceCollector.cs - Enhanced collector for AutoCAD Block References
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;
using UnifiedSnoop.Core.Helpers;


#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
#endif

namespace UnifiedSnoop.Inspectors.AutoCAD
{
    /// <summary>
    /// Enhanced collector for AutoCAD BlockReference objects.
    /// Provides detailed information about attributes, dynamic properties, and block definitions.
    /// </summary>
    public class BlockReferenceCollector : ICollector
    {
        #region Properties

        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD BlockReference Collector";

        #endregion

        #region ICollector Implementation

        /// <summary>
        /// Determines whether this collector can collect properties from the specified object.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>true if obj is a BlockReference; otherwise, false.</returns>
        public bool CanCollect(object obj)
        {
            return obj is BlockReference;
        }

        /// <summary>
        /// Collects properties from an AutoCAD BlockReference.
        /// </summary>
        /// <param name="obj">The BlockReference to collect properties from.</param>
        /// <param name="trans">The active transaction for database access.</param>
        /// <returns>A list of property data collected from the BlockReference.</returns>
        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            var properties = new List<PropertyData>();

            try
            {
                // Get all standard properties using reflection
                var standardProps = ReflectionHelper.GetProperties(obj, trans);
                properties.AddRange(standardProps);

                // Add block-specific information
                if (obj is BlockReference blockRef)
                {
                    AddBlockSummary(blockRef, properties, trans);
                    AddAttributeInformation(blockRef, properties, trans);
                    AddDynamicBlockInformation(blockRef, properties);
                }
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Error",
                    Value = $"Failed to collect BlockReference properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections from an AutoCAD BlockReference.
        /// </summary>
        /// <param name="obj">The BlockReference to get collections from.</param>
        /// <param name="trans">The active transaction for database access.</param>
        /// <returns>A dictionary of collection names and their enumerable values.</returns>
        public Dictionary<string, IEnumerable> GetCollections(object obj, Transaction trans)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            try
            {
                return ReflectionHelper.GetCollections(obj, trans);
            }
            catch
            {
                return new Dictionary<string, IEnumerable>();
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Adds block-specific summary information.
        /// </summary>
        private void AddBlockSummary(BlockReference blockRef, List<PropertyData> properties, Transaction trans)
        {
            try
            {
                // Get block table record
                BlockTableRecord btr = trans.GetObject(blockRef.BlockTableRecord, OpenMode.ForRead) as BlockTableRecord;
                
                if (btr != null)
                {
                    string blockName = btr.Name;
                    bool isAnonymous = btr.IsAnonymous;
                    bool isDynamic = blockRef.IsDynamicBlock;
                    bool hasAttributes = blockRef.AttributeCollection.Count > 0;

                    properties.Add(new PropertyData
                    {
                        Name = "Block Summary",
                        Type = "String",
                        Value = $"Name: {blockName}, Dynamic: {isDynamic}, " +
                               $"Anonymous: {isAnonymous}, Attributes: {hasAttributes}",
                        DeclaringType = "UnifiedSnoop"
                    });
                }
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Block Summary",
                    Type = "Error",
                    Value = $"Error: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Adds attribute information.
        /// </summary>
        private void AddAttributeInformation(BlockReference blockRef, List<PropertyData> properties, Transaction trans)
        {
            try
            {
                AttributeCollection attCol = blockRef.AttributeCollection;
                
                if (attCol.Count > 0)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "--- Attributes ---",
                        Type = "Section",
                        Value = $"{attCol.Count} attributes found",
                        DeclaringType = "UnifiedSnoop"
                    });

                    foreach (ObjectId attId in attCol)
                    {
                        AttributeReference attRef = trans.GetObject(attId, OpenMode.ForRead) as AttributeReference;
                        if (attRef != null)
                        {
                            properties.Add(new PropertyData
                            {
                                Name = $"  Attribute: {attRef.Tag}",
                                Type = "String",
                                Value = attRef.TextString,
                                DeclaringType = "AttributeReference"
                            });
                        }
                    }
                }
                else
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Attributes",
                        Type = "Info",
                        Value = "No attributes",
                        DeclaringType = "UnifiedSnoop"
                    });
                }
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Attribute Information",
                    Type = "Error",
                    Value = $"Error: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Adds dynamic block information.
        /// </summary>
        private void AddDynamicBlockInformation(BlockReference blockRef, List<PropertyData> properties)
        {
            try
            {
                if (blockRef.IsDynamicBlock)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "--- Dynamic Block Properties ---",
                        Type = "Section",
                        Value = "Dynamic block detected",
                        DeclaringType = "UnifiedSnoop"
                    });

                    DynamicBlockReferencePropertyCollection dynProps = blockRef.DynamicBlockReferencePropertyCollection;
                    
                    foreach (DynamicBlockReferenceProperty dynProp in dynProps)
                    {
                        string value = dynProp.Value != null ? dynProp.Value.ToString() : "[null]";
                        
                        properties.Add(new PropertyData
                        {
                            Name = $"  Dynamic: {dynProp.PropertyName}",
                            Type = dynProp.PropertyTypeCode.ToString(),
                            Value = $"{value} (ReadOnly: {dynProp.ReadOnly})",
                            DeclaringType = "DynamicBlockReferenceProperty"
                        });
                    }
                }
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Dynamic Block Information",
                    Type = "Error",
                    Value = $"Error: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }
        }

        #endregion
    }
}

