namespace UnityFlow.Generator.Project
{
    public interface ISpecFlowProjectReader
    {
        SpecFlowProject ReadSpecFlowProject(string projectFilePath, string rootNamespace);
    }
}