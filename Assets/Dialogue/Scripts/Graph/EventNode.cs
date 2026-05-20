using NewGraph;
using System;
using UnityEngine;

[Serializable, Node("#d28533", "Dialogue")]
public class EventNode : GenericNode
{
    public override NodeType ReturnType() => NodeType.Event;
}