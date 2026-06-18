using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform card;
    [SerializeField] private Vector2 moveOnHover;
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
        Vector2 startPos = card.localPosition;
        Vector2 endPos = Vector2.zero;
        if (isHovering)
            endPos = moveOnHover;

        for (float i = 0; i < moveTime; i += Time.deltaTime)
        {
            card.localPosition = Vector2.Lerp(startPos, endPos, i/moveTime);
            yield return null;
        }
    }
}
