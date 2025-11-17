// CollectorRegistry.cs - Registry for managing and routing to appropriate collectors
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Data;

namespace UnifiedSnoop.Core.Collectors
{
    /// <summary>
    /// Central registry for managing property collectors.
    /// Routes objects to the appropriate specialized collector or falls back to reflection.
    /// </summary>
    public class CollectorRegistry
    {
        #region Fields

        private readonly List<ICollector> _collectors;
        private readonly ReflectionCollector _defaultCollector;

        #if NET8_0_OR_GREATER
        private static CollectorRegistry? _instance;
        #else
        private static CollectorRegistry _instance;
        #endif
        private static readonly object _lock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the singleton instance of the CollectorRegistry.
        /// </summary>
        public static CollectorRegistry Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CollectorRegistry();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Gets the count of registered collectors (excluding the default reflection collector).
        /// </summary>
        public int CollectorCount => _collectors.Count;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectorRegistry"/> class.
        /// Private constructor for singleton pattern.
        /// </summary>
        private CollectorRegistry()
        {
            _collectors = new List<ICollector>();
            _defaultCollector = new ReflectionCollector();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Registers a new collector.
        /// </summary>
        /// <param name="collector">The collector to register. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown when collector is null.</exception>
        public void RegisterCollector(ICollector collector)
        {
            if (collector == null)
                throw new ArgumentNullException(nameof(collector));

            lock (_lock)
            {
                if (!_collectors.Contains(collector))
                {
                    _collectors.Add(collector);
                }
            }
        }

        /// <summary>
        /// Unregisters a collector.
        /// </summary>
        /// <param name="collector">The collector to unregister.</param>
        /// <returns>true if the collector was removed; otherwise, false.</returns>
        public bool UnregisterCollector(ICollector collector)
        {
            if (collector == null)
                return false;

            lock (_lock)
            {
                return _collectors.Remove(collector);
            }
        }

        /// <summary>
        /// Gets the appropriate collector for an object.
        /// </summary>
        /// <param name="obj">The object to find a collector for.</param>
        /// <returns>
        /// The first registered collector that can handle the object,
        /// or the default reflection collector if no specialized collector is found.
        /// </returns>
        public ICollector GetCollector(object obj)
        {
            if (obj == null)
                return _defaultCollector;

            lock (_lock)
            {
                // Try to find a specialized collector
                foreach (var collector in _collectors)
                {
                    if (collector.CanCollect(obj))
                    {
                        return collector;
                    }
                }
            }

            // Fall back to reflection collector
            return _defaultCollector;
        }

        /// <summary>
        /// Collects properties from an object using the appropriate collector.
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

            ICollector collector = GetCollector(obj);
            return collector.Collect(obj, trans);
        }

        /// <summary>
        /// Gets collections from an object using the appropriate collector.
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

            ICollector collector = GetCollector(obj);
            return collector.GetCollections(obj, trans);
        }

        /// <summary>
        /// Clears all registered collectors.
        /// </summary>
        /// <remarks>
        /// This does not affect the default reflection collector.
        /// </remarks>
        public void ClearCollectors()
        {
            lock (_lock)
            {
                _collectors.Clear();
            }
        }

        /// <summary>
        /// Gets a list of all registered collector names.
        /// </summary>
        /// <returns>A list of collector names, including the default collector.</returns>
        public List<string> GetCollectorNames()
        {
            var names = new List<string>();
            
            lock (_lock)
            {
                foreach (var collector in _collectors)
                {
                    names.Add(collector.Name);
                }
            }
            
            names.Add($"{_defaultCollector.Name} (Default)");
            return names;
        }

        #endregion
    }
}

