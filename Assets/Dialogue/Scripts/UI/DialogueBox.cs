using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private ResponseManager responseManager;

    [SerializeField] private Image dialogueBoxImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image characterImage;

    private Character character;

    [Header("Phone Settings")]
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private Transform bubbleParent;
    [SerializeField] private Vector2 offsetForTypingBubble;
    [SerializeField] private Vector2 offsetForImageBubble;
    private bool hasSpaceForNewBubble;
    private Bubble newBubble;

    private List<Bubble> bubbles;

    private bool isPhone;

    public string objToSave { get; private set; }

    public void Setup(bool phone)
    {
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
            Debug.LogWarning("Dialogue " + dialogue.name + " has no character selected");
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
        if (dialogue.overrideCharacterName != string.Empty)
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
                EndDialogue(eventObj);
                break;
            case EventType.SendImage:
                if (!isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }

                SendImage(eventObj);
                break;
            case EventType.MoveToDifferentGraph:
                if (isPhone)
                {
                    Debug.LogError("Invalid event type selected on event node " + eventObj.name);
                    return;
                }

                MoveToDifferentGraph(eventObj);
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

        if (eventObj.hideDialogueBox)
        {
            dialogueBoxImage.color = Color.clear;
            dialogueTextColor = dialogueText.color;
            dialogueText.color = Color.clear;
            nameTextColor = nameText.color;
            nameText.color = Color.clear;

            if (!eventObj.keepText)
            {
                dialogueText.text = string.Empty;
                nameText.text = string.Empty;
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
        if (eventObj.hideDialogueBox)
        {
            dialogueBoxImage.color = Color.clear;

            if (!eventObj.keepText)
            {
                dialogueText.text = string.Empty;
                nameText.text = string.Empty;
            }
        }

        if (eventObj.hidePortrait)
            characterImage.color = Color.clear;

        dialogueManager.StartMinigame(eventObj.minigame);
    }

    private void EndDialogue(Event eventObj)
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

    private void MoveToDifferentGraph(Event eventObj)
    {
        dialogueManager.SwitchToDifferentGraph(eventObj.graphToMoveTo);
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
