// Civil3DProfileViewCollector.cs - Specialized collector for Civil 3D ProfileView objects
// Supports both .NET Framework 4.8 (Civil 3D 2024) and .NET 8.0 (Civil 3D 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;

#if CIVIL3D
using Autodesk.Civil.DatabaseServices;
#endif

namespace UnifiedSnoop.Inspectors.Civil3D
{
    /// <summary>
    /// Specialized collector for Civil 3D ProfileView objects.
    /// </summary>
    public class Civil3DProfileViewCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "Civil 3D ProfileView Collector";

        public bool CanCollect(object obj)
        {
#if CIVIL3D
            return obj is ProfileView;
#else
            return false;
#endif
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            var properties = new List<PropertyData>();

#if CIVIL3D
            if (!(obj is ProfileView profileView))
                return properties;

            try
            {
                // Identity
                properties.Add(new PropertyData
                {
                    Name = "Profile View Name",
                    Type = "String",
                    Value = profileView.Name ?? "[Unnamed]",
                    Category = "Identity"
                });

                properties.Add(new PropertyData
                {
                    Name = "Description",
                    Type = "String",
                    Value = profileView.Description ?? "[None]",
                    Category = "Identity"
                });

                // Location
                properties.Add(new PropertyData
                {
                    Name = "Location",
                    Type = "Point3d",
                    Value = $"({profileView.Location.X:F4}, {profileView.Location.Y:F4}, {profileView.Location.Z:F4})",
                    Category = "Geometry"
                });

                // Station Range
                properties.Add(new PropertyData
                {
                    Name = "Start Station",
                    Type = "Double",
                    Value = $"{profileView.StationStart:F4}",
                    Category = "Stations"
                });

                properties.Add(new PropertyData
                {
                    Name = "End Station",
                    Type = "Double",
                    Value = $"{profileView.StationEnd:F4}",
                    Category = "Stations"
                });

                double stationRange = profileView.StationEnd - profileView.StationStart;
                properties.Add(new PropertyData
                {
                    Name = "Station Range",
                    Type = "Double",
                    Value = $"{stationRange:F4}",
                    Category = "Stations"
                });

                // Elevation Range
                properties.Add(new PropertyData
                {
                    Name = "Elevation Min",
                    Type = "Double",
                    Value = $"{profileView.ElevationMin:F4}",
                    Category = "Elevations"
                });

                properties.Add(new PropertyData
                {
                    Name = "Elevation Max",
                    Type = "Double",
                    Value = $"{profileView.ElevationMax:F4}",
                    Category = "Elevations"
                });

                double elevRange = profileView.ElevationMax - profileView.ElevationMin;
                properties.Add(new PropertyData
                {
                    Name = "Elevation Range",
                    Type = "Double",
                    Value = $"{elevRange:F4}",
                    Category = "Elevations"
                });

                // Style
                properties.Add(new PropertyData
                {
                    Name = "Style Name",
                    Type = "String",
                    Value = profileView.StyleName ?? "[Default]",
                    Category = "Style"
                });

                properties.Add(new PropertyData
                {
                    Name = "Style Id",
                    Type = "ObjectId",
                    Value = profileView.StyleId.ToString(),
                    Category = "Style"
                });

                // References
                try
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Alignment Id",
                        Type = "ObjectId",
                        Value = profileView.AlignmentId.ToString(),
                        Category = "References"
                    });
                }
                catch { }

                // Layer
                properties.Add(new PropertyData
                {
                    Name = "Layer",
                    Type = "String",
                    Value = profileView.Layer ?? "[None]",
                    Category = "Entity"
                });

                // Object Properties
                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = profileView.Handle.ToString(),
                    Category = "Object"
                });

                properties.Add(new PropertyData
                {
                    Name = "ObjectId",
                    Type = "ObjectId",
                    Value = profileView.ObjectId.ToString(),
                    Category = "Object"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting ProfileView properties: {ex.Message}",
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

