using NewGraph;
using System;

[Serializable, Node("#007F00FF", "Dialogue")]
public class GenericNode : INode
{
    public virtual NodeType ReturnType() => NodeType.Generic;

    public string nodeName;
}