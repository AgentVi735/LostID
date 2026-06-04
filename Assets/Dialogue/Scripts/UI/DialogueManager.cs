using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private GraphController graphController;
    [SerializeField] private GraphController[] graphs;
    [SerializeField] private Button continueButton;
    [SerializeField] private GenericObj currentObj;
    [SerializeField] private GameObject dialogueCanvas;

    [SerializeField] private UNOManager unoManager;

    [SerializeField] private InputActionAsset inputs;
    private InputAction pauseAction;

    [SerializeField] private string mainMenuScene;

    private void Awake() => Setup();

    private void Setup()
    {
        dialogueBox.Setup();
        graphController = graphs[SaveSystem.loadedPath];
        currentObj = graphController.startingObj;
        string savedObj = SaveSystem.save.saves[SaveSystem.loadedPath].currentNode;
        foreach (GenericObj obj in graphController.dialogueObjs)
        {
            if (obj == null || obj.name != savedObj) continue;
            currentObj = obj;
            break;
        }
        ToggleContinueButton(false);

        pauseAction = inputs.FindAction("UI/Cancel");
        pauseAction.performed += OnEscapeButton;

        dialogueBox.LoadObj(currentObj);
    }

    private void OnEscapeButton(InputAction.CallbackContext context)
    {
        pauseAction.Disable();
        GoToMainMenu(true, false);
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

    public void DisableDialogue()
    {
        dialogueCanvas.SetActive(false);
        unoManager.gameObject.SetActive(true);
    }

    public void ExitMinigame(bool hasWon)
    {
        dialogueCanvas.SetActive(true);
        Event obj = (Event)currentObj;
        Continue(hasWon ? obj.wonMinigameObj : obj.loseMinigameObj);
    }

    public void GoToMainMenu(bool shouldSaveObj, bool isEnding)
    {
        if (isEnding) 
            SaveSystem.save.saves[SaveSystem.loadedPath].currentNode = null;

        if (shouldSaveObj) 
            SaveSystem.save.saves[SaveSystem.loadedPath].currentNode = dialogueBox.objToSave;

        SaveSystem.Save();
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ToggleContinueButton(bool toggle) => continueButton.interactable = toggle;

    private void OnDestroy()
    {
        pauseAction.performed -= OnEscapeButton;
    }
}
