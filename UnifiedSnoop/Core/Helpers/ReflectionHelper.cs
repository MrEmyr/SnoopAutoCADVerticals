// ReflectionHelper.cs - Helper class for safe reflection-based property extraction
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using UnifiedSnoop.Core.Data;


#if NET8_0_OR_GREATER
#pragma warning disable CS8600, CS8601, CS8602, CS8603, CS8604
#endif

namespace UnifiedSnoop.Core.Helpers
{
    /// <summary>
    /// Helper class for extracting property information using reflection.
    /// Provides safe property access with error handling.
    /// </summary>
    public static class ReflectionHelper
    {
        #region Constants

        private const int MAX_COLLECTION_DISPLAY_COUNT = 100;
        private const int MAX_STRING_DISPLAY_LENGTH = 500;

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets all properties from an object using reflection.
        /// </summary>
        /// <param name="obj">The object to inspect. Cannot be null.</param>
        /// <param name="trans">The active transaction for database operations. Cannot be null.</param>
        /// <returns>A list of PropertyData objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="obj"/> or <paramref name="trans"/> is null.
        /// </exception>
        public static List<PropertyData> GetProperties(object obj, Transaction trans)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            var properties = new List<PropertyData>();
            Type type = obj.GetType();

            // Get all public instance properties
            PropertyInfo[] propInfos = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propInfo in propInfos)
            {
                PropertyData propData = ExtractPropertyData(obj, propInfo, trans);
                properties.Add(propData);
            }

            return properties;
        }

        /// <summary>
        /// Extracts property data from a single property.
        /// </summary>
        /// <param name="obj">The object containing the property.</param>
        /// <param name="propInfo">The property information.</param>
        /// <param name="trans">The active transaction for database operations.</param>
        /// <returns>A PropertyData object containing the extracted information.</returns>
        private static PropertyData ExtractPropertyData(object obj, PropertyInfo propInfo, Transaction trans)
        {
            var propData = new PropertyData
            {
                Name = propInfo.Name,
                Type = propInfo.PropertyType.Name,
                DeclaringType = propInfo.DeclaringType?.FullName
            };

            try
            {
                // Skip indexed properties (e.g., this[int index])
                if (propInfo.GetIndexParameters().Length > 0)
                {
                    propData.Value = "[Indexed Property]";
                    return propData;
                }

                // Get the property value
                object value = propInfo.GetValue(obj, null);
                propData.RawValue = value;

                // Format the value for display
                propData.Value = FormatValue(value, trans);

                // Check if this is a collection
                propData.IsCollection = IsCollection(value);
            }
            catch (TargetInvocationException ex)
            {
                // Property getter threw an exception
                propData.HasError = true;
                #if NET8_0_OR_GREATER
                propData.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
                #else
                propData.ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                #endif
                propData.Value = $"[Error: {propData.ErrorMessage}]";
            }
            catch (Exception ex)
            {
                // Other reflection errors
                propData.HasError = true;
                propData.ErrorMessage = ex.Message;
                propData.Value = $"[Error: {ex.Message}]";
            }

            return propData;
        }

        /// <summary>
        /// Formats a value for display.
        /// </summary>
        /// <param name="value">The value to format.</param>
        /// <param name="trans">The active transaction for database operations.</param>
        /// <returns>A formatted string representation of the value.</returns>
        public static string FormatValue(object value, Transaction trans)
        {
            if (value == null)
                return "[null]";

            try
            {
                // Handle ObjectId
                if (value is ObjectId objId)
                {
                    return FormatObjectId(objId, trans);
                }

                // Handle Point3d
                if (value is Point3d pt3d)
                {
                    return $"({pt3d.X:F4}, {pt3d.Y:F4}, {pt3d.Z:F4})";
                }

                // Handle Point2d
                if (value is Point2d pt2d)
                {
                    return $"({pt2d.X:F4}, {pt2d.Y:F4})";
                }

                // Handle Vector3d
                if (value is Vector3d vec3d)
                {
                    return $"({vec3d.X:F4}, {vec3d.Y:F4}, {vec3d.Z:F4})";
                }

                // Handle collections
                if (value is IEnumerable enumerable && !(value is string))
                {
                    return FormatCollection(enumerable);
                }

                // Handle strings
                if (value is string str)
                {
                    if (str.Length > MAX_STRING_DISPLAY_LENGTH)
                    {
                        return str.Substring(0, MAX_STRING_DISPLAY_LENGTH) + "...";
                    }
                    return str;
                }

                // Handle enums
                if (value.GetType().IsEnum)
                {
                    return value.ToString();
                }

                // Handle DBObject references
                if (value is DBObject dbObj)
                {
                    return $"{dbObj.GetType().Name} [{dbObj.ObjectId}]";
                }

                // Default: use ToString()
                string result = value.ToString();
                #if NET8_0_OR_GREATER
                if (result != null && result.Length > MAX_STRING_DISPLAY_LENGTH)
                #else
                if (result.Length > MAX_STRING_DISPLAY_LENGTH)
                #endif
                {
                    return result.Substring(0, MAX_STRING_DISPLAY_LENGTH) + "...";
                }
                
                #if NET8_0_OR_GREATER
                return result ?? "[null]";
                #else
                return result;
                #endif
            }
            catch (Exception ex)
            {
                return $"[Error formatting value: {ex.Message}]";
            }
        }

        /// <summary>
        /// Formats an ObjectId for display.
        /// </summary>
        /// <param name="objId">The ObjectId to format.</param>
        /// <param name="trans">The active transaction for database operations.</param>
        /// <returns>A formatted string representation of the ObjectId.</returns>
        private static string FormatObjectId(ObjectId objId, Transaction trans)
        {
            if (objId.IsNull)
                return "[Null ObjectId]";

            if (!objId.IsValid)
                return $"[Invalid ObjectId: {objId.Handle}]";

            try
            {
                // IMPORTANT: Use OpenMode.ForRead for inspection (RULE 3.1)
                DBObject dbObj = trans.GetObject(objId, OpenMode.ForRead);
                string typeName = dbObj.GetType().Name;
                
                // Try to get a meaningful name if available
                string name = string.Empty;
                if (dbObj is SymbolTableRecord symRec && !string.IsNullOrEmpty(symRec.Name))
                {
                    name = $" \"{symRec.Name}\"";
                }
                
                return $"{typeName}{name} [{objId.Handle}]";
            }
            catch
            {
                return $"[ObjectId: {objId.Handle}]";
            }
        }

        /// <summary>
        /// Formats a collection for display.
        /// </summary>
        /// <param name="enumerable">The collection to format.</param>
        /// <returns>A formatted string representation of the collection.</returns>
        private static string FormatCollection(IEnumerable enumerable)
        {
            try
            {
                int count = 0;
                foreach (var item in enumerable)
                {
                    count++;
                    if (count > MAX_COLLECTION_DISPLAY_COUNT)
                    {
                        return $"[Collection: {count}+ items]";
                    }
                }
                return $"[Collection: {count} items]";
            }
            catch
            {
                return "[Collection]";
            }
        }

        /// <summary>
        /// Determines whether a value is a collection.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>true if the value is a collection; otherwise, false.</returns>
        public static bool IsCollection(object value)
        {
            if (value == null)
                return false;

            // String is enumerable but not considered a collection for our purposes
            if (value is string)
                return false;

            return value is IEnumerable;
        }

        /// <summary>
        /// Gets collections from an object.
        /// </summary>
        /// <param name="obj">The object to inspect.</param>
        /// <param name="trans">The active transaction for database operations.</param>
        /// <returns>A dictionary of collection names and their enumerable values.</returns>
        public static Dictionary<string, IEnumerable> GetCollections(object obj, Transaction trans)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));
            if (trans == null)
                throw new ArgumentNullException(nameof(trans));

            var collections = new Dictionary<string, IEnumerable>();
            Type type = obj.GetType();

            PropertyInfo[] propInfos = type.GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo propInfo in propInfos)
            {
                try
                {
                    // Skip indexed properties
                    if (propInfo.GetIndexParameters().Length > 0)
                        continue;

                    object value = propInfo.GetValue(obj, null);

                    if (IsCollection(value))
                    {
                        collections[propInfo.Name] = (IEnumerable)value;
                    }
                }
                catch
                {
                    // Ignore properties that throw exceptions
                    continue;
                }
            }

            return collections;
        }

        /// <summary>
        /// Gets the display name of an object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The name of the object.</returns>
        public static string GetObjectName(object obj)
        {
            if (obj == null)
                return "[null]";

            // Try to get a 'Name' property
            var nameProp = obj.GetType().GetProperty("Name");
            if (nameProp != null && nameProp.PropertyType == typeof(string))
            {
                var name = nameProp.GetValue(obj) as string;
                if (!string.IsNullOrEmpty(name))
                    return name;
            }

            // For DBObject, try to include handle
            if (obj is Autodesk.AutoCAD.DatabaseServices.DBObject dbObj)
            {
                return $"{obj.GetType().Name} [{dbObj.Handle}]";
            }

            // Fallback to type name
            return obj.GetType().Name;
        }

        #endregion
    }
}

