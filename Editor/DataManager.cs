using System;
using System.IO;
using System.Linq;
using bnj.so_manager.Runtime;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace bnj.so_manager.Editor
{
    // TODO:
    // Button for adding a new object (+pick directory/filename prompt)
    // Button for renaming objects (https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetDatabase.RenameAsset.html)
    // Button for deleting objects
    // Button for selecting asset in project folder
    // Allow for multi-editing like in inspector?

    // Credit to Sirenix Tutorial:
    // https://youtu.be/1zu41Ku46xU
    public class DataManager : OdinMenuEditorWindow
    {
        static readonly Type[] _typesToDisplay =
            TypeCache.GetTypesWithAttribute<ManageableDataAttribute>()
            .OrderBy(x => x.GetAttribute<ManageableDataAttribute>().Order)
            .ToArray();

        Type _selectedType;

        [MenuItem("BNJ/SO Manager")]
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

            var tree = new OdinMenuTree(false, new OdinMenuTreeDrawingConfig
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
            });

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

            // TODO: make folders not selectable!

            var itemCount = tree.MenuItems.Count();
            if (itemCount > 0 && flatten) tree.MenuItems.First().Select();
            if (itemCount > 1 || !flatten) tree.FocusSearchField();

            return tree;
        }
    }
}
