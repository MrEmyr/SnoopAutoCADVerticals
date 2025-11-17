# Services Layer

Business logic and orchestration services.

## Services to Create:

### **1. ObjectDiscoveryService.cs**
Discovers and organizes database objects.

**Methods:**
- `BuildDatabaseTree()` - AutoCAD database structure
- `BuildCivil3DTree()` - Civil 3D object structure
- `FindEntities()` - Selection set processing
- `NavigateCollections()` - Collection traversal

### **2. PropertyExtractionService.cs**
Extracts and formats object properties.

**Methods:**
- `ExtractProperties()` - Get all properties
- `ExtractCollections()` - Get collections
- `FormatValue()` - Format for display
- `HandleSpecialTypes()` - ObjectId, enums, etc.

### **3. PlatformDetectionService.cs**
Detects AutoCAD vs Civil 3D environment.

**Methods:**
- `IsCivil3DAvailable()` - Check for Civil 3D
- `IsCivil3DDocument()` - Check if doc is Civil 3D
- `GetAvailableFeatures()` - Feature detection
- `LoadPlatformSpecificCollectors()` - Dynamic loading

## Service Characteristics:
- **Stateless** - No instance state
- **Testable** - Easy to unit test
- **Reusable** - Used by multiple layers

## Dependencies:
- Core.Collectors
- Core.Data
- Core.Helpers
- Autodesk APIs

## Next Steps:
1. Create PlatformDetectionService
2. Implement ObjectDiscoveryService
3. Build PropertyExtractionService
4. Add caching mechanisms
5. Implement error handling

