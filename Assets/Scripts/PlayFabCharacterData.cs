using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkRiftRPG
{
    public class PlayFabCharacterData
    {
        public string PlayFabID { get; set; }
        public string CharacterID { get; set; }
        public string CharacterName { get; set; }
        public int CharacterLevel { get; set; }
        public int CharacterXP { get; set; }
        public int CharacterGold { get; set; }

        public float WorldPositionX { get; set; } 
        public float WorldPositionY { get; set; }
        public float WorldPositionZ { get; set; }
        public Vector3 WorldPosition { get; set; }

        public bool IsInitialCharacterData { get; set; }


        public PlayFabCharacterData(string playfabID, string characterID, string name, int level, int xp, int gold, float x, float y, float z)
        {
            PlayFabID = playfabID;
            CharacterID = characterID;
            CharacterName = name;

            CharacterLevel = level;
            CharacterXP = xp;
            CharacterGold = gold;

            WorldPositionX = x;
            WorldPositionY = y;
            WorldPositionZ = z;

        }
        public PlayFabCharacterData() { }

        public void SetWorldPosition(float x, float y, float z)
        {
            WorldPosition = new Vector3(x, y, z);

        }
    }
}

