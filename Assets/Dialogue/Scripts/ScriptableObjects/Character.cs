using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Character")]
public class Character : ScriptableObject
{
    public string characterName;
    public Sprite neutralSprite;
    public Sprite happySprite;
    public Sprite angrySprite;
    public Sprite sadSprite;
}
