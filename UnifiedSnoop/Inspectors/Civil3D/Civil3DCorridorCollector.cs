// Civil3DCorridorCollector.cs - Specialized collector for Civil 3D Corridors
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
    /// Specialized collector for Civil 3D Corridor objects.
    /// Provides enhanced property collection for Corridor entities including baselines, assemblies, and surfaces.
    /// </summary>
    public class Civil3DCorridorCollector : ICollector
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private static Type? _corridorType;
        #else
        private static Type _corridorType;
        #endif
        private static bool _typeResolved = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D Corridor Collector";

        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor to resolve Civil 3D types.
        /// </summary>
        static Civil3DCorridorCollector()
        {
            try
            {
                _corridorType = Type.GetType("Autodesk.Civil.DatabaseServices.Corridor, AeccDbMgd");
                _typeResolved = _corridorType != null;
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
        /// <returns>true if obj is a Corridor; otherwise, false.</returns>
        public bool CanCollect(object obj)
        {
            if (obj == null || !_typeResolved || _corridorType == null)
                return false;

            return _corridorType.IsInstanceOfType(obj);
        }

        /// <summary>
        /// Collects properties from a Civil 3D Corridor.
        /// </summary>
        /// <param name="obj">The Corridor to collect properties from.</param>
        /// <param name="trans">The active transaction for database access.</param>
        /// <returns>A list of property data collected from the Corridor.</returns>
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

                // Add corridor-specific summary
                AddCorridorSummary(obj, properties);
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Error",
                    Value = $"Failed to collect Corridor properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections from a Civil 3D Corridor.
        /// </summary>
        /// <param name="obj">The Corridor to get collections from.</param>
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
        /// Adds corridor-specific summary information.
        /// </summary>
        private void AddCorridorSummary(object corridor, List<PropertyData> properties)
        {
            try
            {
                Type corridorType = corridor.GetType();

                // Try to get common corridor properties
                string name = GetPropertyValue<string>(corridor, "Name") ?? "[Unknown]";
                double startStation = GetPropertyValue<double>(corridor, "StartStation");
                double endStation = GetPropertyValue<double>(corridor, "EndStation");
                
                // Try to get baseline count
                int baselineCount = GetCollectionCount(corridor, "Baselines");
                
                // Try to get feature line count
                int featureLineCount = GetCollectionCount(corridor, "FeatureLines");

                // Try to get surfaces count
                int surfaceCount = GetCollectionCount(corridor, "CorridorSurfaces");

                properties.Add(new PropertyData
                {
                    Name = "Corridor Summary",
                    Type = "String",
                    Value = $"Name: {name}, Stations: {startStation:F3} to {endStation:F3}, " +
                           $"Baselines: {baselineCount}, FeatureLines: {featureLineCount}, Surfaces: {surfaceCount}",
                    DeclaringType = "UnifiedSnoop"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Corridor Summary",
                    Type = "Error",
                    Value = $"Error: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Gets a property value using reflection.
        /// </summary>
        private T GetPropertyValue<T>(object obj, string propertyName)
        {
            try
            {
                Type objType = obj.GetType();
                PropertyInfo prop = objType.GetProperty(propertyName);
                
                if (prop != null)
                {
                    object value = prop.GetValue(obj, null);
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return default(T);
        }

        /// <summary>
        /// Gets the count of items in a collection property.
        /// </summary>
        private int GetCollectionCount(object obj, string propertyName)
        {
            try
            {
                Type objType = obj.GetType();
                PropertyInfo prop = objType.GetProperty(propertyName);
                
                if (prop != null)
                {
                    object collection = prop.GetValue(obj, null);
                    if (collection is IEnumerable enumerable)
                    {
                        int count = 0;
                        foreach (var item in enumerable)
                        {
                            count++;
                        }
                        return count;
                    }
                }
            }
            catch
            {
                // Ignore errors
            }

            return 0;
        }

        #endregion
    }
}

