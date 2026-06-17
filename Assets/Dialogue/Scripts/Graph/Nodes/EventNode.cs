using NewGraph;
using System;
using UnityEngine;

[Serializable, Node("#d28533", "Dialogue")]
public class EventNode : GenericNode
{
    public override NodeType ReturnType() => NodeType.Event;

    [Port, SerializeReference]
    public GenericNode nextNode;

    [Serializable]
    public class EventData
    {
        public Event eventObj;

        [GraphDisplay(DisplayType.BothViews)]
        public EventType eventType;

        [GraphDisplay(DisplayType.Inspector)]
        public bool hideDialogueBox;

        [GraphDisplay(DisplayType.Inspector)]
        public bool keepText;

        [GraphDisplay(DisplayType.Inspector)]
        public bool hidePortrait;

        [GraphDisplay(DisplayType.Inspector)]
        public bool removeItems;

        [GraphDisplay(DisplayType.Inspector)]
        public float delay;

        [GraphDisplay(DisplayType.Inspector)]
        public Minigame minigame;

        [Port, SerializeReference] 
        public GenericNode wonMinigameNode;

        [Port, SerializeReference]
        public GenericNode loseMinigameNode;

        [GraphDisplay(DisplayType.Inspector)]
        public Sprite imageToSend;
    }
    public EventData eventData;
}