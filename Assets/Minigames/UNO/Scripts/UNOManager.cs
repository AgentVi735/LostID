using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class UNOManager : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private UNOCardHolder cardHolder;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int startingCards;

    [SerializeField] private Transform cardsParent;
    [SerializeField] private Transform opponentCardsParent;
    [SerializeField] private float distanceBetweenCards;

    private List<UNOCard> playerCards;
    private List<UNOCardObj> playerCardObjs;
    private List<UNOCard> opponentCards;
    private List<UNOCardObj> opponentCardObjs;

    private List<UNOCard> cardsStack;
    private List<UNOCard> disposedStack;

    [SerializeField] private InputActionAsset inputs;
    private InputAction grabCard;
    private InputAction mousePositionAction;
    private Vector2 mousePos;
    private InputAction leftClick;
    private bool hasLeftClicked;

    private Turn turn;

    private void Start()
    {
        Load();

        leftClick = inputs.FindAction("Player/Attack");
        grabCard = inputs.FindAction("Player/Jump");
        mousePositionAction = inputs.FindAction("Player/MousePos");

        leftClick.performed += OnLeftClick;
        grabCard.performed += GrabCard;

        StartCoroutine(LookForCard());
    }

    private IEnumerator LookForCard()
    {
        while (true)
        {
            mousePos = mousePositionAction.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, -10));
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hasLeftClicked)
                {
                    hasLeftClicked = false;
                    print(hit.collider.name);
                }
            }

            yield return null;
        }
    }

    private void OnDestroy()
    {
        grabCard.performed -= GrabCard;
        leftClick.performed -= OnLeftClick;
    }

    private void GrabCard(InputAction.CallbackContext context)
    {
        if (turn != Turn.Player) return;
        GrabCard(1, true);
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (hasLeftClicked) return;
        hasLeftClicked = true;
        Invoke(nameof(ResetLeftClick), Time.deltaTime);
    }

    private void ResetLeftClick()
    {
        hasLeftClicked = false;
    }

    private void Load()
    {
        playerCards = new List<UNOCard>();
        playerCardObjs = new List<UNOCardObj>();
        opponentCards = new List<UNOCard>();
        opponentCardObjs = new List<UNOCardObj>();
        cardsStack = new List<UNOCard>();
        disposedStack = new List<UNOCard>();
        cardsStack = cardHolder.cards.ToList();
        cardsStack = cardsStack.OrderBy(i => Random.value).ToList();

        GrabCard(startingCards, true);
        GrabCard(startingCards, false);

        turn = Turn.Player;
    }

    private void GrabCard(int amt, bool forPlayer)
    {
        List<UNOCard> cards = forPlayer ? playerCards : opponentCards;

        for (int i = 0; i < amt; i++)
        {
            cards.Add(cardsStack[0]);
            cardsStack.RemoveAt(0);
            SpawnCard(forPlayer);
        }

        SortCards(forPlayer);
    }

    private void SpawnCard(bool forPlayer)
    {
        List<UNOCardObj> cardsObjs = forPlayer ? playerCardObjs : opponentCardObjs;
        Transform parent = forPlayer ? cardsParent : opponentCardsParent;

        UNOCardObj obj = Instantiate(cardPrefab, parent.position, Quaternion.identity, parent).GetComponent<UNOCardObj>();
        cardsObjs.Add(obj);
    }

    private void SortCards(bool forPlayer)
    {
        List<UNOCard> cards = forPlayer ? playerCards : opponentCards;
        List<UNOCardObj> cardsObjs = forPlayer ? playerCardObjs : opponentCardObjs;

        Vector3 startingPoint = Vector3.zero;

        startingPoint = new Vector3(startingPoint.x - distanceBetweenCards / 2 * cards.Count, startingPoint.y, startingPoint.z);

        foreach (UNOCardObj card in cardsObjs)
        {
            card.transform.localPosition = startingPoint;
            startingPoint = new Vector3(startingPoint.x + distanceBetweenCards, startingPoint.y, startingPoint.z);
        }
    }
}
