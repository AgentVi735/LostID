using NewGraph;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public ScriptableGraphModel graph;
    public string nfcID;

    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite angrySprite;
    public Sprite sadSprite;

    public Sprite underpartRLayer;
    public Sprite underpartLLayer;
    public Sprite pocketLowRLayer;
    public Sprite idCard;
    public Sprite[] items;
    public Sprite phoneNumber;
    public bool hasCharm;
}
