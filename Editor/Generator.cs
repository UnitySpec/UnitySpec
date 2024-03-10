using System.IO;
using UnityEngine;
using UnityFlow.Generator;

public class Generator
{
    private string _projectFilePath;
    private string _projectFolder;
    private string _outputPath;
    private string _rootNamespace;
    private RunGenerator _runGenerator;

    public Generator()
    {
        _projectFolder = Path.GetFullPath(Application.dataPath);
        _outputPath = Path.Combine(_projectFolder, "output");
        _rootNamespace = Path.GetFullPath(Application.dataPath);
        _projectFilePath = Path.Combine(_projectFolder, "..", "Assembly-CSharp.csproj");
        _runGenerator = new RunGenerator(_projectFilePath, _rootNamespace, new UnityLogger());

    }
    internal void Generate(string specsPath)
    {
        string[] files = Directory.GetFiles(specsPath, "*.feature", SearchOption.AllDirectories);

        var res = _runGenerator.Generate(files, _projectFolder, _outputPath);

        foreach (string path in res)
        {
            Debug.Log($"Generated file in {path}");
        }
    }
}
