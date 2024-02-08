using System.Collections.Generic;

namespace UnityFlow.Plugins
{
    public interface IRuntimePluginLocator
    {
        IReadOnlyList<string> GetAllRuntimePlugins();
    }
}
