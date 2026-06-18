using System;
using System.Collections.Generic;

public class SaveFile
{
    [Serializable]
    public class SaveData
    {
        public string character;
        public string currentNode;
        public MenuItems selectedDessert;
        public MenuItems selectedDrink;
        public bool choseBart;
        public bool isBartLaying;
        public bool hasCat;
    }

    public Dictionary<string, SaveData> saveData;
    public SaveData[] saves;

    public float bgmVolume = 1;
    public float sfxVolume = 1;
}
