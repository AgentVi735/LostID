using NewGraph;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable, Node("#df3636", "Dialogue")]
public class ResponseHolderNode : GenericNode
{
    public override NodeType ReturnType() => NodeType.ResponseHolder;

    public ResponseHolder responseHolder;

    [PortList, SerializeReference]
    public List<ResponseNode> responses;
}