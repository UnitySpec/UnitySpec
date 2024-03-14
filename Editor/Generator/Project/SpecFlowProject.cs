using UnitySpec.General.GeneratorInterfaces;
using UnitySpec.Generator.Configuration;

namespace UnitySpec.Generator.Project
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