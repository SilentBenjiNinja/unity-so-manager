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
        public static bool SelectButtonList(ref Type selectedType, Type[] typesToDisplay)
        {
            // TODO: add second row if too many types!
            var height = Mathf.Clamp(500f / typesToDisplay.Length, 50f, 90f);
            var rect = GUILayoutUtility.GetRect(0, height);

            for (int i = 0; i < typesToDisplay.Length; i++)
            {
                var type = typesToDisplay[i];

                var name = type.GetAttribute<ManageableDataAttribute>().tabName;

                var firstInstanceGuid = AssetDatabase.FindAssets($"t:{type.Name}", new[] { "Assets/ScriptableObjects" }).FirstOrDefault();
                var firstInstancePath = AssetDatabase.GUIDToAssetPath(firstInstanceGuid);
                var firstInstance = AssetDatabase.LoadAssetAtPath(firstInstancePath, type);

                var icon = firstInstance == null ? EditorIcons.TestInconclusive : (EditorGUIUtility.GetIconForObject(firstInstance) ?? EditorIcons.UnityInfoIcon);

                var buttonRect = rect.Split(i, typesToDisplay.Length);
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
