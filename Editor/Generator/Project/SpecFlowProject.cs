using UnityFlow.General.GeneratorInterfaces;
using UnityFlow.Generator.Configuration;

namespace UnityFlow.Generator.Project
{
    public class SpecFlowProject
    {
        public ProjectSettings ProjectSettings { get; set; }
        public SpecFlowProjectConfiguration Configuration { get; set; }

        public SpecFlowProject()
        {
            ProjectSettings = new ProjectSettings();
            Configuration = new SpecFlowProjectConfiguration();
        }
    }
}