using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using TechTalk.SpecFlow.Configuration;
using UnityFlow.Generator.CodeDom;

namespace UnityFlow.Generator.Generation
{
    public class LinePragmaHandler
    {
        private readonly SpecFlowConfiguration _specFlowConfiguration;
        private readonly CodeDomHelper _codeDomHelper;

        public LinePragmaHandler(SpecFlowConfiguration specFlowConfiguration, CodeDomHelper codeDomHelper)
        {
            _specFlowConfiguration = specFlowConfiguration;
            _codeDomHelper = codeDomHelper;
        }


        public void AddLinePragmaInitial(ClassDeclarationSyntax testType, string sourceFile)
        {
            if (_specFlowConfiguration.AllowDebugGeneratedFiles)
                return;

            _codeDomHelper.BindTypeToSourceFile(testType, Path.GetFileName(sourceFile));
        }
    }
}