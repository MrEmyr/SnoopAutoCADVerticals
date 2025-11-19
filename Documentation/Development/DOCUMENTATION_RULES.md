# Documentation Rules

**Last Updated:** November 19, 2025

---

## 📋 Core Rule: All Documentation in Documentation/ Folder

**All `.md` files MUST be stored in the `Documentation/` folder structure.**

### Why This Rule?

1. **Single Source of Truth** - All documentation in one place
2. **Easy Navigation** - Developers know exactly where to look
3. **Consistent Organization** - Standard folder structure
4. **Better Maintenance** - Centralized updates and version control
5. **Clear Separation** - Documentation separate from code

---

## 📁 Documentation Folder Structure

```
Documentation/
├── README.md                           # Main documentation index (start here)
├── PROJECT_README.md                   # Project overview (from UnifiedSnoop/README.md)
├── PROJECT_STRUCTURE.md                # Project organization
├── DOCUMENTATION_CLEANUP_COMPLETE.md   # Documentation reorganization history
├── ARCHIVE_SUMMARY.md                  # Archive summary
│
├── Archive/                            # Historical documentation
│   ├── README.md                      # Archive index
│   ├── Development/                   # Old development docs
│   ├── Technical/                     # Old technical docs
│   ├── UI/                           # Old UI docs
│   └── Deployment/                   # Old deployment docs
│
├── Development/                        # Developer guidelines
│   ├── DEVELOPMENT_RULES.md          # Coding standards and workflow
│   └── DOCUMENTATION_RULES.md        # This file
│
├── Deployment/                         # Deployment and installation
│   ├── DEPLOYMENT_GUIDE.md           # Main deployment guide
│   ├── DEPLOYMENT_CHECK.md           # Latest deployment status
│   └── DEPLOYMENT_SCRIPTS.md         # PowerShell script documentation
│
├── Technical/                          # Technical documentation
│   ├── ERROR_LOGGING.md              # Error logging system
│   ├── PLANT3D_SUPPORT.md            # Plant3D integration
│   ├── VERSION_COMPATIBILITY.md       # Version compatibility matrix
│   ├── UnifiedSnoop_Architecture.drawio  # Architecture diagram
│   └── Components/                    # Component documentation
│       ├── APP_COMPONENT.md          # App layer
│       ├── CORE_COMPONENT.md         # Core services
│       ├── INSPECTORS_COMPONENT.md   # Collectors/Inspectors
│       ├── RESOURCES_COMPONENT.md    # Resources
│       ├── SERVICES_COMPONENT.md     # Business services
│       ├── TESTS_COMPONENT.md        # Testing documentation
│       └── UI_COMPONENT.md           # UI components
│
├── UI/                                 # User interface documentation
│   ├── UI_SPECIFICATION.md           # Complete UI specs
│   ├── UI_FIX_SUMMARY.md            # Latest UI fixes (Nov 2025)
│   ├── UI_FIX_VISUAL_COMPARISON.md  # UI fix visual guide
│   └── UI_Layout.drawio             # UI layout diagram
│
└── User/                              # End-user documentation
    ├── USER_GUIDE.md                 # Complete user guide
    ├── QUICK_TEST_GUIDE.md          # Quick testing guide
    ├── XRECORD_EDITOR.md            # XRecord editor feature
    └── XRECORD_SUPPORT.md           # XRecord support guide
```

---

## ✅ File Naming Conventions

### General Rules
- **ALL CAPS** with underscores: `DEPLOYMENT_GUIDE.md`
- **Descriptive names**: Clearly indicate content
- **No dates in filenames**: Use version control for history
- **Consistent prefixes**: Group related docs (e.g., `UI_*`, `DEPLOYMENT_*`)

### Component Documentation
- Format: `[COMPONENT]_COMPONENT.md`
- Examples:
  - `CORE_COMPONENT.md`
  - `UI_COMPONENT.md`
  - `SERVICES_COMPONENT.md`

### Special Files
- `README.md` - Folder index or overview
- `PROJECT_README.md` - Main project overview
- `PROJECT_STRUCTURE.md` - Project organization

---

## 📝 File Organization by Category

### Development Documentation
**Location:** `Documentation/Development/`

**What belongs here:**
- Coding standards and guidelines
- Development workflow
- Documentation rules
- Git workflow
- Testing procedures
- Development environment setup

**Examples:**
- `DEVELOPMENT_RULES.md`
- `DOCUMENTATION_RULES.md`
- `TESTING_GUIDELINES.md`
- `GIT_WORKFLOW.md`

---

### Deployment Documentation
**Location:** `Documentation/Deployment/`

**What belongs here:**
- Deployment guides
- Installation instructions
- Bundle creation
- Script documentation
- Environment configuration
- Deployment checklists

**Examples:**
- `DEPLOYMENT_GUIDE.md`
- `DEPLOYMENT_CHECK.md`
- `DEPLOYMENT_SCRIPTS.md`

---

### Technical Documentation
**Location:** `Documentation/Technical/`

**What belongs here:**
- Architecture documentation
- System design
- API documentation
- Integration guides
- Technical specifications
- Component documentation

**Examples:**
- `UnifiedSnoop_Architecture.drawio`
- `ERROR_LOGGING.md`
- `VERSION_COMPATIBILITY.md`
- `PLANT3D_SUPPORT.md`

**Subfolders:**
- `Components/` - Individual component docs

---

### UI Documentation
**Location:** `Documentation/UI/`

**What belongs here:**
- UI specifications
- Layout diagrams
- Design guidelines
- UI fix documentation
- Interaction patterns
- Visual design

**Examples:**
- `UI_SPECIFICATION.md`
- `UI_Layout.drawio`
- `UI_FIX_SUMMARY.md`
- `UI_FIX_VISUAL_COMPARISON.md`

---

### User Documentation
**Location:** `Documentation/User/`

**What belongs here:**
- User guides
- Feature documentation
- Quick start guides
- Testing guides
- FAQ
- Troubleshooting

**Examples:**
- `USER_GUIDE.md`
- `QUICK_TEST_GUIDE.md`
- `XRECORD_EDITOR.md`
- `FAQ.md`

---

### Archive
**Location:** `Documentation/Archive/`

**What belongs here:**
- Outdated documentation
- Completed milestone docs
- Superseded versions
- Historical records

**Rules:**
- Always include `README.md` explaining archive
- Maintain original folder structure
- Include archive date
- Document why archived

---

## 🚫 What NOT to Put in Documentation/

### Code Comments
- ✅ Use XML documentation comments in code
- ❌ Don't create separate files for API docs (generate from code)

### Build Outputs
- ✅ Keep build logs in `UnifiedSnoop/bin/`
- ❌ Don't store build artifacts in Documentation/

### Temporary Notes
- ✅ Use git commit messages or issue tracker
- ❌ Don't create "TODO.md" or "NOTES.md"

### Sample Code
- ✅ Keep in `Samples/` folder
- ❌ Don't mix with documentation

### Scripts
- ✅ Keep PowerShell scripts in `UnifiedSnoop/Deploy/`
- ❌ Documentation folder is for .md files, not scripts

---

## 📋 Creating New Documentation

### Step 1: Determine Category
Ask: What type of documentation is this?
- Development → `Development/`
- Deployment → `Deployment/`
- Technical → `Technical/`
- UI/UX → `UI/`
- User-facing → `User/`

### Step 2: Choose Filename
- Use ALL CAPS with underscores
- Be descriptive
- Follow naming conventions
- Check for similar files first

### Step 3: Use Template
```markdown
# [Title]

**Last Updated:** [Date]

---

## Overview
[Brief description]

---

## [Section 1]
[Content]

---

## [Section 2]
[Content]

---

## Related Documentation
- [Link to related docs]
```

### Step 4: Update Index
Add link to `Documentation/README.md`

---

## 🔄 Moving Existing Documentation

### When Moving Files
1. **Determine correct category** (Development, Deployment, Technical, UI, User)
2. **Rename if needed** (follow naming conventions)
3. **Update all links** referencing the old location
4. **Update README.md** in target folder
5. **Commit with clear message** describing the move

### Example Move
```powershell
# Move UI fix documentation
Move-Item "UnifiedSnoop\Docs\UI_FIX_SUMMARY.md" "Documentation\UI\UI_FIX_SUMMARY.md"

# Update links in other files
# Update Documentation/README.md
# Commit changes
```

---

## 🔗 Linking Between Documents

### Relative Paths
Always use relative paths for internal links:

```markdown
# Good
- [User Guide](../User/USER_GUIDE.md)
- [Architecture](../Technical/UnifiedSnoop_Architecture.drawio)

# Bad
- [User Guide](/Documentation/User/USER_GUIDE.md)
- [Architecture](C:\...\UnifiedSnoop_Architecture.drawio)
```

### Link Format
```markdown
- **[Display Name](relative/path/to/file.md)** - Brief description
```

---

## 📊 Documentation Maintenance

### Regular Reviews
- **Weekly:** Check for broken links
- **Per Release:** Update version numbers
- **Per Feature:** Add/update relevant docs
- **Quarterly:** Archive outdated docs

### Version Updates
When updating documentation:
1. Update "Last Updated" date
2. Document changes in git commit
3. Archive old version if major rewrite
4. Update links if filename changed

### Quality Checklist
- [ ] Correct folder location
- [ ] Proper filename convention
- [ ] "Last Updated" date included
- [ ] Table of contents (if > 100 lines)
- [ ] Code blocks have language tags
- [ ] Links are relative paths
- [ ] Listed in README.md index
- [ ] Spell check completed

---

## 🎯 Enforcement

### During Development
- Code review checks documentation updates
- CI/CD can validate documentation structure
- Pre-commit hooks can check for .md files outside Documentation/

### Exceptions
Only these .md files are allowed outside Documentation/:
- **`LICENSE.md`** (root) - Project license
- **`Samples/*/README.md`** - Sample project documentation
- **`.cursor/commands/*.md`** - Cursor IDE commands

---

## 📚 Benefits of This Rule

### For Developers
- ✅ Know exactly where to find documentation
- ✅ Consistent structure across projects
- ✅ Easy to contribute documentation
- ✅ Clear separation of concerns

### For Users
- ✅ Single documentation hub
- ✅ Logical organization
- ✅ Easy navigation
- ✅ Professional appearance

### For Maintenance
- ✅ Easier to keep updated
- ✅ Simpler to archive old docs
- ✅ Better version control
- ✅ Reduced duplication

---

## 🚀 Migration Complete

**Status:** ✅ All .md files moved to Documentation/ (Nov 19, 2025)

**Files Organized:**
- 2 deployment docs → `Deployment/`
- 7 component docs → `Technical/Components/`
- 3 UI docs → `UI/`
- 1 test guide → `User/`
- 1 project overview → Root level
- 20 historical docs → `Archive/`

**Exceptions Remaining:**
- Samples/ - Sample project READMEs (appropriate)
- .cursor/commands/ - IDE commands (appropriate)

---

## ✅ Checklist for New Files

Before creating a new .md file:

- [ ] Is this documentation? (not code, not scripts)
- [ ] Does it belong in Documentation/ folder?
- [ ] Which category? (Development, Deployment, Technical, UI, User)
- [ ] Does a similar file already exist?
- [ ] Follow naming conventions? (ALL_CAPS_WITH_UNDERSCORES.md)
- [ ] Include "Last Updated" date?
- [ ] Add to README.md index?
- [ ] Use relative links?
- [ ] Spell check completed?

---

**Rule Status:** ACTIVE ✅  
**Compliance:** 100%  
**Last Audit:** November 19, 2025

