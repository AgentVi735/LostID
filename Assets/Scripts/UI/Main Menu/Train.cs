using System.Collections;
using UnityEngine;

public class Train : MonoBehaviour
{
    [SerializeField] private RectTransform trainRect;
    [SerializeField] private TrainMiddle[] middleCarts;

#if UNITY_EDITOR
    [SerializeField] private Vector2 trainStartPosEditor;
#endif
    [SerializeField] private Vector2 trainStartPos;
    [SerializeField] private Vector2 trainEndPos;
    [SerializeField] private float moveTime;
    [SerializeField] private float moveTimeToRemove;

    [SerializeField] private float timeToStop;
    private WaitForSeconds waitTimeToStop;

    private void Awake()
    {
        if (timeToStop > 0)
            waitTimeToStop = new WaitForSeconds(timeToStop);
    }

    public IEnumerator Move()
    {
        Vector2 startPos = trainStartPos;
        Vector2 endPos = trainEndPos;

        trainRect.localPosition = startPos;

        yield return null;

        if (timeToStop <= 0)
        {
            for (float i = 0; i < moveTime; i += Time.deltaTime)
            {
                trainRect.localPosition = Vector2.Lerp(startPos, endPos, i / moveTime);
                yield return null;
            }

            trainRect.localPosition = endPos;
        }
        else
        {
#if UNITY_EDITOR
            trainRect.localPosition = trainStartPosEditor;
#endif

            float tempMoveTime = moveTime;
            float time = 0;
            while (tempMoveTime > 0)
            {
                trainRect.localPosition -= new Vector3(tempMoveTime, 0, 0);
                tempMoveTime = moveTime - time * moveTimeToRemove;
                time += Time.deltaTime;
                yield return Time.deltaTime;
            }
        }

        if (timeToStop <= 0) yield break;
        {
            endPos = trainEndPos;

            for (int i = 0; i < middleCarts.Length; i++)
            {
                if (i == middleCarts.Length - 1)
                    yield return StartCoroutine(middleCarts[i].ToggleDoors(true));
                else
                    StartCoroutine(middleCarts[i].ToggleDoors(true));
            }

            yield return waitTimeToStop;

            for (int i = 0; i < middleCarts.Length; i++)
            {
                if (i == middleCarts.Length - 1)
                    yield return StartCoroutine(middleCarts[i].ToggleDoors(false));
                else
                    StartCoroutine(middleCarts[i].ToggleDoors(false));
            }

            float tempMoveTime = 0;
            float time = 0;
            while (trainRect.localPosition.x > endPos.x)
            {
                trainRect.localPosition -= new Vector3(tempMoveTime, 0, 0);
                tempMoveTime = time * moveTimeToRemove;
                time += Time.deltaTime;
                yield return Time.deltaTime;
            }
        }
    }
}
