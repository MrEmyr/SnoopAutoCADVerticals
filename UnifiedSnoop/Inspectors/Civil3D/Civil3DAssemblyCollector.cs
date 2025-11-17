// Civil3DAssemblyCollector.cs - Specialized collector for Civil 3D Assembly objects
// Supports both .NET Framework 4.8 (Civil 3D 2024) and .NET 8.0 (Civil 3D 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;

#if CIVIL3D
using Autodesk.Civil.DatabaseServices;
#endif

namespace UnifiedSnoop.Inspectors.Civil3D
{
    /// <summary>
    /// Specialized collector for Civil 3D Assembly objects.
    /// </summary>
    public class Civil3DAssemblyCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D Assembly Collector";

        public bool CanCollect(object obj)
        {
#if CIVIL3D
            return obj is Assembly;
#else
            return false;
#endif
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            var properties = new List<PropertyData>();

#if CIVIL3D
            if (!(obj is Assembly assembly))
                return properties;

            try
            {
                // Identity
                properties.Add(new PropertyData
                {
                    Name = "Assembly Name",
                    Type = "String",
                    Value = assembly.Name ?? "[Unnamed]",
                    Category = "Identity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Description",
                    Type = "String",
                    Value = assembly.Description ?? "[None]",
                    Category = "Identity"
                });

                // Assembly Groups
                try
                {
                    int groupCount = assembly.Groups.Count;
                    properties.Add(new PropertyData
                    {
                        Name = "Number of Groups",
                        Type = "Int32",
                        Value = groupCount.ToString(),
                        Category = "Structure"
                    });
                }
                catch (System.Exception ex)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Number of Groups",
                        Type = "String",
                        Value = $"[Error: {ex.Message}]",
                        Category = "Structure"
                    });
                }

                // Code Set Style
                try
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Code Set Style Name",
                        Type = "String",
                        Value = assembly.CodeSetStyleName ?? "[Default]",
                        Category = "Style"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "Code Set Style Id",
                        Type = "ObjectId",
                        Value = assembly.CodeSetStyleId.ToString(),
                        Category = "Style"
                    });
                }
                catch { }

                // Object Properties
                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = assembly.Handle.ToString(),
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "ObjectId",
                    Type = "ObjectId",
                    Value = assembly.ObjectId.ToString(),
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "Database",
                    Type = "Database",
                    Value = assembly.Database != null ? "Valid" : "Null",
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Assembly properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }
#endif

            return properties;
        }

        public Dictionary<string, System.Collections.IEnumerable> GetCollections(object obj, Transaction trans)
        {
            var collections = new Dictionary<string, System.Collections.IEnumerable>();

#if CIVIL3D
            if (!(obj is Assembly assembly))
                return collections;

            try
            {
                // Get Assembly Groups
                var groups = new List<object>();
                foreach (var group in assembly.Groups)
                {
                    groups.Add(group);
                }
                if (groups.Count > 0)
                    collections.Add($"Assembly Groups ({groups.Count})", groups);
            }
            catch { }
#endif

            return collections;
        }
    }
}

