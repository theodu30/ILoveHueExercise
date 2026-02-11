using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private UIDocument menu;
    private VisualElement root;
    private Button level1;
    private Button level2;
    private Button level3;
    private Button level4;
    private Button level5;
    private Button quitButton;

    private void Awake()
    {
        menu = GetComponent<UIDocument>();
        root = menu.rootVisualElement;
        level1 = root.Q<Button>("Level1");
        level2 = root.Q<Button>("Level2");
        level3 = root.Q<Button>("Level3");
        level4 = root.Q<Button>("Level4");
        level5 = root.Q<Button>("Level5");
        quitButton = root.Q<Button>("Quit");
    }

    private void OnEnable()
    {
        level1.clicked += () => LoadLevel(1);
        level2.clicked += () => LoadLevel(2);
        level3.clicked += () => LoadLevel(3);
        level4.clicked += () => LoadLevel(4);
        level5.clicked += () => LoadLevel(5);
        quitButton.clicked += QuitGame;
    }

    private void OnDisable()
    {
        level1.clicked -= () => LoadLevel(1);
        level2.clicked -= () => LoadLevel(2);
        level3.clicked -= () => LoadLevel(3);
        level4.clicked -= () => LoadLevel(4);
        level5.clicked -= () => LoadLevel(5);
        quitButton.clicked -= QuitGame;
    }

    private void LoadLevel(int levelNumber)
    {
        SceneManager.LoadScene($"Level{levelNumber}");
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
