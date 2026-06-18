using System.Collections;
using UnityEngine;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private MainMenuManager mainMenuManager;

    [SerializeField] private Train train;

    [SerializeField] private Vector2 timeBetweenTrains;

    private void Start() => StartCoroutine(MoveTrains());

    private IEnumerator MoveTrains()
    {
        while (true)
        {
            float delay = Random.Range(timeBetweenTrains.x, timeBetweenTrains.y);

            yield return new WaitForSeconds(delay);

            train.PlayAnimation();
        }
    }

    public void FinishWalletZoom() => StartCoroutine(mainMenuManager.Zoom(true));
}
