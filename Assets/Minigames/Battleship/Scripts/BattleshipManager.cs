using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BattleshipManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BattleshipOpponentManager opponentManager;
    [SerializeField] private GameObject startButton;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Camera cam;
    [SerializeField] private CharacterParent characterManager;
    [SerializeField] private CharacterParent bartManager;
    [SerializeField] private PoofParticle poofParticle;
    [SerializeField] private AudioSource sfxSource;

    [Header("Board Settings")]
    [SerializeField] private int holesPerRow;
    [SerializeField] private int totalRows;
    private BoatRotation currentRotation;
    private int currentBoatHoles;
    private Vector2Int coordsSelectedBoatIsShownOn;
    private Turn turn;

    [Header("Boards")]
    [SerializeField] private PinHole[] playerHoles;
    [SerializeField] private BattleshipBoardButton[] playerBoardButtons;
    [SerializeField] private PinHole[] opponentHoles;
    [SerializeField] private BattleshipBoardButton[] opponentBoardButtons;

    [Header("Boats")]
    [SerializeField] private string boatTag;
    [SerializeField] private Transform boatsParent;
    [SerializeField] private Boat[] boats2;
    [SerializeField] private Boat[] boats3;
    private Boat selectedBoat;
    private bool shouldSearchForBoat;
    private bool canPutBoatDown;
    private int totalBoats;
    private int boatsPutDown;
    private int playerBoatsSunk;
    private int opponentBoatsSunk;
    private bool didPlayerFinish;
    private bool didOpponentFinish;

    [Header("Inputs")]
    [SerializeField] private InputActionAsset inputs;
    private InputAction mousePositionAction;
    private Vector2 mousePos;
    private InputAction leftClick;
    private bool hasLeftClicked;
    private InputAction rightClick;

    [Header("Animation Settings")]
    [SerializeField] private float delayUntilExit;
    private WaitForSeconds waitDelayUntilExit;
    [SerializeField] private float interactionAnimTime;
    private WaitForSeconds waitInteractionAnimTime;
    private bool isBart;

    private void Awake()
    {
        leftClick = inputs.FindAction("Player/Attack");
        rightClick = inputs.FindAction("Player/RightClick");
        mousePositionAction = inputs.FindAction("Player/MousePos");

        leftClick.performed += OnLeftClick;
        leftClick.canceled += OnLeftClickRelease;
        rightClick.started += OnRightClick;

        leftClick.Enable();
        rightClick.Enable();

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

        opponentManager.Setup(new Vector2Int(holesPerRow, totalRows), opponentHoles);

        waitDelayUntilExit = new WaitForSeconds(delayUntilExit);
        waitInteractionAnimTime = new WaitForSeconds(interactionAnimTime);

        isBart = SaveSystem.currentSave.choseBart;
        if (isBart)
            characterManager = bartManager;
    }

    private void Start()
    {
        StartCoroutine(FindBoat());
        StartCoroutine(opponentManager.StartOpponent());
    }

    public bool CanShoot() => turn == Turn.Player;

    public void OpponentFinishedPlacingBoats()
    {
        didOpponentFinish = true;
        TryEnablingStartButton();
    }

    private void TryEnablingStartButton()
    {
        if (boatsPutDown == totalBoats)
            didPlayerFinish = true;

        if (didOpponentFinish && didPlayerFinish)
            startButton.SetActive(true);
        else
            startButton.SetActive(false);
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
                            Boat otherBoat = hit.collider.GetComponent<Boat>();
                            if (!otherBoat.IsInHole())
                            {
                                OnDeselectBoat();
                                selectedBoat = otherBoat;
                                OnSelectBoat();
                            }
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

        foreach (BattleshipBoardButton b in opponentBoardButtons)
            b.ToggleInteraction(true);

        turn = Turn.Player;
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

    private void OnLeftClickReleaseFinish()
    {
        canPutBoatDown = true;
        if (selectedBoat != null)
            selectedBoat.SetCollider(false);
    }

    private void OnRightClick(InputAction.CallbackContext context)
    {
        if (selectedBoat == null) return;

        currentRotation = currentRotation switch
        {
            BoatRotation.None => BoatRotation.Up,
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

        playerHoles[GetIdx(coordinates)].SetBoat(selectedBoat, currentRotation, holes);
        OnDeselectBoat();
        BoatPutDown();

        return true;
    }

    public void OnShootButton(Vector2Int coordinates)
    {
        BoatHitState hitState = opponentHoles[GetIdx(coordinates)].Shoot(false);
        if (hitState == BoatHitState.Sunk)
            SunkBoat();
        switch (hitState)
        {
            case BoatHitState.Miss:
                AudioManager.PlayOneShot(Sounds.BattleshipSetWhitePin, sfxSource);
                break;
            case BoatHitState.Hit or BoatHitState.Sunk:
                AudioManager.PlayOneShot(Sounds.BattleshipSetRedPin, sfxSource);
                break;
        }
        SwitchTurn();
    }

    public BoatHitState OpponentShoot(int idx)
    {
        BoatHitState hitState = playerHoles[idx].Shoot(true);
        if (hitState == BoatHitState.Sunk)
            SunkBoat();
        switch (hitState)
        {
            case BoatHitState.Miss:
                AudioManager.PlayOneShot(Sounds.BattleshipSetWhitePin, sfxSource);
                break;
            case BoatHitState.Hit or BoatHitState.Sunk:
                AudioManager.PlayOneShot(Sounds.BattleshipSetRedPin, sfxSource);
                break;
        }
        return hitState;
    }

    public void SunkBoat()
    {
        switch (turn)
        {
            case Turn.Player:
            {
                opponentBoatsSunk++;
                if (opponentBoatsSunk == totalBoats)
                {
                    StartCoroutine(FinishMinigame(true));
                        turn = Turn.None;
                }
                break;
            }
            case Turn.Opponent:
            {
                playerBoatsSunk++;
                if (playerBoatsSunk == totalBoats)
                {
                    StartCoroutine(FinishMinigame(false));
                    turn = Turn.None;
                }
                break;
            }
        }
    }

    private IEnumerator FinishMinigame(bool hasWon)
    {
        yield return waitDelayUntilExit;
        dialogueManager.ExitMinigame(hasWon);
        gameObject.SetActive(false);
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
        AudioManager.PlayOneShot(Sounds.BattleshipGrabBoat, sfxSource);
        TryEnablingStartButton();
        startButton.SetActive(false);
    }

    private void BoatPutDown()
    {
        boatsPutDown++;
        AudioManager.PlayOneShot(Sounds.BattleshipSetBoat, sfxSource);
        TryEnablingStartButton();
    }

    public void PutBoatBack()
    {
        if (selectedBoat == null) return;

        selectedBoat.PutBack(boatsParent);
        AudioManager.PlayOneShot(Sounds.BattleshipSetBoat, sfxSource);
    }

    public void SwitchTurn()
    {
        switch (turn)
        {
            case Turn.Player:
                turn = Turn.Opponent;
                opponentManager.StartTurn();
                break;
            case Turn.Opponent:
                turn = Turn.Player;
                break;
        }
    }

    public IEnumerator PlayAnimation()
    {
        characterManager.SetAnimation(CharacterAnimations.BattleshipInteraction, true);
        yield return waitInteractionAnimTime;
    }

    public void PlayParticle(Vector3 pos)
    {
        poofParticle.transform.position = pos;
        poofParticle.Play();
    }

    private void OnDestroy()
    {
        leftClick.performed -= OnLeftClick;
        leftClick.canceled -= OnLeftClickRelease;
        rightClick.started -= OnRightClick;
    }
}
