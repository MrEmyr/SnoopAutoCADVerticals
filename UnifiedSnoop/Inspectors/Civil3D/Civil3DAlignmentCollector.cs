// Civil3DAlignmentCollector.cs - Specialized collector for Civil 3D Alignments
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
    /// Specialized collector for Civil 3D Alignment objects.
    /// Provides enhanced property collection for Alignment entities.
    /// </summary>
    public class Civil3DAlignmentCollector : ICollector
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private static Type? _alignmentType;
        #else
        private static Type _alignmentType;
        #endif
        private static bool _typeResolved = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D Alignment Collector";

        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor to resolve Civil 3D types.
        /// </summary>
        static Civil3DAlignmentCollector()
        {
            try
            {
                _alignmentType = Type.GetType("Autodesk.Civil.DatabaseServices.Alignment, AeccDbMgd");
                _typeResolved = _alignmentType != null;
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
        /// <returns>true if obj is an Alignment; otherwise, false.</returns>
        public bool CanCollect(object obj)
        {
            if (obj == null || !_typeResolved || _alignmentType == null)
                return false;

            return _alignmentType.IsInstanceOfType(obj);
        }

        /// <summary>
        /// Collects properties from a Civil 3D Alignment.
        /// </summary>
        /// <param name="obj">The Alignment to collect properties from.</param>
        /// <param name="trans">The active transaction for database access.</param>
        /// <returns>A list of property data collected from the Alignment.</returns>
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

                // Add alignment-specific summary
                AddAlignmentSummary(obj, properties);
            }
            catch (Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Error",
                    Value = $"Failed to collect Alignment properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections from a Civil 3D Alignment.
        /// </summary>
        /// <param name="obj">The Alignment to get collections from.</param>
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
        /// Adds alignment-specific summary information.
        /// </summary>
        private void AddAlignmentSummary(object alignment, List<PropertyData> properties)
        {
            try
            {
                Type alignmentType = alignment.GetType();

                // Try to get common alignment properties
                string name = GetPropertyValue<string>(alignment, "Name") ?? "[Unknown]";
                double length = GetPropertyValue<double>(alignment, "Length");
                double startStation = GetPropertyValue<double>(alignment, "StartingStation");
                double endStation = GetPropertyValue<double>(alignment, "EndingStation");

                properties.Add(new PropertyData
                {
                    Name = "Alignment Summary",
                    Type = "String",
                    Value = $"Name: {name}, Length: {length:F3}, Stations: {startStation:F3} to {endStation:F3}",
                    DeclaringType = "UnifiedSnoop"
                });
            }
            catch (Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Alignment Summary",
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

        #endregion
    }
}

