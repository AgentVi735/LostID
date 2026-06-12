using System;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [Header("Boat Settings")]
    [SerializeField] private int boatHoles;
    [SerializeField] private Vector3 defaultPos;
    [SerializeField] private Vector3 defaultRot;
    [SerializeField] private Material baseMaterial;
    [SerializeField] private Material transparentMaterial;
    private BoatRotation rotation;
    private PinHole[] holes;
    private bool isSet;

    [Header("References")]
    [SerializeField] private MeshRenderer meshR;
    [SerializeField] private BoxCollider boxCollider;

    public int GetHoles() => boatHoles;

    public bool IsInHole() => isSet;

    public void SetCollider(bool toggle) => boxCollider.enabled = toggle;

    public void Setup(BoatRotation givenRot, PinHole[] givenHoles)
    {
        rotation = givenRot;
        holes = givenHoles;
        isSet = true;
    }

    public void RemoveFromHole()
    {
        foreach (PinHole hole in holes) 
            hole.RemoveBoat();

        isSet = false;
    }

    public BoatRotation ToggleSelection(bool toggle)
    {
        meshR.material = toggle ? transparentMaterial : baseMaterial;
        return rotation;
    }

    public void PutBack()
    {
        transform.SetLocalPositionAndRotation(defaultPos, Quaternion.Euler(defaultRot));
        boxCollider.enabled = true;
    }
}
