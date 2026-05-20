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
        nameBoxRTransform.sizeDelta =
            new Vector2(nameText.preferredWidth + nameBoxSpacing, nameBoxRTransform.sizeDelta.y);
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
}
