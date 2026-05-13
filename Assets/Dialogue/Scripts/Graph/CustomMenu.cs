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
    static CustomMenu()
    {
        field = typeof(NewGraph.GraphWindow).GetField("window", BindingFlags.NonPublic | BindingFlags.Static);
    }

    private static GraphWindow window;
    private static readonly FieldInfo field;

    private static GraphWindow Window => window ??= (GraphWindow)field!.GetValue(null);

    protected override void AddNodeEntries()
    {
        base.AddNodeEntries();
        AddNodeEntry("Dialogues/Refresh SOs", (obj) => { RefreshSO(); });
        AddNodeEntry("Dialogues/Create SOs", (obj) => { CreateMissingSO(); });
        AddNodeEntry("Dialogues/Utility/Remove unused SOs", (obj) => { RemoveUnusedSO(); });
        AddNodeEntry("Dialogues/Utility/Update SOs", (obj) => { UpdateSO(); });
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

        Directory.CreateDirectory("Assets/Dialogue/ScriptableObjects/" + controllerNode.graphController.name);
        Directory.CreateDirectory("Assets/Dialogue/ScriptableObjects/" + controllerNode.graphController.name + "/Dialogues/");
        Directory.CreateDirectory("Assets/Dialogue/ScriptableObjects/" + controllerNode.graphController.name + "/ResponseHolders/");
        Directory.CreateDirectory("Assets/Dialogue/ScriptableObjects/" + controllerNode.graphController.name + "/Responses/");

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
                    AssetDatabase.CreateAsset(dialogue, "Assets/Dialogue/ScriptableObjects/" + controllerNode.graphController.name + "/Dialogues/" + node.GetHashCode() + ".asset");
                    EditorUtility.SetDirty(dialogue);
                    break;
                }
                case NodeType.ResponseHolder:
                    ResponseHolderNode responseHolderNode = (ResponseHolderNode)node.nodeData;

                    if (responseHolderNode.responseHolder != null) continue;
                    ResponseHolder responseHolder = CreateInstance<ResponseHolder>();
                    responseHolder.type = NodeType.ResponseHolder;
                    responseHolderNode.responseHolder = responseHolder;
                    AssetDatabase.CreateAsset(responseHolder, "Assets/Dialogue/ScriptableObjects/" + controllerNode.graphController.name + "/ResponseHolders/" + node.GetHashCode() + ".asset");
                    EditorUtility.SetDirty(responseHolder);
                    break;
            }
        }

        RefreshSO();
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
                    ResponseHolder response = LoadResponseHolder(nodeData, graphController.name);
                    graphController.dialogueObjs[i] = response;
                    EditorUtility.SetDirty(response);
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
            }
        }

        dialogue.character = dialogueData.character;
        dialogue.overrideCharacterName = dialogueData.overrideCharacterName;
        dialogue.text = dialogueData.text;

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
                    AssetDatabase.DeleteAsset("Assets/Dialogue/ScriptableObjects/" + graphName + "/Responses/" + responseNode.GetHashCode() + ".asset");
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
                AssetDatabase.CreateAsset(response, "Assets/Dialogue/ScriptableObjects/" + graphName + "/Responses/" + responseNode.GetHashCode() + ".asset");
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
                }
            }

            response.text = responseNode.text;
        }

        return responseHolder;
    }

    private static void RemoveUnusedSO() // TODO: FIX THIS FUNCTION
    {
        ControllerNode controllerNode = null;

        List<NodeModel> nodes = Window.graphController.graphData.Nodes;
        string[] nodeNames = new string[nodes.Count];

        for (int i = 0; i < nodeNames.Length; i++)
        {
            NodeModel node = nodes[i];
            string nodeName = string.Empty;

            if (node.GetName() == "DialogueNode")
            {
                DialogueNode nodeData = (DialogueNode)node.nodeData;

                nodeName = nodeData.dialogueData.dialogue == null ? string.Empty : nodeData.dialogueData.dialogue.name;
            }
            else if (node.GetName() == "ResponseNode")
            {
                ResponseHolderNode nodeData = (ResponseHolderNode)node.nodeData;

                nodeName = nodeData.responseHolder == null ? string.Empty : nodeData.responseHolder.name;
            }
            else if (node.GetName() == "ControllerNode")
            {
                ControllerNode nodeData = (ControllerNode)node.nodeData;
                controllerNode = nodeData;
            }

            nodeNames[i] = nodeName;
        }

        if (controllerNode == null)
        {
            Debug.LogError("No ControllerNode found");
            return;
        }

        string[] dialogueFiles = Directory.GetFiles(Application.dataPath + "Dialogue/ScriptableObjects/" + controllerNode.graphController.name + "/Dialogues/");
        string[] answerFiles = Directory.GetFiles(Application.dataPath + "Dialogue/ScriptableObjects/" + controllerNode.graphController.name + "/ResponseHolders/");
        string[] files = new string[dialogueFiles.Length + answerFiles.Length];

        for (int i = 0; i < dialogueFiles.Length; i++)
            files[i] = dialogueFiles[i];

        for (int i = 0; i < answerFiles.Length; i++)
            files[i + dialogueFiles.Length] = answerFiles[i];

        bool isAnswer = false;
        for (int i = 0; i < files.Length; i++)
        {
            if (i >= dialogueFiles.Length) isAnswer = true;

            string fileName = files[i];

            if (fileName.EndsWith(".meta")) continue;

            string[] parsedName = fileName.Split("\\");
            fileName = parsedName[^1];
            parsedName = fileName.Split(".");
            fileName = parsedName[0];
            files[i] = fileName;

            bool foundName = false;
            foreach (string t in nodeNames)
            {
                if (fileName != t) continue;
                foundName = true;
                break;
            }

            if (foundName) continue;
            string assetPath = isAnswer ? "ResponseHolders/" : "Dialogues/";
            assetPath = controllerNode.graphController.name + "/" + assetPath;
            AssetDatabase.DeleteAsset("Assets/Dialogue/ScriptableObjects/" + assetPath + fileName + ".asset");
        }
    }

    private static void UpdateSO()
    {
        GraphController graphController = null;

        List<NodeModel> nodes = Window.graphController.graphData.Nodes;

        List<NodeModel> dialogueNodes = new();
        List<NodeModel> answerNodes = new();

        foreach (NodeModel node in nodes)
        {
            if (node.GetName() == "DialogueNode")
                dialogueNodes.Add(node);
            else if (node.GetName() == "ResponseNode")
                answerNodes.Add(node);
            else if (node.GetName() == "ControllerNode")
            {
                ControllerNode nodeData = (ControllerNode)node.nodeData;
                graphController = nodeData.graphController;
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
            dialogueData.text = dialogue.text;
        }

        foreach (NodeModel node in answerNodes)
        {
            ResponseHolderNode responseHolderNode = (ResponseHolderNode)node.nodeData;
            List<ResponseNode> responses = responseHolderNode.responses;
            ResponseHolder responseHolder = responseHolderNode.responseHolder;

            for (int i = 0; i < responses.Count; i++)
            {
                Response response = responseHolder.responses[i];
                ResponseNode responseNode = responses[i];
                responseNode.text = response.text;
            }
        }

        Debug.Log("Saved nodes");
    }
}