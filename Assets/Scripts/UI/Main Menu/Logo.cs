using System.Collections;
using UnityEngine;

public class Logo : MonoBehaviour
{
    [SerializeField] private RectTransform rect;

    [SerializeField] private Vector2 startPos;
    [SerializeField] private Vector2 endPos;
    [SerializeField] private Vector2 endAnchor;
    [SerializeField] private float moveTime;
    [SerializeField] private float timeUntilMoving;
    private WaitForSeconds waitTimeUntilMoving;

    private void Awake() => waitTimeUntilMoving = new WaitForSeconds(timeUntilMoving);

    private void Start() => StartCoroutine(Move());

    private IEnumerator Move()
    {
        yield return waitTimeUntilMoving;

        rect.anchorMin = endAnchor;
        rect.anchorMax = endAnchor;
        rect.pivot = endAnchor;

        rect.anchoredPosition = startPos;

        for (float i = 0; i < moveTime; i += Time.deltaTime)
        {
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, i * moveTime);
            yield return null;
        }
    }
}
