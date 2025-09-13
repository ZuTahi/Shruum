using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Settings")]
    public AudioSource audioSource;
    public AudioClip[] tracks;
    public float delayBetweenTracks = 5f;

    private int currentTrackIndex = 0;

    void Awake()
    {
        // Singleton so it survives across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (tracks.Length > 0 && audioSource != null)
        {
            StartCoroutine(PlayMusicLoop());
        }
    }

    IEnumerator PlayMusicLoop()
    {
        while (true)
        {
            // Play current track
            audioSource.clip = tracks[currentTrackIndex];
            audioSource.Play();

            // Wait until track finishes
            yield return new WaitForSeconds(audioSource.clip.length);

            // Small delay
            yield return new WaitForSeconds(delayBetweenTracks);

            // Move to next track (loop back if end)
            currentTrackIndex = (currentTrackIndex + 1) % tracks.Length;
        }
    }
}
