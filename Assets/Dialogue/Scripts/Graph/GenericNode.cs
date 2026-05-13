using NewGraph;
using System;
using UnityEngine;

[Serializable, Node("#007F00FF", "Dialogue")]
public class GenericNode : INode
{
    public virtual NodeType ReturnType()
    {
        return NodeType.Generic;
    }
}