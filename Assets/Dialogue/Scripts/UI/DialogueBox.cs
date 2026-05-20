using System.Collections;
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

    public void Setup() => responseManager.Setup();

    public void LoadObj(GenericObj givenObj)
    {
        switch (givenObj.type)
        {
            case NodeType.Dialogue:
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
        if (dialogue.character == null)
        {
            Debug.LogError("Dialogue " + dialogue.name + " has no character selected");
            return;
        }
        string charName = dialogue.character.characterName;
        if (dialogue.overrideCharacterName != string.Empty)
            charName = dialogue.overrideCharacterName;
        nameText.text = charName;
        Sprite charSprite = dialogue.sprite switch
        {
            CharacterSprite.None => null,
            CharacterSprite.Neutral => dialogue.character.neutralSprite,
            CharacterSprite.Happy => dialogue.character.happySprite,
            CharacterSprite.Angry => dialogue.character.angrySprite,
            CharacterSprite.Sad => dialogue.character.sadSprite,
            _ => characterImage.sprite
        };
        characterImage.sprite = charSprite;
        dialogueText.text = dialogue.text;
        if (dialogue.nextObj != null)
            dialogueManager.ToggleContinueButton(true);
    }

    private void LoadResponses(ResponseHolder responseHolder)
    {
        dialogueManager.ToggleContinueButton(false);
        responseManager.LoadResponses(responseHolder);
    }

    private void LoadEvent(Event eventObj)
    {
        dialogueManager.ToggleContinueButton(false);

        switch (eventObj.eventType)
        {
            case EventType.None:
                Debug.LogWarning("No event type selected on event node " + eventObj.name);
                break;
            case EventType.Delay:
                StartCoroutine(Delay(eventObj));
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
}
