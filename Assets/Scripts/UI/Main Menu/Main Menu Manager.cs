using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
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
    [SerializeField] private float fadeDelayTime;
    private WaitForSeconds waitFadeDelayTime;

    [SerializeField] private float delayBetweenSwitch;
    private WaitForSeconds waitDelayBetweenSwitch;

    private readonly Color clearColour = new(255, 255, 255, 0);

    private void Awake()
    {
        waitFadeDelayTime = new WaitForSeconds(fadeDelayTime);
        waitDelayBetweenSwitch = new WaitForSeconds(delayBetweenSwitch);
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

        //yield return waitFadeDelayTime;

        //for (float i = 0; i < fadeTime; i += Time.deltaTime)
        //{
        //    float fadeAmt = i / fadeTime;
        //    Color currentColour = Color.Lerp(clearColour, Color.white, fadeAmt);

        //    foreach (Image img in imagesToFade)
        //        img.color = currentColour;

        //    yield return null;
        //}

        //foreach (Image img in imagesToFade)
        //    img.color = Color.white;

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

        //yield return waitFadeDelayTime;

        //for (float i = 0; i < fadeTime; i += Time.deltaTime)
        //{
        //    float fadeAmt = i / fadeTime;
        //    Color currentColour = Color.Lerp(clearColour, Color.white, fadeAmt);

        //    foreach (Image img in imagesToFade)
        //        img.color = currentColour;

        //    yield return null;
        //}

        //foreach (Image img in imagesToFade)
        //    img.color = Color.white;

        yield return waitDelayBetweenSwitch;

        if (cardCharacter == character)
            canSwitch = true;
        else
            UnloadCharacter();
    }

    private void Load()
    {
        //if (character.underpartRLayer != null)
        //{
            underpartRLayer.sprite = character.underpartRLayer;
        //    //underpartRLayer.color = Color.white;
        //}
        //else
            underpartRLayer.color = clearColour;

        //if (character.underpartLLayer != null)
        //{
        underpartLLayer.sprite = character.underpartLLayer;
        //    //underpartLLayer.color = Color.white;
        //}
        //else
            underpartLLayer.color = clearColour;

        //if (character.pocketLowRLayer != null)
        //{
        pocketLowRLayer.sprite = character.pocketLowRLayer;
        //    //pocketLowRLayer.color = Color.white;
        //}
        //else
            pocketLowRLayer.color = clearColour;

        //if (character.idCard != null)
        //{
        idCard.sprite = character.idCard;
        //    //idCard.color = Color.white;
        //    //membershipCard.color = Color.white;
        //    //debitCard.color = Color.white;
        //    //transportCard.color = Color.white;
        //}
        //else
        //{
            idCard.color = clearColour;
        idCard.color = clearColour;
        membershipCard.color = clearColour;
        debitCard.color = clearColour;
        transportCard.color = clearColour;
        //}
        //if (character.idCard == null)
        //{
        //    membershipCard.color = clearColour;
        //    debitCard.color = clearColour;
        //    transportCard.color = clearColour;
        //}

        //if (character.phoneNumber != null)
        //{
        phoneNumber.sprite = character.phoneNumber;
        //    //phoneNumber.color = Color.white;
        //}
        //else
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
            //img.color = Color.white;
            img.color = clearColour;
        }

        Color charmColor;
        if (character.hasCharm)
            charmColor = clearColour;
        else
            charmColor = clearColour;

        foreach (Image img in charm)
            img.color = charmColor;

    }
}
