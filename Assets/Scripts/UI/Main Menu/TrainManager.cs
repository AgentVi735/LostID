using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrainManager : MonoBehaviour
{
    [SerializeField] private Train[] trains;
    private Train currentTrain;

    [SerializeField] private Vector2 timeBetweenTrains;

    private void Start()
    {
        StartCoroutine(MoveTrains());
    }

    private IEnumerator MoveTrains()
    {
        while (true)
        {
            float delay = Random.Range(timeBetweenTrains.x, timeBetweenTrains.y);

            yield return new WaitForSeconds(delay);

            currentTrain = trains[Random.Range(0, trains.Length)];
            yield return currentTrain.Move();
        }
    }
}
