using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResponseManager : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueBox dialogueBox;

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonHolder;
    [SerializeField] private int buttonCount;

    private ResponseButton[] buttons;

    private ResponseHolder currentHolder;

    private bool isPhone;

    public void Setup(bool phone)
    {
        isPhone = phone;
        buttons = new ResponseButton[buttonCount];
        for (int i = 0; i < buttonCount; i++)
        {
            ResponseButton button = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, buttonHolder).GetComponent<ResponseButton>();
            buttons[i] = button;
            button.Setup(this);
        }
    }

    public void LoadResponses(ResponseHolder givenHolder)
    {
        currentHolder = givenHolder;

        int count = currentHolder.responses.Length;

        if (count > buttonCount)
        {
            Debug.LogError("There are too little buttons for response holder: " + currentHolder.name + " to show all responses.");
            count = buttonCount;
        }

        for (int i = 0; i < count; i++)
        {
            ResponseButton button = buttons[i];
            button.LoadButton(currentHolder.responses[i]);
        }
    }

    public void GotResponse(Response response)
    {
        EventSystem.current.SetSelectedGameObject(null);

        foreach (ResponseButton button in buttons)
            button.gameObject.SetActive(false);

        if (isPhone)
        {
            StartCoroutine(GotResponsePhone(response));
            return;
        }

        if (response.nextObj != null)
            dialogueManager.Continue(response.nextObj);
    }

    private IEnumerator GotResponsePhone(Response response)
    {
        yield return StartCoroutine(dialogueBox.CreatePlayerBubble(response.text));

        if (response.nextObj != null)
            dialogueManager.Continue(response.nextObj);
    }
}
