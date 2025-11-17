// ObjectNode.cs - Tree node data model for hierarchical object display
// Supports both .NET Framework 4.8 (AutoCAD 2024) and .NET 8.0 (AutoCAD 2025+)

using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;

#if NET8_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace UnifiedSnoop.Core.Data
{
    /// <summary>
    /// Represents a node in the object tree hierarchy.
    /// Used for displaying objects in a TreeView control.
    /// </summary>
    public class ObjectNode
    {
        #region Fields

        #if NET8_0_OR_GREATER
        private List<ObjectNode>? _children;
        #else
        private List<ObjectNode> _children;
        #endif

        #endregion

        #region Properties

        #if NET8_0_OR_GREATER
        /// <summary>
        /// Gets or sets the display name for this node.
        /// </summary>
        [NotNull]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the object associated with this node.
        /// </summary>
        public object? Object { get; set; }

        /// <summary>
        /// Gets or sets the ObjectId if this node represents a database object.
        /// </summary>
        public ObjectId? ObjectId { get; set; }
        #else
        /// <summary>
        /// Gets or sets the display name for this node.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the object associated with this node.
        /// </summary>
        public object Object { get; set; }

        /// <summary>
        /// Gets or sets the ObjectId if this node represents a database object.
        /// </summary>
        public ObjectId ObjectId { get; set; }
        #endif

        /// <summary>
        /// Gets or sets a value indicating whether this node represents a collection.
        /// </summary>
        public bool IsCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this node has been expanded.
        /// Used for lazy loading of child nodes.
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Gets the child nodes. Children are lazy-loaded.
        /// </summary>
        public List<ObjectNode> Children
        {
            get
            {
                #if NET8_0_OR_GREATER
                return _children ??= new List<ObjectNode>();
                #else
                if (_children == null)
                {
                    _children = new List<ObjectNode>();
                }
                return _children;
                #endif
            }
        }

        /// <summary>
        /// Gets a value indicating whether this node has children.
        /// </summary>
        public bool HasChildren
        {
            get
            {
                #if NET8_0_OR_GREATER
                return _children != null && _children.Count > 0;
                #else
                return _children != null && _children.Count > 0;
                #endif
            }
        }

        /// <summary>
        /// Gets or sets additional tag data for this node.
        /// </summary>
        #if NET8_0_OR_GREATER
        public object? Tag { get; set; }
        #else
        public object Tag { get; set; }
        #endif

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNode"/> class.
        /// </summary>
        public ObjectNode()
        {
            #if NET48
            Name = string.Empty;
            ObjectId = Autodesk.AutoCAD.DatabaseServices.ObjectId.Null;
            #endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNode"/> class with a name.
        /// </summary>
        /// <param name="name">The display name for this node.</param>
        public ObjectNode(string name) : this()
        {
            #if NET8_0_OR_GREATER
            Name = name ?? throw new ArgumentNullException(nameof(name));
            #else
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            Name = name;
            #endif
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectNode"/> class with a name and object.
        /// </summary>
        /// <param name="name">The display name for this node.</param>
        /// <param name="obj">The object associated with this node.</param>
        public ObjectNode(string name, object obj) : this(name)
        {
            Object = obj;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds a child node to this node.
        /// </summary>
        /// <param name="child">The child node to add.</param>
        /// <exception cref="ArgumentNullException">Thrown when child is null.</exception>
        public void AddChild(ObjectNode child)
        {
            if (child == null)
                throw new ArgumentNullException(nameof(child));

            Children.Add(child);
        }

        /// <summary>
        /// Clears all child nodes.
        /// </summary>
        public void ClearChildren()
        {
            #if NET8_0_OR_GREATER
            _children?.Clear();
            #else
            if (_children != null)
            {
                _children.Clear();
            }
            #endif
            IsExpanded = false;
        }

        /// <summary>
        /// Returns a string representation of this node.
        /// </summary>
        /// <returns>The name of this node.</returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}

