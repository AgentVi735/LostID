using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private Transform defaultCamPos;
    [SerializeField] private Transform battleshipCamPos;

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

        currentPosition = newPos;
        cam.transform.parent = currentPosition switch
        {
            CameraPositions.Default => defaultCamPos,
            CameraPositions.Battleship => battleshipCamPos,
            _ => cam.transform.parent
        };

        cam.transform.localPosition = Vector3.zero;
        cam.transform.localEulerAngles = Vector3.zero;
    }
}
