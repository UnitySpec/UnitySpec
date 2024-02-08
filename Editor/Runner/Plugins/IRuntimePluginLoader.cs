using UnityFlow.Tracing;

namespace UnityFlow.Plugins
{
    public interface IRuntimePluginLoader
    {
        IRuntimePlugin LoadPlugin(string pluginAssemblyName, ITraceListener traceListener, bool traceMissingPluginAttribute);
    }
}