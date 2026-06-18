using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    public static SaveFile save;
    public static SaveFile.SaveData currentSave;
    public static int loadedPath;

    [SerializeField] private CharactersHolder charactersHolder;
    private const string savePath = "/SaveData.json";

    private static bool isInitialized;
    private static bool fakeSaveFile;

    private void Awake()
    {
        if (isInitialized) return;
        LookForSave();
        isInitialized = true;
    }

    private void LookForSave()
    {
        if (File.Exists(Application.persistentDataPath + savePath))
            LoadSave();
        else
            CreateSave();
    }

    public static void CreateFakeDevSave(int path)
    {
        Debug.Log("Creating new temporary save data");
        save = new SaveFile
        {
            saveData = new Dictionary<string, SaveFile.SaveData>
            {
                {
                    "Sabrina", new SaveFile.SaveData
                    {
                        character = "Sabrina",
                        currentNode = null
                    }
                },
                {
                    "John", new SaveFile.SaveData
                    {
                        character = "John",
                        currentNode = null
                    }
                },
                { "Robyn", new SaveFile.SaveData
                {
                    character = "Robyn",
                    currentNode = null
                } }
            },
            bgmVolume = 1,
            sfxVolume = 1
        };

        fakeSaveFile = true;
        loadedPath = path;
        Save();
    }

    private void CreateSave()
    {
        save = new SaveFile
        {
            saveData = new Dictionary<string, SaveFile.SaveData>()
        };

        foreach (Character character in charactersHolder.characters)
        {
            save.saveData.Add(character.characterName, new SaveFile.SaveData());
            save.saveData[character.characterName].character = character.characterName;
        }

        Save();
    }

    private void LoadSave()
    {
        string json = File.ReadAllText(Application.persistentDataPath + savePath);

        try
        {
            save = JsonUtility.FromJson<SaveFile>(json);
        }
        catch
        {
            Debug.LogError("Save file could not be loaded");
            CreateSave();
            return;
        }

        if (save == null)
        {
            Debug.LogError("Save file could not be loaded");
            CreateSave();
            return;
        }

        save.saveData = new Dictionary<string, SaveFile.SaveData>();
        foreach (SaveFile.SaveData data in save.saves)
            save.saveData.Add(data.character, data);
    }

    public static void Save()
    {
        save.saves = new SaveFile.SaveData[save.saveData.Count];

        int idx = 0;
        foreach (KeyValuePair<string, SaveFile.SaveData> data in save.saveData)
        {
            save.saves[idx] = data.Value;
            idx++;
        }

        currentSave = save.saves[loadedPath];

        if (fakeSaveFile)
            return;
        string json = JsonUtility.ToJson(save);
        try
        {
            File.WriteAllText(Application.persistentDataPath + savePath, json);
        }
        catch
        {
            Debug.LogError("Save data could not be saved");
        }
    }

    public void ResetSave() => CreateSave();
}
