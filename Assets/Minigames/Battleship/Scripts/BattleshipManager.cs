using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleshipManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject startButton;

    [Header("Board Settings")]
    [SerializeField] private int holesPerRow;
    [SerializeField] private int totalRows;
    private BoatRotation currentRotation;
    private int currentBoatHoles;
    private Vector2Int coordsSelectedBoatIsShownOn;

    [Header("Boards")]
    [SerializeField] private PinHole[] playerHoles;
    [SerializeField] private BattleshipBoardButton[] playerBoardButtons;
    [SerializeField] private PinHole[] opponentHoles;
    [SerializeField] private BattleshipBoardButton[] opponentBoardButtons;

    [Header("Boats")]
    [SerializeField] private string boatTag;
    [SerializeField] private Boat[] boats2;
    [SerializeField] private Boat[] boats3;
    private Boat selectedBoat;
    private bool shouldSearchForBoat;
    private bool canPutBoatDown;
    private int totalBoats;
    private int boatsPutDown;

    [Header("Inputs")]
    [SerializeField] private InputActionAsset inputs;
    private InputAction mousePositionAction;
    private Vector2 mousePos;
    private InputAction leftClick;
    private bool hasLeftClicked;
    private InputAction rightClick;

    private void Awake()
    {
        leftClick = inputs.FindAction("Player/Attack");
        rightClick = inputs.FindAction("Player/RightClick");
        mousePositionAction = inputs.FindAction("Player/MousePos");

        leftClick.performed += OnLeftClick;
        leftClick.canceled += OnLeftClickRelease;
        rightClick.started += OnRightClick;

    }

    private void Start()
    {
        startButton.SetActive(false);
        totalBoats = boats2.Length + boats3.Length;
        currentRotation = BoatRotation.Up;
        int holes = holesPerRow * totalRows;
        int currentX = 0;
        int currentY = 0;
        for (int i = 0; i < holes; i++)
        {
            PinHole playerHole = playerHoles[i];
            BattleshipBoardButton playerButton = playerBoardButtons[i];
            PinHole opponentHole = opponentHoles[i];
            BattleshipBoardButton opponentButton = opponentBoardButtons[i];
            playerHole.Setup(playerButton, new Vector2Int(currentX, currentY));
            opponentHole.Setup(opponentButton, new Vector2Int(currentX, currentY));
            currentX++;
            if (currentX != holesPerRow) continue;
            currentX = 0;
            currentY++;
            if (currentY == totalRows) break;
        }

        StartCoroutine(FindBoat());
    }

    private IEnumerator FindBoat()
    {
        shouldSearchForBoat = true;
        hasLeftClicked = false;
        canPutBoatDown = false;

        while (shouldSearchForBoat)
        {
            mousePos = mousePositionAction.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, -10));
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag(boatTag))
            {
                leftClick.Enable();

                if (hasLeftClicked)
                {
                    if (selectedBoat != null)
                    {
                        if (hit.collider.gameObject == selectedBoat.gameObject)
                            OnDeselectBoat();
                        else
                        {
                            OnDeselectBoat();
                            selectedBoat = hit.collider.GetComponent<Boat>();
                            OnSelectBoat();
                        }
                    }
                    else
                    {
                        selectedBoat = hit.collider.GetComponent<Boat>();
                        OnSelectBoat();
                    }

                    hasLeftClicked = false;
                }
            }
            else
            {
                hasLeftClicked = false;
                leftClick.Disable();
            }

            yield return null;
        }
    }

    public void OnStartButton()
    {
        startButton.SetActive(false);
        shouldSearchForBoat = false;
        canPutBoatDown = false;
        leftClick.Disable();
        rightClick.Disable();
        mousePositionAction.Disable();
        foreach (BattleshipBoardButton button in playerBoardButtons)
        {
            button.ToggleInteraction(false);
            button.enabled = false;
        }
    }

    private void OnSelectBoat()
    {
        canPutBoatDown = false;
        currentRotation = selectedBoat.ToggleSelection(true);
        currentBoatHoles = selectedBoat.GetHoles();
        if (selectedBoat.IsInHole())
        {
            BoatPickedUp();
            selectedBoat.RemoveFromHole();
        }
        rightClick.Enable();
    }

    private void OnDeselectBoat()
    {
        selectedBoat.ToggleSelection(false);
        selectedBoat.SetCollider(true);
        currentBoatHoles = 0;
        selectedBoat = null;
        rightClick.Disable();
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (hasLeftClicked) return;
        hasLeftClicked = true;
    }

    private void OnLeftClickRelease(InputAction.CallbackContext context)
    {
        canPutBoatDown = false;
        Invoke(nameof(OnLeftClickReleaseFinish), Time.deltaTime);
    }

    private void OnLeftClickReleaseFinish() => canPutBoatDown = true;

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (selectedBoat == null) return;

        currentRotation = currentRotation switch
        {
            BoatRotation.Up => BoatRotation.Right,
            BoatRotation.Right => BoatRotation.Down,
            BoatRotation.Down => BoatRotation.Left,
            BoatRotation.Left => BoatRotation.Up,
            _ => currentRotation
        };

        int idx = GetIdx(coordsSelectedBoatIsShownOn);
        Boat boat = CanBoatShow(coordsSelectedBoatIsShownOn);
        if (boat != null)
            playerHoles[idx].ShowBoat(boat, currentRotation);
        else
            PutBoatBack();
    }

    public bool OnBoatButton(Vector2Int coordinates)
    {
        if (selectedBoat == null || !canPutBoatDown) return false;

        PinHole[] holes = new PinHole[currentBoatHoles];

        int totalHoles = totalRows * holesPerRow - 1;
        int idx = GetIdx(coordinates);
        if (idx < 0 || idx > totalHoles)
            return false;

        PinHole hole = playerHoles[idx];
        bool canSetBoat = !hole.HasBoat();
        if (!canSetBoat)
            return false;

        holes[0] = hole;

        Vector2Int checkCoords = currentRotation switch
        {
            BoatRotation.Up => new Vector2Int(coordinates.x, coordinates.y - 1),
            BoatRotation.Right => new Vector2Int(coordinates.x + 1, coordinates.y),
            BoatRotation.Down => new Vector2Int(coordinates.x, coordinates.y + 1),
            BoatRotation.Left => new Vector2Int(coordinates.x - 1, coordinates.y),
            _ => coordinates
        };

        totalHoles = totalRows * holesPerRow - 1;
        idx = GetIdx(checkCoords);
        if (idx < 0 || idx > totalHoles)
            return false;

        if (checkCoords.x >= holesPerRow || checkCoords.x < 0 || checkCoords.y >= totalRows || checkCoords.y < 0)
            return false;

        hole = playerHoles[idx];
        canSetBoat = !hole.HasBoat();
        if (!canSetBoat)
            return false;

        holes[1] = hole;

        if (currentBoatHoles == 3)
        {
            checkCoords = currentRotation switch
            {
                BoatRotation.Up => new Vector2Int(coordinates.x, coordinates.y + 1),
                BoatRotation.Right => new Vector2Int(coordinates.x - 1, coordinates.y),
                BoatRotation.Down => new Vector2Int(coordinates.x, coordinates.y - 1),
                BoatRotation.Left => new Vector2Int(coordinates.x + 1, coordinates.y),
                _ => coordinates
            };

            idx = GetIdx(checkCoords);
            if (idx < 0 || idx > totalHoles)
                return false;

            if (checkCoords.x >= holesPerRow || checkCoords.x < 0 || checkCoords.y >= totalRows || checkCoords.y < 0)
                return false;

            hole = playerHoles[idx];
            canSetBoat = !hole.HasBoat();
            if (!canSetBoat)
                return false;

            holes[2] = hole;
        }

        playerHoles[GetIdx(coordinates)]
            .SetBoat(selectedBoat, currentRotation, holes);

        OnDeselectBoat();

        BoatPutDown();

        return true;
    }

    public void OnShootButton(Vector2Int coordinates)
    {
        Debug.Log(coordinates);

        opponentHoles[GetIdx(coordinates)].Shoot();
    }

    public Boat CanBoatShow(Vector2Int coordinates)
    {
        if (selectedBoat == null) return null;

        int totalHoles = totalRows * holesPerRow - 1;
        int idx = GetIdx(coordinates);
        if (idx < 0 || idx > totalHoles)
            return null;

        bool canSetBoat = !playerHoles[idx].HasBoat();
        if (!canSetBoat)
            return null;

        Vector2Int checkCoords = currentRotation switch
        {
            BoatRotation.Up => new Vector2Int(coordinates.x, coordinates.y - 1),
            BoatRotation.Right => new Vector2Int(coordinates.x + 1, coordinates.y),
            BoatRotation.Down => new Vector2Int(coordinates.x, coordinates.y + 1),
            BoatRotation.Left => new Vector2Int(coordinates.x - 1, coordinates.y),
            _ => coordinates
        };

        totalHoles = totalRows * holesPerRow - 1;
        idx = GetIdx(checkCoords);
        if (idx < 0 || idx > totalHoles)
            return null;

        if (checkCoords.x >= holesPerRow || checkCoords.x < 0 || checkCoords.y >= totalRows || checkCoords.y < 0)
            return null;

        canSetBoat = !playerHoles[idx].HasBoat();
        if (!canSetBoat)
            return null;

        if (currentBoatHoles != 3)
        {
            coordsSelectedBoatIsShownOn = coordinates;
            return selectedBoat;
        }
        checkCoords = currentRotation switch
        {
            BoatRotation.Up => new Vector2Int(coordinates.x, coordinates.y + 1),
            BoatRotation.Right => new Vector2Int(coordinates.x - 1, coordinates.y),
            BoatRotation.Down => new Vector2Int(coordinates.x, coordinates.y - 1),
            BoatRotation.Left => new Vector2Int(coordinates.x + 1, coordinates.y),
            _ => coordinates
        };

        idx = GetIdx(checkCoords);
        if (idx < 0 || idx > totalHoles)
            return null;

        if (checkCoords.x >= holesPerRow || checkCoords.x < 0 || checkCoords.y >= totalRows || checkCoords.y < 0)
            return null;

        canSetBoat = !playerHoles[idx].HasBoat();
        if (!canSetBoat)
            return null;

        coordsSelectedBoatIsShownOn = coordinates;
        return selectedBoat;
    }

    private void BoatPickedUp()
    {
        boatsPutDown--;
        startButton.SetActive(false);
    }

    private void BoatPutDown()
    {
        boatsPutDown++;

        if (boatsPutDown == totalBoats) 
            startButton.SetActive(true);
    }

    public void PutBoatBack()
    {
        if (selectedBoat != null)
            selectedBoat.PutBack();
    }

    private int GetIdx(Vector2Int coords)
    {
        int idx = 0;
        if (coords.y == 0)
        {
            idx = coords.x;
        }
        else
        {
            idx += holesPerRow * coords.y;
            idx += coords.x;
        }

        return idx;
    }

    public BoatRotation GetRotation() => currentRotation;

    private void OnDestroy()
    {
        leftClick.performed -= OnLeftClick;
        leftClick.canceled -= OnLeftClickRelease;
        rightClick.started -= OnRightClick;
    }
}
