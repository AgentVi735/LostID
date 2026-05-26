using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private SaveSystem saveSystem;

    [SerializeField] private Character[] characters;
    private Character character;
    [SerializeField] private Character emptyCharacter;

    private Character cardCharacter;
    private bool canSwitch;

    [SerializeField] private Image membershipCard;
    [SerializeField] private Image debitCard;
    [SerializeField] private Image transportCard;
    [SerializeField] private Image underpartRLayer;
    [SerializeField] private Image underpartLLayer;
    [SerializeField] private Image pocketLowRLayer;
    [SerializeField] private Image idCard;
    [SerializeField] private Image[] items;
    [SerializeField] private Image phoneNumber;
    [SerializeField] private Image[] charm;
    
    [SerializeField] private float fadeTime;
    [SerializeField] private float delayBetweenSwitch;
    private WaitForSeconds waitDelayBetweenSwitch;

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] private string mainMenuScene;
    [SerializeField] private string gameScene;

    private bool hasLoaded;

    private readonly Color clearColour = new(255, 255, 255, 0);

    private void Awake()
    {
        waitDelayBetweenSwitch = new WaitForSeconds(delayBetweenSwitch);
        LoadSettings();
    }

    private void Start()
    {
        character = emptyCharacter;
        Load();
        canSwitch = true;
    }

    public void NewCharacter(Character givenCharacter)
    {
        cardCharacter = givenCharacter;
        if (canSwitch)
            LoadCharacter();
    }

    public void RemoveCharacter()
    {
        cardCharacter = emptyCharacter;
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
        canSwitch = false;

        List<Image> imagesToFade = new();
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
            imagesToFade.Add(membershipCard);
            imagesToFade.Add(debitCard);
            imagesToFade.Add(transportCard);
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

            yield return null;
        }

        foreach (Image img in imagesToFade) 
            img.color = clearColour;

        character = emptyCharacter;
        Load();

        yield return waitDelayBetweenSwitch;

        if (cardCharacter == character)
            canSwitch = true;
        else
            LoadCharacter();
    }

    private IEnumerator FadeNew()
    {
        canSwitch = false;

        Load();

        List<Image> imagesToFade = new();
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
            imagesToFade.Add(membershipCard);
            imagesToFade.Add(debitCard);
            imagesToFade.Add(transportCard);
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

            yield return null;
        }

        foreach (Image img in imagesToFade)
            img.color = Color.white;


        yield return waitDelayBetweenSwitch;

        if (cardCharacter == character)
            canSwitch = true;
        else
            UnloadCharacter();
    }

    private void Load()
    {
        underpartRLayer.sprite = character.underpartRLayer;
        underpartRLayer.color = clearColour;

        underpartLLayer.sprite = character.underpartLLayer;
            underpartLLayer.color = clearColour;

        pocketLowRLayer.sprite = character.pocketLowRLayer;
            pocketLowRLayer.color = clearColour;

        idCard.sprite = character.idCard;
            idCard.color = clearColour;
        idCard.color = clearColour;
        membershipCard.color = clearColour;
        debitCard.color = clearColour;
        transportCard.color = clearColour;

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

    public void OnStartButton() => SceneManager.LoadScene(gameScene);

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
        SceneManager.LoadScene(mainMenuScene);
    }
}
