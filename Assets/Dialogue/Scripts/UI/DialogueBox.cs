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

    [Header("Characters")]
    private Character character;
    private CharacterManager characterManager;

    [Header("Phone Settings")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform bubbleParent;
    [SerializeField] private Vector2 offsetForTypingBubble;
    [SerializeField] private Vector2 offsetForImageBubble;
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
        responseManager.Setup(phone);
    }

    public void LoadObj(GenericObj givenObj)
    {
        switch (givenObj.type)
        {
            case NodeType.Dialogue:
                if (isPhone)
                    StartCoroutine(LoadDialoguePhone((Dialogue)givenObj));
                else
                    LoadDialogue((Dialogue)givenObj);
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

    private void LoadDialogue(Dialogue dialogue)
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
                    return;
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
        Sprite charSprite = dialogue.sprite switch
        {
            CharacterSprite.None => null,
            CharacterSprite.Neutral => character.neutralSprite,
            CharacterSprite.Happy => character.happySprite,
            CharacterSprite.Angry => character.angrySprite,
            CharacterSprite.Sad => character.sadSprite,
            _ => characterImage.sprite
        };
        characterImage.sprite = charSprite;
        characterManager.ChangeMaterial(dialogue.sprite);
        characterImage.color = Color.white;
        dialogueText.text = dialogue.text;
        if (dialogue.nextObj != null)
            dialogueManager.ToggleContinueButton(true);
    }

    private IEnumerator LoadDialoguePhone(Dialogue dialogue)
    {
        if (!hasSpaceForNewBubble)
            MoveBubblesUp(offsetForTypingBubble);

        if (!newBubble)
        {
            newBubble = Instantiate(bubblePrefab, bubbleParent.position, Quaternion.identity, bubbleParent)
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
            newBubble = Instantiate(bubblePrefab, bubbleParent.position, Quaternion.identity, bubbleParent)
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

    private void MoveBubblesUp(Vector2 amt)
    {
        hasSpaceForNewBubble = true;
        foreach (Bubble bubble in bubbles)
            bubble.rect.anchoredPosition += amt;
    }

    public void CheckIfShouldMoveBubbles(float amtToGoUpTotal)
    {
        if (offsetForTypingBubble.y >= amtToGoUpTotal) return;

        amtToGoUpTotal -= offsetForTypingBubble.y;
        MoveBubblesUp(new Vector2(offsetForTypingBubble.x, amtToGoUpTotal));
    }

    private void CreateTypingBubble()
    {
        Bubble bubble = Instantiate(bubblePrefab, bubbleParent.position, Quaternion.identity, bubbleParent)
            .GetComponent<Bubble>();
        bubbles.Add(bubble);
        StartCoroutine(bubble.Load(null, null, true));
        newBubble = bubble;
    }

    public IEnumerator CreatePlayerBubble(string text)
    {
        if (!hasSpaceForNewBubble)
            MoveBubblesUp(offsetForTypingBubble);

        Bubble bubble = Instantiate(bubblePrefab, bubbleParent.position, Quaternion.identity, bubbleParent)
            .GetComponent<Bubble>();
        bubbles.Add(bubble);
        yield return StartCoroutine(bubble.Load(this, text, false));

        hasSpaceForNewBubble = false;
        newBubble = null;
    }
}
