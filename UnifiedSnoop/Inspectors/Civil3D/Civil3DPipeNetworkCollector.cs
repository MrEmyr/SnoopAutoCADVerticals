// Civil3DPipeNetworkCollector.cs - Specialized collector for Civil 3D Pipe Networks
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
    /// Specialized collector for Civil 3D Pipe Network objects.
    /// Provides enhanced property collection for Pipe Network entities including pipes, structures, and network rules.
    /// </summary>
    public class Civil3DPipeNetworkCollector : ICollector
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private static Type? _pipeNetworkType;
        #else
        private static Type _pipeNetworkType;
        #endif
        private static bool _typeResolved = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D Pipe Network Collector";

        #endregion

        #region Constructor

        /// <summary>
        /// Static constructor to resolve Civil 3D types.
        /// </summary>
        static Civil3DPipeNetworkCollector()
        {
            try
            {
                _pipeNetworkType = Type.GetType("Autodesk.Civil.DatabaseServices.Network, AeccDbMgd");
                _typeResolved = _pipeNetworkType != null;
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
        /// <returns>true if obj is a Pipe Network; otherwise, false.</returns>
        public bool CanCollect(object obj)
        {
            if (obj == null || !_typeResolved || _pipeNetworkType == null)
                return false;

            return _pipeNetworkType.IsInstanceOfType(obj);
        }

        /// <summary>
        /// Collects properties from a Civil 3D Pipe Network.
        /// </summary>
        /// <param name="obj">The Pipe Network to collect properties from.</param>
        /// <param name="trans">The active transaction for database access.</param>
        /// <returns>A list of property data collected from the Pipe Network.</returns>
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

                // Add pipe network-specific summary
                AddPipeNetworkSummary(obj, properties);
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Error",
                    Value = $"Failed to collect Pipe Network properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections from a Civil 3D Pipe Network.
        /// </summary>
        /// <param name="obj">The Pipe Network to get collections from.</param>
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
        /// Adds pipe network-specific summary information.
        /// </summary>
        private void AddPipeNetworkSummary(object network, List<PropertyData> properties)
        {
            try
            {
                Type networkType = network.GetType();

                // Try to get common pipe network properties
                string name = GetPropertyValue<string>(network, "Name") ?? "[Unknown]";
                
                // Try to get network type (e.g., Storm, Sanitary)
                string networkType_Value = GetPropertyValue<string>(network, "NetworkType") ?? "N/A";
                
                // Try to get pipe and structure counts
                int pipeCount = GetCollectionCount(network, "GetPipeIds");
                int structureCount = GetCollectionCount(network, "GetStructureIds");

                properties.Add(new PropertyData
                {
                    Name = "Pipe Network Summary",
                    Type = "String",
                    Value = $"Name: {name}, Type: {networkType_Value}, " +
                           $"Pipes: {pipeCount}, Structures: {structureCount}",
                    DeclaringType = "UnifiedSnoop"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Pipe Network Summary",
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
        /// Gets the count of items in a collection from a method that returns IDs.
        /// </summary>
        private int GetCollectionCount(object obj, string methodName)
        {
            try
            {
                Type objType = obj.GetType();
                MethodInfo method = objType.GetMethod(methodName);
                
                if (method != null)
                {
                    object result = method.Invoke(obj, null);
                    if (result is IEnumerable enumerable)
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

