using TMPro;
using UnityEngine;

public class ResponseButton : MonoBehaviour
{
    private ResponseManager responseManager;

    [SerializeField] private TMP_Text text;
    private Response response;

    [Header("Phone Settings")]
    [SerializeField] private bool isForPhone;
    [SerializeField] private RectTransform textRect;
    [SerializeField] private RectTransform rect;
    [SerializeField] private Vector2 paddedArea;

    public void Setup(ResponseManager givenManager, bool givenPhone)
    {
        gameObject.SetActive(false);
        responseManager = givenManager;
        isForPhone = givenPhone;
    }

    public void LoadButton(Response givenResponse)
    {
        response = givenResponse;
        text.text = response.text;
        gameObject.SetActive(true);

        textRect.sizeDelta = text.GetPreferredValues();
        rect.sizeDelta = text.GetPreferredValues() + paddedArea;
    }

    public void OnClick()
    {
        responseManager.GotResponse(response);
    }
}
