// Civil3DProfileCollector.cs - Specialized collector for Civil 3D Profile objects
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
    /// Specialized collector for Civil 3D Profile objects.
    /// </summary>
    public class Civil3DProfileCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D Profile Collector";

        public bool CanCollect(object obj)
        {
#if CIVIL3D
            return obj is Profile;
#else
            return false;
#endif
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            var properties = new List<PropertyData>();

#if CIVIL3D
            if (!(obj is Profile profile))
                return properties;

            try
            {
                // Profile Identity
                properties.Add(new PropertyData
                {
                    Name = "Profile Name",
                    Type = "String",
                    Value = profile.Name ?? "[Unnamed]",
                    Category = "Identity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Description",
                    Type = "String",
                    Value = profile.Description ?? "[None]",
                    Category = "Identity"
                });

                // Profile Type
                properties.Add(new PropertyData
                {
                    Name = "Profile Type",
                    Type = "ProfileType",
                    Value = profile.ProfileType.ToString(),
                    Category = "General"
                });

                // Geometry
                properties.Add(new PropertyData
                {
                    Name = "Start Station",
                    Type = "Double",
                    Value = $"{profile.StartingStation:F4}",
                    Category = "Geometry"
                });

                properties.Add(new PropertyData
                {
                    Name = "End Station",
                    Type = "Double",
                    Value = $"{profile.EndingStation:F4}",
                    Category = "Geometry"
                });

                double length = profile.EndingStation - profile.StartingStation;
                properties.Add(new PropertyData
                {
                    Name = "Length",
                    Type = "Double",
                    Value = $"{length:F4}",
                    Category = "Geometry"
                });

                // Elevation Info
                try
                {
                    double minElev = profile.ElevationMin;
                    double maxElev = profile.ElevationMax;

                    properties.Add(new PropertyData
                    {
                        Name = "Minimum Elevation",
                        Type = "Double",
                        Value = $"{minElev:F4}",
                        Category = "Elevation"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "Maximum Elevation",
                        Type = "Double",
                        Value = $"{maxElev:F4}",
                        Category = "Elevation"
                    });

                    properties.Add(new PropertyData
                    {
                        Name = "Elevation Range",
                        Type = "Double",
                        Value = $"{maxElev - minElev:F4}",
                        Category = "Elevation"
                    });
                }
                catch { }

                // Style
                properties.Add(new PropertyData
                {
                    Name = "Style Name",
                    Type = "String",
                    Value = profile.StyleName ?? "[Default]",
                    Category = "Style"
                });

                properties.Add(new PropertyData
                {
                    Name = "Style Id",
                    Type = "ObjectId",
                    Value = profile.StyleId.ToString(),
                    Category = "Style"
                });

                // Parent Alignment
                try
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Alignment Id",
                        Type = "ObjectId",
                        Value = profile.AlignmentId.ToString(),
                        Category = "References"
                    });
                }
                catch { }

                // Object Properties
                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = profile.Handle.ToString(),
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "ObjectId",
                    Type = "ObjectId",
                    Value = profile.ObjectId.ToString(),
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting Profile properties: {ex.Message}",
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
            if (!(obj is Profile profile))
                return collections;

            try
            {
                // Profile Entities (PVIs, etc.) can be added here if needed
                // For now, returning empty collections
            }
            catch { }
#endif

            return collections;
        }
    }
}

