using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleshipBoardButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    private PinHole pinHole;
    [SerializeField] private BattleshipManager battleshipManager;
    [SerializeField] private Button button;

    [Header("Settings")]
    private Vector2Int coordinates;
    [SerializeField] private bool isPlayerButton;

    public BattleshipManager GetManager() => battleshipManager;

    public void OnPointerEnter(PointerEventData data)
    {
        if (!isPlayerButton) return;
        Boat boat = battleshipManager.CanBoatShow(coordinates);
        if (boat == null) return;
        pinHole.ShowBoat(boat, battleshipManager.GetRotation());
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (!isPlayerButton) return;
        battleshipManager.PutBoatBack();
    }

    public void OnPlayerButton()
    {
        bool allowed = battleshipManager.OnBoatButton(coordinates);
        if (!allowed) return;
        button.interactable = false;
    }

    public void ToggleInteraction(bool toggle)
    {
        button.interactable = toggle;
    }

    public void OnOpponentButton()
    {
        button.interactable = false;
        battleshipManager.OnShootButton(coordinates);
    }

    public void Setup(PinHole givenPinHole, Vector2Int givenCoords)
    {
        pinHole = givenPinHole;
        coordinates = givenCoords;
    }
}
