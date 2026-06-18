using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Bubble : MonoBehaviour
{
    [Header("Bubble References")]
    [SerializeField] private TMP_Text text;
    public RectTransform rect;
    [SerializeField] private RectTransform bubbleRect;
    [SerializeField] private HorizontalLayoutGroup layoutGroup;
    [SerializeField] private Image bubbleImage;
    [SerializeField] private Image sentImage;

    [Header("Player")]
    [SerializeField] private Color playerColor;
    [SerializeField] private Vector2 playerAnchor;
    [SerializeField] private Vector2 playerPos;
    [SerializeField] private TextAnchor playerLayoutAnchor;
    [SerializeField] private Sprite playerImage;

    [Header("NPC")]
    [SerializeField] private Color npcColor;
    [SerializeField] private Vector2 npcAnchor;
    [SerializeField] private Vector2 npcPos;
    [SerializeField] private TextAnchor npcLayoutAnchor;
    [SerializeField] private Sprite npcImage;

    [Header("Typing Animation")]
    [SerializeField] private GameObject typingBubblesParent;
    [SerializeField] private Animator typingBubblesAnimator;
    private bool isTyping;
    private static readonly int ToggleAnim = Animator.StringToHash("Toggle");

    public IEnumerator Load(DialogueBox givenDialogueBox, string givenText, bool isNPC)
    {
        if (isTyping)
            StopTypingAnimation();

        bubbleImage.sprite = isNPC ? npcImage : playerImage;
        bubbleImage.color = isNPC ? npcColor : playerColor;

        text.text = givenText;
        text.gameObject.SetActive(true);

        yield return null;

        layoutGroup.childAlignment = isNPC ? npcLayoutAnchor : playerLayoutAnchor;
        rect.anchorMin = isNPC ? npcAnchor : playerAnchor;
        rect.anchorMax = isNPC ? npcAnchor : playerAnchor;
        rect.pivot = isNPC ? npcAnchor : playerAnchor;
        rect.anchoredPosition = isNPC ? npcPos : playerPos;

        if (givenText == null)
            StartTypingAnimation();
        else
            givenDialogueBox.MoveBubblesExceptForLast(bubbleRect.sizeDelta.y);
    }

    private void StartTypingAnimation()
    {
        isTyping = true;
        text.gameObject.SetActive(false);
        typingBubblesParent.SetActive(true);
        typingBubblesAnimator.SetBool(ToggleAnim, true);
    }

    private void StopTypingAnimation()
    {
        isTyping = false;
        typingBubblesAnimator.SetBool(ToggleAnim, false);
        typingBubblesParent.SetActive(false);
    }

    public IEnumerator Load(Sprite givenSprite)
    {
        text.gameObject.SetActive(false);
        bubbleImage.color = playerColor;
        bubbleImage.sprite = playerImage;

        sentImage.sprite = givenSprite;
        sentImage.gameObject.SetActive(true);

        yield return null;

        rect.anchorMin = playerAnchor;
        rect.anchorMax = playerAnchor;
        rect.pivot = playerAnchor;
        rect.anchoredPosition = playerPos;
    }
}
