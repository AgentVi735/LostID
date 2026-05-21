using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite angrySprite;
    public Sprite sadSprite;

    public Sprite underpartRLayer;
    public Sprite underpartRLayerFade;
    public Sprite underpartLLayer;
    public Sprite underpartLLayerFade;
    public Sprite pocketLowRLayer;
    public Sprite pocketLowRLayerFade;
    public Sprite idCard;
    public Sprite[] items;
    public Sprite[] itemsFade;
    public Sprite phoneNumber;
    public bool hasCharm;
}
