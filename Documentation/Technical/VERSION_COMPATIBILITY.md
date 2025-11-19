# Version Compatibility Guide

**UnifiedSnoop Multi-Version Support**

---

## 📊 **Supported Versions Matrix**

| Product | Version | .NET Framework | NuGet Package | Build Target | Compilation Symbol |
|---------|---------|----------------|---------------|--------------|-------------------|
| **AutoCAD** | 2024 | .NET Framework 4.8 | AutoCAD.NET 24.3.1 | `net48` | `ACAD2024` |
| **AutoCAD** | 2025 | .NET 8.0 | AutoCAD.NET 25.0.1 | `net8.0-windows` | `ACAD2025_OR_GREATER` |
| **AutoCAD** | 2026 | .NET 8.0 | AutoCAD.NET 25.0.1 | `net8.0-windows` | `ACAD2025_OR_GREATER` |
| **Civil 3D** | 2024 | .NET Framework 4.8 | Civil3D.NET 13.6.272 | `net48` | `ACAD2024` |
| **Civil 3D** | 2025 | .NET 8.0 | Civil3D.NET 13.8.280 | `net8.0-windows` | `ACAD2025_OR_GREATER` |
| **Civil 3D** | 2026 | .NET 8.0 | Civil3D.NET 13.8.280 | `net8.0-windows` | `ACAD2025_OR_GREATER` |

---

## 🔄 **The .NET Framework Change**

### **Critical Breaking Change Between 2024 and 2025:**

**2024 Versions:**
- Platform: .NET Framework 4.8
- Runtime: CLR 4.0
- Language: C# 7.3 compatible
- Features: No nullable reference types, no init properties

**2025+ Versions:**
- Platform: .NET 8.0
- Runtime: CoreCLR
- Language: C# 12.0
- Features: Modern C# features available

---

## 🏗️ **Build Configuration**

### **Project File Setup:**

The project uses **multi-targeting** to build separate DLLs for each framework:

```xml
<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>
```

### **Build Output:**

Building the solution produces:

```
bin/
├── x64/
    ├── Debug/
    │   ├── net48/
    │   │   └── UnifiedSnoop.dll      ← For AutoCAD/Civil 3D 2024
    │   └── net8.0-windows/
    │       └── UnifiedSnoop.dll      ← For AutoCAD/Civil 3D 2025+
    └── Release/
        ├── net48/
        │   └── UnifiedSnoop.dll
        └── net8.0-windows/
            └── UnifiedSnoop.dll
```

---

## 💻 **Writing Version-Specific Code**

### **Example 1: Basic Conditional Compilation**

```csharp
using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;

#if ACAD2025_OR_GREATER
using System.Diagnostics.CodeAnalysis;  // .NET 8.0 feature
#endif

namespace UnifiedSnoop.Core
{
    public class Example
    {
        #if NET48
        // .NET Framework 4.8 code (2024)
        public string Name { get; set; }
        #elif NET8_0_OR_GREATER
        // .NET 8.0 code (2025+)
        public required string Name { get; init; }
        #endif
        
        public void ProcessObject(object obj)
        {
            #if ACAD2024
            // AutoCAD 2024 specific code
            Console.WriteLine("Running in AutoCAD 2024");
            #else
            // AutoCAD 2025+ code
            Console.WriteLine("Running in AutoCAD 2025+");
            #endif
        }
    }
}
```

### **Example 2: Nullable Reference Types**

```csharp
public class PropertyData
{
    #if NET8_0_OR_GREATER
    // .NET 8.0: Use nullable reference types
    public string? Name { get; set; }
    public string? Value { get; set; }
    public object? RawValue { get; set; }
    
    [NotNull]
    public string Type { get; set; } = string.Empty;
    #else
    // .NET Framework 4.8: Traditional approach
    public string Name { get; set; }
    public string Value { get; set; }
    public object RawValue { get; set; }
    public string Type { get; set; }
    #endif
}
```

### **Example 3: Modern C# Features**

```csharp
#if NET8_0_OR_GREATER
// Use record (C# 9.0+)
public record PropertyData
{
    public required string Name { get; init; }
    public string? Value { get; init; }
    public bool HasError { get; init; }
}
#else
// Use traditional class
public class PropertyData
{
    public string Name { get; set; }
    public string Value { get; set; }
    public bool HasError { get; set; }
    
    // Manual Equals/GetHashCode for .NET Framework 4.8
    public override bool Equals(object obj)
    {
        if (obj is PropertyData other)
        {
            return Name == other.Name && 
                   Value == other.Value && 
                   HasError == other.HasError;
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return (Name, Value, HasError).GetHashCode();
    }
}
#endif
```

### **Example 4: API Version Detection at Runtime**

```csharp
using Autodesk.AutoCAD.ApplicationServices;

namespace UnifiedSnoop.Services
{
    public static class VersionDetection
    {
        /// <summary>
        /// Gets the AutoCAD version number (24, 25, or 26)
        /// </summary>
        public static int GetAutoCADVersion()
        {
            return Application.Version.Major;
        }
        
        /// <summary>
        /// Determines if running in AutoCAD/Civil 3D 2024
        /// </summary>
        public static bool IsVersion2024()
        {
            return GetAutoCADVersion() == 24;
        }
        
        /// <summary>
        /// Determines if running in AutoCAD/Civil 3D 2025+
        /// </summary>
        public static bool IsVersion2025OrGreater()
        {
            return GetAutoCADVersion() >= 25;
        }
        
        /// <summary>
        /// Gets the .NET Framework being used
        /// </summary>
        public static string GetFrameworkVersion()
        {
            #if NET48
            return ".NET Framework 4.8";
            #elif NET8_0_OR_GREATER
            return ".NET 8.0";
            #else
            return "Unknown";
            #endif
        }
        
        /// <summary>
        /// Gets build information
        /// </summary>
        public static string GetBuildInfo()
        {
            var acadVersion = GetAutoCADVersion();
            var framework = GetFrameworkVersion();
            return $"AutoCAD {acadVersion}xx / {framework}";
        }
    }
}
```

### **Example 5: Graceful Feature Degradation**

```csharp
public class FeatureManager
{
    public void UseOptionalFeature()
    {
        var version = VersionDetection.GetAutoCADVersion();
        
        if (version >= 25)
        {
            // Use 2025+ specific API
            UseModernAPI();
        }
        else
        {
            // Fallback for 2024
            UseLegacyAPI();
        }
    }
    
    private void UseModernAPI()
    {
        #if ACAD2025_OR_GREATER
        // Use new API features available in 2025+
        Console.WriteLine("Using modern API");
        #endif
    }
    
    private void UseLegacyAPI()
    {
        // Compatible with all versions
        Console.WriteLine("Using legacy API");
    }
}
```

---

## 🔌 **API Differences Between Versions**

### **AutoCAD 2024 vs 2025+ API Changes:**

| Feature | AutoCAD 2024 (.NET 4.8) | AutoCAD 2025+ (.NET 8.0) |
|---------|-------------------------|--------------------------|
| **Nullable Types** | Not available | Available |
| **Init Properties** | Not available | Available |
| **Records** | Not available | Available |
| **Pattern Matching** | Limited | Full support |
| **Async/Await** | Older pattern | Modern pattern |

### **Civil 3D API Considerations:**

Most Civil 3D APIs remain compatible, but always check:
- [Civil 3D 2024 API Docs](https://help.autodesk.com/view/CIV3D/2024/ENU/?guid=GUID-DA303320-B66D-4F4F-A4F4-9FBBEC0754E0)
- [Civil 3D 2025 API Docs](https://help.autodesk.com/view/CIV3D/2025/ENU/?guid=GUID-DA303320-B66D-4F4F-A4F4-9FBBEC0754E0)
- [Civil 3D 2026 API Docs](https://help.autodesk.com/view/CIV3D/2026/ENU/?guid=GUID-DA303320-B66D-4F4F-A4F4-9FBBEC0754E0)

---

## 📦 **Deployment Strategy**

### **Bundle Structure:**

```
UnifiedSnoop.bundle/
├── PackageContents.xml
└── Contents/
    ├── NET48/                          ← For AutoCAD/Civil 3D 2024
    │   ├── UnifiedSnoop.dll
    │   └── UnifiedSnoop.deps.json
    └── NET80/                          ← For AutoCAD/Civil 3D 2025+
        ├── UnifiedSnoop.dll
        └── UnifiedSnoop.deps.json
```

### **PackageContents.xml:**

```xml
<?xml version="1.0" encoding="utf-8"?>
<ApplicationPackage 
    SchemaVersion="1.0" 
    ProductType="Application" 
    Name="UnifiedSnoop"
    Description="Unified AutoCAD and Civil 3D Object Inspector"
    Author="Your Name"
    AppVersion="1.0.0"
    ProductCode="{YOUR-GUID-HERE}"
    UpgradeCode="{YOUR-UPGRADE-GUID-HERE}">

  <CompanyDetails 
    Name="Your Company" 
    Url="http://yourwebsite.com" 
    Email="support@yourcompany.com" />

  <!-- AutoCAD/Civil 3D 2024 (.NET Framework 4.8) -->
  <Components Description="UnifiedSnoop for AutoCAD 2024">
    <RuntimeRequirements 
      OS="Win64" 
      Platform="AutoCAD|Civil3D"
      SeriesMin="R24.0" 
      SeriesMax="R24.99" />
    <ComponentEntry 
      AppName="UnifiedSnoop" 
      Version="1.0.0" 
      ModuleName="./NET48/UnifiedSnoop.dll"
      AppDescription="UnifiedSnoop for AutoCAD 2024" />
  </Components>

  <!-- AutoCAD/Civil 3D 2025+ (.NET 8.0) -->
  <Components Description="UnifiedSnoop for AutoCAD 2025+">
    <RuntimeRequirements 
      OS="Win64" 
      Platform="AutoCAD|Civil3D"
      SeriesMin="R25.0" />
    <ComponentEntry 
      AppName="UnifiedSnoop" 
      Version="1.0.0" 
      ModuleName="./NET80/UnifiedSnoop.dll"
      AppDescription="UnifiedSnoop for AutoCAD 2025+" />
  </Components>

</ApplicationPackage>
```

---

## 🧪 **Testing Matrix**

### **Required Test Configurations:**

| Configuration | Product | Version | .NET | Status |
|---------------|---------|---------|------|--------|
| Config 1 | AutoCAD | 2024 | 4.8 | ⬜ Not Tested |
| Config 2 | AutoCAD | 2025 | 8.0 | ⬜ Not Tested |
| Config 3 | AutoCAD | 2026 | 8.0 | ⬜ Not Tested |
| Config 4 | Civil 3D | 2024 | 4.8 | ⬜ Not Tested |
| Config 5 | Civil 3D | 2025 | 8.0 | ⬜ Not Tested |
| Config 6 | Civil 3D | 2026 | 8.0 | ⬜ Not Tested |

### **Test Checklist Per Version:**

- [ ] DLL loads successfully
- [ ] Commands execute without errors
- [ ] Context menu appears
- [ ] Main form displays
- [ ] Can inspect database objects
- [ ] Can inspect entities
- [ ] Collections expand correctly
- [ ] Properties display accurately
- [ ] No memory leaks
- [ ] Performance acceptable

---

## 🔧 **Build Commands**

### **Build for All Targets:**
```bash
dotnet build UnifiedSnoop.csproj -c Release
```

### **Build for Specific Target:**
```bash
# Build for 2024 only
dotnet build UnifiedSnoop.csproj -c Release -f net48

# Build for 2025+ only
dotnet build UnifiedSnoop.csproj -c Release -f net8.0-windows
```

### **Clean Build:**
```bash
dotnet clean UnifiedSnoop.csproj
dotnet build UnifiedSnoop.csproj -c Release
```

---

## 📝 **Best Practices**

### **✅ DO:**

1. **Always test both builds:**
   - Build and test `net48` build in AutoCAD 2024
   - Build and test `net8.0-windows` build in AutoCAD 2025+

2. **Use conditional compilation:**
   ```csharp
   #if NET48
   // 2024 specific code
   #elif NET8_0_OR_GREATER
   // 2025+ specific code
   #endif
   ```

3. **Check API documentation for each version:**
   - Don't assume APIs are the same across versions

4. **Provide fallbacks:**
   - If a feature isn't available in 2024, provide alternative

5. **Runtime version detection:**
   - Use `VersionDetection.GetAutoCADVersion()` for runtime decisions

### **❌ DON'T:**

1. **Don't use .NET 8.0 features without guards:**
   ```csharp
   // Bad - will fail in .NET Framework 4.8 build
   public required string Name { get; init; }
   
   // Good - conditional compilation
   #if NET8_0_OR_GREATER
   public required string Name { get; init; }
   #else
   public string Name { get; set; }
   #endif
   ```

2. **Don't assume single build works for all:**
   - Always deploy version-specific DLLs

3. **Don't skip testing in older versions:**
   - 2024 support is just as important as 2025+

---

## 🚀 **Quick Reference**

### **Compilation Symbols:**

| Symbol | When Defined | Use For |
|--------|--------------|---------|
| `NET48` | Building for .NET Framework 4.8 | 2024-specific code |
| `NET8_0_OR_GREATER` | Building for .NET 8.0+ | 2025+ specific code |
| `ACAD2024` | Building for .NET Framework 4.8 | AutoCAD 2024 features |
| `ACAD2025_OR_GREATER` | Building for .NET 8.0+ | AutoCAD 2025+ features |

### **Check Current Build:**

```csharp
public static void CheckBuild()
{
    #if NET48
    Console.WriteLine("Building for .NET Framework 4.8 (AutoCAD 2024)");
    #elif NET8_0_OR_GREATER
    Console.WriteLine("Building for .NET 8.0 (AutoCAD 2025+)");
    #endif
}
```

---

## 📚 **Related Documentation**

- [DEVELOPMENT_RULES.md](./DEVELOPMENT_RULES.md) - Development standards
- [README.md](./README.md) - Project overview
- [DEVELOPMENT_CHECKLIST.md](./DEVELOPMENT_CHECKLIST.md) - Implementation tasks

---

**Last Updated:** November 14, 2025  
**Applies To:** UnifiedSnoop v1.0+

