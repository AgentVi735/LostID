using UnityEngine;

public class EventSystemKeeper : MonoBehaviour
{
    private static EventSystemKeeper instance;

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
