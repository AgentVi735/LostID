using System.Collections;
using UnityEngine;

public class PinHole : MonoBehaviour
{
    [Header("References")]
    private BattleshipBoardButton button;

    [Header("Pin Prefabs")]
    [SerializeField] private GameObject whitePinPrefab;
    [SerializeField] private GameObject redPinPrefab;

    [Header("Pin")]
    private bool hasRedPin;
    [SerializeField] private Vector2Int coordinates;
    [SerializeField] private Vector3 pinOffsetWhenBoat;

    [Header("Boat")]
    private Boat boat;
    private bool hasBoat;
    [SerializeField] private Vector3 boatOffset;
    [SerializeField] private Vector3 boat2OffsetX;
    [SerializeField] private Vector3 boat2OffsetZ;

    public Vector2Int GetCoords() => coordinates;

    public bool HasBoat() => hasBoat;

    public void Setup(BattleshipBoardButton givenButton, Vector2Int givenCoords)
    {
        button = givenButton;
        coordinates = givenCoords;
        button.Setup(this, coordinates);
    }

    public void ShowBoat(Boat selectedBoat, BoatRotation rotation)
    {
        Quaternion rotationPos = rotation switch
        {
            BoatRotation.Up => Quaternion.Euler(0, -90, 0),
            BoatRotation.Right => Quaternion.Euler(0, 180, 0),
            BoatRotation.Down => Quaternion.Euler(0, 90, 0),
            BoatRotation.Left => Quaternion.Euler(0, 0, 0),
            _ => Quaternion.identity
        };

        rotationPos = Quaternion.Euler(rotationPos.eulerAngles -= transform.rotation.eulerAngles);

        Vector3 pos = transform.position;
        if (selectedBoat.GetHoles() == 2)
        {
            switch (rotation)
            {
                case BoatRotation.Up:
                    pos += boat2OffsetX;
                    break;
                case BoatRotation.Right:
                    pos += boat2OffsetZ;
                    break;
                case BoatRotation.Down:
                    pos += -boat2OffsetX;
                    break;
                case BoatRotation.Left:
                    pos += -boat2OffsetZ;
                    break;
            }
        }

        pos += boatOffset;

        selectedBoat.transform.SetPositionAndRotation(pos, rotationPos);
        selectedBoat.SetCollider(false);
    }

    public void SetBoat(Boat givenBoat, bool shouldDisableInteraction)
    {
        boat = givenBoat;
        hasBoat = true;
        if (shouldDisableInteraction)
            button.ToggleInteraction(false);
    }

    public BoatHitState Shoot(bool isFromOpponent)
    {
        BoatHitState hitState = BoatHitState.Miss;
        hasRedPin = hasBoat;
        if (hasRedPin)
            hitState = BoatHitState.Hit;
        if (!isFromOpponent)
            Instantiate(hasRedPin ? redPinPrefab : whitePinPrefab, transform.position, transform.rotation, transform);
        else
            StartCoroutine(PlayInteractAnimation());
        if (boat != null && boat.Hit())
            hitState = BoatHitState.Sunk;
        return hitState;
    }

    private IEnumerator PlayInteractAnimation()
    {
        Vector3 pos = transform.position;
        if (hasRedPin)
            pos += pinOffsetWhenBoat;
        yield return StartCoroutine(button.GetManager().PlayAnimation());
        Instantiate(hasRedPin ? redPinPrefab : whitePinPrefab, pos, transform.rotation, transform);
        button.GetManager().SwitchTurn();
    }

    public void SetBoat(Boat givenBoat, BoatRotation rotation, PinHole[] otherHoles)
    {
        Quaternion rotationPos = rotation switch
        {
            BoatRotation.Up => Quaternion.Euler(0, -90, 0),
            BoatRotation.Right => Quaternion.Euler(0, 180, 0),
            BoatRotation.Down => Quaternion.Euler(0, 90, 0),
            BoatRotation.Left => Quaternion.Euler(0, 0, 0),
            _ => Quaternion.identity
        };

        rotationPos = Quaternion.Euler(rotationPos.eulerAngles -= transform.rotation.eulerAngles);

        Vector3 pos = transform.position;
        if (givenBoat.GetHoles() == 2)
        {
            switch (rotation)
            {
                case BoatRotation.Up:
                    pos += boat2OffsetX;
                    break;
                case BoatRotation.Right:
                    pos += boat2OffsetZ;
                    break;
                case BoatRotation.Down:
                    pos += -boat2OffsetX;
                    break;
                case BoatRotation.Left:
                    pos += -boat2OffsetZ;
                    break;
            }
        }

        pos += boatOffset;

        boat = givenBoat;
        boat.transform.parent = transform;
        boat.transform.SetPositionAndRotation(pos, rotationPos);
        boat.Setup(rotation, otherHoles);
        boat.ToggleSelection(true);

        hasBoat = true;

        foreach (PinHole hole in otherHoles)
            hole.SetBoat(boat, true);
    }

    public void SetBoatOpponent(Boat givenBoat, BoatRotation rotation, PinHole[] holes)
    {
        boat = givenBoat;
        boat.Setup(rotation, holes);

        hasBoat = true;

        foreach (PinHole hole in holes)
            hole.SetBoat(boat, false);
    }

    public void RemoveBoat()
    {
        boat = null;
        hasBoat = false;
        Invoke(nameof(RemoveBoatFinish), Time.deltaTime);
    }

    private void RemoveBoatFinish()
    {
        button.ToggleInteraction(true);
    }
}
