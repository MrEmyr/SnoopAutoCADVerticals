// Civil3DPointGroupCollector.cs - Specialized collector for Civil 3D PointGroup objects
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
    /// Specialized collector for Civil 3D PointGroup objects.
    /// </summary>
    public class Civil3DPointGroupCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D PointGroup Collector";

        public bool CanCollect(object obj)
        {
#if CIVIL3D
            return obj != null && obj.GetType().Name == "PointGroup";
#else
            return false;
#endif
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            var properties = new List<PropertyData>();

#if CIVIL3D
            // Using dynamic to handle PointGroup which may not be directly accessible
            try
            {
                dynamic pointGroup = obj;

                // Identity
                properties.Add(new PropertyData
                {
                    Name = "Point Group Name",
                    Type = "String",
                    Value = pointGroup.Name?.ToString() ?? "[Unnamed]",
                    Category = "Identity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Description",
                    Type = "String",
                    Value = pointGroup.Description?.ToString() ?? "[None]",
                    Category = "Identity"
                });

                // Point Count
                try
                {
                    var pointCount = pointGroup.GetPointNumbers();
                    if (pointCount != null)
                    {
                        int count = 0;
                        foreach (var pt in pointCount)
                            count++;

                        properties.Add(new PropertyData
                        {
                            Name = "Number of Points",
                            Type = "Int32",
                            Value = count.ToString(),
                            Category = "Statistics"
                        });
                    }
                }
                catch (System.Exception ex)
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Number of Points",
                        Type = "String",
                        Value = $"[Error: {ex.Message}]",
                        Category = "Statistics"
                    });
                }

                // Query Builder Info
                try
                {
                    var queryBuilder = pointGroup.QueryBuilder;
                    if (queryBuilder != null)
                    {
                        properties.Add(new PropertyData
                        {
                            Name = "Has Query Builder",
                            Type = "Boolean",
                            Value = "True",
                            Category = "Query"
                        });
                    }
                }
                catch { }

                // Object Properties
                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = pointGroup.Handle?.ToString() ?? "[Unknown]",
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "ObjectId",
                    Type = "ObjectId",
                    Value = pointGroup.ObjectId?.ToString() ?? "[Unknown]",
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting PointGroup properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }
#endif

            return properties;
        }

        public Dictionary<string, System.Collections.IEnumerable> GetCollections(object obj, Transaction trans)
        {
            return new Dictionary<string, System.Collections.IEnumerable>();
        }
    }
}

