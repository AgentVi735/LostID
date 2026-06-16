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
    [SerializeField] private string happyAnimation;
    [SerializeField] private string sadAnimation;
    [SerializeField] private string battleshipInteractionAnimation;
    [SerializeField] private string holdCardsAnimation;
    [SerializeField] private string layCardDownAnimation;

    private bool finishedAnimation;

    public void Setup(Transform givenHolder)
    {
        characterHolder = givenHolder.GetComponent<CharacterParent>();
        characterHolder.Setup(this);
        animator.SetLayerWeight(1, 1);
        animator.SetLayerWeight(2, 1);
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

    public void SetAnimation(CharacterAnimations sentAnimation, bool toggle)
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
            case CharacterAnimations.Happy:
                animator.SetBool(happyAnimation, toggle);
                break;
            case CharacterAnimations.Sad:
                animator.SetBool(sadAnimation, toggle);
                break;
            case CharacterAnimations.BattleshipInteraction:
                animator.SetTrigger(battleshipInteractionAnimation);
                break;
            case CharacterAnimations.HoldCards:
                animator.SetBool(holdCardsAnimation, toggle);
                break;
            case CharacterAnimations.LayCardDown:
                animator.SetTrigger(layCardDownAnimation);
                break;
        }
    }

    public IEnumerator StartWalkToSeat()
    {
        finishedAnimation = false;
        SetAnimation(CharacterAnimations.Walk, true);
        characterHolder.WalkToChair();
        while (!finishedAnimation)
            yield return null;
        SetAnimation(CharacterAnimations.Sit, true);
    }

    public IEnumerator LeaveSeat()
    {
        finishedAnimation = false;
        characterHolder.LeaveChair();
        while (!finishedAnimation)
            yield return null;
    }

    public void FinishedAnimation() => finishedAnimation = true;
}
