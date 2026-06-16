using UnityEngine;

public class CameraKeeper : MonoBehaviour
{
    private static CameraKeeper instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
