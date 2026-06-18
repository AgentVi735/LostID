using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Event")]
public class Event : GenericObj
{
    public EventType eventType;
    public bool disableSaving;
    public bool hideDialogueBox;
    public bool keepText;
    public bool hidePortrait;
    public bool removeItems;
    public float delay;
    public Minigame minigame;
    public GenericObj wonMinigameObj;
    public GenericObj loseMinigameObj;
    public Sprite imageToSend;
}
