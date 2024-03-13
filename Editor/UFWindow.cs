using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class UFWindow : EditorWindow
{
    private Generator _generator = new();

    [MenuItem("Window/UnityFlow")]
    public static void ShowExample()
    {
        UFWindow wnd = GetWindow<UFWindow>();
        wnd.titleContent = new GUIContent("UnityFlow");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

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
            _generator.Generate();
        });
        return button;
    }
}
