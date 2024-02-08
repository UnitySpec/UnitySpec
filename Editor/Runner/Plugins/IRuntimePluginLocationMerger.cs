using System.Collections.Generic;

namespace UnityFlow.Plugins
{
    public interface IRuntimePluginLocationMerger
    {
        IReadOnlyList<string> Merge(IReadOnlyList<string> pluginPaths);
    }
}