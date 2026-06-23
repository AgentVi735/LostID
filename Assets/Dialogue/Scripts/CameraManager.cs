using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Animator camAnimator;
    [SerializeField] private DialogueManager dialogueManager;

    [SerializeField] private string battleshipAnim;

    private CameraPositions currentPosition;

    private void Awake()
    {
        if (cam != null)
            ChangePosition(CameraPositions.Default);
    }

    public void ChangePosition(CameraPositions newPos)
    {
        if (currentPosition == newPos)
            return;

        switch (newPos)
        {
            default:
            case CameraPositions.Default:
                if (currentPosition == CameraPositions.Battleship)
                {
                    dialogueManager.ShowFrame();
                    camAnimator.SetBool(battleshipAnim, false);
                }
                break;
            case CameraPositions.Battleship:
                camAnimator.SetBool(battleshipAnim, true);
                break;;
        }

        currentPosition = newPos;
    }
}
