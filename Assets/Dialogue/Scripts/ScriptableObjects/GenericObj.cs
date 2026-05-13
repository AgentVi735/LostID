using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Generic Object")]
public class GenericObj : ScriptableObject
{
    public NodeType type = NodeType.Generic;

    public GenericObj nextObj;
}
