using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Character character;

    [SerializeField] private Image underpartRLayer;
    [SerializeField] private Image underpartLLayer;
    [SerializeField] private Image pocketLowRLayer;
    [SerializeField] private Image idCard;
    [SerializeField] private Image[] objects;
    [SerializeField] private Image phoneNumber;
    [SerializeField] private GameObject charm;

    private void Start()
    {
        Load();
    }

    private void Load()
    {
        if (character.underpartRLayer != null)
        {
            underpartRLayer.sprite = character.underpartRLayer;
            underpartRLayer.gameObject.SetActive(true);
        }
        else
            underpartRLayer.gameObject.SetActive(false);

        if (character.underpartLLayer != null)
        {
            underpartLLayer.sprite = character.underpartLLayer;
            underpartLLayer.gameObject.SetActive(true);
        }
        else
            underpartLLayer.gameObject.SetActive(false);

        if (character.pocketLowRLayer != null)
        {
            pocketLowRLayer.sprite = character.pocketLowRLayer;
            pocketLowRLayer.gameObject.SetActive(true);
        }
        else
            pocketLowRLayer.gameObject.SetActive(false);

        idCard.sprite = character.idCard;
        phoneNumber.sprite = character.phoneNumber;

        for (int i = 0; i < objects.Length; i++)
        {
            if (character.objects.Length < i + 1)
            {
                objects[i].gameObject.SetActive(false);
                continue;
            }

            Sprite charObj = character.objects[i];
            if (charObj == null)
            {
                objects[i].gameObject.SetActive(false);
                continue;
            }

            Image img = objects[i];
            img.sprite = charObj;
            img.gameObject.SetActive(true);
        }

        charm.SetActive(character.hasCharm);
    }
}
