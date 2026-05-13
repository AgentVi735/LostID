using NewGraph;
using System;
using UnityEngine;

[Serializable, Node("#138160", "Dialogue", createInputPort = false)]
public class ControllerNode : GenericNode
{
    public override NodeType ReturnType()
    {
        return NodeType.Controller;
    }

    [Port, SerializeReference]
    public GenericNode startingNode;

    public GraphController graphController;
}