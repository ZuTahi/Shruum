using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{   
    public Animator animator; // Assign Animator in Inspector
    public float transitionTime = 1f; // how long your fade takes

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(DoFade(sceneName));
    }

    private System.Collections.IEnumerator DoFade(string sceneName)
    {
        animator.SetTrigger("Start"); // play fade animation
        yield return new WaitForSeconds(transitionTime); // wait until fade finishes
        SceneManager.LoadScene(sceneName);
    }
}
