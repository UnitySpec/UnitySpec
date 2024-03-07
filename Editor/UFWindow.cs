using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UFWindow : EditorWindow
{
    private Generator _generator = new();
    [SerializeField]
    string _specsPath;

    [MenuItem("Window/UnityFlow")]
    public static void ShowExample()
    {
        //_specsPath = "Specs";
        //_generator = new Generator();
        //UFWindow wnd = CreateInstance<UFWindow>();

        UFWindow wnd = GetWindow<UFWindow>();
        wnd.titleContent = new GUIContent("UnityFlow");
        //wnd.Show();
    }


    //public void OnGUI()
    //{
    //    if (_specsPath is null)
    //    {
    //        _specsPath = "Specs";
    //    }
    //    GUILayout.Space(10);
    //    EditorGUILayout.LabelField("Click the button below to generate the test files.", EditorStyles.wordWrappedLabel);
    //    EditorGUILayout.LabelField("You can edit the path to the feature files below.", EditorStyles.wordWrappedLabel);
    //    GUILayout.Space(10);
    //    _specsPath = EditorGUILayout.TextField("Path to features: (Assets\\..)", _specsPath);
    //    GUILayout.Space(10);
    //    if(GUILayout.Button("Generate"))
    //    {
    //        string fullpath = Path.Combine(Path.GetFullPath(Application.dataPath), _specsPath);
    //        _generator.Generate(fullpath);
    //        EditorUtility.RequestScriptReload();
    //    }
    //}
    public void CreateGUI()
    {
        _generator = new();
        _specsPath = Path.Combine(Path.GetFullPath(Application.dataPath), "Specs");
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        root.Add(GetLabel());
        root.Add(GetButton());
    }

    private static Label GetLabel()
    {
        return new Label()
        {
            style =
            {
                whiteSpace = WhiteSpace.Normal
            },
            text = "Welcome to UnityFlow, clik the button below to generate test files."
        };
    }

    private Button GetButton()
    {
        Button button = new Button()
        {
            style =
            {
                whiteSpace = WhiteSpace.Normal
            },
            text = "Click here to generate test files"
        };
        button.RegisterCallback((ClickEvent evt) =>
        {
            _generator.Generate(_specsPath);
            EditorUtility.RequestScriptReload();
        });
        return button;
    }
}
