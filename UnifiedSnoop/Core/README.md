# Core Layer

Core infrastructure for data collection and object management.

## Subfolders:

### **Collectors/**
- `ICollector.cs` - Main collector interface
- `ReflectionCollector.cs` - Reflection-based collection (Civil3DSnoop style)
- `PropertyCollector.cs` - Property extraction
- `MethodCollector.cs` - Method discovery

### **Data/**
- `PropertyData.cs` - Property information model
- `ObjectNode.cs` - Tree node data model
- `CollectionInfo.cs` - Collection metadata
- `TreeNodeData.cs` - UI tree data

### **Helpers/**
- `TransactionHelper.cs` - Transaction management
- `ObjectHelper.cs` - Object utilities
- `ReflectionHelper.cs` - Reflection utilities
- `FormatHelper.cs` - Value formatting

## Key Interfaces:

```csharp
public interface ICollector
{
    string Name { get; }
    bool CanCollect(object obj);
    List<PropertyData> Collect(object obj, Transaction trans);
    Dictionary<string, IEnumerable> GetCollections(object obj, Transaction trans);
}
```

## Design Patterns:
- **Strategy Pattern:** Collectors
- **Factory Pattern:** Object creation
- **Dispose Pattern:** Resource management

## Next Steps:
1. Define ICollector interface
2. Create PropertyData model
3. Implement TransactionHelper
4. Create ReflectionCollector
5. Add helper utilities

