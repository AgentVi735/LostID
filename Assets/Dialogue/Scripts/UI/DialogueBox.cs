using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private ResponseManager responseManager;

    [SerializeField] private Image dialogueBoxImage;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image nameBoxImage;
    [SerializeField] private RectTransform nameBoxRTransform;
    [SerializeField] private float nameBoxSpacing;
    [SerializeField] private TMP_Text nameText;

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
            default:
                Debug.LogError("Current dialogue object is invalid.");
                break;
        }
    }

    private void LoadDialogue(Dialogue givenDialogue)
    {
        dialogueManager.ToggleContinueButton(false);
        string charName = givenDialogue.character.characterName;
        if (givenDialogue.overrideCharacterName != string.Empty)
            charName = givenDialogue.overrideCharacterName;
        nameText.text = charName;
        nameBoxRTransform.sizeDelta = new Vector2(nameText.preferredWidth + nameBoxSpacing, nameBoxRTransform.sizeDelta.y);
        dialogueText.text = givenDialogue.text;
        dialogueManager.ToggleContinueButton(true);
    }

    private void LoadResponses(ResponseHolder givenHolder)
    {
        dialogueManager.ToggleContinueButton(false);
        responseManager.LoadResponses(givenHolder);
    }
}
