using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;

namespace bnj.so_manager.Editor
{
    /// <summary>
    /// Automatically adds the <c>SO_MANAGER</c> scripting define symbol when this package is present,
    /// allowing other packages to conditionally compile <c>[ManageableData]</c> usage via
    /// <c>#if SO_MANAGER</c> without taking a hard dependency on com.bnj.so-manager.
    /// </summary>
    [InitializeOnLoad]
    public static class SoManagerSymbolDefiner
    {
        private const string Symbol = "SO_MANAGER";

        static SoManagerSymbolDefiner()
        {
            AddSymbol();
        }

        private static void AddSymbol()
        {
            var target = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);

            PlayerSettings.GetScriptingDefineSymbols(target, out var defines);

            if (!defines.Contains(Symbol))
            {
                var updated = new List<string>(defines) { Symbol };
                PlayerSettings.SetScriptingDefineSymbols(target, updated.ToArray());
            }
        }
    }
}
