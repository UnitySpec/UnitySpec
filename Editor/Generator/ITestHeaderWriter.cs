using System;

namespace UnityFlow.Generator
{
    public interface ITestHeaderWriter
    {
        Version DetectGeneratedTestVersion(string generatedTestContent);
    }
}
