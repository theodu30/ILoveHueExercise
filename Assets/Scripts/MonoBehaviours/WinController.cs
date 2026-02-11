using UnityEngine;
using UnityEngine.UIElements;

public class WinController : MonoBehaviour
{
    private UIDocument winScreen;
    private VisualElement root;
    private VisualElement window;
    private Button homeRoot;
    private Button nextWindow;
    public string homeSceneName = "MainMenu";
    public string nextSceneName = "";

    private void Awake()
    {
        winScreen = GetComponent<UIDocument>();
        root = winScreen.rootVisualElement;
        window = root.Q<VisualElement>("window");
        homeRoot = root.Q<Button>("Home");
        nextWindow = window.Q<Button>("Next");
        window.style.display = DisplayStyle.None;
    }

    private void OnEnable()
    {
        GameEvents.OnGameWin += ShowWinScreen;
        homeRoot.clicked += OnHomeClicked;
        nextWindow.clicked += OnNextClicked;
    }

    private void OnDisable()
    {
        GameEvents.OnGameWin -= ShowWinScreen;
        homeRoot.clicked -= OnHomeClicked;
        nextWindow.clicked -= OnNextClicked;
    }

    private void ShowWinScreen(object sender, System.EventArgs e)
    {
        window.style.display = DisplayStyle.Flex;
    }

    private void OnHomeClicked()
    {
        if (!string.IsNullOrEmpty(homeSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(homeSceneName);
        }
    }

    private void OnNextClicked()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }
}
