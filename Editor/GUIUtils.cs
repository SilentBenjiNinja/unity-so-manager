using System;
using System.Linq;
using bnj.so_manager.Runtime;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace bnj.so_manager.Editor
{
    // Credit to Sirenix Tutorial:
    // https://youtu.be/1zu41Ku46xU
    public static class GUIUtils
    {
        const float MinButtonWidth = 80f; // Minimum width for each button
        const int MinButtonsPerRow = 1;
        const int DefaultMaxButtonsPerRow = 8;

        public static bool SelectButtonList(ref Type selectedType, Type[] typesToDisplay, float availableWidth)
        {
            var totalTypes = typesToDisplay.Length;

            // Calculate max buttons per row based on available width
            var maxButtonsPerRow = availableWidth > 0
                ? Mathf.Max(MinButtonsPerRow, Mathf.FloorToInt(availableWidth / MinButtonWidth))
                : DefaultMaxButtonsPerRow;

            // Calculate optimal row count and buttons per row for balanced layout
            var rowCount = Mathf.CeilToInt((float)totalTypes / maxButtonsPerRow);
            var buttonsPerRow = Mathf.CeilToInt((float)totalTypes / rowCount);

            var buttonHeight = Mathf.Clamp(500f / Mathf.Min(totalTypes, maxButtonsPerRow), 30f, 50f);
            var totalHeight = buttonHeight * rowCount;

            var fullRect = GUILayoutUtility.GetRect(0, totalHeight);

            for (int i = 0; i < totalTypes; i++)
            {
                var type = typesToDisplay[i];

                var name = type.GetAttribute<ManageableDataAttribute>().tabName;

                var firstInstanceGuid = AssetDatabase.FindAssets($"t:{type.Name}", new[] { "Assets/ScriptableObjects" }).FirstOrDefault();
                var firstInstancePath = AssetDatabase.GUIDToAssetPath(firstInstanceGuid);
                var firstInstance = AssetDatabase.LoadAssetAtPath(firstInstancePath, type);

                var icon = firstInstance == null ? EditorIcons.TestInconclusive : (EditorGUIUtility.GetIconForObject(firstInstance) ?? EditorIcons.UnityInfoIcon);

                // Calculate which row and column this button is in
                var row = i / buttonsPerRow;
                var col = i % buttonsPerRow;
                var buttonsInThisRow = Mathf.Min(buttonsPerRow, totalTypes - (row * buttonsPerRow));

                // Calculate the rect for this button
                var rowRect = new Rect(
                    fullRect.x,
                    fullRect.y + (row * buttonHeight),
                    fullRect.width,
                    buttonHeight
                );

                var buttonRect = rowRect.Split(col, buttonsInThisRow);
                var isSelected = type == selectedType;

                if (SelectButton(buttonRect, new() { image = icon, tooltip = name }, isSelected))
                {
                    selectedType = type;
                    return true;
                }
            }

            return false;
        }

        public static bool SelectButton(Rect rect, GUIContent content, bool selected)
        {
            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                return true;

            if (Event.current.type == EventType.Repaint)
            {
                var style = new GUIStyle(EditorStyles.miniButton)
                {
                    fixedHeight = rect.height,
                    padding = new(6, 6, 6, 6)
                };
                style.Draw(rect, content, false, false, selected, false);
            }

            return false;
        }
    }
}
