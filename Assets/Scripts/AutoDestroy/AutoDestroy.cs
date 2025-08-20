using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float Time = 1f;
    void Start()
    {
        Destroy(gameObject, Time); // or whatever duration you want
    }
}
