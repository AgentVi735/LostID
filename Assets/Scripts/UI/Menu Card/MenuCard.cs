using UnityEngine;
using UnityEngine.UI;

public class MenuCard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private DialogueBox dialogueBox;

    [Header("Menu")]
    [SerializeField] private GameObject card;
    [SerializeField] private GameObject confirmButton;
    private MenuItems selectedDessert;
    private MenuItems selectedDrinks;
    private MenuItems npcDessert;
    private MenuItems npcDrink;
    [SerializeField] private Image[] itemHighlights;
    [SerializeField] private Color highlightColor;

    private void Awake() => confirmButton.SetActive(false);

    public void Show(MenuItems sentDessert, MenuItems sentDrink)
    {
        npcDessert = sentDessert;
        npcDrink = sentDrink;
        card.SetActive(true);
    }

    public void OnButton(int num)
    {
        MenuItems item = (MenuItems)num;

        bool toggle;
        int disableStart;
        int disableEnd;

        if (num <= 4)
        {
            if (selectedDessert != item)
            {
                toggle = true;
                selectedDessert = item;
            }
            else
            {
                toggle = false;
                selectedDessert = MenuItems.None;
            }

            disableStart = 0;
            disableEnd = 4;
        }
        else
        {
            if (selectedDrinks != item)
            {
                toggle = true;
                selectedDrinks = item;
            }
            else
            {
                toggle = false;
                selectedDrinks = MenuItems.None;
            }
            disableStart = 4;
            disableEnd = 6;
        }

        num -= 1;
        for (int i = disableStart; i < disableEnd; i++)
            if (num == i)
                itemHighlights[i].color = toggle ? highlightColor : Color.clear;
            else
                itemHighlights[i].color = Color.clear;

        confirmButton.SetActive(selectedDrinks != MenuItems.None && selectedDessert != MenuItems.None);
    }

    public void OnConfirmButton()
    {
        card.SetActive(false);
        SaveSystem.currentSave.selectedDessert = selectedDessert;
        SaveSystem.currentSave.selectedDrink = selectedDrinks;
        itemSpawner.SpawnItems(selectedDessert, selectedDrinks, npcDessert, npcDrink);
        dialogueBox.FinishMenu();
    }

    public void RemoveItems() => itemSpawner.RemoveItems();
}
