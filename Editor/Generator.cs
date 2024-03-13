using System.IO;
using UnityEngine;
using UnityFlow.General.Build;
using UnityFlow.Generator;

public class Generator
{
    private string _projectFolder;
    private string _outputPath;
    private RunGenerator _runGenerator;
    private ITaskLoggingWrapper _logger;

    public Generator()
    {
        _projectFolder = Path.GetFullPath(Application.dataPath);
        _outputPath = Path.Combine(_projectFolder, "output");
        _logger = new UnityLogger();
        string root = Directory.GetParent(Application.dataPath).ToString();
        string _projectFilePath = Path.Combine(root, "Assembly-CSharp.csproj");
        _runGenerator = new RunGenerator(_projectFilePath, null, _logger);

    }
    internal void Generate(string specsPath, SearchOption searchOption = SearchOption.AllDirectories)
    {
        string[] files = Directory.GetFiles(specsPath, "*.feature", searchOption);

        var res = _runGenerator.Generate(files, _projectFolder, _outputPath);

        foreach (string path in res)
        {
            _logger.LogMessage($"Generated file in {path}");
        }
    }

    internal void Generate()
    {
        var settings = UnityFlowSettingsContainer.GetSettings();
        var folders = settings.FeatureFolder;
        foreach (string folder in folders)
        {
            string completeFolder = Path.Combine(Path.GetFullPath(Application.dataPath), folder);
            Generate(completeFolder, settings.SearchOption);
        }
    }
}
