using Lando;
using System.Collections;
using UnityEngine;

public class NFCSystem : MonoBehaviour
{
    [SerializeField] private MainMenuManager mainMenu;

    [SerializeField] private CharactersHolder characters;
    [SerializeField] private Character currentCharacter;

    private Cardreader cardReader;

    private void Awake()
    {
        cardReader = new Cardreader();

        cardReader.CardConnected += OnCardConnected;
        cardReader.CardDisconnected += OnCardDisconnected;

        EnableCardReader();
    }

    public void EnableCardReader()
    {
        cardReader.StartWatch();

        Debug.Log("Lando: Started watching for cards");
    }

    public void DisableCardReader()
    {
        cardReader.StopWatch();

        Debug.Log("Lando: Stopped watching for cards");
    }

    private void OnCardConnected(object sender, CardreaderEventArgs e)
    {
        cardReader.SetBuzzerOutputForCardDetection(e.Card, false);
        string cardId = e.Card.Id;
        Debug.Log($"ACR122U: Card connected with UID: {cardId}");

        foreach (Character character in characters.characters)
        {
            if (character.nfcID != cardId) continue;

            FoundChar(character);
            break;
        }
    }

    private void FoundChar(Character character)
    {
        currentCharacter = character;
        Debug.Log(currentCharacter);
        mainMenu.NewCharacter(character);
    }

    private void OnCardDisconnected(object sender, CardreaderEventArgs e)
    {
        Debug.Log($"ACR122U: Card disconnected");
        currentCharacter = null;
        mainMenu.RemoveCharacter();
    }

    private void OnDestroy()
    {
        DisableCardReader();
        cardReader.Dispose();
        Debug.Log("Lando: Stopped watching and disposed reader");

        cardReader.CardConnected -= OnCardConnected;
        cardReader.CardDisconnected -= OnCardDisconnected;
    }
}
