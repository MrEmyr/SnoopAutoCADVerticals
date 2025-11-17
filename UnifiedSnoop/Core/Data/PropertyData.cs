// PropertyData.cs - Core data model for property information
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace UnifiedSnoop.Core.Data
{
    /// <summary>
    /// Represents property information collected from an object.
    /// This class is version-compatible across .NET Framework 4.8 and .NET 8.0.
    /// </summary>
    public class PropertyData
    {
        #region Properties

        #if NET8_0_OR_GREATER
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the property type name.
        /// </summary>
        [NotNull]
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the formatted string value of the property.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets the raw (unformatted) property value.
        /// </summary>
        public object? RawValue { get; set; }

        /// <summary>
        /// Gets or sets the full name of the type that declares this property.
        /// </summary>
        public string? DeclaringType { get; set; }

        /// <summary>
        /// Gets or sets the category for organizing properties in the UI.
        /// </summary>
        public string? Category { get; set; }
        #else
        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the property type name.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the formatted string value of the property.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the raw (unformatted) property value.
        /// </summary>
        public object RawValue { get; set; }

        /// <summary>
        /// Gets or sets the full name of the type that declares this property.
        /// </summary>
        public string DeclaringType { get; set; }

        /// <summary>
        /// Gets or sets the category for organizing properties in the UI.
        /// </summary>
        public string Category { get; set; }
        #endif

        /// <summary>
        /// Gets or sets a value indicating whether this property represents a collection.
        /// </summary>
        public bool IsCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an error occurred while accessing this property.
        /// </summary>
        public bool HasError { get; set; }

        /// <summary>
        /// Gets or sets the error message if HasError is true.
        /// </summary>
        #if NET8_0_OR_GREATER
        public string? ErrorMessage { get; set; }
        #else
        public string ErrorMessage { get; set; }
        #endif

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyData"/> class.
        /// </summary>
        public PropertyData()
        {
            #if NET48
            Name = string.Empty;
            Type = string.Empty;
            Value = string.Empty;
            DeclaringType = string.Empty;
            #endif
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a string representation of this property data.
        /// </summary>
        /// <returns>A string containing the property name and value.</returns>
        public override string ToString()
        {
            if (HasError)
            {
                return $"{Name ?? "[Unknown]"}: [Error: {ErrorMessage ?? "Unknown error"}]";
            }

            return $"{Name ?? "[Unknown]"} = {Value ?? "[Empty]"}";
        }

        #endregion
    }
}

