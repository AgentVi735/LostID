using NewGraph;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/GraphController")]
public class GraphController : ScriptableObject
{
    public ScriptableGraphModel graph;
    public Character character;
    public GenericObj startingObj;
    public GenericObj[] dialogueObjs;
}
