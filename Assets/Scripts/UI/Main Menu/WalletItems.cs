using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class WalletItems : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform[] items;
    [SerializeField] private Vector2[] moveOnHover;
    [SerializeField] private float moveTime;

    private bool isHovering;
    private Coroutine moveCoroutine;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isHovering) return;
        isHovering = true;
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Move());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isHovering) return;
        isHovering = false;
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(Move());
    }

    private IEnumerator Move()
    {
        Vector2[] startPos = new Vector2[items.Length];
        Vector2[] endPos = new Vector2[items.Length];

        for (int i = 0; i < items.Length; i++)
        {
            startPos[i] = items[i].localPosition;
            endPos[i] = isHovering ? moveOnHover[i] : Vector2.zero;
        }

        for (float i = 0; i < moveTime; i += Time.deltaTime)
        {
            float moveAmt = i / moveTime;
            for (int j = 0; j < items.Length; j++) 
                items[j].localPosition = Vector2.Lerp(startPos[j], endPos[j], moveAmt);
            yield return null;
        }
    }
}
