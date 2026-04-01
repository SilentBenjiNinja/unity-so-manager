# 1.2.2

### Improvements

* Assembly definition references switched from GUID to name-based — ensures references resolve correctly when the package is installed via UPM or Git submodule

---

# 1.2.1

### Fixes

* Fixed intermittent ImGUI `BeginGroup`/`EndGroup` error caused by modifying tree selection mid-draw in folder deselection logic

---

# 1.2.0

### Improvements

* Added XML documentation to all public APIs (`ManageableDataAttribute`, `ScriptableObjectManager`, `GUIUtils`, `SoManagerSymbolDefiner`)
* Improved `package.json` description
* Expanded README with full feature list, setup guide, keyboard shortcut reference, and optional dependency pattern

---

# 1.1.0

### New

* Added `SoManagerSymbolDefiner` — automatically defines the `SO_MANAGER` scripting define symbol on import, allowing other packages to wrap `[ManageableData]` usage in `#if SO_MANAGER` without a hard dependency on this package

---

# 1.0.7

### Improvements

* Added setup section to README

---

# 1.0.6

### Improvements

* Renamed editor script to `ScriptableObjectManager`

---

# 1.0.5

### Improvements

* Renamed window and file from Data Manager to SO Manager
* Cleaned up unused folders

---

# 1.0.4

### Fixes

* Reduced minimum tab button width for better layout at narrow window sizes

---

# 1.0.3

### Fixes

* Category tab buttons now render correctly

---

# 1.0.2

### New

* Create new asset of type via dialog
* Duplicate asset with auto-incremented name
* Delete asset(s) with confirmation
* Multi-select editing via merged Odin property tree
* Ping / reveal asset in Project window
* Keyboard shortcuts: `N` create, `Ctrl+D` duplicate, `F2` rename, `Del` delete, `F` ping
* Context menu on right-click with all asset operations

---

# 1.0.1

### Improvements

* Updated `package.json` display name and description

---

# 1.0.0

* Initial release — Odin-based editor window with `[ManageableData]` attribute and tabbed asset browsing