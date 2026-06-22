using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEditor.Rendering.MaterialUpgrader;

public class Logo : MonoBehaviour
{
    [Header("Logo")]
    [SerializeField] private RectTransform rect;
    [SerializeField] private Vector2 startPos;
    [SerializeField] private Vector2 endPos;
    [SerializeField] private Vector2 endAnchor;
    [SerializeField] private float moveTime;
    [SerializeField] private float timeUntilMoving;
    private WaitForSeconds waitTimeUntilMoving;

    [Header("Text")]
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private string textToWrite;
    [SerializeField] private float textSpeed;
    private WaitForSeconds waitTextSpeed;
    [SerializeField] private float delayUntilTextAppears;
    private WaitForSeconds waitDelayUntilTextAppears;


    private void Awake()
    {
        waitTimeUntilMoving = new WaitForSeconds(timeUntilMoving);
        waitTextSpeed = new WaitForSeconds(textSpeed);
        waitDelayUntilTextAppears = new WaitForSeconds(delayUntilTextAppears);
    }

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

        yield return waitDelayUntilTextAppears;

        int textLength = textToWrite.Length;
        for (int i = 0; i < textLength; i++)
        {
            infoText.text += textToWrite[i];
            yield return waitTextSpeed;
        }
    }
}
