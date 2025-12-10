using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using bnj.so_manager.Runtime;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace bnj.so_manager.Editor
{
    // TODO:
    // Button for renaming objects (https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetDatabase.RenameAsset.html)
    // Change name from data manager? (e.g. SO Manager)

    // Credit to Sirenix Tutorial:
    // https://youtu.be/1zu41Ku46xU
    public class DataManager : OdinMenuEditorWindow
    {
        static readonly Type[] _typesToDisplay =
            TypeCache.GetTypesWithAttribute<ManageableDataAttribute>()
            .OrderBy(x => x.GetAttribute<ManageableDataAttribute>().Order)
            .ToArray();

        Type _selectedType;

        PropertyTree _multiSelectPropertyTree;

        [MenuItem("BNJ/Data Manager")]
        static void OpenWindow() => GetWindow<DataManager>().Show();

        protected override void OnImGUI()
        {
            if (GUIUtils.SelectButtonList(ref _selectedType, _typesToDisplay))
                ForceMenuTreeRebuild();

            base.OnImGUI();
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var assetPaths = _selectedType == null ? new string[] { } :
                AssetDatabase.FindAssets($"t:{_selectedType.Name}", new[] { "Assets/ScriptableObjects" })
                    .Select(x => AssetDatabase.GUIDToAssetPath(x));

            var tree = new OdinMenuTree(true, new OdinMenuTreeDrawingConfig
            {
                DrawSearchToolbar = assetPaths.Count() > 1,
                AutoHandleKeyboardNavigation = false,
                DefaultMenuStyle = new()
                {
                    IconSize = 24,
                    IconOffset = -7,
                    TriangleSize = 20,
                    NotSelectedIconAlpha = .3f,
                },
            })
            {
                Config = { UseCachedExpandedStates = true }
            };

            tree.DefaultMenuStyle.SetIconPadding(0);
            tree.Selection.SupportsMultiSelect = true;

            if (_selectedType == null) return tree;

            if (assetPaths.Count() < 1)
            {
                Debug.LogWarning($"No matching files of class {_selectedType.Name} were found. Make sure there is at least one ScriptableObject of the missing type in the Assets/ScriptableObjects folder!");
                return tree;
            }

            var flatten = assetPaths.Select(x => Directory.GetParent(x).FullName)
                .Distinct().Count() < 2;

            tree.AddAllAssetsAtPath(null, "Assets/ScriptableObjects/", _selectedType, true, flatten)
                .AddThumbnailIcons();

            tree.SortMenuItemsByName();

            // Add context menu items and prevent folder selection
            tree.EnumerateTree().AddThumbnailIcons().ForEach(item =>
            {
                AddContextMenu(item);
                PreventFolderSelection(item);
            });

            var itemCount = tree.MenuItems.Count();
            if (itemCount > 0 && flatten) tree.MenuItems.First().Select();
            if (itemCount > 1 || !flatten) tree.FocusSearchField();

            return tree;
        }

        void PreventFolderSelection(OdinMenuItem menuItem)
        {
            // Folders don't have a UnityEngine.Object value, only actual assets do
            if (menuItem.Value is UnityEngine.Object) return;

            // This is a folder - prevent it from being selected
            menuItem.OnDrawItem += (item) =>
            {
                // If this folder item is selected, deselect it
                if (item.IsSelected)
                {
                    // Find the first actual asset to select instead
                    var firstAsset = MenuTree.EnumerateTree()
                        .FirstOrDefault(x => x.Value is UnityEngine.Object);

                    if (firstAsset != null)
                    {
                        firstAsset.Select(false);
                    }
                }
            };
        }

        void AddContextMenu(OdinMenuItem menuItem)
        {
            // Only add context menu to actual assets, not folders
            if (menuItem.Value is not UnityEngine.Object) return;

            menuItem.OnRightClick += (item) =>
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Create New (N)"), false, () => CreateNewAssetOfType(item));
                menu.AddItem(new GUIContent("Duplicate (Ctrl+D)"), false, () => DuplicateAsset(item));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent("Rename (F2)"), false, () => StartRename(item));
                menu.AddItem(new GUIContent("Delete (Del)"), false, () => DeleteAsset(item));
                menu.AddItem(new GUIContent("Select (F)"), false, () => PingAsset(item));

                menu.ShowAsContext();
            };
        }

        protected override void DrawEditors()
        {
            var selected = MenuTree.Selection;

            // Handle multi-selection - create a merged property tree
            if (selected.Count > 1)
            {
                // Get all selected objects
                var selectedObjects = selected
                    .Select(x => x.Value)
                    .OfType<UnityEngine.Object>()
                    .ToArray();

                if (selectedObjects.Length > 0)
                {
                    // Draw the multi-object editor
                    SirenixEditorGUI.BeginHorizontalToolbar();
                    {
                        GUILayout.Label($"{selectedObjects.Length} items selected");
                    }
                    SirenixEditorGUI.EndHorizontalToolbar();

                    // Dispose old tree if selection changed
                    if (_multiSelectPropertyTree != null &&
                        (_multiSelectPropertyTree.WeakTargets.Count != selectedObjects.Length ||
                         !_multiSelectPropertyTree.WeakTargets.SequenceEqual(selectedObjects)))
                    {
                        _multiSelectPropertyTree.Dispose();
                        _multiSelectPropertyTree = null;
                    }

                    // Create property tree if needed
                    if (_multiSelectPropertyTree == null)
                    {
                        _multiSelectPropertyTree = PropertyTree.Create(selectedObjects);
                    }

                    // Draw the property tree
                    _multiSelectPropertyTree.Draw(true);
                }

                // Don't call base - this prevents drawing individual editors
                return;
            }

            // Clean up multi-select tree when switching to single selection
            if (_multiSelectPropertyTree != null)
            {
                _multiSelectPropertyTree.Dispose();
                _multiSelectPropertyTree = null;
            }

            // Only call base for single selection
            base.DrawEditors();
        }

        protected override void OnBeginDrawEditors()
        {
            base.OnBeginDrawEditors();
            HandleKeyboardShortcuts();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Clean up the property tree when window closes
            if (_multiSelectPropertyTree != null)
            {
                _multiSelectPropertyTree.Dispose();
                _multiSelectPropertyTree = null;
            }
        }

        void HandleKeyboardShortcuts()
        {
            var currentEvent = Event.current;

            // Handle keyboard shortcuts only when this window is focused
            if (currentEvent.type != EventType.KeyDown) return;

            var selected = MenuTree?.Selection?.FirstOrDefault();
            if (selected?.Value == null) return;

            // N for create new of same type
            if (currentEvent.keyCode == KeyCode.N)
            {
                CreateNewAssetOfType(selected);
                currentEvent.Use();
            }

            // Ctrl+D for duplicate
            if (currentEvent.control && currentEvent.keyCode == KeyCode.D)
            {
                DuplicateAsset(selected);
                currentEvent.Use();
            }

            // F2 for rename
            if (currentEvent.keyCode == KeyCode.F2)
            {
                StartRename(selected);
                currentEvent.Use(); // Prevents other systems from handling this event
            }

            // Delete key for deletion
            if (currentEvent.keyCode == KeyCode.Delete)
            {
                DeleteAssets(MenuTree.Selection);
                currentEvent.Use();
            }

            // F key for ping/select in project
            if (currentEvent.keyCode == KeyCode.F)
            {
                PingAsset(selected);
                currentEvent.Use();
            }
        }

        void CreateNewAssetOfType(OdinMenuItem menuItem)
        {
            if (menuItem.Value is not UnityEngine.Object asset) return;

            var assetType = asset.GetType();
            var assetPath = AssetDatabase.GetAssetPath(asset);
            var directory = Path.GetDirectoryName(assetPath);

            // Default name based on the type
            var defaultName = $"New{assetType.Name}.asset";

            // SaveFilePanel returns absolute path, need to convert to relative
            var defaultPath = Path.Combine(Application.dataPath.Replace("Assets", ""), directory);

            var absolutePath = EditorUtility.SaveFilePanel(
                $"Create new {assetType.Name}",
                defaultPath,
                defaultName,
                "asset"
            );

            if (string.IsNullOrEmpty(absolutePath)) return; // User cancelled

            // Convert absolute path to project-relative path
            var relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);

            // Check if file already exists
            if (File.Exists(relativePath))
            {
                EditorUtility.DisplayDialog("File Exists", "A file with that name already exists.", "OK");
                return;
            }

            // Create the ScriptableObject instance
            var newAsset = ScriptableObject.CreateInstance(assetType);
            AssetDatabase.CreateAsset(newAsset, relativePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Rebuild tree and select the new asset
            ForceMenuTreeRebuild();
            TrySelectMenuItemWithObject(newAsset);
        }

        void DuplicateAsset(OdinMenuItem menuItem)
        {
            if (menuItem.Value is not UnityEngine.Object asset) return;

            var assetPath = AssetDatabase.GetAssetPath(asset);
            var directory = Path.GetDirectoryName(assetPath);
            var originalName = Path.GetFileNameWithoutExtension(assetPath);

            // Find a unique name with numeric suffix
            var newName = FindUniqueAssetName(directory, originalName);
            var newPath = Path.Combine(directory, newName + ".asset");

            // Use Unity's CopyAsset which preserves all data
            var success = AssetDatabase.CopyAsset(assetPath, newPath);

            if (success)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // Rebuild tree and select the duplicated asset
                ForceMenuTreeRebuild();
                var duplicatedAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(newPath);
                TrySelectMenuItemWithObject(duplicatedAsset);
            }
            else
            {
                Debug.LogError($"Failed to duplicate asset: {assetPath}");
            }
        }

        string FindUniqueAssetName(string directory, string baseName)
        {
            // Check if the name already ends with a number in parentheses
            var nameWithoutNumber = baseName;
            var startNumber = 1;

            // Pattern to match names like "MyAsset (2)"
            var match = System.Text.RegularExpressions.Regex.Match(baseName, @"^(.*?)\s*\((\d+)\)$");
            if (match.Success)
            {
                nameWithoutNumber = match.Groups[1].Value;
                startNumber = int.Parse(match.Groups[2].Value) + 1;
            }

            // Find next available number
            string testName;
            int counter = startNumber;
            do
            {
                testName = $"{nameWithoutNumber} ({counter})";
                var testPath = Path.Combine(directory, testName + ".asset");

                if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(testPath) == null)
                {
                    return testName;
                }
                counter++;
            } while (counter < 1000); // Safety limit

            // Fallback
            return $"{nameWithoutNumber} ({counter})";
        }

        void StartRename(OdinMenuItem menuItem)
        {
            if (menuItem.Value is not UnityEngine.Object asset) return;

            var assetPath = AssetDatabase.GetAssetPath(asset);
            var currentName = Path.GetFileNameWithoutExtension(assetPath);
            var directory = Path.GetDirectoryName(assetPath);

            // Alternative: Use a simpler approach with EditorInputDialog from Odin
            // This creates an inline text field in a popup window
            var renamePopup = new RenamePopup { AssetName = currentName };
            var result = InspectObjectInDropDown(
                renamePopup,
                300
            );

            if (result != null && !string.IsNullOrWhiteSpace(renamePopup.AssetName) && renamePopup.AssetName != currentName)
            {
                PerformRename(assetPath, directory, renamePopup.AssetName);
            }
        }

        void PerformRename(string assetPath, string directory, string newName)
        {
            var newPath = Path.Combine(directory, newName + ".asset");

            // Check if target already exists
            if (AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(newPath) != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "File Exists",
                    $"An asset named '{newName}' already exists in this location. Rename cancelled.",
                    "OK"))
                {
                    return;
                }
                return;
            }

            var errorMessage = AssetDatabase.RenameAsset(assetPath, newName);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                Debug.LogError($"Failed to rename asset: {errorMessage}");
            }
            else
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                ForceMenuTreeRebuild();
            }
        }

        void DeleteAsset(OdinMenuItem menuItem)
        {
            DeleteAssets(new[] { menuItem });
        }

        void DeleteAssets(IEnumerable<OdinMenuItem> menuItems)
        {
            var itemsToDelete = menuItems.Where(x => x.Value is UnityEngine.Object).ToList();
            if (itemsToDelete.Count == 0) return;

            var assetNames = string.Join("\n", itemsToDelete.Select(x => x.Name));
            var message = itemsToDelete.Count == 1
                ? $"Are you sure you want to delete '{itemsToDelete[0].Name}'?"
                : $"Are you sure you want to delete {itemsToDelete.Count} assets?\n\n{assetNames}";

            // DisplayDialogComplex creates a dialog near the mouse cursor
            var choice = EditorUtility.DisplayDialogComplex(
                "Delete Assets",
                message,
                "Delete",
                "Cancel",
                ""
            );

            if (choice != 0) return; // User cancelled (0 = Delete, 1 = Cancel)

            foreach (var item in itemsToDelete)
            {
                if (item.Value is UnityEngine.Object asset)
                {
                    var path = AssetDatabase.GetAssetPath(asset);
                    AssetDatabase.DeleteAsset(path);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            ForceMenuTreeRebuild();
        }

        void PingAsset(OdinMenuItem menuItem)
        {
            if (menuItem.Value is UnityEngine.Object asset)
            {
                // Ping highlights the asset in the Project window with a yellow flash
                EditorGUIUtility.PingObject(asset);

                // Also select it in the Project window
                Selection.activeObject = asset;
            }
        }

        // Helper class for rename popup
        class RenamePopup
        {
            [HideLabel]
            public string AssetName;
        }
    }
}
