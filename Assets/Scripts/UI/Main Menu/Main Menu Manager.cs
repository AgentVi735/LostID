using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Character[] characters;
    private int charIdx;
    private Character character;
    [SerializeField] private Character emptyCharacter;

    private bool canSwitch;

    [SerializeField] private Image underpartRLayer;
    [SerializeField] private Image underpartRLayerFade;
    [SerializeField] private Image underpartLLayer;
    [SerializeField] private Image underpartLLayerFade;
    [SerializeField] private Image pocketLowRLayer;
    [SerializeField] private Image pocketLowRLayerFade;
    [SerializeField] private Image idCard;
    [SerializeField] private Image idCardFade;
    [SerializeField] private Image[] items;
    [SerializeField] private Image[] itemsFade;
    [SerializeField] private Image phoneNumber;
    [SerializeField] private Image phoneNumberFade;
    [SerializeField] private Image[] charm;
    [SerializeField] private Image[] charmFade;

    [SerializeField] private float fadeTime;
    [SerializeField] private float fadeDelayTime;
    private WaitForSeconds waitFadeDelayTime;

    private readonly Color clearColour = new(255, 255, 255, 0);

    private void Awake()
    {
        waitFadeDelayTime = new WaitForSeconds(fadeDelayTime);
    }

    private void Start()
    {
        character = characters[charIdx];
        Load();
        canSwitch = true;
    }

    private void Update()
    {
        if (canSwitch && Input.GetKeyDown(KeyCode.Return)) 
            StartCoroutine(SwitchCharacters());
    }

    private IEnumerator SwitchCharacters()
    {
        canSwitch = false;
        charIdx++;
        if (charIdx > characters.Length - 1)
            charIdx = 0;

        yield return StartCoroutine(FadeEmpty());

        yield return new WaitForSeconds(0.5f);

        character = characters[charIdx];

        yield return StartCoroutine(FadeNew());

        yield return new WaitForSeconds(0.5f);

        canSwitch = true;
    }

    private IEnumerator FadeEmpty()
    {
        List<Image> imagesToFade = new();

        if (character.underpartRLayer != null)
        {
            imagesToFade.Add(underpartRLayerFade);
            underpartRLayerFade.sprite = character.underpartRLayerFade;
        }

        if (character.underpartLLayer != null)
        {
            imagesToFade.Add(underpartLLayerFade);
            underpartLLayerFade.sprite = character.underpartLLayerFade;
        }

        if (character.pocketLowRLayer != null)
        {
            imagesToFade.Add(pocketLowRLayerFade);
            pocketLowRLayerFade.sprite = character.pocketLowRLayerFade;
        }

        imagesToFade.Add(idCardFade);
        imagesToFade.Add(phoneNumberFade);

        for (int i = 0; i < itemsFade.Length; i++)
        {
            if (character.items.Length < i + 1)
                continue;

            Sprite charObj = character.itemsFade[i];
            if (charObj == null)
                continue;

            Image img = itemsFade[i];
            img.sprite = charObj;
            imagesToFade.Add(img);
        }

        if (character.hasCharm) 
            imagesToFade.AddRange(charmFade);

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            float fadeAmt = i / fadeTime;
            Color currentColour = Color.Lerp(clearColour, Color.black, fadeAmt);

            foreach (Image img in imagesToFade) 
                img.color = currentColour;

            yield return null;
        }

        foreach (Image img in imagesToFade) 
            img.color = Color.black;

        character = emptyCharacter;
        Load();

        yield return waitFadeDelayTime;

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            float fadeAmt = i / fadeTime;
            Color currentColour = Color.Lerp(Color.black, clearColour, fadeAmt);

            foreach (Image img in imagesToFade)
                img.color = currentColour;

            yield return null;
        }

        foreach (Image img in imagesToFade)
            img.color = clearColour;
    }

    private IEnumerator FadeNew()
    {
        List<Image> imagesToFade = new();

        if (character.underpartRLayer != null)
        {
            imagesToFade.Add(underpartRLayerFade);
            underpartRLayerFade.sprite = character.underpartRLayerFade;
        }

        if (character.underpartLLayer != null)
        {
            imagesToFade.Add(underpartLLayerFade);
            underpartLLayerFade.sprite = character.underpartLLayerFade;
        }

        if (character.pocketLowRLayer != null)
        {
            imagesToFade.Add(pocketLowRLayerFade);
            pocketLowRLayerFade.sprite = character.pocketLowRLayerFade;
        }

        imagesToFade.Add(idCardFade);
        imagesToFade.Add(phoneNumberFade);

        for (int i = 0; i < itemsFade.Length; i++)
        {
            if (character.items.Length < i + 1)
                continue;

            Sprite charObj = character.itemsFade[i];
            if (charObj == null)
                continue;

            Image img = itemsFade[i];
            img.sprite = charObj;
            imagesToFade.Add(img);
        }

        if (character.hasCharm)
            imagesToFade.AddRange(charmFade);

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            float fadeAmt = i / fadeTime;
            Color currentColour = Color.Lerp(clearColour, Color.black, fadeAmt);

            foreach (Image img in imagesToFade)
                img.color = currentColour;

            yield return null;
        }

        foreach (Image img in imagesToFade)
            img.color = Color.black;

        Load();

        yield return waitFadeDelayTime;

        for (float i = 0; i < fadeTime; i += Time.deltaTime)
        {
            float fadeAmt = i / fadeTime;
            Color currentColour = Color.Lerp(Color.black, clearColour, fadeAmt);

            foreach (Image img in imagesToFade)
                img.color = currentColour;

            yield return null;
        }

        foreach (Image img in imagesToFade)
            img.color = clearColour;
    }

    private void Load()
    {
        if (character.underpartRLayer != null)
        {
            underpartRLayer.sprite = character.underpartRLayer;
            underpartRLayer.color = Color.white;
        }
        else
            underpartRLayer.color = Color.clear;

        if (character.underpartLLayer != null)
        {
            underpartLLayer.sprite = character.underpartLLayer;
            underpartLLayer.color = Color.white;
        }
        else
            underpartLLayer.color = Color.clear;

        if (character.pocketLowRLayer != null)
        {
            pocketLowRLayer.sprite = character.pocketLowRLayer;
            pocketLowRLayer.color = Color.white;
        }
        else
            pocketLowRLayer.color = Color.clear;

        if (character.idCard != null)
        {
            idCard.sprite = character.idCard;
            idCard.color = Color.white;
        }
        else
            idCard.color = Color.clear;

        if (character.phoneNumber != null)
        {
            phoneNumber.sprite = character.phoneNumber;
            phoneNumber.color = Color.white;
        }
        else
            phoneNumber.color = Color.clear;

        for (int i = 0; i < items.Length; i++)
        {
            if (character.items.Length < i + 1)
            {
                items[i].color = Color.clear;
                continue;
            }

            Sprite charObj = character.items[i];
            if (charObj == null)
            {
                items[i].color = Color.clear;
                continue;
            }

            Image img = items[i];
            img.sprite = charObj;
            img.color = Color.white;
        }

        Color charmColor = character.hasCharm ? Color.white : Color.clear;
        foreach (Image img in charm) 
            img.color = charmColor;

    }
}
