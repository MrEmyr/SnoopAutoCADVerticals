// Civil3DDocumentCollector.cs - Specialized collector for Civil 3D Document
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;
using UnifiedSnoop.Core.Helpers;


#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
#endif

namespace UnifiedSnoop.Inspectors.Civil3D
{
    /// <summary>
    /// Specialized collector for Civil 3D Document objects.
    /// Provides enhanced property collection for CivilDocument compared to reflection.
    /// </summary>
    public class Civil3DDocumentCollector : ICollector
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private static Type? _civilDocType;
        #else
        private static Type _civilDocType;
        #endif
        private static bool _typeResolved = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D Document Collector";

        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor to resolve Civil 3D types.
        /// </summary>
        static Civil3DDocumentCollector()
        {
            try
            {
                _civilDocType = Type.GetType("Autodesk.Civil.ApplicationServices.CivilDocument, AecBaseMgd");
                _typeResolved = _civilDocType != null;
            }
            catch
            {
                _typeResolved = false;
            }
        }

        #endregion

        #region ICollector Implementation

        /// <summary>
        /// Determines whether this collector can collect properties from the specified object.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>true if obj is a CivilDocument; otherwise, false.</returns>
        public bool CanCollect(object obj)
        {
            if (obj == null || !_typeResolved || _civilDocType == null)
                return false;

            return _civilDocType.IsInstanceOfType(obj);
        }

        /// <summary>
        /// Collects properties from a Civil 3D Document.
        /// </summary>
        /// <param name="obj">The CivilDocument to collect properties from.</param>
        /// <param name="trans">The active transaction for database access.</param>
        /// <returns>A list of property data collected from the Civil 3D Document.</returns>
        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            var properties = new List<PropertyData>();

            try
            {
                // First, get all standard properties using reflection
                var standardProps = ReflectionHelper.GetProperties(obj, trans);
                properties.AddRange(standardProps);

                // Add custom/enhanced properties specific to Civil 3D Document
                AddCivil3DSpecificProperties(obj, properties, trans);
            }
            catch (Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Error",
                    Value = $"Failed to collect Civil 3D Document properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections from a Civil 3D Document.
        /// </summary>
        /// <param name="obj">The CivilDocument to get collections from.</param>
        /// <param name="trans">The active transaction for database access.</param>
        /// <returns>A dictionary of collection names and their enumerable values.</returns>
        public Dictionary<string, IEnumerable> GetCollections(object obj, Transaction trans)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            var collections = new Dictionary<string, IEnumerable>();

            try
            {
                // Get standard collections using reflection
                var standardCollections = ReflectionHelper.GetCollections(obj, trans);
                foreach (var kvp in standardCollections)
                {
                    collections[kvp.Key] = kvp.Value;
                }

                // Add Civil 3D specific collections
                AddCivil3DCollections(obj, collections);
            }
            catch
            {
                // If we fail, just return what we have
            }

            return collections;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Adds Civil 3D specific properties.
        /// </summary>
        private void AddCivil3DSpecificProperties(object civilDoc, List<PropertyData> properties, Transaction trans)
        {
            try
            {
                Type docType = civilDoc.GetType();

                // Add summary property for total object counts
                int alignmentCount = GetCollectionCount(civilDoc, "GetAlignmentIds");
                int surfaceCount = GetCollectionCount(civilDoc, "GetSurfaceIds");
                int corridorCount = GetCollectionCount(civilDoc, "CorridorCollection");
                int pipeNetworkCount = GetCollectionCount(civilDoc, "GetPipeNetworkIds");

                properties.Add(new PropertyData
                {
                    Name = "Civil 3D Summary",
                    Type = "String",
                    Value = $"Alignments: {alignmentCount}, Surfaces: {surfaceCount}, " +
                           $"Corridors: {corridorCount}, Pipe Networks: {pipeNetworkCount}",
                    DeclaringType = "UnifiedSnoop"
                });
            }
            catch (Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Civil 3D Summary",
                    Type = "Error",
                    Value = $"Error: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Adds Civil 3D specific collections.
        /// </summary>
        private void AddCivil3DCollections(object civilDoc, Dictionary<string, IEnumerable> collections)
        {
            // Civil 3D collections are typically accessed through methods, not properties
            // The reflection-based collector will already get the property-based collections
            // This method is here for future enhancements if needed
        }

        /// <summary>
        /// Gets the count of items in a Civil 3D collection.
        /// </summary>
        private int GetCollectionCount(object civilDoc, string methodOrPropertyName)
        {
            try
            {
                Type docType = civilDoc.GetType();

                // Try as a method first
                MethodInfo method = docType.GetMethod(methodOrPropertyName);
                if (method != null)
                {
                    object result = method.Invoke(civilDoc, null);
                    if (result is IEnumerable enumerable)
                    {
                        int count = 0;
                        foreach (var item in enumerable)
                        {
                            count++;
                        }
                        return count;
                    }
                    #if NET8_0_OR_GREATER
                    if (result is ICollection collection)
                    {
                        return collection.Count;
                    }
                    #else
                    if (result != null)
                    {
                        var collProp = result.GetType().GetProperty("Count");
                        if (collProp != null)
                        {
                            object countObj = collProp.GetValue(result, null);
                            if (countObj is int intCount)
                            {
                                return intCount;
                            }
                        }
                    }
                    #endif
                }

                // Try as a property
                PropertyInfo prop = docType.GetProperty(methodOrPropertyName);
                if (prop != null)
                {
                    object result = prop.GetValue(civilDoc, null);
                    if (result is IEnumerable enumerable)
                    {
                        int count = 0;
                        foreach (var item in enumerable)
                        {
                            count++;
                        }
                        return count;
                    }
                    #if NET8_0_OR_GREATER
                    if (result is ICollection collection)
                    {
                        return collection.Count;
                    }
                    #else
                    if (result != null)
                    {
                        var collProp = result.GetType().GetProperty("Count");
                        if (collProp != null)
                        {
                            object countObj = collProp.GetValue(result, null);
                            if (countObj is int intCount)
                            {
                                return intCount;
                            }
                        }
                    }
                    #endif
                }

                return 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}

