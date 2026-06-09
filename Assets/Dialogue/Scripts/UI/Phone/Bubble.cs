using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bubble : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private RectTransform textRect;
    public RectTransform rect;
    [SerializeField] private Image bubbleImage;
    [SerializeField] private Image sentImage;

    [SerializeField] private Vector2 paddedArea;

    [SerializeField] private Color npcColor;
    [SerializeField] private Color playerColor;

    [SerializeField] private Vector2 npcAnchor;
    [SerializeField] private Vector2 playerAnchor;

    [SerializeField] private Vector2 npcPos;
    [SerializeField] private Vector2 playerPos;

    [SerializeField] private Sprite npcImage;
    [SerializeField] private Sprite playerImage;

    [SerializeField] private Animator typingBubblesAnimator;
    [SerializeField] private Vector2 typingBubbleSize;

    private bool isTyping;

    public IEnumerator Load(DialogueBox givenDialogueBox, string givenText, bool isNPC)
    {
        if (isTyping)
            StopTypingAnimation();

        bubbleImage.sprite = isNPC ? npcImage : playerImage;
        bubbleImage.color = isNPC ? npcColor : playerColor;

        text.text = givenText;
        text.gameObject.SetActive(true);

        yield return null;

        Vector2 newSize = text.GetPreferredValues();
        if (newSize.y is > 61 and < 62 && !text.text.Contains("<br>"))
            newSize = new Vector2(newSize.x, 30);
        textRect.sizeDelta = newSize;
        rect.sizeDelta = text.GetPreferredValues() + paddedArea;
        if (givenText == null) 
            rect.sizeDelta = typingBubbleSize;
        rect.anchorMin = isNPC ? npcAnchor : playerAnchor;
        rect.anchorMax = isNPC ? npcAnchor : playerAnchor;
        rect.pivot = isNPC ? npcAnchor : playerAnchor;
        rect.anchoredPosition = isNPC ? npcPos : playerPos;

        if (givenText == null)
            StartTypingAnimation();
        else
            givenDialogueBox.CheckIfShouldMoveBubbles(rect.sizeDelta.y);
    }

    private void StartTypingAnimation()
    {
        isTyping = true;
        text.gameObject.SetActive(false);
        typingBubblesAnimator.gameObject.SetActive(true);
        typingBubblesAnimator.SetBool("Toggle", true);
    }

    private void StopTypingAnimation()
    {
        isTyping = false;
        typingBubblesAnimator.SetBool("Toggle", false);
        typingBubblesAnimator.gameObject.SetActive(false);
    }

    public IEnumerator Load(Sprite givenSprite)
    {
        text.gameObject.SetActive(false);
        bubbleImage.color = playerColor;
        bubbleImage.sprite = playerImage;

        sentImage.sprite = givenSprite;
        sentImage.gameObject.SetActive(true);

        yield return null;

        //rect.sizeDelta = new Vector2(sentImage.preferredWidth, sentImage.preferredHeight) + paddedArea;
        rect.sizeDelta = sentImage.rectTransform.sizeDelta + paddedArea;
        rect.anchorMin = playerAnchor;
        rect.anchorMax = playerAnchor;
        rect.pivot = playerAnchor;
        rect.anchoredPosition = playerPos;
    }
}
