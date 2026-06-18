using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PauseManager pauseManager;
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private CameraManager camManager;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private Button continueButton;
    private SceneSwitcher sceneSwitcher;

    [Header("Graph")]
    [SerializeField] private bool isPhone;
    private GraphController graphController;
    private GenericObj currentObj;
#if UNITY_EDITOR
    [SerializeField] private int editorPathToLoad;
#endif

    [Header("Minigames")]
    [SerializeField] private UNOManager unoManager;
    [SerializeField] private BattleshipManager battleshipManager;

    [Header("Inputs")]
    [SerializeField] private InputActionAsset inputs;
    private InputAction pauseAction;

    [Header("Character")]
    [SerializeField] private CharactersHolder characters;
    private Character character;
    private CharacterManager characterManager;
    [SerializeField] private CharacterParent characterParent;

    [Header("Bart")]
    [SerializeField] private CharacterParent bartParent;
    private CharacterManager bartManager;
    [SerializeField] private Animator catAnimator;
    [SerializeField] private string catShowAnim;
    [SerializeField] private string catLeaveAnim;
    [SerializeField] private PoofParticle poofParticle;

    public Character GetGraphCharacter() => graphController.character;

    public CharacterManager GetCharacterManager() => characterManager;

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
            SaveSystem.CreateFakeDevSave(editorPathToLoad);
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
        characterManager = Instantiate(character.prefab, characterParent.transform.position, characterParent.transform.rotation, characterParent.transform)
            .GetComponent<CharacterManager>();

        characterManager.Setup(characterParent);
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

    public void ShowCat() => catAnimator.SetTrigger(catShowAnim);

    public void CatLeave() => catAnimator.SetTrigger(catLeaveAnim);

    public void SwitchToBart()
    {
        graphController = characters.bart.graph;
        SaveSystem.save.saves[SaveSystem.loadedPath].choseBart = true;
        Continue(graphController.startingObj);
    }

    public void SwitchSettingsToBart() => StartCoroutine(SpawnBart());

    private IEnumerator SpawnBart()
    {
        poofParticle.Play(3);

        Destroy(catAnimator.gameObject);

        character = characters.bart;

        bartManager = Instantiate(character.prefab, bartParent.transform.position, bartParent.transform.rotation, bartParent.transform)
            .GetComponent<CharacterManager>();

        bartManager.Setup(bartParent);
        bartManager.SetAnimation(CharacterAnimations.Walk, true);

        yield return null;

        bartParent.ResetForBart();
    }

    public IEnumerator BartSitDown()
    {
        poofParticle.Play(3);
        bartParent.transform.position = Vector3.down * 2;

        yield return null;

        Destroy(characterManager.gameObject);
        characterManager = bartManager;

        characterManager.SetAnimation(CharacterAnimations.Walk, false);
        characterManager.SetAnimation(CharacterAnimations.Sit, true);

        yield return new WaitForSeconds(0.6f);

        bartParent.SetBartSitPos();
    }

    private void OnDestroy()
    {
        if (pauseAction != null)
            pauseAction.performed -= OnEscapeButton;
    }
}
