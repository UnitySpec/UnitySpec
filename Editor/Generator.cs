using System.IO;
using UnityEngine;
using UnityFlow.General.Build;
using UnityFlow.Generator;

public class Generator
{
    private string _projectFilePath;
    private string _projectFolder;
    private string _outputPath;
    private string _rootNamespace;
    private RunGenerator _runGenerator;
    private ITaskLoggingWrapper _logger;

    public Generator() 
    {
        _projectFolder = Path.GetFullPath(Application.dataPath);
        _outputPath = Path.Combine(_projectFolder, "output");
        _rootNamespace = Path.GetFullPath(Application.dataPath);
        _projectFilePath = Path.Combine(_projectFolder, "..", "Assembly-CSharp.csproj");
        _logger = new UnityLogger();
        _runGenerator = new RunGenerator(_projectFilePath, _rootNamespace, _logger);

    }
    internal void Generate(string specsPath)
    {
        string[] files = Directory.GetFiles(specsPath, "*.feature", SearchOption.AllDirectories);

        var res = _runGenerator.Generate(files, _projectFolder, _outputPath);

        foreach (string path in res)
        {
            _logger.LogMessage($"Generated file in {path}");
        }
    }
}
