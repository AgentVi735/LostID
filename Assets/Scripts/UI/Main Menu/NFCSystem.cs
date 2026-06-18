using Lando;
using UnityEngine;

public class NFCSystem : MonoBehaviour
{
    [SerializeField] private MainMenuManager mainMenu;

    [SerializeField] private CharactersHolder characters;

    private Cardreader cardReader;

    private void Awake()
    {
        cardReader = new Cardreader();

        cardReader.CardConnected += OnCardConnected;
        cardReader.CardDisconnected += OnCardDisconnected;

        EnableCardReader();
    }

    public void EnableCardReader() => cardReader.StartWatch();

    public void DisableCardReader() => cardReader.StopWatch();

    private void OnCardConnected(object sender, CardreaderEventArgs e)
    {
        cardReader.SetBuzzerOutputForCardDetection(e.Card, false);
        string cardId = e.Card.Id;

        foreach (Character character in characters.characters)
        {
            if (character.nfcID != cardId) continue;

            FoundChar(character);
            break;
        }
    }

    private void FoundChar(Character character) => mainMenu.NewCharacter(character);

    private void OnCardDisconnected(object sender, CardreaderEventArgs e) => mainMenu.RemoveCharacter();

    private void OnDestroy()
    {
        DisableCardReader();
        cardReader.Dispose();;

        cardReader.CardConnected -= OnCardConnected;
        cardReader.CardDisconnected -= OnCardDisconnected;
    }
}
