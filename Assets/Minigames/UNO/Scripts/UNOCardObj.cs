using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UNOCardObj : MonoBehaviour
{
    public UNOCard card { get; private set; }
    public bool canClick { get; private set; }

    [SerializeField] private GameObject wildcardBalls;

    private Camera cam;
    private UNOManager unoManager;

    private InputActionAsset inputs;
    private InputAction mousePositionAction;
    private Vector2 mousePos;
    private InputAction leftClick;
    private bool hasLeftClicked;

    [SerializeField] private string ballTag;
    
    //TEMP
    [SerializeField] private TMP_Text cardText;

    private bool lookForBalls;

    public void Load(UNOCard givenCard, InputActionAsset givenInputs, bool isPlayerCard, UNOManager givenUnoManager)
    {
        card = givenCard;
        cardText.text = card.colour + " " + card.type;
        if (card.type != UNOCardType.Wildcard || !isPlayerCard) return;
        wildcardBalls.SetActive(false);
        cam = Camera.main;
        unoManager = givenUnoManager;
        inputs = givenInputs; 
        leftClick = inputs.FindAction("Player/Attack");
        mousePositionAction = inputs.FindAction("Player/MousePos");

        leftClick.performed += OnLeftClick;
    }

    public void OnPlay()
    {
        wildcardBalls.SetActive(false);
    }

    public void CheckCard(UNOCard cardToCheckWith)
    {
        if (cardToCheckWith.colour == card.colour || cardToCheckWith.type == card.type)
            canClick = true;
        else
            canClick = false;

        if (card.type == UNOCardType.Wildcard)
            canClick = true;

        if (cardToCheckWith.colour == UNOCardColor.None)
            canClick = true;
    }

    public void ShowWildcardBalls(bool toggle)
    {
        wildcardBalls.SetActive(toggle);

        if (toggle)
        {

            Debug.Log("starting routine");
            StartCoroutine(LookForBalls());
        }
        else
        {
            Debug.Log("stopping routine");
            lookForBalls = false;
        }
    }

    private IEnumerator LookForBalls()
    {
        bool isLookingAtBall = false;
        lookForBalls = true;

        while (lookForBalls)
        {
            mousePos = mousePositionAction.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, -10));
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.CompareTag(ballTag))
            {
                if (!isLookingAtBall)
                {
                    leftClick.Enable();
                    hasLeftClicked = false;
                }

                isLookingAtBall = true;

                if (hasLeftClicked)
                {
                    hasLeftClicked = false;
                    WildcardBall ball = hit.collider.GetComponent<WildcardBall>();
                    OnClickedBall(ball.colour);
                }
            }
            else
            {
                if (isLookingAtBall)
                    leftClick.Disable();
                isLookingAtBall = false;
                hasLeftClicked = false;
            }

            yield return null;
        }
    }

    private void OnClickedBall(UNOCardColor givenColour)
    {
        lookForBalls = false;
        card.colour = givenColour;
        leftClick.performed -= OnLeftClick;
        leftClick = null;
        mousePositionAction = null;
        inputs = null;
        cam = null;
        unoManager.PlayWildcard(this);
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (hasLeftClicked) return;
        hasLeftClicked = true;
    }

    private void OnDestroy()
    {
        if (leftClick != null)
            leftClick.performed -= OnLeftClick;
    }

    public void DebugLogCard()
    {
#if UNITY_EDITOR
        Debug.Log(card.colour + " " + card.type);
#endif
    }
}
