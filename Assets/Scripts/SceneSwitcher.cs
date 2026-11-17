
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    [SerializeField] private InputAction GoToMenuAction;

    private static SceneSwitcher instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnEnable()
    {
        GoToMenuAction.Enable();
        GoToMenuAction.started += GoToMenu;
    }

    private void OnDisable()
    {
        GoToMenuAction.Disable();
        GoToMenuAction.started -= GoToMenu;
    }

    public void SceneSwitch(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SceneSwitch(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }

    private void GoToMenu(InputAction.CallbackContext context)
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        SceneSwitch("MainMenu");
    }
}
