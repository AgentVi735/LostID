using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private GraphController graphController;
    [SerializeField] private Button continueButton;
    [SerializeField] private GenericObj currentObj;

    private void Awake() => Setup();

    private void Setup()
    {
        dialogueBox.Setup();
        currentObj = graphController.startingObj;
        ToggleContinueButton(false);
        dialogueBox.LoadObj(currentObj);
    }

    public void ContinueButton()
    {
        currentObj = currentObj.nextObj;
        Continue(currentObj);
    }

    public void Continue(GenericObj obj)
    {
        ToggleContinueButton(false);
        currentObj = obj;
        dialogueBox.LoadObj(currentObj);
    }

    public void ToggleContinueButton(bool toggle) => continueButton.interactable = toggle;
}
