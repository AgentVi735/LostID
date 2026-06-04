using NewGraph;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Application = UnityEngine.Application;
using GraphWindow = NewGraph.GraphWindow;

[CustomContextMenu]
public class CustomMenu : NewGraph.ContextMenu
{
    static CustomMenu() => field = typeof(GraphWindow).GetField("window", BindingFlags.NonPublic | BindingFlags.Static);

    private static GraphWindow window;
    private static readonly FieldInfo field;

    private const string fullPath = "Assets/Dialogue/Graphs/";
    private const string shortPath = "Dialogue/Graphs/";

    private const string dialogueFolder = "/Dialogues/";
    private const string responseHolderFolder = "/ResponseHolders/";
    private const string responseFolder = "/Responses/";
    private const string eventFolder = "/Events/";

    private const string defaultGenericNodeName = "GenericNode";
    private const string defaultControllerNodeName = "ControllerNode";
    private const string defaultDialogueNodeName = "DialogueNode";
    private const string defaultResponseHolderNodeName = "ResponseHolderNode";
    private const string defaultResponseNodeName = "ResponseNode";
    private const string defaultEventNodeName = "EventNode";

    private static GraphWindow Window => window ??= (GraphWindow)field!.GetValue(null);

    protected override void AddNodeEntries()
    {
        base.AddNodeEntries();
        AddNodeEntry("Dialogues/Update Objects", (obj) => { UpdateObjects(); });
        AddNodeEntry("Dialogues/Create Graph Directories", (obj) => { CreateGraphDirectories(); });
        AddNodeEntry("Dialogues/Old/Create Objects", (obj) => { CreateObjects(); });
        AddNodeEntry("Dialogues/Old/Refresh SOs", (obj) => { RefreshSO(); });
        AddNodeEntry("Dialogues/Utility/Remove unused SOs", (obj) => { RemoveUnusedSO(); });
        AddNodeEntry("Dialogues/Utility/Update SOs", (obj) => { UpdateSO(); });
    }

    private static void UpdateObjects()
    {
        RemoveAllSO();
        CreateMissingSO();
        RefreshSO();
    }

    private static void CreateObjects()
    {
        CreateMissingSO();
        RefreshSO();
    }

    private static void CreateGraphDirectories()
    {
        ControllerNode controllerNode = null;
        List<NodeModel> nodes = Window.graphController.graphData.Nodes;

        foreach (var node in nodes)
        {
            GenericNode genericNode = (GenericNode)node.nodeData;

            if (genericNode.ReturnType() != NodeType.Controller) continue;
            ControllerNode nodeData = (ControllerNode)node.nodeData;
            controllerNode = nodeData;
            break;
        }


        if (controllerNode == null)
        {
            Debug.LogError("No ControllerNode found");
            return;
        }

        Directory.CreateDirectory(fullPath + controllerNode.graphController.name);
        Directory.CreateDirectory(fullPath + controllerNode.graphController.name + dialogueFolder);
        Directory.CreateDirectory(fullPath + controllerNode.graphController.name + responseHolderFolder);
        Directory.CreateDirectory(fullPath + controllerNode.graphController.name + responseFolder);
        Directory.CreateDirectory(fullPath + controllerNode.graphController.name + eventFolder);

        Debug.Log("Graph directories created at " + fullPath + controllerNode.graphController.name);
    }

    private static void CreateMissingSO()
    {
        ControllerNode controllerNode = null;
        List<NodeModel> nodes = Window.graphController.graphData.Nodes;

        foreach (var node in nodes)
        {
            GenericNode genericNode = (GenericNode)node.nodeData;

            if (genericNode.ReturnType() != NodeType.Controller) continue;
            ControllerNode nodeData = (ControllerNode)node.nodeData;
            controllerNode = nodeData;
            break;
        }


        if (controllerNode == null)
        {
            Debug.LogError("No ControllerNode found");
            return;
        }

        Directory.CreateDirectory(fullPath + controllerNode.graphController.name);
        Directory.CreateDirectory(fullPath + controllerNode.graphController.name + dialogueFolder);
        Directory.CreateDirectory(fullPath + controllerNode.graphController.name + responseHolderFolder);
        Directory.CreateDirectory(fullPath + controllerNode.graphController.name + responseFolder);
        Directory.CreateDirectory(fullPath + controllerNode.graphController.name + eventFolder);

        foreach (var node in nodes)
        {
            GenericNode genericNode = (GenericNode)node.nodeData;

            switch (genericNode.ReturnType())
            {
                case NodeType.Dialogue:
                {
                    DialogueNode dialogueNode = (DialogueNode)node.nodeData;

                    if (dialogueNode.dialogueData.dialogue != null) continue;
                    Dialogue dialogue = CreateInstance<Dialogue>();
                    dialogue.type = NodeType.Dialogue;
                    dialogueNode.dialogueData.dialogue = dialogue;
                    string assetName = node.GetHashCode().ToString();
                    string nodeName = node.GetName();
                    if (nodeName != defaultDialogueNodeName)
                        assetName = nodeName;
                    AssetDatabase.CreateAsset(dialogue, fullPath + controllerNode.graphController.name + dialogueFolder + assetName + ".asset");
                    EditorUtility.SetDirty(dialogue);
                    break;
                }
                case NodeType.ResponseHolder:
                {
                    ResponseHolderNode responseHolderNode = (ResponseHolderNode)node.nodeData;

                    if (responseHolderNode.responseHolder != null) continue;
                    ResponseHolder responseHolder = CreateInstance<ResponseHolder>();
                    responseHolder.type = NodeType.ResponseHolder;
                    responseHolderNode.responseHolder = responseHolder;
                    string assetName = node.GetHashCode().ToString();
                    string nodeName = node.GetName();
                    if (nodeName != defaultResponseHolderNodeName)
                        assetName = nodeName;
                    AssetDatabase.CreateAsset(responseHolder, fullPath + controllerNode.graphController.name + responseHolderFolder + assetName + ".asset");
                    EditorUtility.SetDirty(responseHolder);
                    break;
                }
                case NodeType.Response:
                {
                    ResponseNode responseNode = (ResponseNode)node.nodeData;

                    if (responseNode.response != null) continue;
                    Response response = CreateInstance<Response>();
                    response.type = NodeType.Response;
                    responseNode.response = response;
                    string assetName = node.GetHashCode().ToString();
                    string nodeName = node.GetName();
                    if (nodeName != defaultResponseNodeName)
                        assetName = nodeName;
                    AssetDatabase.CreateAsset(response, fullPath + controllerNode.graphController.name + responseFolder + assetName + ".asset");
                    EditorUtility.SetDirty(response);
                    break;
                }
                case NodeType.Event:
                {
                    EventNode eventNode = (EventNode)node.nodeData;

                    if (eventNode.eventData.eventObj != null) continue;
                    Event eventObj = CreateInstance<Event>();
                    eventObj.type = NodeType.Event;
                    eventNode.eventData.eventObj = eventObj;
                    string assetName = node.GetHashCode().ToString();
                    string nodeName = node.GetName();
                    if (nodeName != defaultEventNodeName)
                        assetName = nodeName;
                    AssetDatabase.CreateAsset(eventObj, fullPath + controllerNode.graphController.name + eventFolder + assetName + ".asset");
                    EditorUtility.SetDirty(eventObj);
                    break;
                }
            }
        }
    }

    private static void RemoveAllSO()
    {
        RefreshNames();

        ControllerNode controllerNode = null;
        GraphController graphController = null;

        List<NodeModel> nodes = Window.graphController.graphData.Nodes;

        foreach (var node in nodes)
        {
            GenericNode genericNode = (GenericNode)node.nodeData;

            if (genericNode.ReturnType() != NodeType.Controller) continue;
            ControllerNode nodeData = (ControllerNode)node.nodeData;
            graphController = nodeData.graphController;
            controllerNode = nodeData;
            break;
        }

        if (graphController == null)
        {
            Debug.LogError("No GraphController found");
            return;
        }

        string[] nodeNames = new string[nodes.Count];

        if (nodeNames.Length == 0)
            return;

        for (int i = 0; i < nodeNames.Length; i++)
        {
            NodeModel node = nodes[i];

            string nodeName = string.Empty;

            GenericNode nodeData = (GenericNode)node.nodeData;
            switch (nodeData.ReturnType())
            {
                case NodeType.Dialogue:
                    DialogueNode dialogueNode = (DialogueNode)node.nodeData;
                    nodeName = dialogueNode.dialogueData.dialogue == null ? string.Empty : dialogueNode.dialogueData.dialogue.name;
                    break;
                case NodeType.ResponseHolder:
                    ResponseHolderNode responseHolderNode = (ResponseHolderNode)node.nodeData;
                    nodeName = responseHolderNode.responseHolder == null ? string.Empty : responseHolderNode.responseHolder.name;
                    break;
                case NodeType.Response:
                    ResponseNode responseNode = (ResponseNode)node.nodeData;
                    nodeName = responseNode.response == null ? string.Empty : responseNode.response.name;
                    break;
                case NodeType.Event:
                    EventNode eventNode = (EventNode)node.nodeData;
                    nodeName = eventNode.eventData.eventObj == null ? string.Empty : eventNode.eventData.eventObj.name;
                    break;
            }

            nodeNames[i] = nodeName;
        }

        if (controllerNode == null)
        {
            Debug.LogError("No ControllerNode found");
            return;
        }

        string[] dialogueFiles = Directory.GetFiles(Application.dataPath + "/" + shortPath + controllerNode.graphController.name + dialogueFolder);
        string[] responseHolderFiles = Directory.GetFiles(Application.dataPath + "/" + shortPath + controllerNode.graphController.name + responseHolderFolder);
        string[] responseFiles = Directory.GetFiles(Application.dataPath + "/" + shortPath + controllerNode.graphController.name + responseFolder);
        string[] eventFiles = Directory.GetFiles(Application.dataPath + "/" + shortPath + controllerNode.graphController.name + eventFolder);

        foreach (string file in dialogueFiles) 
            AssetDatabase.DeleteAsset(fullPath + controllerNode.graphController.name + "/" + dialogueFolder + GetFileName(file) + ".asset");
        foreach (string file in responseHolderFiles)
            AssetDatabase.DeleteAsset(fullPath + controllerNode.graphController.name + "/" + responseHolderFolder + GetFileName(file) + ".asset");
        foreach (string file in responseFiles)
            AssetDatabase.DeleteAsset(fullPath + controllerNode.graphController.name + "/" + responseFolder + GetFileName(file) + ".asset");
        foreach (string file in eventFiles)
            AssetDatabase.DeleteAsset(fullPath + controllerNode.graphController.name + "/" + eventFolder + GetFileName(file) + ".asset");
    }

    private static void RefreshSO()
    {
        ControllerNode controllerNode = null;
        GraphController graphController = null;

        List<NodeModel> nodes = Window.graphController.graphData.Nodes;

        foreach (var node in nodes)
        {
            GenericNode genericNode = (GenericNode)node.nodeData;

            if (genericNode.ReturnType() != NodeType.Controller) continue;
            ControllerNode nodeData = (ControllerNode)node.nodeData;
            graphController = nodeData.graphController;
            controllerNode = nodeData;
            graphController.character = controllerNode.character;
            graphController.character.graph = graphController.graph;
            break;
        }

        if (graphController == null)
        {
            Debug.LogError("No GraphController found");
            return;
        }

        graphController.dialogueObjs = new GenericObj[nodes.Count];

        for (int i = 0; i < nodes.Count; i++)
        {
            NodeModel node = nodes[i];

            GenericNode nodeData = (GenericNode)node.nodeData;
            switch (nodeData.ReturnType())
            {
                case NodeType.Dialogue:
                    Dialogue dialogue = LoadDialogue(nodeData);
                    graphController.dialogueObjs[i] = dialogue;
                    EditorUtility.SetDirty(dialogue);
                    break;
                case NodeType.ResponseHolder:
                    ResponseHolder responseHolder = LoadResponseHolder(nodeData, graphController.name);
                    graphController.dialogueObjs[i] = responseHolder;
                    EditorUtility.SetDirty(responseHolder);
                    break;
                case NodeType.Response:
                    ResponseNode responseNode = (ResponseNode)nodeData;
                    Response response = responseNode.response;
                    graphController.dialogueObjs[i] = response;
                    break;
                case NodeType.Event:
                    Event eventObj = LoadEvent(nodeData);
                    graphController.dialogueObjs[i] = eventObj;
                    EditorUtility.SetDirty(eventObj);
                    break;
            }
        }

        if (controllerNode?.startingNode != null)
        {
            NodeType nodeType = controllerNode.startingNode.ReturnType();

            switch (nodeType)
            {
                case NodeType.Dialogue:
                {
                    DialogueNode nodeData = (DialogueNode)controllerNode.startingNode;
                    controllerNode.graphController.startingObj = nodeData.dialogueData.dialogue;
                    break;
                }
                case NodeType.ResponseHolder:
                {
                    ResponseHolderNode nodeData = (ResponseHolderNode)controllerNode.startingNode;
                    controllerNode.graphController.startingObj = nodeData.responseHolder;
                    break;
                }
                case NodeType.Event:
                {
                    EventNode nodeData = (EventNode)controllerNode.startingNode;
                    controllerNode.graphController.startingObj = nodeData.eventData.eventObj;
                    break;
                }
            }
        }

        EditorUtility.SetDirty(graphController);

        Debug.Log("Saved nodes");
    }

    private static Dialogue LoadDialogue(GenericNode nodeData)
    {
        DialogueNode dialogueNode = (DialogueNode)nodeData;
        DialogueNode.DialogueData dialogueData = dialogueNode.dialogueData;
        Dialogue dialogue = dialogueData.dialogue;

        if (dialogueNode.nextNode != null)
        {
            switch (dialogueNode.nextNode.ReturnType())
            {
                case NodeType.Dialogue:
                    dialogue.nextObj = ((DialogueNode)dialogueNode.nextNode).dialogueData.dialogue;
                    break;
                case NodeType.ResponseHolder:
                    dialogue.nextObj = ((ResponseHolderNode)dialogueNode.nextNode).responseHolder;
                    break;
                case NodeType.Event:
                    dialogue.nextObj = ((EventNode)dialogueNode.nextNode).eventData.eventObj;
                    break;
            }
        }

        dialogue.character = dialogueData.character;
        dialogue.overrideCharacterName = dialogueData.overrideCharacterName;
        dialogue.text = dialogueData.text;
        dialogue.sprite = dialogueData.sprite;
        dialogue.overrideSprite = dialogueData.overrideSprite;

        return dialogue;
    }

    private static ResponseHolder LoadResponseHolder(GenericNode nodeData, string graphName)
    {
        ResponseHolderNode responseHolderNode = (ResponseHolderNode)nodeData;
        ResponseHolder responseHolder = responseHolderNode.responseHolder;
        List<ResponseNode> responses = responseHolderNode.responses;

        if (responseHolder.responses == null || responseHolder.responses.Length <= responses.Count)
        {
            foreach (ResponseNode responseNode in responses)
            {
                if (responseNode.response) 
                    AssetDatabase.DeleteAsset(fullPath + graphName + responseFolder + responseNode.nodeName + ".asset");
            }

            responseHolder.responses = new Response[responses.Count];
        }

        for (int i = 0; i < responses.Count; i++)
        {
            ResponseNode responseNode = responses[i];
            Response response;

            if (responseHolder.responses[i] == null)
            {
                response = CreateInstance<Response>();
                response.type = NodeType.Response;
                responseNode.response = response;
                AssetDatabase.CreateAsset(response, fullPath + graphName + responseFolder + responseNode.nodeName + ".asset");
                responseHolder.responses[i] = response;
                EditorUtility.SetDirty(response);
            }

            response = responseHolder.responses[i];

            if (responseNode.nextNode != null)
            {
                switch (responseNode.nextNode.ReturnType())
                {
                    case NodeType.Dialogue:
                        response.nextObj = ((DialogueNode)responseNode.nextNode).dialogueData.dialogue;
                        break;
                    case NodeType.ResponseHolder:
                        response.nextObj = ((ResponseHolderNode)responseNode.nextNode).responseHolder;
                        break;
                    case NodeType.Event:
                        response.nextObj = ((EventNode)responseNode.nextNode).eventData.eventObj;
                        break;
                }
            }

            response.text = responseNode.text;
        }

        return responseHolder;
    }

    private static Event LoadEvent(GenericNode nodeData)
    {
        EventNode eventNode = (EventNode)nodeData;
        EventNode.EventData eventData = eventNode.eventData;
        Event eventObj = eventData.eventObj;

        if (eventNode.nextNode != null)
        {
            switch (eventNode.nextNode.ReturnType())
            {
                case NodeType.Dialogue:
                    eventObj.nextObj = ((DialogueNode)eventNode.nextNode).dialogueData.dialogue;
                    break;
                case NodeType.ResponseHolder:
                    eventObj.nextObj = ((ResponseHolderNode)eventNode.nextNode).responseHolder;
                    break;
                case NodeType.Event:
                    eventObj.nextObj = ((EventNode)eventNode.nextNode).eventData.eventObj;
                    break;
            }
        }

        eventObj.eventType = eventData.eventType;
        eventObj.hideDialogueBox = eventData.hideDialogueBox;
        eventObj.keepText = eventData.keepText;
        eventObj.hidePortrait = eventData.hidePortrait;
        eventObj.delay = eventData.delay;
        eventObj.minigame = eventData.minigame;

        if (eventData.wonMinigameNode != null)
        {
            switch (eventData.wonMinigameNode.ReturnType())
            {
                case NodeType.Dialogue:
                    eventObj.wonMinigameObj = ((DialogueNode)eventData.wonMinigameNode).dialogueData.dialogue;
                    break;
                case NodeType.ResponseHolder:
                    eventObj.wonMinigameObj = ((ResponseHolderNode)eventData.wonMinigameNode).responseHolder;
                    break;
                case NodeType.Event:
                    eventObj.wonMinigameObj = ((EventNode)eventData.wonMinigameNode).eventData.eventObj;
                    break;
            }
        }

        if (eventData.loseMinigameNode != null)
        {
            switch (eventData.loseMinigameNode.ReturnType())
            {
                case NodeType.Dialogue:
                    eventObj.loseMinigameObj = ((DialogueNode)eventData.loseMinigameNode).dialogueData.dialogue;
                    break;
                case NodeType.ResponseHolder:
                    eventObj.loseMinigameObj = ((ResponseHolderNode)eventData.loseMinigameNode).responseHolder;
                    break;
                case NodeType.Event:
                    eventObj.loseMinigameObj = ((EventNode)eventData.loseMinigameNode).eventData.eventObj;
                    break;
            }
        }

        return eventObj;
    }

    private static bool RefreshNames()
    {
        GraphController graphController = null;

        List<NodeModel> nodes = Window.graphController.graphData.Nodes;

        foreach (NodeModel node in nodes)
        {
            GenericNode genericNode = (GenericNode)node.nodeData;

            if (genericNode.ReturnType() != NodeType.Controller) continue;
            ControllerNode nodeData = (ControllerNode)node.nodeData;
            graphController = nodeData.graphController;
            string name = node.GetName();
            if (name == defaultEventNodeName)
                name = node.GetHashCode().ToString();
            nodeData.nodeName = name;
            break;
        }

        if (graphController == null)
        {
            Debug.LogError("No GraphController found");
            return false;
        }

        graphController.dialogueObjs = new GenericObj[nodes.Count];

        List<string> names = new();

        foreach (NodeModel node in nodes)
        {
            GenericNode nodeData = (GenericNode)node.nodeData;
            string name = node.GetName();
            switch (nodeData.ReturnType())
            {
                case NodeType.Dialogue:
                    DialogueNode dialogueNode = (DialogueNode)nodeData;

                    if (name == defaultDialogueNodeName)
                        name = node.GetHashCode().ToString();
                    if (names.Contains(name))
                    {
                        Debug.LogError("Duplicate node name found: " + name);
                        return false;
                    }

                    dialogueNode.nodeName = name;
                    break;
                case NodeType.ResponseHolder:
                    ResponseHolderNode responseHolderNode = (ResponseHolderNode)nodeData;

                    if (name == defaultResponseHolderNodeName)
                        name = node.GetHashCode().ToString();
                    if (names.Contains(name))
                    {
                        Debug.LogError("Duplicate node name found: " + name);
                        return false;
                    }

                    responseHolderNode.nodeName = name;
                    break;
                case NodeType.Response:
                    ResponseNode responseNode = (ResponseNode)nodeData;

                    if (name == defaultResponseNodeName)
                        name = node.GetHashCode().ToString();
                    if (names.Contains(name))
                    {
                        Debug.LogError("Duplicate node name found: " + name);
                        return false;
                    }

                    responseNode.nodeName = name;
                    break;
                case NodeType.Generic:
                    if (name == defaultGenericNodeName)
                        name = node.GetHashCode().ToString();
                    if (names.Contains(name))
                    {
                        Debug.LogError("Duplicate node name found: " + name);
                        return false;
                    }

                    nodeData.nodeName = name;
                    break;
                case NodeType.Event:
                    EventNode eventNode = (EventNode)nodeData;
                    if (name == defaultEventNodeName)
                        name = node.GetHashCode().ToString();
                    if (names.Contains(name))
                    {
                        Debug.LogError("Duplicate node name found: " + name);
                        return false;
                    }

                    eventNode.nodeName = name;
                    break;
            }

            names.Add(name);
        }

        return true;
    }

    private static void RemoveUnusedSO()
    {
        bool hasValidNames = RefreshNames();
        if (!hasValidNames)
        {
            Debug.LogError("Could not remove objects due to invalid node names");
            return;
        }

        ControllerNode controllerNode = null;
        GraphController graphController = null;

        List<NodeModel> nodes = Window.graphController.graphData.Nodes;

        foreach (var node in nodes)
        {
            GenericNode genericNode = (GenericNode)node.nodeData;

            if (genericNode.ReturnType() != NodeType.Controller) continue;
            ControllerNode nodeData = (ControllerNode)node.nodeData;
            graphController = nodeData.graphController;
            controllerNode = nodeData;
            break;
        }

        if (graphController == null)
        {
            Debug.LogError("No GraphController found");
            return;
        }

        string[] nodeNames = new string[nodes.Count];

        for (int i = 0; i < nodeNames.Length; i++)
        {
            NodeModel node = nodes[i];

            string nodeName = string.Empty;

            GenericNode nodeData = (GenericNode)node.nodeData;
            switch (nodeData.ReturnType())
            {
                case NodeType.Dialogue:
                    DialogueNode dialogueNode = (DialogueNode)node.nodeData;
                    nodeName = dialogueNode.dialogueData.dialogue == null ? string.Empty : dialogueNode.dialogueData.dialogue.name;
                    break;
                case NodeType.ResponseHolder:
                    ResponseHolderNode responseHolderNode = (ResponseHolderNode)node.nodeData;
                    nodeName = responseHolderNode.responseHolder == null ? string.Empty : responseHolderNode.responseHolder.name;
                    break;
                case NodeType.Response:
                    ResponseNode responseNode = (ResponseNode)node.nodeData;
                    nodeName = responseNode.response == null ? string.Empty : responseNode.response.name;
                    break;
                case NodeType.Event:
                    EventNode eventNode = (EventNode)node.nodeData;
                    nodeName = eventNode.eventData.eventObj == null ? string.Empty : eventNode.eventData.eventObj.name;
                    break;
            }

            nodeNames[i] = nodeName;
        }

        if (controllerNode == null)
        {
            Debug.LogError("No ControllerNode found");
            return;
        }

        string[] dialogueFiles = Directory.GetFiles(Application.dataPath + "/" + shortPath + controllerNode.graphController.name + dialogueFolder);
        string[] responseHolderFiles = Directory.GetFiles(Application.dataPath + "/" + shortPath + controllerNode.graphController.name + responseHolderFolder);
        string[] responseFiles = Directory.GetFiles(Application.dataPath + "/" + shortPath + controllerNode.graphController.name + responseFolder);
        string[] eventFiles = Directory.GetFiles(Application.dataPath + "/" + shortPath + controllerNode.graphController.name + eventFolder);

        RemoveFiles(dialogueFiles, nodeNames, controllerNode.graphController.name + "/" + dialogueFolder);
        RemoveFiles(responseHolderFiles, nodeNames, controllerNode.graphController.name + "/" + responseHolderFolder);
        RemoveFiles(responseFiles, nodeNames, controllerNode.graphController.name + "/" + responseFolder);
        RemoveFiles(eventFiles, nodeNames, controllerNode.graphController.name + "/" + eventFolder);

        Debug.Log("Successfully removed all unused objects");
    }

    private static string GetFileName(string fileName)
    {
        if (fileName.EndsWith(".meta")) return string.Empty;

        string[] parsedName = fileName.Split("/");
        fileName = parsedName[^1];
        parsedName = fileName.Split(".");
        return parsedName[0];
    }

    private static void RemoveFiles(string[] files, string[] nodeNames, string assetPath)
    {
        foreach (string file in files)
        {
            var fileName = GetFileName(file);

            bool foundName = false;
            foreach (string nodeName in nodeNames)
            {
                if (fileName != nodeName) continue;
                foundName = true;
                break;
            }

            if (foundName) continue;
            AssetDatabase.DeleteAsset(fullPath + assetPath + fileName + ".asset");
        }
    }

    private static void UpdateSO()
    {
        GraphController graphController = null;

        List<NodeModel> nodes = Window.graphController.graphData.Nodes;

        List<NodeModel> dialogueNodes = new();
        List<NodeModel> responseHolderNodes = new();
        List<NodeModel> responseNodes = new();
        List<NodeModel> eventNodes = new();

        foreach (NodeModel node in nodes)
        {
            GenericNode genericNode = (GenericNode)node.nodeData;

            switch (genericNode.ReturnType())
            {
                case NodeType.Generic:
                    break;
                case NodeType.Dialogue:
                    dialogueNodes.Add(node);
                    break;
                case NodeType.ResponseHolder:
                    responseHolderNodes.Add(node);
                    break;
                case NodeType.Response:
                    responseNodes.Add(node);
                    break;
                case NodeType.Event:
                    eventNodes.Add(node);
                    break;
                case NodeType.Controller:
                    ControllerNode controllerNode = (ControllerNode)node.nodeData;
                    graphController = controllerNode.graphController;
                    controllerNode.character = graphController.character;
                    break;
                default:
                    Debug.LogError("Node found with no type.");
                    break;
            }
        }

        if (graphController == null)
        {
            Debug.LogError("No GraphController found");
            return;
        }

        foreach (NodeModel node in dialogueNodes)
        {
            DialogueNode nodeData = (DialogueNode)node.nodeData;
            DialogueNode.DialogueData dialogueData = nodeData.dialogueData;
            Dialogue dialogue = dialogueData.dialogue;

            dialogueData.character = dialogue.character;
            dialogueData.overrideCharacterName = dialogue.overrideCharacterName;
            dialogueData.text = dialogue.text;
            dialogueData.sprite = dialogue.sprite;
        }

        foreach (NodeModel node in responseNodes)
        {
            ResponseNode responseNode = (ResponseNode)node.nodeData;
            Response response = responseNode.response;

            responseNode.text = response.text;
        }

        foreach (NodeModel node in eventNodes)
        {
            EventNode nodeData = (EventNode)node.nodeData;
            EventNode.EventData eventData = nodeData.eventData;
            Event eventObj = eventData.eventObj;

            eventData.eventType = eventObj.eventType;
            eventData.hideDialogueBox = eventObj.hideDialogueBox;
            eventData.keepText = eventObj.keepText;
            eventData.hidePortrait = eventObj.hidePortrait;
            eventData.delay = eventObj.delay;
            eventData.minigame = eventObj.minigame;
        }

        Debug.Log("Saved nodes");
    }
}