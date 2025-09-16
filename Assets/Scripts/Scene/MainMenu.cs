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

    [Header("Audio")]
    public AudioClip navigateClip;
    public AudioClip selectClip;
    private AudioSource audioSource;

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

        // Add or reuse AudioSource
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D sound
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            currentIndex--;
            if (currentIndex < 0) currentIndex = menuOptions.Length - 1;
            UpdateSelection();
            PlayNavigateSound();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            currentIndex++;
            if (currentIndex >= menuOptions.Length) currentIndex = 0;
            UpdateSelection();
            PlayNavigateSound();
        }

        if (Input.GetKeyDown(KeyCode.Space)) // Space = select
        {
            SelectOption();
            PlaySelectSound();
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

    private void PlayNavigateSound()
    {
        if (navigateClip != null)
            audioSource.PlayOneShot(navigateClip, 0.7f); // softer
    }

    private void PlaySelectSound()
    {
        if (selectClip != null)
            audioSource.PlayOneShot(selectClip, 1f);
    }
}
