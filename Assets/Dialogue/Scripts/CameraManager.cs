using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private Transform defaultCamPos;
    [SerializeField] private Transform battleshipCamPos;

    private CameraPositions currentPosition;

    private void Awake()
    {
        ChangePosition(CameraPositions.Default);
    }

    public void ChangePosition(CameraPositions newPos)
    {
        if (currentPosition == newPos)
            return;

        currentPosition = newPos;
        switch (currentPosition)
        {
            case CameraPositions.Default:
                cam.transform.parent = defaultCamPos;
                break;
            case CameraPositions.Battleship:
                cam.transform.parent = battleshipCamPos;
                break;
        }

        cam.transform.localPosition = Vector3.zero;
        cam.transform.localEulerAngles = Vector3.zero;
    }
}
