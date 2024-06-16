using System.IO;
using UnityEngine;
using UnitySpec.General.Build;
using UnitySpec.Generator;

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
        string[] files;
        try
        {
            files = Directory.GetFiles(specsPath, "*.feature", searchOption);
        }
        catch (DirectoryNotFoundException)
        {
            throw new TestGeneratorException($"Could not find the directory {specsPath}, please set a valid path in Settings > UnitySpec Settings.");
        }

        var res = _runGenerator.Generate(files, _projectFolder, _outputPath);

        foreach (string path in res)
        {
            _logger.LogMessage($"Generated file in {path}");
        }
    }

    internal void Generate()
    {
        var settings = UnitySpecSettingsContainer.GetSettings();
        var folderNames = settings.FeatureFolder;
        foreach (string name in folderNames)
        {
            foreach (var folderPath in Directory.GetDirectories(Application.dataPath, name, SearchOption.AllDirectories))
            {
                string completeFolder = Path.Combine(Path.GetFullPath(Application.dataPath), folderPath);
                Generate(completeFolder, settings.SearchOption);
            }
        }
    }
}
