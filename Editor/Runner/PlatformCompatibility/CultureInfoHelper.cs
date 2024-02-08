using System.Globalization;

namespace UnityFlow.Compatibility
{
    internal static class CultureInfoHelper
    {
        public static CultureInfo GetCultureInfo(string cultureName)
        {
            return CultureInfo.GetCultureInfo(cultureName);
        }
    }
}
