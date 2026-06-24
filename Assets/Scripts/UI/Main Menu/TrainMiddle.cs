using System.Collections;
using UnityEngine;

public class TrainMiddle : MonoBehaviour
{
    [SerializeField] private RectTransform leftDoor;
    [SerializeField] private RectTransform rightDoor;

    [SerializeField] private Vector2 leftDoorOpenPos;
    [SerializeField] private Vector2 rightDoorOpenPos;

    [SerializeField] private float openDoorsTime;

    [SerializeField] private AudioSource sfxSource;

    public IEnumerator ToggleDoors(bool toggle)
    {
        Vector2 startPosL = toggle ? Vector2.zero : leftDoorOpenPos;
        Vector2 startPosR = toggle ? Vector2.zero : rightDoorOpenPos;

        Vector2 endPosL = toggle ? leftDoorOpenPos : Vector2.zero;
        Vector2 endPosR = toggle ? rightDoorOpenPos : Vector2.zero;

        if (toggle)
            AudioManager.PlayOneShot(Sounds.TrainDoorsOpen, sfxSource);

        for (float i = 0; i < openDoorsTime; i += Time.deltaTime)
        {
            leftDoor.localPosition = Vector2.Lerp(startPosL, endPosL, i / openDoorsTime);
            rightDoor.localPosition = Vector2.Lerp(startPosR, endPosR, i / openDoorsTime);
            yield return null;
        }

        leftDoor.localPosition = endPosL;
        rightDoor.localPosition = endPosR;
    }
}
