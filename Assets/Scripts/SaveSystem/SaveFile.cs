using System;
using System.Collections.Generic;

public class SaveFile
{
    [Serializable]
    public class SaveData
    {
        public string character;
        public string currentNode;
    }

    public Dictionary<string, SaveData> saveData;
    public SaveData[] saves;
}
