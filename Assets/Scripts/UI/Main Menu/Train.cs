using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Train : MonoBehaviour
{
    [Header("Carts")]
    [SerializeField] private TrainMiddle[] middleCarts;

    [Header("Animator")]
    [SerializeField] private string animationString;
    [SerializeField] private int animationCount;
    [SerializeField] private Animator animator;

    private void Awake() => animator.SetInteger(animationString, -1);

    public void PlayAnimation() => animator.SetInteger(animationString, Random.Range(0, animationCount));

    public void OpenDoors() => StartCoroutine(ToggleDoorsCoroutine(true));

    public void CloseDoors() => StartCoroutine(ToggleDoorsCoroutine(false));

    private IEnumerator ToggleDoorsCoroutine(bool toggle)
    {
        for (int i = 0; i < middleCarts.Length; i++)
            if (i == middleCarts.Length - 1)
                yield return StartCoroutine(middleCarts[i].ToggleDoors(toggle));
            else
                StartCoroutine(middleCarts[i].ToggleDoors(toggle));
    }

    public void FinishAnimation() => animator.SetInteger(animationString, -1);
}
