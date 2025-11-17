# Inspectors Layer

Type-specific object inspectors for detailed analysis.

## Subfolders:

### **AutoCAD/**
AutoCAD object inspectors:
- `DatabaseInspector.cs`
- `EntityInspector.cs`
- `BlockTableInspector.cs`
- `LayerTableInspector.cs`
- `DimensionStyleInspector.cs`
- `TextStyleInspector.cs`
- `LinetypeInspector.cs`
- More as needed...

### **Civil3D/**
Civil 3D object inspectors:
- `CivilDocumentInspector.cs`
- `AlignmentInspector.cs`
- `ProfileInspector.cs`
- `SurfaceInspector.cs`
- `CorridorInspector.cs`
- `AssemblyInspector.cs`
- `PipeNetworkInspector.cs`
- `PointGroupInspector.cs`
- More as needed...

## Inspector Interface:

```csharp
public interface IInspector
{
    bool CanInspect(object obj);
    List<PropertyData> Inspect(object obj, Transaction trans);
    string GetDisplayName(object obj);
}
```

## Purpose:
- **Enhanced Display** - Better formatting than raw reflection
- **Type-Specific Logic** - Handle special cases
- **Additional Data** - Add computed properties
- **Better Organization** - Group related properties

## Pattern:
Each inspector augments reflection data with:
- Custom formatting
- Related object links
- Computed values
- Better descriptions

## Next Steps:
1. Create IInspector interface
2. Implement base AutoCAD inspectors
3. Add Civil 3D inspectors
4. Register with collector system
5. Add inspector discovery

