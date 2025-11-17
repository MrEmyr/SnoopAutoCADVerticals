// Civil3DSurfaceCollector.cs - Specialized collector for Civil 3D Surfaces
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
    /// Specialized collector for Civil 3D Surface objects.
    /// Provides enhanced property collection for Surface entities.
    /// </summary>
    public class Civil3DSurfaceCollector : ICollector
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private static Type? _surfaceType;
        #else
        private static Type _surfaceType;
        #endif
        private static bool _typeResolved = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D Surface Collector";

        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor to resolve Civil 3D types.
        /// </summary>
        static Civil3DSurfaceCollector()
        {
            try
            {
                // Try to resolve the Surface type
                _surfaceType = Type.GetType("Autodesk.Civil.DatabaseServices.Surface, AeccDbMgd");
                if (_surfaceType == null)
                {
                    // Try alternate assembly name for different versions
                    _surfaceType = Type.GetType("Autodesk.Civil.DatabaseServices.TinSurface, AeccDbMgd");
                }
                _typeResolved = _surfaceType != null;
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
        /// <returns>true if obj is a Surface; otherwise, false.</returns>
        public bool CanCollect(object obj)
        {
            if (obj == null || !_typeResolved || _surfaceType == null)
                return false;

            // Check if it's a Surface or derived from Surface
            Type objType = obj.GetType();
            return _surfaceType.IsAssignableFrom(objType) || objType.Name.Contains("Surface");
        }

        /// <summary>
        /// Collects properties from a Civil 3D Surface.
        /// </summary>
        /// <param name="obj">The Surface to collect properties from.</param>
        /// <param name="trans">The active transaction for database access.</param>
        /// <returns>A list of property data collected from the Surface.</returns>
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

                // Add surface-specific summary
                AddSurfaceSummary(obj, properties);
            }
            catch (Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Error",
                    Value = $"Failed to collect Surface properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections from a Civil 3D Surface.
        /// </summary>
        /// <param name="obj">The Surface to get collections from.</param>
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
        /// Adds surface-specific summary information.
        /// </summary>
        private void AddSurfaceSummary(object surface, List<PropertyData> properties)
        {
            try
            {
                Type surfaceType = surface.GetType();

                // Try to get common surface properties
                string name = GetPropertyValue<string>(surface, "Name") ?? "[Unknown]";
                double minElevation = GetPropertyValue<double>(surface, "MinElevation");
                double maxElevation = GetPropertyValue<double>(surface, "MaxElevation");
                
                // Try to get statistics
                object stats = GetPropertyValue<object>(surface, "GeneralProperties");
                int pointCount = 0;
                int triangleCount = 0;

                if (stats != null)
                {
                    pointCount = GetPropertyValue<int>(stats, "NumberOfPoints");
                    triangleCount = GetPropertyValue<int>(stats, "NumberOfTriangles");
                }

                properties.Add(new PropertyData
                {
                    Name = "Surface Summary",
                    Type = "String",
                    Value = $"Name: {name}, Elevation: {minElevation:F3} to {maxElevation:F3}, " +
                           $"Points: {pointCount}, Triangles: {triangleCount}",
                    DeclaringType = "UnifiedSnoop"
                });
            }
            catch (Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Surface Summary",
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
                if (obj == null)
                    return default(T);

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

