using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.AdminModels;

namespace DarkRiftRPG
{
    public struct PlayFabCharacterData
    {
        public string PlayFabID;
        public string CharacterID;
        public string CharacterName;
        public string CharacterLevel;
        public string CharacterXP;
        public string CharacterGold;

        public PlayFabCharacterData (string playfabID, string characterID, string name, string level, string xp, string gold)
        {
            PlayFabID = playfabID;
            CharacterID = characterID;
            CharacterName = name;
            CharacterLevel = level;
            CharacterXP = xp;
            CharacterGold = gold;
        }
    }

    public class PlayFabAPICaller : MonoBehaviour
    {
        public static PlayFabAPICaller Instance;

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(this);
        }

        public void TryCreateNewCharacter(string name, string playfabID, ushort clientID)
        {

            GrantCharacterToUserRequest grantCharacterToUserRequest = new GrantCharacterToUserRequest
            {
                CharacterName = name,
                CharacterType = "Basic",
                PlayFabId = playfabID
            };

            PlayFabServerAPI.GrantCharacterToUser(grantCharacterToUserRequest,
            result =>
            {
                UpdateCharacterStatisticsRequest inititalCharacterUpdateRequest = new UpdateCharacterStatisticsRequest()
                {
                    CharacterId = result.CharacterId,
                    PlayFabId = playfabID,
                    CharacterStatistics = new Dictionary<string, int>()
                    {
                        {"Level", 1 },
                        {"XP", 0 },
                        {"Gold", 0 }
                    }
                };

                SetInitialCharacterStats(clientID, inititalCharacterUpdateRequest);
            },
            error =>
            {
                Debug.Log($"Error: {error.ErrorMessage}");
                ServerManager.Instance.SendToClient(clientID, Tags.RegisterNewCharacterResponse, new RegisterNewCharacterResponseData(false));

            });
        }

        private static void SetInitialCharacterStats(ushort clientID, UpdateCharacterStatisticsRequest inititalCharacterUpdateRequest)
        {
            PlayFabServerAPI.UpdateCharacterStatistics(inititalCharacterUpdateRequest,
            result =>
            {
                ServerManager.Instance.SendToClient(clientID, Tags.RegisterNewCharacterResponse, new RegisterNewCharacterResponseData(true));
            },
            error =>
            {
                Debug.Log($"Error: {error.ErrorMessage}");
                ServerManager.Instance.SendToClient(clientID, Tags.RegisterNewCharacterResponse, new RegisterNewCharacterResponseData(false));

            });
        }

        public void TryRetrieveCharacterData(ushort clientID, string playfabID, string characterID)
        {

            ListUsersCharactersRequest listUsersCharactersRequest = new ListUsersCharactersRequest
            {
                PlayFabId = playfabID
            };

            PlayFabServerAPI.GetAllUsersCharacters(listUsersCharactersRequest,
            result =>
            {
                foreach (CharacterResult character in result.Characters)
                {
                    if (character.CharacterId == characterID)
                    {
                        GetCharacterStatisticsRequest getCharacterStatisticsRequest = new GetCharacterStatisticsRequest
                        {
                            CharacterId = characterID,
                            PlayFabId = playfabID
                        };

                        PlayFabServerAPI.GetCharacterStatistics(getCharacterStatisticsRequest,
                        result =>
                        {
                            PlayFabCharacterData characterData = new PlayFabCharacterData(
                                playfabID,
                                character.CharacterId,
                                character.CharacterName,
                                result.CharacterStatistics["Level"].ToString(),
                                result.CharacterStatistics["XP"].ToString(),
                                result.CharacterStatistics["Gold"].ToString());

                            SetCharacterDataForPlayer(clientID, characterData);

                            PlayerManager.Instance.SpawnPlayerOnServer(clientID, characterData.CharacterName);
                        },
                        error =>
                        {
                            Debug.Log("Error retrieving character stats");
                        });
                    }
                }
            },
            error =>
            {
                Debug.Log("Error retrieving character");
            });
        }

        private static void SetCharacterDataForPlayer(ushort clientID, PlayFabCharacterData characterData)
        {
            ConnectedClient clientToSpawn = ServerManager.Instance.ConnectedClients[clientID];
            clientToSpawn.CurrentCharacterData = characterData;
        }
    }
}

