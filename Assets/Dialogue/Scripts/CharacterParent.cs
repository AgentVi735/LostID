using UnityEngine;

public class CharacterParent : MonoBehaviour
{
    private static readonly int ToChair = Animator.StringToHash("WalkToChair");
    private static readonly int LeaveFromChair = Animator.StringToHash("LeaveFromChair");
    [SerializeField] private Animator animator;

    private CharacterManager character;

    public void Setup(CharacterManager givenCharacter) => character = givenCharacter;

    public void SetAnimation(CharacterAnimations sentAnimation, bool toggle) => character.SetAnimation(sentAnimation, toggle);

    public void WalkToChair() => animator.SetTrigger(ToChair);

    public void SitDown() => character.FinishedAnimation();

    public void LeaveChair() => animator.SetTrigger(LeaveFromChair);

    public void StandUp() => character.SetAnimation(CharacterAnimations.Sit, false);

    public void StartWalking() => character.SetAnimation(CharacterAnimations.Walk, true);

    public void StopWalking() => character.SetAnimation(CharacterAnimations.Walk, false);

    public void FinishLeaving() => character.FinishedAnimation();
}
