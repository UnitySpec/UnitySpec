using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

/// <summary>
/// Import any files with the .feature extension
/// </summary>
[ScriptedImporter(1, "feature", AllowCaching = true)]
public sealed class FeatureAssetImporter : ScriptedImporter
{

    private const string FeatureExtension = "feature";

    private const string DefaultContent = @"
Feature: Feature1

A short summary of the feature

@tag1
Scenario: [scenario name]
	Given [context]
	When [action]
	Then [outcome]
    ";

    [MenuItem("Assets/Create/New Feature", false, 1)]
    private static void CreateNewFeature()
    {
        ProjectWindowUtil.CreateAssetWithContent(
            "NewFeature.feature",
            DefaultContent);
    }


    public override void OnImportAsset(AssetImportContext ctx)
    {
        var assetPath = ctx.assetPath;
        var contents = File.ReadAllText(assetPath);
        var asset = new TextAsset(contents);
        ctx.AddObjectToAsset("text", asset);
        ctx.SetMainObject(asset);

        // If extension not included in our project add it.
        TryIncludeFeatureExtension();
    }

    private static void TryIncludeFeatureExtension()
    {
        if (EditorSettings.projectGenerationUserExtensions.Contains(FeatureExtension)) return;
        var list = EditorSettings.projectGenerationUserExtensions.ToList();
        list.Add(FeatureExtension);
        EditorSettings.projectGenerationUserExtensions = list.ToArray();
        AssetDatabase.Refresh();
    }
}

