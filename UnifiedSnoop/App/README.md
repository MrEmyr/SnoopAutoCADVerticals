# App Layer

Application entry point, commands, and menu integration.

## Files to Create:

### **1. App.cs**
- Implements `IExtensionApplication`
- Initialize() - Register collectors, add menus
- Terminate() - Cleanup
- Platform detection

### **2. Commands.cs**
- `[CommandMethod]` attributes
- SnoopDatabase
- SnoopEntities
- SnoopCivil3DDoc (conditional)
- SnoopAlignments (conditional)
- Other snoop commands

### **3. ContextMenu.cs**
- Right-click menu integration
- Dynamic menu items based on platform
- Command execution handlers

## Key Responsibilities:
- AutoCAD/Civil 3D initialization
- Command registration
- UI integration
- Platform-specific feature loading

## Dependencies:
- Autodesk.AutoCAD.Runtime
- Autodesk.AutoCAD.ApplicationServices
- Autodesk.AutoCAD.Windows
- Core.Collectors
- Services

## Next Steps:
1. Create App.cs with IExtensionApplication
2. Add platform detection
3. Register collectors
4. Implement commands
5. Add context menu

