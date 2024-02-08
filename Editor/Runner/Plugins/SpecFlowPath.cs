namespace UnityFlow.Plugins
{
    public class SpecFlowPath : ISpecFlowPath
    {
        public string GetPathToSpecFlowDll()
        {
            return typeof(SpecFlowPath).Assembly.Location;
        }
    }
}
