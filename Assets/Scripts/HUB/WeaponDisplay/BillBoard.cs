using UnityEngine;

public class Billboard : MonoBehaviour
{
    private void LateUpdate()
    {
        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}
