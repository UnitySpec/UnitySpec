using System;

namespace UnitySpec.Generator
{
    public interface ITestHeaderWriter
    {
        Version DetectGeneratedTestVersion(string generatedTestContent);
    }
}
