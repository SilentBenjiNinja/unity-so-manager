using System;

namespace bnj.so_manager.Runtime
{
    /// <summary>
    /// Marks a <see cref="UnityEngine.ScriptableObject"/> class as manageable in the
    /// <see cref="bnj.so_manager.Editor.ScriptableObjectManager"/> window.
    /// <para>
    /// Tagged types appear as tab buttons at the top of the window. Assets of the tagged type
    /// must be stored under <c>Assets/ScriptableObjects/</c> to be discovered.
    /// </para>
    /// <para>
    /// If <c>com.bnj.so-manager</c> is installed the <c>SO_MANAGER</c> scripting define symbol
    /// is added automatically, allowing optional use of this attribute via <c>#if SO_MANAGER</c>.
    /// </para>
    /// </summary>
    /// <example>
    /// <code>
    /// #if SO_MANAGER
    /// [ManageableData("Enemy Data", Order = 10)]
    /// #endif
    /// [CreateAssetMenu(menuName = "Game/Enemy Data")]
    /// public class EnemyData : ScriptableObject { }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Class)]
    public class ManageableDataAttribute : Attribute
    {
        /// <summary>
        /// The label shown on the tab button in the SO Manager window.
        /// </summary>
        public readonly string tabName;

        /// <summary>
        /// Controls the left-to-right order of the tab button relative to other managed types.
        /// Lower values appear first.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Marks this ScriptableObject type as manageable in the SO Manager window.
        /// </summary>
        /// <param name="tabName">The label shown on the tab button.</param>
        public ManageableDataAttribute(string tabName)
        {
            this.tabName = tabName;
        }
    }
}
