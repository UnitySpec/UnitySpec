using System;

namespace UnitySpec.Runner.Attributes
{
    [AttributeUsage(
        AttributeTargets.Property |
        AttributeTargets.Field,
        AllowMultiple = true)]
    public class TableAliasesAttribute : Attribute
    {
        public TableAliasesAttribute(params string[] aliases)
        {
            Aliases = aliases;
        }

        public string[] Aliases { get; }
    }
}