using System.Collections;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private SkinnedMeshRenderer meshR;
    private CharacterParent characterHolder;

    [Header("Character")]
    [SerializeField] private Character characterInfo;

    [Header("Animations")]
    [SerializeField] private string walkAnimation;
    [SerializeField] private string sitAnimation;

    private bool finishedAnimation;

    public void Setup(Transform givenHolder)
    {
        characterHolder = givenHolder.GetComponent<CharacterParent>();
        characterHolder.Setup(this);
    }

    public void ChangeMaterial(CharacterSprite sprite)
    {
        Material charMat = sprite switch
        {
            CharacterSprite.None => null,
            CharacterSprite.Neutral => characterInfo.neutralMat,
            CharacterSprite.Happy => characterInfo.happyMat,
            CharacterSprite.Angry => characterInfo.angryMat,
            CharacterSprite.Sad => characterInfo.sadMat,
            _ => characterInfo.neutralMat
        };

        if (charMat == null) return;
        meshR.material = charMat;
    }

    public void StartAnimation(CharacterAnimations sentAnimation, bool toggle)
    {
        switch (sentAnimation)
        {
            default:
            case CharacterAnimations.None:
                Debug.LogWarning("No animation sent");
                break;
            case CharacterAnimations.Walk:
                animator.SetBool(walkAnimation, toggle);
                break;
            case CharacterAnimations.Sit:
                animator.SetBool(sitAnimation, toggle);
                break;
        }
    }

    public IEnumerator StartWalkToSeat()
    {
        StartAnimation(CharacterAnimations.Walk, true);
        characterHolder.WalkToChair();
        while (!finishedAnimation)
            yield return null;
        StartAnimation(CharacterAnimations.Sit, true);
    }

    public void FinishedAnimation() => finishedAnimation = true;
}
