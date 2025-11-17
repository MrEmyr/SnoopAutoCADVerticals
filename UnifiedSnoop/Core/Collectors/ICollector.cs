// ICollector.cs - Core interface for property collection
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Data;

namespace UnifiedSnoop.Core.Collectors
{
    /// <summary>
    /// Defines the contract for collecting properties from objects.
    /// Implementations of this interface use various strategies (reflection, specific inspectors, etc.)
    /// to extract property information from AutoCAD and Civil 3D objects.
    /// </summary>
    public interface ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Determines whether this collector can collect properties from the specified object.
        /// </summary>
        /// <param name="obj">The object to check.</param>
        /// <returns>true if this collector can handle the object; otherwise, false.</returns>
        bool CanCollect(object obj);

        /// <summary>
        /// Collects properties from the specified object.
        /// </summary>
        /// <param name="obj">The object to collect properties from. Cannot be null.</param>
        /// <param name="trans">The active transaction for database access. Cannot be null.</param>
        /// <returns>A list of property data collected from the object.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="obj"/> or <paramref name="trans"/> is null.
        /// </exception>
        List<PropertyData> Collect(object obj, Transaction trans);

        /// <summary>
        /// Gets collections from the specified object.
        /// Collections are properties that implement IEnumerable and contain multiple items.
        /// </summary>
        /// <param name="obj">The object to get collections from. Cannot be null.</param>
        /// <param name="trans">The active transaction for database access. Cannot be null.</param>
        /// <returns>
        /// A dictionary where keys are collection names and values are the enumerable collections.
        /// Returns an empty dictionary if no collections are found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="obj"/> or <paramref name="trans"/> is null.
        /// </exception>
        Dictionary<string, IEnumerable> GetCollections(object obj, Transaction trans);
    }
}

