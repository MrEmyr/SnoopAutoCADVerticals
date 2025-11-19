# UnifiedSnoop Development Rules

**Version:** 1.1  
**Last Updated:** November 14, 2025 (Added Sprint Completion Rules)  
**Status:** Living Document - Update as needed

---

## 📋 **Table of Contents**

1. [Sprint Completion Rules](#sprint-completion-rules) ⭐ NEW
2. [Version Support Strategy](#version-support-strategy)
3. [Code Quality Standards](#code-quality-standards)
4. [API Usage Rules](#api-usage-rules)
5. [Architecture Principles](#architecture-principles)
6. [Naming Conventions](#naming-conventions)
7. [Error Handling](#error-handling)
8. [Testing Requirements](#testing-requirements)
9. [Documentation Standards](#documentation-standards)
10. [Version Control](#version-control)
11. [Performance Guidelines](#performance-guidelines)

---

## 🚀 **Sprint Completion Rules**

These rules ensure quality and completeness at the end of each development sprint or phase.

### **RULE 0.1: Zero Errors and Warnings (MANDATORY)**
✅ **MUST:** At the end of each sprint/phase, the project MUST build with:
- **Zero errors** 
- **Zero warnings** (both C# compiler warnings and linter warnings)

**Build Command to Verify:**
```powershell
dotnet build -c Release
```

**Expected Output:**
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

❌ **DON'T:** Consider a sprint complete if any warnings or errors exist

**Why:** Warnings often indicate potential bugs, deprecated API usage, or code quality issues that can cause runtime problems.

---

### **RULE 0.2: Complete Work - No Partial Features (MANDATORY)**
✅ **MUST:** Each sprint/phase must be 100% complete before moving to the next
- All planned features fully implemented
- All features tested and working
- No "TODO" comments for sprint features
- No placeholder code
- No partially implemented functionality

❌ **DON'T:** Leave features partially complete or defer work to "later"

**Sprint Completion Checklist:**
- [ ] All features implemented
- [ ] All features tested manually
- [ ] Build successful (0 errors, 0 warnings)
- [ ] Code committed
- [ ] Documentation updated
- [ ] Deployment successful

---

### **RULE 0.3: Continuous Build Verification (AUTOMATED)**
✅ **DO:** Run builds periodically during development without asking for permission
- Build after completing each feature
- Build after fixing compilation errors
- Build before moving to the next task
- Multiple builds per sprint are expected and encouraged

❌ **DON'T:** Ask for permission to run builds - do it automatically

**When to Build:**
1. After implementing a feature
2. After fixing compilation errors
3. Before starting the next feature
4. At regular intervals (every 30-60 minutes of coding)
5. Before marking a task as complete

**Build Workflow:**
```powershell
# Quick verification build
dotnet build -c Release

# If errors exist, fix them immediately
# Then build again until successful
```

---

### **RULE 0.4: Deployment Requires Explicit Permission**
⚠️ **MUST ASK:** Always request user permission before deploying to bundle location

**Correct Workflow:**
```
1. Complete all features
2. Build succeeds (0 errors, 0 warnings)
3. Ask user: "Ready to deploy to bundle?"
4. Wait for user confirmation
5. Deploy only after confirmation
```

❌ **DON'T:** Deploy automatically without asking

**Why:** Deployment affects the testing environment. User may want to:
- Review code first
- Commit changes first
- Wait for a specific time to test
- Have AutoCAD closed first

**Deployment Command (After Permission):**
```powershell
.\Deploy\Quick-Deploy.ps1
```

---

### **RULE 0.5: Sprint Verification Report (RECOMMENDED)**
✅ **RECOMMENDED:** At sprint completion, provide a summary:

**Template:**
```
Sprint X Complete! ✅

Features Implemented: [list]
Build Status: ✅ Success (0 errors, 0 warnings)
Files Created/Modified: [count]
Lines of Code: [estimate]
Testing Status: Ready
Deployment Status: Pending user approval

Ready to deploy?
```

---

## 🎯 **Version Support Strategy**

### **Supported Versions**

| Product | Versions | .NET Framework | Target |
|---------|----------|----------------|--------|
| **AutoCAD** | 2024 | .NET Framework 4.8 | `net48` |
| **AutoCAD** | 2025, 2026 | .NET 8.0 | `net8.0-windows` |
| **Civil 3D** | 2024 | .NET Framework 4.8 | `net48` |
| **Civil 3D** | 2025, 2026 | .NET 8.0 | `net8.0-windows` |

### **RULE 1.1: Multi-Targeting**
✅ **DO:** Use multi-targeting in the project file
```xml
<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>
```

✅ **DO:** Build separate DLLs for each framework version

❌ **DON'T:** Try to use a single DLL for all versions

### **RULE 1.1.1: Separate Build Outputs (MANDATORY)**
✅ **MUST:** Always maintain separate build outputs for 2024 and 2025+

**Build Output Structure:**
```
bin/
├── x64/
│   └── Release/
│       ├── net48/                      ← For AutoCAD/Civil 3D 2024
│       │   └── UnifiedSnoop.dll
│       └── net8.0-windows/             ← For AutoCAD/Civil 3D 2025+
│           └── UnifiedSnoop.dll
```

**Deployment:**
- Load `net48\UnifiedSnoop.dll` in AutoCAD/Civil 3D 2024
- Load `net8.0-windows\UnifiedSnoop.dll` in AutoCAD/Civil 3D 2025+

❌ **DON'T:** Mix DLLs between versions - this will cause runtime errors

**Build Command:**
```powershell
dotnet build -c Release
```
This automatically builds both versions

### **RULE 1.1.2: Automatic Bundle Deployment (RECOMMENDED)**
✅ **RECOMMENDED:** Use deployment scripts for testing

**Bundle Deployment Location:**
```
C:\ProgramData\Autodesk\ApplicationPlugins\UnifiedSnoop.bundle\
├── PackageContents.xml
└── Contents\
    ├── 2024\       ← net48 DLL for AutoCAD/Civil 3D 2024
    └── 2025\       ← net8.0 DLL for AutoCAD/Civil 3D 2025+
```

**Deployment Scripts:**
- `Deploy/Quick-Deploy.ps1` - One-click build and deploy
- `Deploy/Deploy-ToBundle.ps1` - Full deployment with options
- See `Deploy/DEPLOYMENT_README.md` for details

**Benefits:**
- Automatic loading in AutoCAD (no NETLOAD needed)
- Separate from development binaries
- Easy testing across versions

**Quick Deploy Command:**
```powershell
.\Deploy\Quick-Deploy.ps1
```

### **RULE 1.2: Conditional Compilation**
✅ **DO:** Use conditional compilation symbols for version-specific code

```csharp
#if NET48
    // Code for AutoCAD/Civil 3D 2024
    using System;
#elif NET8_0_OR_GREATER
    // Code for AutoCAD/Civil 3D 2025+
    using System.Diagnostics.CodeAnalysis;
#endif
```

### **RULE 1.3: API Version Detection**
✅ **DO:** Detect AutoCAD/Civil 3D version at runtime

```csharp
public static class VersionDetection
{
    public static int GetAutoCADVersion()
    {
        var version = Application.Version;
        return version.Major; // Returns 24, 25, or 26
    }
    
    public static bool IsNet48Build()
    {
        #if NET48
        return true;
        #else
        return false;
        #endif
    }
}
```

### **RULE 1.4: Version-Specific Features**
✅ **DO:** Gracefully handle version-specific features

```csharp
public void UseFeature()
{
    if (GetAutoCADVersion() >= 25)
    {
        // Use 2025+ feature
    }
    else
    {
        // Fallback for 2024
    }
}
```

❌ **DON'T:** Crash if a feature isn't available in older versions

---

## 💎 **Code Quality Standards**

### **RULE 2.1: Target Quality Level**
✅ **TARGET:** A+ Code Quality (98/100 like MgdDbg)

**Metrics:**
- Zero compiler warnings
- Zero linter errors
- Clean code analysis
- Proper nullability annotations
- XML documentation on public APIs

### **RULE 2.2: Code Analysis**
✅ **DO:** Enable all recommended code analyzers

```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest</AnalysisLevel>
    <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

### **RULE 2.3: Code Reviews**
✅ **DO:** All code must be reviewed before merging
- Check against these rules
- Verify API compliance
- Test in both .NET Framework and .NET 8.0 builds

---

## 🔌 **API Usage Rules**

### **RULE 3.1: OpenMode - CRITICAL ⚠️**
Based on API review findings, this is the most critical rule:

✅ **DO:** Use `OpenMode.ForRead` for inspection/read-only operations
```csharp
// CORRECT - Reading/inspecting objects
DBObject obj = trans.GetObject(objId, OpenMode.ForRead);
```

❌ **DON'T:** Use `OpenMode.ForWrite` unless actually modifying
```csharp
// WRONG - Causes locking issues for read-only operations
DBObject obj = trans.GetObject(objId, OpenMode.ForWrite); // ❌
```

**Why:** Using ForWrite for read-only operations causes:
- Object locking conflicts
- Multi-user access issues
- Performance degradation
- Unnecessary write permissions

### **RULE 3.2: Transaction Management**
✅ **DO:** Always use transactions within `using` statements

```csharp
using (Transaction trans = db.TransactionManager.StartTransaction())
{
    try
    {
        // Operations
        trans.Commit();
    }
    catch
    {
        trans.Abort();
        throw;
    }
}
```

✅ **DO:** Use the TransactionHelper class for complex scenarios

```csharp
using (TransactionHelper helper = new TransactionHelper(db))
{
    helper.Start();
    // Operations
    helper.Commit();
}
```

❌ **DON'T:** Leave transactions uncommitted or undisposed

### **RULE 3.3: Resource Disposal**
✅ **DO:** Implement IDisposable pattern correctly

```csharp
public class MyClass : IDisposable
{
    private bool _disposed = false;
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Dispose managed resources
            }
            _disposed = true;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

### **RULE 3.4: API Compatibility**
✅ **DO:** Check API documentation for each version
- [AutoCAD 2024 API](https://help.autodesk.com/view/OARX/2024/ENU/)
- [AutoCAD 2025 API](https://help.autodesk.com/view/OARX/2025/ENU/)
- [AutoCAD 2026 API](https://help.autodesk.com/view/OARX/2026/ENU/)
- [Civil 3D 2024 API](https://help.autodesk.com/view/CIV3D/2024/ENU/)
- [Civil 3D 2025 API](https://help.autodesk.com/view/CIV3D/2025/ENU/)
- [Civil 3D 2026 API](https://help.autodesk.com/view/CIV3D/2026/ENU/)

✅ **DO:** Use conditional compilation for version-specific APIs

```csharp
#if ACAD2025_OR_GREATER
    // Use 2025+ specific API
#else
    // Use compatible alternative
#endif
```

### **RULE 3.5: Deprecated APIs**
❌ **DON'T:** Use deprecated APIs
- `ObjectId.Open()` / `ObjectId.Close()` - Use transactions instead
- `Thread.Abort()` - Use `CancellationToken` instead
- `BinaryFormatter` - Use modern serialization

---

## 🏗️ **Architecture Principles**

### **RULE 4.1: Layer Separation**
✅ **DO:** Maintain strict layer boundaries

```
UI Layer → Services Layer → Core Layer → Platform Layer
```

❌ **DON'T:** Skip layers or create circular dependencies

### **RULE 4.2: Dependency Injection**
✅ **DO:** Use constructor injection for dependencies

```csharp
public class PropertyExtractionService
{
    private readonly ICollector _collector;
    
    public PropertyExtractionService(ICollector collector)
    {
        _collector = collector ?? throw new ArgumentNullException(nameof(collector));
    }
}
```

### **RULE 4.3: Interface Segregation**
✅ **DO:** Keep interfaces small and focused

```csharp
// Good - Single responsibility
public interface ICollector
{
    bool CanCollect(object obj);
    List<PropertyData> Collect(object obj, Transaction trans);
}

// Bad - Too many responsibilities
public interface IMegaInterface
{
    // 20+ methods
}
```

### **RULE 4.4: Extensibility**
✅ **DO:** Design for extensibility using patterns
- Collector pattern for data collection
- Inspector pattern for type-specific handling
- Strategy pattern for algorithms

---

## 📛 **Naming Conventions**

### **RULE 5.1: C# Naming Standards**
Follow Microsoft's C# naming conventions:

| Element | Convention | Example |
|---------|-----------|---------|
| **Classes** | PascalCase | `PropertyData`, `TransactionHelper` |
| **Interfaces** | IPascalCase | `ICollector`, `IInspector` |
| **Methods** | PascalCase | `CollectProperties()`, `GetValue()` |
| **Properties** | PascalCase | `Name`, `Value`, `IsCollection` |
| **Public Fields** | PascalCase | `MaxItems` |
| **Private Fields** | _camelCase | `_transaction`, `_database` |
| **Constants** | PascalCase | `MaxRetries`, `DefaultTimeout` |
| **Parameters** | camelCase | `objectId`, `transaction` |
| **Local Variables** | camelCase | `result`, `count` |

### **RULE 5.2: File Naming**
✅ **DO:** Match file names to primary type

```
PropertyData.cs        ← class PropertyData
ICollector.cs          ← interface ICollector
TransactionHelper.cs   ← class TransactionHelper
```

### **RULE 5.3: Namespace Organization**
✅ **DO:** Use consistent namespace hierarchy

```csharp
namespace UnifiedSnoop.Core.Collectors { }
namespace UnifiedSnoop.Core.Data { }
namespace UnifiedSnoop.Services { }
namespace UnifiedSnoop.Inspectors.AutoCAD { }
namespace UnifiedSnoop.Inspectors.Civil3D { }
```

---

## 🚨 **Error Handling**

### **RULE 6.1: Exception Handling Strategy**
✅ **DO:** Catch specific exceptions

```csharp
try
{
    // Operation
}
catch (Autodesk.AutoCAD.Runtime.Exception ex)
{
    // Handle AutoCAD-specific errors
    LogError($"AutoCAD error: {ex.ErrorStatus}", ex);
}
catch (ArgumentNullException ex)
{
    // Handle null arguments
    LogError("Null argument", ex);
}
```

❌ **DON'T:** Catch generic Exception unless re-throwing

```csharp
// Bad
try { }
catch (Exception) { } // Swallows all errors

// Good
try { }
catch (Exception ex)
{
    LogError("Unexpected error", ex);
    throw; // Re-throw after logging
}
```

### **RULE 6.2: User-Friendly Messages**
✅ **DO:** Provide clear, actionable error messages

```csharp
// Good
throw new InvalidOperationException(
    "Cannot inspect object because the transaction is not active. " +
    "Ensure Start() is called before Collect().");

// Bad
throw new Exception("Error"); // Not helpful
```

### **RULE 6.3: Graceful Degradation**
✅ **DO:** Degrade gracefully for unknown types

```csharp
public List<PropertyData> Collect(object obj)
{
    try
    {
        return CollectInternal(obj);
    }
    catch (Exception ex)
    {
        // Return partial data rather than crashing
        return new List<PropertyData>
        {
            new PropertyData
            {
                Name = "Error",
                Value = $"Could not collect properties: {ex.Message}",
                HasError = true
            }
        };
    }
}
```

### **RULE 6.4: Logging**
✅ **DO:** Log errors for diagnostics

```csharp
public class ErrorLogger
{
    public static void LogError(string message, Exception ex)
    {
        Debug.WriteLine($"[ERROR] {message}: {ex}");
        // Consider file logging for production
    }
}
```

---

## 🧪 **Testing Requirements**

### **RULE 7.1: Test Coverage**
✅ **TARGET:** Minimum test coverage
- Core logic: 80%+
- Services: 70%+
- Inspectors: 60%+
- UI: 50%+

### **RULE 7.2: Test Organization**
✅ **DO:** Organize tests by component

```
Tests/
├── Unit/
│   ├── Core/
│   │   ├── CollectorTests.cs
│   │   └── TransactionHelperTests.cs
│   └── Services/
│       └── PlatformDetectionTests.cs
├── Integration/
│   ├── AutoCAD2024Tests.cs
│   ├── AutoCAD2025Tests.cs
│   └── Civil3DTests.cs
└── TestHelpers/
    └── MockObjects.cs
```

### **RULE 7.3: Test Naming**
✅ **DO:** Use descriptive test names

```csharp
[Fact]
public void Collector_WhenObjectIsNull_ThrowsArgumentNullException()
{
    // Arrange, Act, Assert
}

[Fact]
public void TransactionHelper_WhenCommitted_ReleasesTransaction()
{
    // Arrange, Act, Assert
}
```

### **RULE 7.4: Version-Specific Testing**
✅ **DO:** Test in all supported versions
- Test in AutoCAD 2024 (.NET 4.8)
- Test in AutoCAD 2025+ (.NET 8.0)
- Test in Civil 3D 2024 (.NET 4.8)
- Test in Civil 3D 2025+ (.NET 8.0)

---

## 📚 **Documentation Standards**

### **RULE 8.1: XML Documentation**
✅ **DO:** Add XML documentation to all public APIs

```csharp
/// <summary>
/// Collects properties from the specified object using reflection.
/// </summary>
/// <param name="obj">The object to inspect. Cannot be null.</param>
/// <param name="trans">The active transaction. Cannot be null.</param>
/// <returns>A list of property data for the object.</returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="obj"/> or <paramref name="trans"/> is null.
/// </exception>
public List<PropertyData> Collect(object obj, Transaction trans)
{
    // Implementation
}
```

### **RULE 8.2: Code Comments**
✅ **DO:** Comment complex logic and "why", not "what"

```csharp
// Good - Explains why
// Use ForRead to avoid locking issues in multi-user environments
DBObject obj = trans.GetObject(objId, OpenMode.ForRead);

// Bad - Obvious what
// Get the object
DBObject obj = trans.GetObject(objId, OpenMode.ForRead);
```

### **RULE 8.3: README Updates**
✅ **DO:** Update README when adding major features
✅ **DO:** Document breaking changes
✅ **DO:** Keep version history current

---

## 🔄 **Version Control**

### **RULE 9.1: Commit Messages**
✅ **DO:** Use clear, descriptive commit messages

```
Good:
- "Add Civil 3D 2025 API support with conditional compilation"
- "Fix: Correct OpenMode usage in ReflectionCollector"
- "Refactor: Extract transaction logic to helper class"

Bad:
- "update"
- "fix bug"
- "changes"
```

### **RULE 9.2: Branch Strategy**
✅ **DO:** Use feature branches

```
main                    ← Production-ready code
├── develop            ← Development integration
    ├── feature/core-collectors
    ├── feature/ui-forms
    └── feature/civil3d-inspectors
```

### **RULE 9.3: Pull Requests**
✅ **DO:** All changes go through PR review
✅ **DO:** Include tests with PRs
✅ **DO:** Update documentation in same PR

---

## ⚡ **Performance Guidelines**

### **RULE 10.1: Performance Targets**
✅ **TARGET:** Response times
- Object inspection: <1 second
- Property collection: <500ms
- Collection expansion: <2 seconds
- Large drawings (10,000+ entities): <5 seconds

### **RULE 10.2: Optimization**
✅ **DO:** Use lazy loading for collections

```csharp
public class ObjectNode
{
    private List<ObjectNode>? _children;
    
    public List<ObjectNode> Children
    {
        get
        {
            if (_children == null)
            {
                _children = LoadChildren(); // Lazy load
            }
            return _children;
        }
    }
}
```

✅ **DO:** Cache expensive operations

```csharp
private static Dictionary<Type, PropertyInfo[]> _propertyCache = new();

public PropertyInfo[] GetProperties(Type type)
{
    if (!_propertyCache.TryGetValue(type, out var properties))
    {
        properties = type.GetProperties();
        _propertyCache[type] = properties;
    }
    return properties;
}
```

### **RULE 10.3: Memory Management**
✅ **DO:** Dispose of resources promptly
✅ **DO:** Avoid holding large objects in memory
✅ **DO:** Use `using` statements for IDisposable

---

## 🔐 **Security Rules**

### **RULE 11.1: Input Validation**
✅ **DO:** Validate all inputs

```csharp
public void ProcessObject(object obj)
{
    if (obj == null)
        throw new ArgumentNullException(nameof(obj));
    
    // Process
}
```

### **RULE 11.2: Safe Reflection**
✅ **DO:** Handle reflection errors gracefully

```csharp
try
{
    var value = property.GetValue(obj, null);
}
catch (TargetInvocationException ex)
{
    // Log and continue, don't crash
    LogError($"Could not get {property.Name}", ex);
}
```

---

## 📝 **Version-Specific Considerations**

### **RULE 12.1: .NET Framework 4.8 (2024)**
✅ **DO:** Remember limitations
- No nullable reference types
- No init-only properties
- No records
- Different async patterns

```csharp
#if NET48
    // Use older patterns
    public string Name { get; set; }
#else
    // Use modern patterns
    public string Name { get; init; } = string.Empty;
#endif
```

### **RULE 12.2: .NET 8.0 (2025+)**
✅ **DO:** Use modern features when available

```csharp
#if NET8_0_OR_GREATER
    // Use modern features
    public required string Name { get; init; }
    
    [NotNull]
    public object? Value { get; set; }
#endif
```

### **RULE 12.3: Compilation Symbols**
✅ **DO:** Define custom symbols in project file

```xml
<PropertyGroup Condition="'$(TargetFramework)' == 'net48'">
  <DefineConstants>$(DefineConstants);ACAD2024</DefineConstants>
</PropertyGroup>
<PropertyGroup Condition="'$(TargetFramework)' == 'net8.0-windows'">
  <DefineConstants>$(DefineConstants);ACAD2025_OR_GREATER</DefineConstants>
</PropertyGroup>
```

---

## ✅ **Checklist for New Code**

Before committing any code, verify:

- [ ] Follows OpenMode rules (ForRead for inspection)
- [ ] Proper transaction management
- [ ] Resource disposal (using statements)
- [ ] Multi-targeting compatibility tested
- [ ] XML documentation on public APIs
- [ ] Unit tests added
- [ ] No compiler warnings
- [ ] No linter errors
- [ ] Error handling in place
- [ ] Performance acceptable
- [ ] Tested in both .NET 4.8 and .NET 8.0 builds
- [ ] Code reviewed

---

## 📖 **Reference Documentation**

### **API Documentation:**
- [AutoCAD 2024 API](https://help.autodesk.com/view/OARX/2024/ENU/?guid=GUID-C3F3C736-40CF-44A0-9210-55F6A939B6F2)
- [AutoCAD 2025 API](https://help.autodesk.com/view/OARX/2025/ENU/)
- [AutoCAD 2026 API](https://help.autodesk.com/view/OARX/2026/ENU/)
- [Civil 3D 2024 API](https://help.autodesk.com/view/CIV3D/2024/ENU/?guid=GUID-DA303320-B66D-4F4F-A4F4-9FBBEC0754E0)
- [Civil 3D 2025 API](https://help.autodesk.com/view/CIV3D/2025/ENU/?guid=GUID-DA303320-B66D-4F4F-A4F4-9FBBEC0754E0)
- [Civil 3D 2026 API](https://help.autodesk.com/view/CIV3D/2026/ENU/?guid=GUID-DA303320-B66D-4F4F-A4F4-9FBBEC0754E0)

### **Internal Documentation:**
- Architecture: `/Documentation/UnifiedSnoop_Architecture.drawio`
- Implementation Plan: `/Documentation/UnifiedSnoop_Implementation_Plan.md`
- API Reviews: `/Documentation/API_REVIEW_*.md`
- Sample Code: `/Samples/`

---

## 🔄 **Rule Updates**

### **Adding New Rules:**
1. Document the rule clearly with examples
2. Explain the "why" behind the rule
3. Show both correct (✅) and incorrect (❌) examples
4. Update the version number
5. Notify team of changes

### **Version History:**
| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-14 | Initial rules document created |

---

## 💬 **Questions or Clarifications**

If a rule is unclear or needs modification:
1. Discuss with team
2. Document the decision
3. Update this file
4. Commit with clear message

---

**This is a living document. Update it as the project evolves!**

---

**Last Reviewed:** November 14, 2025  
**Next Review:** [Set quarterly review dates]  
**Owner:** Development Team

