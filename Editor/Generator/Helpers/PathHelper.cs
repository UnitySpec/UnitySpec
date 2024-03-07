using System.IO;

namespace UnityFlow.Generator.Helpers
{
    public static class PathHelper
    {
        public static string SanitizeDirectorySeparatorChar(string path)
        {
            return path.Replace(@"\", $"{Path.DirectorySeparatorChar}").Replace(@"/", $"{Path.DirectorySeparatorChar}");
        }
    }
}
