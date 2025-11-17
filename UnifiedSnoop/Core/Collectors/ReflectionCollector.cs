// ReflectionCollector.cs - Default collector using reflection
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Data;
using UnifiedSnoop.Core.Helpers;

namespace UnifiedSnoop.Core.Collectors
{
    /// <summary>
    /// Default collector that uses reflection to extract properties from any object.
    /// This collector can handle any object type and serves as a fallback when
    /// no specialized collector is available.
    /// </summary>
    public class ReflectionCollector : ICollector
    {
        #region Properties

        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Reflection Collector";

        #endregion

        #region ICollector Implementation

        /// <summary>
        /// Determines whether this collector can collect properties from the specified object.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>
        /// Always returns true as this is the default fallback collector
        /// that can handle any object type.
        /// </returns>
        public bool CanCollect(object obj)
        {
            // This is the universal fallback collector - it can handle any object
            return obj != null;
        }

        /// <summary>
        /// Collects properties from the specified object using reflection.
        /// </summary>
        /// <param name="obj">The object to collect properties from. Cannot be null.</param>
        /// <param name="trans">The active transaction for database access. Cannot be null.</param>
        /// <returns>A list of property data collected from the object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="obj"/> or <paramref name="trans"/> is null.
        /// </exception>
        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            try
            {
                return ReflectionHelper.GetProperties(obj, trans);
            }
            catch (Exception ex)
            {
                // If reflection fails completely, return an error property
                var errorList = new List<PropertyData>
                {
                    new PropertyData
                    {
                        Name = "Error",
                        Type = "Error",
                        Value = $"Failed to collect properties: {ex.Message}",
                        HasError = true,
                        ErrorMessage = ex.Message
                    }
                };
                return errorList;
            }
        }

        /// <summary>
        /// Gets collections from the specified object using reflection.
        /// </summary>
        /// <param name="obj">The object to get collections from. Cannot be null.</param>
        /// <param name="trans">The active transaction for database access. Cannot be null.</param>
        /// <returns>
        /// A dictionary where keys are collection names and values are the enumerable collections.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="obj"/> or <paramref name="trans"/> is null.
        /// </exception>
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
            catch (Exception)
            {
                // If we can't get collections, return an empty dictionary
                return new Dictionary<string, IEnumerable>();
            }
        }

        #endregion
    }
}

