// XRecordCollector.cs - Specialized collector for AutoCAD XRecord objects
// XRecords store custom application data as typed values (ResultBuffer)
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using UnifiedSnoop.Core.Collectors;
using UnifiedSnoop.Core.Data;

#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
#endif

namespace UnifiedSnoop.Inspectors.AutoCAD
{
    /// <summary>
    /// Specialized collector for AutoCAD XRecord objects.
    /// XRecords store custom application data as typed values in a ResultBuffer.
    /// </summary>
    public class XRecordCollector : ICollector
    {
        /// <summary>
        /// Gets the name of this collector.
        /// </summary>
        public string Name => "AutoCAD XRecord Collector";

        public bool CanCollect(object obj)
        {
            return obj is Xrecord;
        }

        public List<PropertyData> Collect(object obj, Transaction trans)
        {
            if (!(obj is Xrecord xrec))
                return new List<PropertyData>();

            var properties = new List<PropertyData>();

            try
            {
                // Add basic XRecord information
                properties.Add(new PropertyData
                {
                    Name = "Object Type",
                    Type = "String",
                    Value = "XRecord (Custom Application Data)",
                    Category = "General"
                });

                properties.Add(new PropertyData
                {
                    Name = "Handle",
                    Type = "Handle",
                    Value = xrec.Handle.ToString(),
                    Category = "General"
                });

                // Get the data stored in the XRecord
                ResultBuffer data = xrec.Data;
                
                if (data != null)
                {
                    int count = 0;
                    var dataList = new List<TypedValue>();
                    
                    // Count and collect all typed values
                    foreach (TypedValue tv in data)
                    {
                        dataList.Add(tv);
                        count++;
                    }

                    properties.Add(new PropertyData
                    {
                        Name = "Data Entry Count",
                        Type = "Int32",
                        Value = count.ToString(),
                        Category = "Data"
                    });

                    // Add section header
                    if (count > 0)
                    {
                        properties.Add(new PropertyData
                        {
                            Name = "--- XRecord Data ---",
                            Type = "Section",
                            Value = $"{count} entries",
                            Category = "Data"
                        });

                        // Display each typed value with detailed formatting
                        int index = 0;
                        foreach (TypedValue tv in dataList)
                        {
                            AddTypedValueProperty(properties, tv, index);
                            index++;
                        }
                    }
                }
                else
                {
                    properties.Add(new PropertyData
                    {
                        Name = "Data",
                        Type = "ResultBuffer",
                        Value = "[Empty - No data stored]",
                        Category = "Data"
                    });
                }

            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = "Error",
                    Type = "Exception",
                    Value = $"Error collecting XRecord properties: {ex.Message}",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }

            return properties;
        }

        /// <summary>
        /// Gets collections from an XRecord (typically none, but included for interface compliance).
        /// </summary>
        public Dictionary<string, IEnumerable> GetCollections(object obj, Transaction trans)
        {
            // XRecords don't typically have child collections
            return new Dictionary<string, IEnumerable>();
        }

        #region Private Helper Methods

        /// <summary>
        /// Adds a property entry for a TypedValue, formatted based on its DXF code.
        /// </summary>
        private void AddTypedValueProperty(List<PropertyData> properties, TypedValue tv, int index)
        {
            short code = tv.TypeCode;
            object value = tv.Value;
            string formattedValue;
            string typeName;

            try
            {
                // Format the value based on DXF code ranges
                // Reference: AutoCAD DXF Reference Guide
                
                if (code == 0)
                {
                    // Entity type (string)
                    typeName = "String (Entity Type)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (((code >= 1) && (code <= 4)) || ((code >= 6) && (code <= 9)))
                {
                    // Text strings
                    typeName = "String";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code == 5 || code == 105)
                {
                    // Handle (string)
                    typeName = "Handle";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if ((code >= 10) && (code <= 18))
                {
                    // Point (3 reals)
                    typeName = "Point3d";
                    if (value is Point3d pt)
                    {
                        formattedValue = $"({pt.X:F4}, {pt.Y:F4}, {pt.Z:F4})";
                    }
                    else if (value is Point2d pt2d)
                    {
                        formattedValue = $"({pt2d.X:F4}, {pt2d.Y:F4})";
                    }
                    else
                    {
                        formattedValue = value?.ToString() ?? "[null]";
                    }
                }
                else if ((code >= 38) && (code <= 59))
                {
                    // Double precision floating point
                    typeName = "Double";
                    if (value is double dblVal)
                    {
                        formattedValue = $"{dblVal:F6}";
                    }
                    else
                    {
                        formattedValue = value?.ToString() ?? "[null]";
                    }
                }
                else if ((code >= 60) && (code <= 79))
                {
                    // 16-bit integer
                    typeName = "Int16";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if ((code >= 90) && (code <= 99))
                {
                    // 32-bit integer
                    typeName = "Int32";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 100 && code <= 102)
                {
                    // String (subclass marker, etc.)
                    typeName = "String";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 140 && code <= 147)
                {
                    // Double precision scalar
                    typeName = "Double";
                    if (value is double dblVal)
                    {
                        formattedValue = $"{dblVal:F6}";
                    }
                    else
                    {
                        formattedValue = value?.ToString() ?? "[null]";
                    }
                }
                else if (code >= 170 && code <= 175)
                {
                    // 16-bit integer
                    typeName = "Int16";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 280 && code <= 289)
                {
                    // 8-bit integer
                    typeName = "Int8";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code == 290 || code == 292 || code == 293)
                {
                    // Boolean
                    typeName = "Boolean";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 300 && code <= 319)
                {
                    // Arbitrary text string
                    typeName = "String";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 320 && code <= 329)
                {
                    // Arbitrary handle
                    typeName = "Handle";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 330 && code <= 369)
                {
                    // Soft-pointer handle
                    typeName = "Handle (Soft Pointer)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 370 && code <= 379)
                {
                    // 16-bit integer (lineweight, etc.)
                    typeName = "Int16";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 380 && code <= 389)
                {
                    // 16-bit integer (plot style, etc.)
                    typeName = "Int16";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 390 && code <= 399)
                {
                    // String (plot style handle, etc.)
                    typeName = "String/Handle";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 400 && code <= 409)
                {
                    // 16-bit integer
                    typeName = "Int16";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 410 && code <= 419)
                {
                    // String
                    typeName = "String";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 420 && code <= 429)
                {
                    // 32-bit integer (color)
                    typeName = "Int32 (Color)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 430 && code <= 439)
                {
                    // String (color name)
                    typeName = "String (Color Name)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 440 && code <= 449)
                {
                    // 32-bit integer (transparency)
                    typeName = "Int32 (Transparency)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 450 && code <= 459)
                {
                    // 32-bit integer
                    typeName = "Int32";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 460 && code <= 469)
                {
                    // Double precision
                    typeName = "Double";
                    if (value is double dblVal)
                    {
                        formattedValue = $"{dblVal:F6}";
                    }
                    else
                    {
                        formattedValue = value?.ToString() ?? "[null]";
                    }
                }
                else if (code >= 470 && code <= 479)
                {
                    // String
                    typeName = "String";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code == 1000)
                {
                    // Extended data string
                    typeName = "String (XData)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code == 1001)
                {
                    // Extended data application name
                    typeName = "String (XData AppName)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 1002 && code <= 1009)
                {
                    // Extended data control string
                    typeName = "String (XData Control)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else if (code >= 1010 && code <= 1013)
                {
                    // Extended data point
                    typeName = "Point3d (XData)";
                    if (value is Point3d pt)
                    {
                        formattedValue = $"({pt.X:F4}, {pt.Y:F4}, {pt.Z:F4})";
                    }
                    else
                    {
                        formattedValue = value?.ToString() ?? "[null]";
                    }
                }
                else if (code >= 1040 && code <= 1042)
                {
                    // Extended data double
                    typeName = "Double (XData)";
                    if (value is double dblVal)
                    {
                        formattedValue = $"{dblVal:F6}";
                    }
                    else
                    {
                        formattedValue = value?.ToString() ?? "[null]";
                    }
                }
                else if (code >= 1070 && code <= 1071)
                {
                    // Extended data integer
                    typeName = "Int16 (XData)";
                    formattedValue = value?.ToString() ?? "[null]";
                }
                else
                {
                    // Unknown or unhandled code
                    typeName = value?.GetType().Name ?? "Unknown";
                    formattedValue = value?.ToString() ?? "[null]";
                }

                properties.Add(new PropertyData
                {
                    Name = $"  [{index}] DXF {code}",
                    Type = typeName,
                    Value = formattedValue,
                    Category = "Data",
                    DeclaringType = "TypedValue"
                });
            }
            catch (System.Exception ex)
            {
                properties.Add(new PropertyData
                {
                    Name = $"  [{index}] DXF {code}",
                    Type = "Error",
                    Value = $"Error parsing value: {ex.Message}",
                    Category = "Data",
                    HasError = true,
                    ErrorMessage = ex.Message
                });
            }
        }

        #endregion
    }
}

