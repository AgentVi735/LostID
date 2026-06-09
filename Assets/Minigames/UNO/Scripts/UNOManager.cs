using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UNOManager : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private DialogueManager dialogueManager;

    [SerializeField] private UNOCardHolder cardHolder;

    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private int startingCards;

    [SerializeField] private Transform cardsParent;
    [SerializeField] private Transform opponentCardsParent;
    [SerializeField] private float distanceBetweenCards;

    [SerializeField] private string cardTag;
    [SerializeField] private string disposedCardTag;
    [SerializeField] private string wildcardBallTag;

    [SerializeField] private Vector3 disposeStackPos;
    private UNOCardObj disposedStackCard;
    [SerializeField] private Image bigDisposedStackCard;

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

    private Coroutine lookingForCardsCoroutine;

    [SerializeField] private int playerCardsThresholdForPlusCards;

    private void Start() => Load();

    private void LoadInput()
    {
        leftClick = inputs.FindAction("Player/Attack");
        grabCard = inputs.FindAction("Player/Jump");
        mousePositionAction = inputs.FindAction("Player/MousePos");

        grabCard.performed += GrabCard;
        leftClick.performed += OnLeftClick;
    }

    private void SwitchTurns()
    {
        switch (turn)
        {
            case Turn.Player:
                SetTurn(Turn.Opponent);
                break;
            case Turn.Opponent:
                SetTurn(Turn.Player);
                break;
        }
    }

    private void SetTurn(Turn newTurn)
    {
        turn = newTurn;
        switch (turn)
        {
            case Turn.Player:
                grabCard.Enable();
                CheckCards();
                lookingForCardsCoroutine = StartCoroutine(LookForCard());
                break;
            case Turn.Opponent:
                grabCard.Disable();
                OpponentStart();
                break;
            default:
                grabCard.Disable();
                break;
        }
    }

    private void CheckCards()
    {
        if (disposedStackCard == null) return;
        UNOCard topCard = disposedStackCard.card;
        List<UNOCardObj> cardsObjs = turn == Turn.Player ? playerCardObjs : opponentCardObjs;

        foreach (UNOCardObj card in cardsObjs) 
            card.CheckCard(topCard);
    }

    private IEnumerator LookForCard()
    {
        bool isLookingAtCard = false;
        UNOCardObj card = null;
        UNOCardObj wildcard = null;
        bool hasWildcardSelected = false;

        while (turn == Turn.Player)
        {
            mousePos = mousePositionAction.ReadValue<Vector2>();
            Ray ray = cam.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, -10));
            if (Physics.Raycast(ray, out RaycastHit hit) && (hit.collider.CompareTag(cardTag) || hit.collider.CompareTag(wildcardBallTag)))
            {

                if (!isLookingAtCard)
                {
                    leftClick.Enable();
                    hasLeftClicked = false;
                }

                isLookingAtCard = true;
                if (hasWildcardSelected && hit.collider.CompareTag(cardTag) &&
                    hit.collider.gameObject != wildcard.gameObject)
                {
                    wildcard.ShowWildcardBalls(false);
                    wildcard = null;
                    hasWildcardSelected = false;
                }

                if (hasLeftClicked && !hasWildcardSelected)
                {
                    hasLeftClicked = false;
                    UNOCardObj hitComponent = hit.collider.GetComponent<UNOCardObj>();
                    if (hitComponent != null)
                        card = hitComponent;
                    if (card != null)
                    {
                        bool isWildcard;
                        if (card.card.type == UNOCardType.Wildcard)
                        {
                            isWildcard = true;
                            hasWildcardSelected = true;
                            wildcard = card;
                        }
                        else
                        {
                            isWildcard = false;
                        }

                        bool canClick = card.canClick;
                        if (canClick)
                        {
                            if (!isWildcard)
                                PlayCard(card);
                            else
                                card.ShowWildcardBalls(true);
                        }
                    }
                }

            }
            else
            {
                if (isLookingAtCard)
                    leftClick.Disable();
                if (hasWildcardSelected && wildcard != null)
                {
                    wildcard.ShowWildcardBalls(false);
                    wildcard = null;
                    hasWildcardSelected = false;
                }

                isLookingAtCard = false;
                card = null;
                hasLeftClicked = false;
            }

            yield return null;
        }
    }

    public void PlayWildcard(UNOCardObj card)
    {
        if (lookingForCardsCoroutine != null)
            StopCoroutine(lookingForCardsCoroutine);
        PlayCard(card);
    }

    private void PlayCard(UNOCardObj card)
    {
        if (lookingForCardsCoroutine != null)
            StopCoroutine(lookingForCardsCoroutine);
        card.OnPlay();
        List<UNOCard> cards = turn == Turn.Player ? playerCards : opponentCards;
        List<UNOCardObj> cardsObjs = turn == Turn.Player ? playerCardObjs : opponentCardObjs;
        cards.Remove(card.card);
        cardsObjs.Remove(card);
        card.transform.SetPositionAndRotation(disposeStackPos, Quaternion.identity);
        card.transform.SetParent(transform);
        disposedStack.Add(disposedStackCard.card);
        Destroy(disposedStackCard.gameObject);
        disposedStackCard = card;
        bigDisposedStackCard.sprite = disposedStackCard.card.sprite;
        card.DebugLogCard();
        SortCards(turn == Turn.Player);

        if (disposedStackCard.card.type == UNOCardType.PlusTwo) 
            GrabCard(2, turn != Turn.Player);
        if (disposedStackCard.card.type == UNOCardType.Wildcard)
        {
            if (turn == Turn.Opponent)
            {
                int redColours = 0;
                int blueColours = 0;
                int yellowColours = 0;
                int greenColours = 0;
                foreach (UNOCard opponentCard in opponentCards)
                {
                    switch (opponentCard.colour)
                    {
                        case UNOCardColor.Red:
                            redColours++;
                            break;
                        case UNOCardColor.Blue:
                            blueColours++;
                            break;
                        case UNOCardColor.Yellow:
                            yellowColours++;
                            break;
                        case UNOCardColor.Green:
                            greenColours++;
                            break;
                    }
                }

                UNOCardColor highestColour = UNOCardColor.Red;
                int max = redColours;

                if (blueColours > max)
                {
                    max = blueColours;
                    highestColour = UNOCardColor.Blue;
                }
                if (yellowColours > max)
                {
                    max = yellowColours;
                    highestColour = UNOCardColor.Yellow;
                }
                if (greenColours > max)
                {
                    highestColour = UNOCardColor.Green;
                }

                disposedStackCard.card.colour = highestColour;
                Debug.Log("Opponent: changed colour to: " + highestColour);
            }
        }

        if (cards.Count > 0)
        {
            SwitchTurns();
            return;
        }

        gameObject.SetActive(false);
        dialogueManager.ExitMinigame(turn == Turn.Player);
    }

    private void ShuffleNewStack()
    {
        cardsStack = disposedStack.OrderBy(i => Random.value).ToList();
        disposedStack.Clear();
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
        SwitchTurns();
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (hasLeftClicked) return;
        hasLeftClicked = true;
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

        GrabStartCard();

        LoadInput();

        SetTurn(Turn.Player);
    }

    private void GrabStartCard()
    {
        UNOCardObj obj = Instantiate(cardPrefab, disposeStackPos, Quaternion.identity, transform).GetComponent<UNOCardObj>();

        obj.Load(cardsStack[0], null, false, null);
        cardsStack.RemoveAt(0);
        disposedStackCard = obj;
        bigDisposedStackCard.sprite = disposedStackCard.card.sprite;
        disposedStackCard.DebugLogCard();
    }

    private void GrabCard(int amt, bool forPlayer)
    {
        List<UNOCard> cards = forPlayer ? playerCards : opponentCards;

        for (int i = 0; i < amt; i++)
        {
            cards.Add(cardsStack[0]);
            SpawnCard(forPlayer, cardsStack[0]);
            cardsStack.RemoveAt(0);
            if (cardsStack.Count == 0)
                ShuffleNewStack();
        }

        SortCards(forPlayer);
    }

    private void SpawnCard(bool forPlayer, UNOCard card)
    {
        List<UNOCardObj> cardsObjs = forPlayer ? playerCardObjs : opponentCardObjs;
        Transform parent = forPlayer ? cardsParent : opponentCardsParent;
        Quaternion rotation = Quaternion.Euler(forPlayer ? new Vector3(-90,0,0) : new Vector3(-90, 0, 180)); // TODO: Set these in a variable!!!!!

        UNOCardObj obj = Instantiate(cardPrefab, parent.position, rotation, parent).GetComponent<UNOCardObj>();
        cardsObjs.Add(obj);
        obj.Load(card, inputs, forPlayer, this);
    }

    private void SortCards(bool forPlayer)
    {
        List<UNOCard> cards;
        List<UNOCardObj> cardsObjs;
        if (forPlayer)
        {
            playerCards = playerCards
                .OrderBy(c => (int)c.colour)
                .ThenBy(c => (int)c.type)
                .ToList();
            cards = playerCards;

            playerCardObjs = playerCardObjs
                .OrderBy(c => (int)c.card.colour)
                .ThenBy(c => (int)c.card.type)
                .ToList();
            cardsObjs = playerCardObjs;
        }
        else
        {
            opponentCards = opponentCards
                .OrderBy(c => (int)c.colour)
                .ThenBy(c => (int)c.type)
                .ToList();
            cards = opponentCards;

            opponentCardObjs = opponentCardObjs
                .OrderBy(c => (int)c.card.colour)
                .ThenBy(c => (int)c.card.type)
                .ToList();
            cardsObjs = opponentCardObjs;
        }

        Vector3 startingPoint = Vector3.zero;

        startingPoint = new Vector3(startingPoint.x - distanceBetweenCards / 2 * cards.Count, startingPoint.y, startingPoint.z);

        foreach (UNOCardObj card in cardsObjs)
        {
            card.transform.localPosition = startingPoint;
            startingPoint = new Vector3(startingPoint.x + distanceBetweenCards, startingPoint.y, startingPoint.z);
        }

        CheckCards();
    }

    private void OpponentStart()
    {
        CheckCards();
        OpponentCheckForCards();
    }

    private void OpponentCheckForCards()
    {
        if (opponentCards.Count == 0)
        {
            OpponentGrabCard();
            return;
        }

        List<UNOCardObj> playableCards = new();
        int hasPlusCards = 0;
        int hasWildcards = 0;
        int correctColours = 0;
        int correctType = 0;
        foreach (UNOCardObj card in opponentCardObjs.Where(card => card.canClick))
        {
            playableCards.Add(card);
            switch (card.card.type)
            {
                case UNOCardType.PlusTwo:
                    hasPlusCards++;
                    break;
                case UNOCardType.Wildcard:
                    hasWildcards++;
                    break;
            }

            if (card.card.colour == disposedStackCard.card.colour && card.card.type != UNOCardType.Wildcard)
                correctColours++;

            if (card.card.type == disposedStackCard.card.type)
                correctType++;
        }

        if (playableCards.Count == 0)
        {
            OpponentGrabCard();
            return;
        }

        if (playerCardObjs.Count <= playerCardsThresholdForPlusCards)
        {
            if (hasPlusCards > 0)
            {
                UNOCardObj cardToPlay = null;
                foreach (UNOCardObj playableCard in playableCards.Where(playableCard => playableCard.card.type == UNOCardType.PlusTwo))
                {
                    if (cardToPlay == null)
                        cardToPlay = playableCard;
                    else
                    {
                        if (playableCard.card.colour != disposedStackCard.card.colour) continue;
                        cardToPlay = playableCard;
                        break;
                    }
                }

                if (cardToPlay != null)
                {
                    OpponentPlayCard(cardToPlay);
                    return;
                }
            }
        }

        if (correctColours == 0 && correctType == 0 && hasWildcards > 0)
        {
            UNOCardObj cardToPlay = playableCards.FirstOrDefault(playableCard => playableCard.card.type == UNOCardType.Wildcard);

            if (cardToPlay != null)
            {
                OpponentPlayCard(cardToPlay);
                return;
            }
        }

        if (correctColours > 0 || correctType > 0)
        {
            List<UNOCardObj> correctPlayableCards = playableCards.Where(card => (card.card.colour == disposedStackCard.card.colour && card.card.type != UNOCardType.Wildcard) || card.card.type == disposedStackCard.card.type).ToList();

            int rnd = Random.Range(0, correctPlayableCards.Count - 1);
            UNOCardObj cardToPlay = correctPlayableCards[rnd];
            OpponentPlayCard(cardToPlay);
            return;
        }

        Debug.LogError("Couldn't play a card");
        disposedStackCard.DebugLogCard();
        OpponentGrabCard();
    }

    private void OpponentPlayCard(UNOCardObj cardObj)
    {
        Debug.Log("Opponent: playing card");
        PlayCard(cardObj);
    }

    private void OpponentGrabCard()
    {
        Debug.Log("Opponent: grabbing card");
        disposedStackCard.DebugLogCard();
        GrabCard(1, false);
        OpponentEndTurn();
    }

    private void OpponentEndTurn()
    {
        SwitchTurns();
    }
}
