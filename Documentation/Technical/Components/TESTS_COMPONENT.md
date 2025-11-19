# Tests

Unit tests and integration tests for UnifiedSnoop.

## Test Categories:

### **Unit Tests:**
- Core.Collectors
- Core.Data
- Core.Helpers
- Services

### **Integration Tests:**
- UI components
- Command execution
- Platform detection

## Test Framework:
- xUnit / NUnit / MSTest
- Moq for mocking
- FluentAssertions for assertions

## Test Structure:
```
Tests/
├── Unit/
│   ├── Collectors/
│   ├── Services/
│   └── Helpers/
├── Integration/
│   ├── Commands/
│   └── UI/
└── TestHelpers/
    ├── MockObjects.cs
    └── TestUtilities.cs
```

## Coverage Goals:
- Core logic: 80%+
- Services: 70%+
- UI: 50%+

## Next Steps:
1. Add test project reference
2. Create test framework
3. Add unit tests for core
4. Add integration tests
5. Set up CI/CD testing

