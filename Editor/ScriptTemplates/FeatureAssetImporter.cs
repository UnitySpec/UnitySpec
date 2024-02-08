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

    public override void OnImportAsset(AssetImportContext ctx)
    {
        // Create our FeatureAsset
        var asset = ScriptableObject.CreateInstance<FeatureAsset>();
        var assetPath = ctx.assetPath;
        asset.filePath = assetPath;
        asset.Load();
        ctx.AddObjectToAsset(GUID.Generate().ToString(), asset);
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

