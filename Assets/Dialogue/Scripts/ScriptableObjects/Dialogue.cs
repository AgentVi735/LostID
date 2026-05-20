using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue")]
public class Dialogue : GenericObj
{
    public Character character;
    public string overrideCharacterName;
    public string text;
    public CharacterSprite sprite;
    public Sprite overrideSprite;
}
