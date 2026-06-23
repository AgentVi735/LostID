using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private static readonly int ZoomAnimation = Animator.StringToHash("Zoom");

    [Header("References")]
    private SceneSwitcher sceneSwitcher;
    [SerializeField] private SaveSystem saveSystem;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Logo logo;
    [SerializeField] private Animator stationAnimator;

    [Header("Characters")]
    [SerializeField] private CharactersHolder characters;
    private Character character;

    [Header("NFC")]
    private Character cardCharacter;
    private bool canSwitch;

    [Header("Menus")]
    [SerializeField] private GameObject station;
    [SerializeField] private GameObject stationCloseUp;
    [SerializeField] private CanvasGroup stationCloseUpGroup;
    [SerializeField] private CanvasGroup wallet;
    [SerializeField] private CanvasGroup phone;

    [Header("Station")]
    [SerializeField] private Image smallWallet;
    [SerializeField] private Button smallWalletButton;

    [Header("Wallet")]
    [SerializeField] private Image membershipCard;
    [SerializeField] private Image debitCard;
    [SerializeField] private Image transportCard;
    [SerializeField] private CanvasGroup membershipCardGroup;
    [SerializeField] private CanvasGroup debitCardGroup;
    [SerializeField] private CanvasGroup transportCardGroup;
    [SerializeField] private Image underpartRLayer;
    [SerializeField] private Image underpartLLayer;
    [SerializeField] private Image pocketLowRLayer;
    [SerializeField] private Image idCard;
    [SerializeField] private Image[] items;
    [SerializeField] private Image phoneNumber;
    [SerializeField] private Image[] charm;

    [Header("Fades")]
    [SerializeField] private float stationCloseUpFade;
    [SerializeField] private float walletFade;
    [SerializeField] private float phoneFade;
    [SerializeField] private float delayBetweenWalletAndPhone;
    private WaitForSeconds waitDelayBetweenWalletAndPhone;
    [SerializeField] private float fadeTime;
    [SerializeField] private float delayBetweenSwitch;
    private WaitForSeconds waitDelayBetweenSwitch;

    [Header("Sliders")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Inputs")]
    [SerializeField] private InputActionAsset inputs;
    private InputAction switchCharacter;
    private InputAction leaveWallet;
    private int switchCharacterIdx;

    [Header("Other")]
    private bool hasLoaded;
    private readonly Color clearColour = new(255, 255, 255, 0);

    private void Awake()
    {
        sceneSwitcher = FindFirstObjectByType<SceneSwitcher>();
        waitDelayBetweenWalletAndPhone = new WaitForSeconds(delayBetweenWalletAndPhone);
        waitDelayBetweenSwitch = new WaitForSeconds(delayBetweenSwitch);
        LoadSettings();
    }

    private void Start()
    {
        character = characters.empty;
        Load();
        smallWallet.sprite = null;
        smallWallet.color = clearColour;
        smallWalletButton.interactable = false;

        leaveWallet = inputs.FindAction("UI/Cancel");
        switchCharacter = inputs.FindAction("Player/Switch Character");

        leaveWallet.performed += LeaveWallet;
        leaveWallet.Disable();
        switchCharacter.performed += SwitchCharacter;
        switchCharacterIdx = -1;

        ToggleCanSwitch(true);
    }

    private void SwitchCharacter(InputAction.CallbackContext context)
    {
        switchCharacterIdx++;
        if (switchCharacterIdx > characters.characters.Length - 1)
            switchCharacterIdx = 0;

        StartCoroutine(SwitchChar());
    }

    private IEnumerator SwitchChar()
    {
        if (character)
            RemoveCharacter();

        yield return null;

        NewCharacter(characters.characters[switchCharacterIdx]);
    }

    private void OnDestroy()
    {
        switchCharacter.performed -= SwitchCharacter;
        leaveWallet.performed -= LeaveWallet;
    }

    public void NewCharacter(Character givenCharacter)
    {
        cardCharacter = givenCharacter;
        if (canSwitch)
            LoadCharacter();
    }

    public void RemoveCharacter()
    {
        cardCharacter = characters.empty;
        if (canSwitch)
            UnloadCharacter();
    }

    private void UnloadCharacter()
    {
        StartCoroutine(FadeEmpty());
    }

    private void LoadCharacter()
    {
        character = cardCharacter;
        StartCoroutine(FadeNew());
    }

    private IEnumerator FadeEmpty()
    {
        ToggleCanSwitch(false);

        List<Image> imagesToFade = new();
        List<CanvasGroup> groupsToFade = new();

        if (character.smallWallet != null)
        {
            imagesToFade.Add(smallWallet);
            smallWallet.sprite = character.smallWallet;
            smallWalletButton.interactable = false;
        }

        if (character.underpartRLayer != null)
        {
            imagesToFade.Add(underpartRLayer);
            underpartRLayer.sprite = character.underpartRLayer;
        }

        if (character.underpartLLayer != null)
        {
            imagesToFade.Add(underpartLLayer);
            underpartLLayer.sprite = character.underpartLLayer;
        }

        if (character.pocketLowRLayer != null)
        {
            imagesToFade.Add(pocketLowRLayer);
            pocketLowRLayer.sprite = character.pocketLowRLayer;
        }

        if (character.idCard != null)
        {
            imagesToFade.Add(idCard);
            groupsToFade.Add(membershipCardGroup);
            groupsToFade.Add(debitCardGroup);
            groupsToFade.Add(transportCardGroup);
            idCard.sprite = character.idCard;
        }

        if (character.phoneNumber != null)
        {
            imagesToFade.Add(phoneNumber);
            phoneNumber.sprite = character.phoneNumber;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (character.items.Length < i + 1)
                continue;

            Sprite charObj = character.items[i];
            if (charObj == null)
                continue;

            Image img = items[i];
            img.sprite = charObj;
            imagesToFade.Add(img);
        }

        if (character.hasCharm)
            imagesToFade.AddRange(charm);

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            float fadeAmt = i / fadeTime;
            Color currentColour = Color.Lerp(Color.white, clearColour, fadeAmt);

            foreach (Image img in imagesToFade)
                img.color = currentColour;

            foreach (CanvasGroup grp in groupsToFade)
                grp.alpha = currentColour.a;

            yield return null;
        }

        foreach (Image img in imagesToFade)
            img.color = clearColour;

        foreach (CanvasGroup grp in groupsToFade)
            grp.alpha = clearColour.a;

        character = characters.empty;
        Load();

        yield return waitDelayBetweenSwitch;

        if (cardCharacter == character)
            ToggleCanSwitch(true);
        else
            LoadCharacter();
    }

    private IEnumerator FadeNew()
    {
        ToggleCanSwitch(false);

        Load();

        List<Image> imagesToFade = new();
        List<CanvasGroup> groupsToFade = new();

        if (character.smallWallet != null)
        {
            imagesToFade.Add(smallWallet);
            smallWallet.sprite = character.smallWallet;
            smallWalletButton.interactable = true;
        }
        else
            smallWalletButton.interactable = false;

        if (character.underpartRLayer != null)
        {
            imagesToFade.Add(underpartRLayer);
            underpartRLayer.sprite = character.underpartRLayer;
        }

        if (character.underpartLLayer != null)
        {
            imagesToFade.Add(underpartLLayer);
            underpartLLayer.sprite = character.underpartLLayer;
        }

        if (character.pocketLowRLayer != null)
        {
            imagesToFade.Add(pocketLowRLayer);
            pocketLowRLayer.sprite = character.pocketLowRLayer;
        }

        if (character.idCard != null)
        {
            imagesToFade.Add(idCard);
            groupsToFade.Add(membershipCardGroup);
            groupsToFade.Add(debitCardGroup);
            groupsToFade.Add(transportCardGroup);
            idCard.sprite = character.idCard;
        }

        if (character.phoneNumber != null)
        {
            imagesToFade.Add(phoneNumber);
            phoneNumber.sprite = character.phoneNumber;
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (character.items.Length < i + 1)
                continue;

            Sprite charObj = character.items[i];
            if (charObj == null)
                continue;

            Image img = items[i];
            img.sprite = charObj;
            imagesToFade.Add(img);
        }

        if (character.hasCharm)
            imagesToFade.AddRange(charm);

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            float fadeAmt = i / fadeTime;
            Color currentColour = Color.Lerp(clearColour, Color.white, fadeAmt);

            foreach (Image img in imagesToFade)
                img.color = currentColour;

            foreach (CanvasGroup grp in groupsToFade)
                grp.alpha = currentColour.a;

            yield return null;
        }

        foreach (Image img in imagesToFade)
            img.color = Color.white;

        foreach (CanvasGroup grp in groupsToFade)
            grp.alpha = Color.white.a;


        yield return waitDelayBetweenSwitch;

        if (cardCharacter == character)
            ToggleCanSwitch(true);
        else
            UnloadCharacter();
    }

    private void Load()
    {
        smallWallet.sprite = character.smallWallet;
        smallWallet.color = clearColour;

        underpartRLayer.sprite = character.underpartRLayer;
        underpartRLayer.color = clearColour;

        underpartLLayer.sprite = character.underpartLLayer;
        underpartLLayer.color = clearColour;

        pocketLowRLayer.sprite = character.pocketLowRLayer;
        pocketLowRLayer.color = clearColour;

        idCard.sprite = character.idCard;
        idCard.color = clearColour;
        idCard.color = clearColour;
        membershipCardGroup.alpha = 0;
        debitCardGroup.alpha = 0;
        transportCardGroup.alpha = 0;

        phoneNumber.sprite = character.phoneNumber;
        phoneNumber.color = clearColour;

        for (int i = 0; i < items.Length; i++)
        {
            if (character.items.Length < i + 1)
            {
                items[i].color = clearColour;
                items[i].sprite = null;
                continue;
            }

            Sprite charObj = character.items[i];
            if (charObj == null)
            {
                items[i].color = clearColour;
                items[i].sprite = null;
                continue;
            }

            Image img = items[i];
            img.sprite = charObj;
            img.color = clearColour;
        }

        foreach (Image img in charm)
            img.color = clearColour;
    }

    public void OnSmallWallet()
    {
        ToggleCanSwitch(false);
        smallWalletButton.interactable = false;
        stationAnimator.SetBool(ZoomAnimation, true);
    }

    private void LeaveWallet(InputAction.CallbackContext context) => StartCoroutine(Zoom(false));

    public IEnumerator Zoom(bool toggle)
    {
        ToggleCanSwitch(false);
        stationCloseUpGroup.alpha = toggle ? 0 : 1;
        stationCloseUpGroup.interactable = false;
        stationCloseUp.SetActive(true);

        for (float i = 0; i < stationCloseUpFade; i += Time.deltaTime)
        {
            if (toggle)
                stationCloseUpGroup.alpha = i / stationCloseUpFade;
            else
                stationCloseUpGroup.alpha = 1 - i / stationCloseUpFade;
            yield return null;
        }

        stationCloseUpGroup.alpha = toggle ? 1 : 0;
        stationCloseUpGroup.interactable = toggle;
        ToggleCanSwitch(true);
        if (toggle)
            leaveWallet.Enable();
        else
        {
            leaveWallet.Disable();
            stationAnimator.SetBool(ZoomAnimation, false);
            smallWalletButton.interactable = true;
            stationCloseUp.SetActive(false);
        }
    }

    private void ToggleCanSwitch(bool toggle)
    {
        if (toggle == canSwitch) return;
        canSwitch = toggle;
        if (character == cardCharacter) return;
        if (cardCharacter == characters.empty)
            RemoveCharacter();
        else if (cardCharacter != null)
            LoadCharacter();
    }

    private void LoadSettings()
    {
        hasLoaded = false;
        bgmSlider.value = SaveSystem.save.bgmVolume;
        sfxSlider.value = SaveSystem.save.sfxVolume;
        hasLoaded = true;
    }

    public void SaveSettings()
    {
        if (!hasLoaded) return;
        SaveSystem.save.bgmVolume = bgmSlider.value;
        SaveSystem.save.sfxVolume = sfxSlider.value;
        SaveSystem.Save();
    }

    public void OnStartButton()
    {
        for (int i = 0; i < characters.characters.Length; i++)
        {
            if (characters.characters[i].name != character.characterName) continue;
            SaveSystem.loadedPath = i;
            SaveSystem.currentSave = SaveSystem.save.saves[i];
            break;
        }
        SaveSystem.Save();

        if (string.IsNullOrEmpty(SaveSystem.currentSave.currentNode))
            StartCoroutine(ShowPhone());
        else
            StartGame();
    }

    private IEnumerator ShowPhone()
    {
        leaveWallet.performed -= LeaveWallet;
        ToggleCanSwitch(false);
        wallet.alpha = 1;
        wallet.interactable = false;

        for (float i = 0; i < walletFade; i += Time.deltaTime)
        {
            wallet.alpha = 1 - i / walletFade;
            yield return null;
        }

        wallet.alpha = 0;
        wallet.gameObject.SetActive(false);

        yield return waitDelayBetweenWalletAndPhone;

        phone.alpha = 0;
        phone.interactable = false;
        phone.gameObject.SetActive(true);

        for (float i = 0; i < phoneFade; i += Time.deltaTime)
        {
            phone.alpha = i / phoneFade;
            yield return null;
        }

        phone.alpha = 1;
        phone.interactable = true;
        dialogueManager.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        SaveSystem.Save();
        sceneSwitcher.ChangeScene(Scenes.Cafe, Scenes.MainMenu);
    }

    public void OnQuitButton() => Quit();

    private static void Quit()
    {
        SaveSystem.Save();
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    public void OnResetButton()
    {
        saveSystem.ResetSave();
        sceneSwitcher.ChangeScene(Scenes.MainMenu, Scenes.MainMenu);
    }
}
