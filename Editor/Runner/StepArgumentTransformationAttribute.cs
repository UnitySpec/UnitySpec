using System;

namespace UnitySpec
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class StepArgumentTransformationAttribute : Attribute
    {
        public string Regex { get; set; }

        public StepArgumentTransformationAttribute(string regex)
        {
            Regex = regex;
        }

        public StepArgumentTransformationAttribute()
        {
            Regex = null;
        }
    }
}