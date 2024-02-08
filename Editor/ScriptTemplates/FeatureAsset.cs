using System;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class FeatureAsset : ScriptableObject
{

    [SerializeField]
    private TextAsset contents;

    [HideInInspector] public string filePath;

    private FeatureData data = new();

    public void Save()
    {
        GenerateNewInstance();

        if (filePath == null)
        {
            Debug.LogError("Failed to save feature");
        }
        using StreamWriter streamWriter = new(filePath);
        streamWriter.Write(data.contents);
    }

    private void GenerateNewInstance()
    {
        data ??= new FeatureData();

        data.contents = contents;
    }

    public void Load()
    {
        //using StreamReader streamReader = new StreamReader(filePath);
        //contents = streamReader.ReadToEnd();
        //AssetDatabase.ImportAsset(filePath);
        contents = Resources.Load<TextAsset>(filePath);
    }
}

[Serializable]
public sealed class FeatureData
{
    public TextAsset contents;
}