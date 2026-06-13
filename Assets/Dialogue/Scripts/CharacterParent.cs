using UnityEngine;

public class CharacterParent : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private CharacterManager character;

    private static readonly int ToChair = Animator.StringToHash("WalkToChair");

    public void Setup(CharacterManager givenCharacter) => character = givenCharacter;

    public void WalkToChair()
    {
        animator.SetTrigger(ToChair);
    }

    public void SitDown()
    {
        character.FinishedAnimation();
    }
}
