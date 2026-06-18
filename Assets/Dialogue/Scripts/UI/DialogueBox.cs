using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private ResponseManager responseManager;
    [SerializeField] private MenuCard menuCard;

    [Header("Objects")]
    [SerializeField] private Image dialogueBoxImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image characterImage;
    [SerializeField] private Wallet wallet;

    [Header("Typing Animation")]
    [SerializeField] private float defaultTypingSpeed;
    private WaitForSeconds typingSpeedWait;

    [Header("Characters")]
    private Character character;
    private CharacterManager characterManager;
    private CharacterSprite currentSprite;

    [Header("Phone Settings")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform bubbleParent;
    [SerializeField] private Vector2 bubbleSpawnPos;
    [SerializeField] private Vector2 offsetForTypingBubble;
    [SerializeField] private Vector2 offsetForImageBubble;
    [SerializeField] private Image sendButton;
    [SerializeField] private Sprite sendImage;
    [SerializeField] private Sprite voiceImage;
    [SerializeField] private int maxCharacters;
    private List<Bubble> bubbles;
    private Bubble newBubble;
    private bool hasSpaceForNewBubble;
    private bool isPhone;
    private bool isInCardMenu;

    public string objToSave { get; private set; }

    public void Setup(bool phone, CharacterManager givenManager)
    {
        characterManager = givenManager;
        isPhone = phone;
        if (phone)
            bubbles = new List<Bubble>();
        typingSpeedWait = new WaitForSeconds(defaultTypingSpeed);
        responseManager.Setup(phone);
    }

    public void LoadObj(GenericObj givenObj)
    {
        switch (currentSprite)
        {
            case CharacterSprite.Happy:
                characterManager.SetAnimation(CharacterAnimations.Happy, false);
                break;
            case CharacterSprite.Sad:
                characterManager.SetAnimation(CharacterAnimations.Sad, false);
                break;
        }

        switch (givenObj.type)
        {
            case NodeType.Dialogue:
                StartCoroutine(isPhone ? LoadDialoguePhone((Dialogue)givenObj) : LoadDialogue((Dialogue)givenObj));
                break;
            case NodeType.ResponseHolder:
                LoadResponses((ResponseHolder)givenObj);
                break;
            case NodeType.Event:
                LoadEvent((Event)givenObj);
                break;
            default:
                Debug.LogError("Current dialogue object is invalid.");
                break;
        }
    }

    private IEnumerator LoadDialogue(Dialogue dialogue)
    {
        dialogueManager.ToggleContinueButton(false);
        objToSave = dialogue.name;
        if (dialogue.character == null)
        {
            if (character == null)
            {
                character = dialogueManager.GetGraphCharacter();
                if (character == null)
                {
                    Debug.LogError("Graph has no character selected.");
                    yield break;
                }
            }
        }
        else
            character = dialogue.character;

        string charName = character.characterName;
        if (dialogue.overrideCharacterName != "")
            charName = dialogue.overrideCharacterName;
        nameText.text = charName;
        dialogueBoxImage.color = Color.white;
        currentSprite = dialogue.sprite;
        Sprite charSprite = currentSprite switch
        {
            CharacterSprite.None => null,
            CharacterSprite.Neutral => character.neutralSprite,
            CharacterSprite.Happy => character.happySprite,
            CharacterSprite.Angry => character.angrySprite,
            CharacterSprite.Sad => character.sadSprite,
            _ => characterImage.sprite
        };
        if (dialogue.overrideSprite != null)
            charSprite = dialogue.overrideSprite;
        characterImage.sprite = charSprite;
        characterManager.ChangeMaterial(currentSprite);
        switch (currentSprite)
        {
            case CharacterSprite.Happy:
                characterManager.SetAnimation(CharacterAnimations.Happy, true);
                break;
            case CharacterSprite.Sad:
                characterManager.SetAnimation(CharacterAnimations.Sad, true);
                break;
        }
        characterImage.color = Color.white;

        float typingSpeed = defaultTypingSpeed;
        if (dialogue.overrideTextSpeed > 0)
            typingSpeed = dialogue.overrideTextSpeed;
        typingSpeedWait = new WaitForSeconds(typingSpeed);
        string fullText = dialogue.text;
        int textLength = fullText.Length;
        for (int i = 1; i < textLength + 1; i++)
        {
            string text = fullText[..i];
            if (i < textLength)
            {
                if (fullText[i - 1].ToString() == "<")
                {
                    int charsTillEnd = 0;
                    while (true)
                    {
                        charsTillEnd++;
                        if (fullText[i - 1 + charsTillEnd].ToString() == ">")
                            break;
                    }

                    i += charsTillEnd;
                    text = fullText[..i];
                }
            }

            dialogueText.text = text;
            yield return typingSpeedWait;
        }

        if (dialogue.nextObj != null)
            dialogueManager.ToggleContinueButton(true);
    }

    private IEnumerator LoadDialoguePhone(Dialogue dialogue)
    {
        if (!hasSpaceForNewBubble)
            MoveBubblesUp(offsetForTypingBubble);

        if (!newBubble)
        {
            newBubble = Instantiate(bubblePrefab, bubbleSpawnPos, Quaternion.identity, bubbleParent)
                .GetComponent<Bubble>();
            bubbles.Add(newBubble);
        }

        yield return StartCoroutine(newBubble.Load(this, dialogue.text, true));

        hasSpaceForNewBubble = false;
        newBubble = null;

        if (dialogue.nextObj != null)
            dialogueManager.Continue(dialogue.nextObj);
    }

    private void LoadResponses(ResponseHolder responseHolder)
    {
        dialogueManager.ToggleContinueButton(false);
        responseManager.LoadResponses(responseHolder);
    }

    private void LoadEvent(Event eventObj)
    {
        if (!isPhone)
        {
            objToSave = eventObj.name;
            dialogueManager.ToggleContinueButton(false);
        }

        switch (eventObj.eventType)
        {
            case EventType.Delay:
                StartCoroutine(isPhone ? DelayPhone(eventObj) : Delay(eventObj));
                break;
            case EventType.StartMinigame:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                StartMinigame(eventObj);
                break;
            case EventType.EndDialogue:
                EndDialogue();
                break;
            case EventType.SendImage:
                if (!isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                SendImage(eventObj);
                break;
            case EventType.NPCAppear:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                StartCoroutine(NPCAppear(eventObj));
                break;
            case EventType.NPCLeave:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                StartCoroutine(NPCLeave(eventObj));
                break;
            case EventType.ShowMenu:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                StartCoroutine(ShowMenu(eventObj));
                break;
            case EventType.WalletSlide:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                StartCoroutine(SlideWallet(eventObj));
                break;
            case EventType.MoveToBartGraph:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                MoveToBartGraph(eventObj);
                break;
            case EventType.ShowCat:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                ShowCat(eventObj);
                break;
            case EventType.CatLeave:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                CatLeave(eventObj);
                break;
            case EventType.SwitchSettingsToBart:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                SwitchSettingsToBart(eventObj);
                break;
            case EventType.BartSitDown:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }
                StartCoroutine(BartSitDown(eventObj));
                break;
            default:
                Debug.LogError("No event type selected on event node " + eventObj.name);
                break;
        }
    }

    private IEnumerator Delay(Event eventObj)
    {
        Color dialogueTextColor = Color.black;
        Color nameTextColor = Color.black;

        if (eventObj.removeItems)
            menuCard.RemoveItems();

        if (eventObj.hideDialogueBox)
        {
            dialogueBoxImage.color = Color.clear;
            dialogueTextColor = dialogueText.color;
            dialogueText.color = Color.clear;
            nameTextColor = nameText.color;
            nameText.color = Color.clear;

            if (!eventObj.keepText)
            {
                dialogueText.text = "";
                nameText.text = "";
            }
        }

        if (eventObj.hidePortrait)
            characterImage.color = Color.clear;

        WaitForSeconds delayTime = new(eventObj.delay);
        yield return delayTime;

        if (eventObj.hideDialogueBox)
        {
            dialogueBoxImage.color = Color.white;
            dialogueText.color = dialogueTextColor;
            nameText.color = nameTextColor;
        }

        if (eventObj.hidePortrait)
            characterImage.color = Color.white;

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private IEnumerator DelayPhone(Event eventObj)
    {
        if (!hasSpaceForNewBubble && eventObj.hideDialogueBox)
        {
            MoveBubblesUp(offsetForTypingBubble);
            CreateTypingBubble();
        }

        WaitForSeconds delayTime = new(eventObj.delay);
        yield return delayTime;

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private void StartMinigame(Event eventObj)
    {
        PreEventCheck(eventObj);

        dialogueManager.StartMinigame(eventObj.minigame);
    }

    private void EndDialogue()
    {
        if (isPhone)
            dialogueManager.GoToDialogueScene();
        else
            dialogueManager.GoToMainMenu(false, true);
    }

    private void SendImage(Event eventObj)
    {
        if (!hasSpaceForNewBubble)
            MoveBubblesUp(offsetForImageBubble);

        if (!newBubble)
        {
            newBubble = Instantiate(bubblePrefab, bubbleSpawnPos, Quaternion.identity, bubbleParent)
                .GetComponent<Bubble>();
            bubbles.Add(newBubble);
        }

        StartCoroutine(newBubble.Load(eventObj.imageToSend));

        newBubble = null;
        hasSpaceForNewBubble = false;

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private void MoveToBartGraph(Event eventObj)
    {
        PreEventCheck(eventObj);

        dialogueManager.SwitchToBart();
    }

    private void SwitchSettingsToBart(Event eventObj)
    {
        PreEventCheck(eventObj);

        dialogueManager.SwitchSettingsToBart();
        character = dialogueManager.GetGraphCharacter();

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private void ShowCat(Event eventObj)
    {
        PreEventCheck(eventObj);

        dialogueManager.ShowCat();

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private void CatLeave(Event eventObj)
    {
        PreEventCheck(eventObj);

        dialogueManager.CatLeave();

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private IEnumerator NPCAppear(Event eventObj)
    {
        PreEventCheck(eventObj);

        yield return StartCoroutine(characterManager.StartWalkToSeat());

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private IEnumerator NPCLeave(Event eventObj)
    {
        PreEventCheck(eventObj);

        yield return StartCoroutine(characterManager.LeaveSeat());

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private IEnumerator ShowMenu(Event eventObj)
    {
        dialogueManager.ToggleEscapeButton(false);

        PreEventCheck(eventObj);

        menuCard.Show(character.dessert, character.drink);
        isInCardMenu = true;
        while (isInCardMenu)
            yield return null;

        dialogueManager.ToggleEscapeButton(true);

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private IEnumerator BartSitDown(Event eventObj)
    {
        yield return StartCoroutine(dialogueManager.BartSitDown());

        characterManager = dialogueManager.GetCharacterManager();

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    public void FinishMenu() => isInCardMenu = false;

    private IEnumerator SlideWallet(Event eventObj)
    {
        PreEventCheck(eventObj);

        yield return wallet.StartSlide(character.hasCharm);

        if (eventObj.nextObj != null)
            dialogueManager.Continue(eventObj.nextObj);
    }

    private void PreEventCheck(Event eventObj)
    {
        if (eventObj.removeItems)
            menuCard.RemoveItems();

        if (eventObj.hideDialogueBox)
        {
            dialogueBoxImage.color = Color.clear;

            if (!eventObj.keepText)
            {
                dialogueText.text = "";
                nameText.text = "";
            }
        }

        if (eventObj.hidePortrait)
            characterImage.color = Color.clear;
    }

    public void MoveBubblesExceptForLast(float bubbleHeight)
    {
        if (offsetForTypingBubble.y >= bubbleHeight) return;

        bubbleHeight -= offsetForTypingBubble.y;
        Vector2 amt = new(offsetForTypingBubble.x, bubbleHeight);

        hasSpaceForNewBubble = true;
        for (int i = 0; i < bubbles.Count; i++)
        {
            if (i == bubbles.Count - 1) break;
            bubbles[i].rect.anchoredPosition += amt;
        }
    }

    private void MoveBubblesUp(Vector2 amt)
    {
        hasSpaceForNewBubble = true;
        foreach (Bubble bubble in bubbles)
            bubble.rect.anchoredPosition += amt;
    }

    private void CreateTypingBubble()
    {
        Bubble bubble = Instantiate(bubblePrefab, bubbleSpawnPos, Quaternion.identity, bubbleParent)
            .GetComponent<Bubble>();
        bubbles.Add(bubble);
        StartCoroutine(bubble.Load(null, null, true));
        newBubble = bubble;
    }

    public IEnumerator CreatePlayerBubble(string sentText)
    {
        int textLength = sentText.Length;
        sendButton.sprite = sendImage;
        for (int i = 1; i < textLength + 1; i++)
        {
            string text = i > maxCharacters ? sentText[(i - maxCharacters)..i] : sentText[..i];
            if (i < textLength)
            {
                switch (sentText[i - 1].ToString())
                {
                    case "<":
                    {
                        int charsTillEnd = 0;
                        while (true)
                        {
                            charsTillEnd++;
                            if (sentText[i - 1 + charsTillEnd].ToString() == ">")
                                break;
                        }

                        i += charsTillEnd;
                        text = i > maxCharacters ? sentText[(i - maxCharacters)..i] : sentText[..i];
                        break;
                    }
                    case "\\":
                    {
                        int charsTillEnd = 0;
                        while (true)
                        {
                            charsTillEnd++;
                            int newIdx = i - 1 + charsTillEnd;
                            if (newIdx >= textLength)
                            {
                                charsTillEnd--;
                                break;
                            }
                            if (sentText[newIdx].ToString() == " ")
                                break;
                        }

                        i += charsTillEnd;
                        text = i > maxCharacters ? sentText[(i - maxCharacters)..i] : sentText[..i];
                        break;
                    }
                }
            }

            dialogueText.text = text;
            yield return typingSpeedWait;
        }

        if (!hasSpaceForNewBubble)
            MoveBubblesUp(offsetForTypingBubble);

        Bubble bubble = Instantiate(bubblePrefab, bubbleSpawnPos, Quaternion.identity, bubbleParent)
            .GetComponent<Bubble>();
        bubbles.Add(bubble);
        dialogueText.text = "";
        sendButton.sprite = voiceImage;

        yield return StartCoroutine(bubble.Load(this, sentText, false));

        hasSpaceForNewBubble = false;
        newBubble = null;
    }
}
