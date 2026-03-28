# Scriptable Object Manager for Unity

An Odin Inspector-powered Editor window for browsing, editing, creating, duplicating, renaming, and deleting ScriptableObject assets. Tag any ScriptableObject class with `[ManageableData]` to register it as a tab in the window.

> Requires [Odin Inspector](https://odininspector.com/).
> Based on a [tutorial by Sirenix](https://youtu.be/1zu41Ku46xU).

## Features

- **Tabbed browsing** — one tab per `[ManageableData]` type, with icon and label
- **Full asset management** — create, duplicate, rename, and delete assets without leaving the window
- **Multi-select editing** — select multiple assets and edit shared fields simultaneously
- **Keyboard shortcuts** — `N` create, `Ctrl+D` duplicate, `F2` rename, `Del` delete, `F` ping in Project window
- **Context menu** — right-click any asset for the same operations
- **Auto-discovery** — picks up all assets under `Assets/ScriptableObjects/` automatically
- **`SO_MANAGER` symbol** — automatically defines the scripting define symbol on import, so other packages can wrap `[ManageableData]` in `#if SO_MANAGER` without a hard dependency on this package

## Setup

### 1. Tag your ScriptableObject

```csharp
#if SO_MANAGER
[ManageableData("Enemy Data", Order = 10)]
#endif
[CreateAssetMenu(menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string EnemyName;
    public int Health;
}
```

The `Order` parameter controls the left-to-right position of the tab relative to other types.

### 2. Place assets in the right folder

Assets must live under `Assets/ScriptableObjects/` to be discovered. Subfolders are supported — the window flattens the list automatically when all assets share a single folder.

### 3. Open the window

```
BNJ > SO Manager
```

Select a tab to browse assets of that type.

## Keyboard Shortcuts

| Key | Action |
|---|---|
| `N` | Create new asset of the same type in the same folder |
| `Ctrl+D` | Duplicate selected asset |
| `F2` | Rename selected asset |
| `Del` | Delete selected asset(s) |
| `F` | Ping / reveal selected asset in the Project window |

## Optional dependency pattern

If you want to use `[ManageableData]` in your own packages without forcing consumers to install so-manager, wrap the attribute and its `using` directive in a preprocessor guard:

```csharp
#if SO_MANAGER
using bnj.so_manager.Runtime;
#endif

#if SO_MANAGER
[ManageableData("My Data")]
#endif
[CreateAssetMenu(menuName = "Game/My Data")]
public class MyData : ScriptableObject { }
```

When so-manager is installed the `SO_MANAGER` symbol is defined automatically and the attribute compiles in. When it is absent the symbol is undefined and the attribute is excluded — no compile errors.
* Optional: Add an icon to your scriptable object script to have an easier time finding it in the SO Manager
