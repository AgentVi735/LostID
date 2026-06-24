using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Character")]
public class Character : ScriptableObject
{
    [Header("Info")]
    public string characterName;
    public GraphController graph;
    public GraphController phoneGraph;
    public string nfcID;

    [Header("Sprites")]
    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite angrySprite;
    public Sprite sadSprite;

    [Header("Model")]
    public GameObject prefab;
    public Material neutralMat;
    public Material happyMat;
    public Material sadMat;
    public Material angryMat;

    [Header("Wallet")]
    public Sprite smallWallet;
    public Sprite underpartRLayer;
    public Sprite underpartLLayer;
    public Sprite pocketLowRLayer;
    public Sprite idCard;
    public Sprite[] items;
    public Sprite phoneNumber;
    public bool hasCharm;

    [Header("Other Info")]
    public MenuItems dessert;
    public MenuItems drink;
    public float overrideTextWidth;
}
