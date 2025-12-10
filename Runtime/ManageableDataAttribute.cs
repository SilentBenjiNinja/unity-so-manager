using System;

namespace bnj.so_manager.Runtime
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ManageableDataAttribute : Attribute
    {
        public readonly string tabName;
        public int Order { get; set; }

        public ManageableDataAttribute(string tabName)
        {
            this.tabName = tabName;
        }
    }
}
