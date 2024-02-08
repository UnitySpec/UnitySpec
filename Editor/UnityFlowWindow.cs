using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityFlow.Generator;

public class UnityFlowWindow : EditorWindow
{
    Compiler compiler = new Compiler();

    [MenuItem("Window/UnityFlow")]
    public static void ShowUnityFlowWindow()
    {
        UnityFlowWindow wnd = GetWindow<UnityFlowWindow>();
        wnd.titleContent = new GUIContent("UnityFlow");
    }

    public void CreateGUI()
    {
        rootVisualElement.Add(GetLabel());
        rootVisualElement.Add(GetButton());
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
            compiler.Generate();
            EditorUtility.RequestScriptReload();
        });
        return button;
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
}
