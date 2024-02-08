using System.Text.RegularExpressions;

namespace UnityFlow.Bindings
{
    internal static class RegexFactory
    {
        private static RegexOptions RegexOptions = RegexOptions.CultureInvariant;

        public static Regex Create(string regexString)
        {
            return regexString == null ? null : new Regex("^" + regexString + "$", RegexOptions);
        }
    }
}
