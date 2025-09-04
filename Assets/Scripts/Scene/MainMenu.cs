using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TextMeshProUGUI[] menuOptions; // Assign Start & Exit TMP texts in inspector
    private int currentIndex = 0;
    public Color highlightedColor = Color.yellow;
    public Color normalColor = Color.white;
    private SceneTransition sceneTransition;

    private void Start()
    {
        UpdateSelection();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Find the transition in this scene
        sceneTransition = Object.FindFirstObjectByType<SceneTransition>();
        if (sceneTransition == null)
        {
            Debug.LogError("SceneTransition prefab is missing in this scene!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = menuOptions.Length - 1;
            UpdateSelection();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            currentIndex++;
            if (currentIndex >= menuOptions.Length) currentIndex = 0;
            UpdateSelection();
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Space = select
        {
            SelectOption();
        }
    }

    void UpdateSelection()
    {
        for (int i = 0; i < menuOptions.Length; i++)
        {
            if (i == currentIndex)
            {
                menuOptions[i].color = highlightedColor;
                menuOptions[i].fontSize = 150;
            }
            else
            {
                menuOptions[i].color = normalColor;
                menuOptions[i].fontSize = 120;
            }
        }
    }

    void SelectOption()
    {
        switch (currentIndex)
        {
            case 0: // Start
                Debug.Log("Starting game...");
                if (sceneTransition != null)
                {
                    sceneTransition.FadeToScene("HubScene");
                }
                else
                {
                    // Fallback if transition is missing
                    SceneManager.LoadScene("HubScene");
                }
                break;

            case 1: // Exit
                Debug.Log("Exiting game...");
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                break;
        }
    }
}
