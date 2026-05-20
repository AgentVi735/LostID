using NewGraph;
using System;
using UnityEngine;

[Serializable, Node("#df3636", "Dialogue")]
public class ResponseNode : GenericNode
{
    public override NodeType ReturnType() => NodeType.Response;

    public Response response;

    [Port, SerializeReference]
    public GenericNode nextNode;

    [GraphDisplay(DisplayType.BothViews)]
    public string text;
}