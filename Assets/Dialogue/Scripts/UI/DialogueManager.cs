using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    private SceneSwitcher sceneSwitcher;
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private CameraManager camManager;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private Button continueButton;

    [Header("Graph")]
    [SerializeField] private bool isPhone;
    private GraphController graphController;
    private GenericObj currentObj;

    [Header("Minigames")]
    [SerializeField] private UNOManager unoManager;
    [SerializeField] private BattleshipManager battleshipManager;

    [Header("Inputs")]
    [SerializeField] private InputActionAsset inputs;
    private InputAction pauseAction;

    [Header("Scenes")]
    [SerializeField] private string mainMenuScene;
    [SerializeField] private string dialogueScene;

    [Header("Character")]
    [SerializeField] private CharactersHolder characters;
    private Character character;
    private CharacterManager characterManager;
    [SerializeField] private Transform characterSpawn;

    [Header("Bart")]
    [SerializeField] private Character bart;

    public Character GetGraphCharacter() => graphController.character;

    private void Awake()
    {
        sceneSwitcher = FindFirstObjectByType<SceneSwitcher>();
        if (isPhone)
            SetupPhone();
        else
            Setup();
    }

    private void Setup()
    {
#if UNITY_EDITOR
        if (SaveSystem.save == null)
            SaveSystem.CreateFakeDevSave();
        if (SaveSystem.save == null)
            return;
#endif

        SaveFile.SaveData save = SaveSystem.save.saves[SaveSystem.loadedPath];
        string nameToCheckFor = save.character;

        foreach (Character c in characters.characters)
        {
            if (c.characterName != nameToCheckFor) continue;
            character = c;
            break;
        }

        SetupCharacter();
        dialogueBox.Setup(false, characterManager);

        graphController = character.graph;
        currentObj = graphController.startingObj;
        string savedObj = SaveSystem.save.saves[SaveSystem.loadedPath].currentNode;
        foreach (GenericObj obj in graphController.dialogueObjs)
        {
            if (obj == null || obj.name != savedObj) continue;
            currentObj = obj;
            break;
        }
        ToggleContinueButton(false);

        pauseManager.Setup();

        pauseAction = inputs.FindAction("UI/Cancel");
        pauseAction.performed += OnEscapeButton;

        dialogueBox.LoadObj(currentObj);
    }

    private void SetupCharacter()
    {
        characterManager = Instantiate(character.prefab, characterSpawn.position, characterSpawn.rotation, characterSpawn)
            .GetComponent<CharacterManager>();

        characterManager.Setup(characterSpawn);
    }

    private void SetupPhone()
    {
        string nameToCheckFor = SaveSystem.save.saves[SaveSystem.loadedPath].character;

        foreach (Character c in characters.characters)
        {
            if (c.characterName != nameToCheckFor) continue;
            character = c;
            break;
        }

        dialogueBox.Setup(true, null);

        graphController = character.phoneGraph;
        currentObj = graphController.startingObj;

        dialogueBox.LoadObj(currentObj);
    }

    public void ToggleEscapeButton(bool toggle)
    {
        if (toggle)
            pauseAction.Disable();
        else
            pauseAction.Enable();
    }

    private void OnEscapeButton(InputAction.CallbackContext context) => pauseManager.OpenPause();

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

    public void StartMinigame(Minigame minigame)
    {
        dialogueCanvas.SetActive(false);

        switch (minigame)
        {
            default:
            case Minigame.None:
                Debug.LogError("No minigame selected on node " + currentObj.name);
                return;
            case Minigame.UNO:
                unoManager.gameObject.SetActive(true);
                break;
            case Minigame.Battleship:
                camManager.ChangePosition(CameraPositions.Battleship);
                battleshipManager.gameObject.SetActive(true);
                break;
        }
    }

    public void ExitMinigame(bool hasWon)
    {
        camManager.ChangePosition(CameraPositions.Default);
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
        sceneSwitcher.ChangeScene(Scenes.MainMenu, Scenes.Cafe);
    }

    public void GoToDialogueScene() => mainMenuManager.StartGame();

    public void ToggleContinueButton(bool toggle)
    {
        if (isPhone) return;
        continueButton.interactable = toggle;
    }

    public void SwitchToBart()
    {
        graphController = bart.graph;
        Continue(graphController.startingObj);
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= OnEscapeButton;
    }
}
