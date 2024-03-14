using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Create a new type of Settings Asset.
class UnitySpecSettingsContainer : ScriptableObject
{
    public const string k_UnitySpecSettingsPath = "Assets/UnitySpec.asset";

    [SerializeField]
    private List<string> m_FeatureFolders;

    [SerializeField]
    private SearchOption m_SearchOption;

    internal static UnitySpecSettingsContainer GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<UnitySpecSettingsContainer>(k_UnitySpecSettingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<UnitySpecSettingsContainer>();
            settings.m_FeatureFolders = new List<string> { "Specs" };
            settings.m_SearchOption = SearchOption.AllDirectories;
            AssetDatabase.CreateAsset(settings, k_UnitySpecSettingsPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }

    internal static UnitySpecSettings GetSettings()
    {
        var settings = GetOrCreateSettings();
        return new UnitySpecSettings(settings.m_FeatureFolders, settings.m_SearchOption);
    }
   
}

class UnitySpecSettings
{
    public List<string> FeatureFolder { get; private set; }
    public SearchOption SearchOption { get; private set; }

    public UnitySpecSettings(List<string> featureFolders, SearchOption searchOption)
    {
        this.FeatureFolder = featureFolders;
        this.SearchOption = searchOption;
    }
}

// Register a SettingsProvider using IMGUI for the drawing framework:
static class UnitySpecRettingsIMGUIRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateUnitySpecSettingsProvider()
    {
        // First parameter is the path in the Settings window.
        // Second parameter is the scope of this setting: it only appears in the Project Settings window.
        var provider = new SettingsProvider("Project/UnitySpecSettings", SettingsScope.Project)
        {
            // By default the last token of the path is used as display name if no label is provided.
            label = "UnitySpec Settings",
            // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
            guiHandler = (searchContext) =>
            {
                var settings = UnitySpecSettingsContainer.GetSerializedSettings();
                EditorGUILayout.PropertyField(settings.FindProperty("m_FeatureFolders"), new GUIContent("Specification folders"));
                EditorGUILayout.PropertyField(settings.FindProperty("m_SearchOption"), new GUIContent("Search option"));
                settings.ApplyModifiedPropertiesWithoutUndo();
            },

            // Populate the search keywords to enable smart search filtering and label highlighting:
            keywords = new HashSet<string>(new[] { "UnitySpec", "Specs folder", "Feature folders" })
        };

        return provider;
    }
}