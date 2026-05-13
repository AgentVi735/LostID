using NewGraph;
using System;
using UnityEngine;

[Serializable, Node("#8d51c5", "Dialogue")]
public class DialogueNode : GenericNode
{
    public override NodeType ReturnType()
    {
        return NodeType.Dialogue;
    }

    [Port, SerializeReference]
    public GenericNode nextNode;

    [Serializable]
    public class DialogueData
    {
        public Dialogue dialogue;

        [GraphDisplay(DisplayType.BothViews)]
        public Character character;

        [GraphDisplay(DisplayType.Inspector)]
        public string overrideCharacterName;

        [GraphDisplay(DisplayType.BothViews)]
        public string text;
    }
    public DialogueData dialogueData;
}