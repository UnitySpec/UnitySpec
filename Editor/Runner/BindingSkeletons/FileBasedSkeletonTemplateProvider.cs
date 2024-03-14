using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnitySpec.ErrorHandling;

namespace UnitySpec.BindingSkeletons
{
    public class FileBasedSkeletonTemplateProvider : ISkeletonTemplateProvider
    {
        public string GetStepDefinitionTemplate(ProgrammingLanguage language, bool withRegex)
        {
            string template;
            if (withRegex)
            {
                template = @"[{attribute}(@""{regex}"")]
public void {methodName}({parameters})
{
    _scenarioContext.Pending();
}";
            } else
            {
                template = @"[{attribute}]
public void {methodName}({parameters})
{
    _scenarioContext.Pending();
}";
            }
            return template;
        }

        public string GetStepDefinitionClassTemplate(ProgrammingLanguage language)
        {
            return @"using System;
using UnityFlow;

namespace {namespace}
{
    [Binding]
    public class {className}
    {
        private readonly ScenarioContext _scenarioContext;

        public {className}(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }
{bindings}
    }
}";
        }
    }
}