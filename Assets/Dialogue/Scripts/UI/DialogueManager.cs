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
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private DialogueBox dialogueBox;
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private Button continueButton;
    [SerializeField] private Image continueImage;
    private SceneSwitcher sceneSwitcher;

    [Header("Graph")]
    [SerializeField] private bool isPhone;
    private GraphController graphController;
    private GenericObj currentObj;
#if UNITY_EDITOR
    [SerializeField] private int editorPathToLoad;
    [SerializeField] private bool editorUseTestGraph;
    [SerializeField] private GraphController editorTestGraph;
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
    [SerializeField] private string catSkipAnim;
    [SerializeField] private PoofParticle bartPoofParticle;
    [SerializeField] private GameObject frame;

    public Character GetGraphCharacter() => graphController.character;

    public CharacterManager GetCharacterManager() => characterManager;

    private void Awake()
    {
        sceneSwitcher = FindFirstObjectByType<SceneSwitcher>();
        if (isPhone)
            SetupPhone();
        else
            StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
#if UNITY_EDITOR
        if (SaveSystem.save == null)
        {
            SaveSystem.CreateFakeDevSave(editorPathToLoad);
            if (SaveSystem.save == null)
                yield break;

            SaveSystem.currentSave = SaveSystem.save.saves[SaveSystem.loadedPath];
        }
#endif

        SaveFile.SaveData save = SaveSystem.currentSave;
        string nameToCheckFor = save.character;

        foreach (Character c in characters.characters)
        {
            if (c.characterName != nameToCheckFor) continue;
            character = c;
            break;
        }

        SetupCharacter();

        if (save.choseBart)
            character = characters.bart;

        graphController = character.graph;

#if UNITY_EDITOR
        if (editorUseTestGraph && editorTestGraph != null)
            graphController = editorTestGraph;
#endif

        dialogueBox.Setup(false, characterManager);
        string savedObj = save.currentNode;
        if (!string.IsNullOrEmpty(savedObj))
        {
            foreach (GenericObj obj in graphController.dialogueObjs)
            {
                if (obj == null || obj.name != savedObj) continue;
                currentObj = obj;
                break;
            }

            if (currentObj != null)
                yield return StartCoroutine(SetupSave());
        }
        else
            currentObj = graphController.startingObj;

        if (currentObj == null)
        {
            Debug.LogError("No saved object could be found. Might be a corrupt save?");
            yield break;
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

    private IEnumerator SetupSave()
    {
        if (SaveSystem.currentSave.isBartLaying)
        {
            characterManager.SetAnimation(CharacterAnimations.Sit, true);
            characterParent.SpawnAtChair();

            Destroy(catAnimator.gameObject);

            character = characters.bart;

            bartManager = Instantiate(character.prefab, bartParent.transform.position, bartParent.transform.rotation, bartParent.transform)
                .GetComponent<CharacterManager>();

            bartManager.Setup(bartParent);
            bartManager.SetAnimation(CharacterAnimations.Walk, true);

            yield return null;

            bartParent.ResetForBart();
        }
        else if (SaveSystem.currentSave.choseBart)
        {
            character = characters.bart;
            bartManager = Instantiate(character.prefab, bartParent.transform.position, bartParent.transform.rotation, bartParent.transform)
                .GetComponent<CharacterManager>();
            characterManager = bartManager;
            bartManager.Setup(bartParent);

            characterManager.SetAnimation(CharacterAnimations.Sit, true);
            bartParent.SpawnAtChair();
            bartParent.SetBartSitPos();
        }
        else
        {
            characterManager.SetAnimation(CharacterAnimations.Sit, true);
            characterParent.SpawnAtChair();
        }

        if (SaveSystem.currentSave.hasCat)
            catAnimator.SetTrigger(catSkipAnim);

        if (SaveSystem.currentSave.hasItems)
            itemSpawner.SpawnItems(SaveSystem.currentSave.selectedDessert, SaveSystem.currentSave.selectedDrink, character.dessert, character.drink, false);

        if (SaveSystem.currentSave.isShowingFrame)
            ShowFrame();
    }

    private void SetupPhone()
    {
        string nameToCheckFor = SaveSystem.currentSave.character;

        foreach (Character c in characters.characters)
        {
            if (c.characterName != nameToCheckFor) continue;
            character = c;
            break;
        }

        graphController = character.phoneGraph;
        currentObj = graphController.startingObj;
        dialogueBox.Setup(true, null);

        pauseAction = inputs.FindAction("UI/Cancel");
        pauseAction.performed += OnEscapeButtonPhone;

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

    private void OnEscapeButtonPhone(InputAction.CallbackContext context) => GoToDialogueScene();

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
            SaveSystem.currentSave.currentNode = null;

        if (shouldSaveObj)
            SaveSystem.currentSave.currentNode = dialogueBox.objToSave;

        SaveSystem.Save();
        sceneSwitcher.ChangeScene(Scenes.MainMenu, Scenes.Cafe);
    }

    public void GoToDialogueScene() => mainMenuManager.StartGame();

    public void ToggleContinueButton(bool toggle)
    {
        if (isPhone) return;
        continueButton.interactable = toggle;
        continueImage.color = toggle ? Color.white : Color.clear;
    }

    public void ShowCat()
    {
        catAnimator.SetTrigger(catShowAnim);
        SaveSystem.currentSave.hasCat = true;
        SaveSystem.Save();
    }

    public void CatLeave()
    {
        catAnimator.SetTrigger(catLeaveAnim);
        SaveSystem.currentSave.hasCat = true;
        SaveSystem.Save();
    }

    public void SwitchToBart()
    {
        graphController = characters.bart.graph;
        SaveSystem.currentSave.choseBart = true;
        Continue(graphController.startingObj);
        SaveSystem.Save();
    }

    public void SwitchSettingsToBart() => StartCoroutine(SpawnBart());

    private IEnumerator SpawnBart()
    {
        bartPoofParticle.Play();

        Destroy(catAnimator.gameObject);
        SaveSystem.currentSave.hasCat = false;
        SaveSystem.Save();

        character = characters.bart;

        bartManager = Instantiate(character.prefab, bartParent.transform.position, bartParent.transform.rotation, bartParent.transform)
            .GetComponent<CharacterManager>();

        bartManager.Setup(bartParent);
        bartManager.SetAnimation(CharacterAnimations.Walk, true);

        yield return null;

        SaveSystem.currentSave.isBartLaying = true;
        bartParent.ResetForBart();
        SaveSystem.Save();
    }

    public IEnumerator BartSitDown()
    {
        bartPoofParticle.Play();
        bartParent.transform.position = Vector3.down * 2;

        yield return null;

        Destroy(characterManager.gameObject);
        characterManager = bartManager;

        characterManager.SetAnimation(CharacterAnimations.Walk, false);
        characterManager.SetAnimation(CharacterAnimations.Sit, true);

        yield return new WaitForSeconds(0.6f);

        SaveSystem.currentSave.isBartLaying = false;
        bartParent.SetBartSitPos();
        SaveSystem.Save();
    }

    public void ShowFrame()
    {
        if (!SaveSystem.currentSave.choseBart) return;
        SaveSystem.currentSave.isShowingFrame = true;
        frame.SetActive(true);
        SaveSystem.Save();
    }

    private void OnDestroy()
    {
        pauseAction.performed -= OnEscapeButton;
        pauseAction.performed -= OnEscapeButtonPhone;
    }
}
