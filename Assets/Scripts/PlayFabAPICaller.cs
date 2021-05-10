using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ServerModels;
using PlayFab.AdminModels;
using PlayFab.DataModels;
using System;
using PlayFab.AuthenticationModels;
using PlayFab.Json;
using System.Threading.Tasks;
using System.Linq;

namespace DarkRiftRPG
{
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

        //Other classes should access any of these methods with the Try version of that method

        #region Character Creation
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
                PlayFabCharacterData newCharacterInitialData = CreateNewCharacterInitialData(playfabID, result.CharacterId, name);
                TrySaveCharacterData(newCharacterInitialData);

            },
            error =>
            {
                Debug.Log($"Error: Unable To Set Initial Character Data");
                Debug.Log($"Error: {error.ErrorMessage}");
                ServerManager.Instance.SendToClient(clientID, Tags.RegisterNewCharacterResponse, new RegisterNewCharacterResponseData(false));

            });
        }
        private PlayFabCharacterData CreateNewCharacterInitialData(string playfabID, string characterID, string characterName)
        {
            PlayFabCharacterData newCharacterInitialData = new PlayFabCharacterData(playfabID, characterID, characterName, 0, 0, 0, 1 ,0, 0);
            newCharacterInitialData.IsInitialCharacterData = true;

            return newCharacterInitialData;
        }
        #endregion

        #region Character Data Retrieval 
        public void TryRetrieveAllPlayerCharacters(ushort clientIdOfCaller, string playfabID, string characterID)
        {
            Debug.Log($"Trying to get character data for character: {characterID}");
            ListUsersCharactersRequest listUsersCharactersRequest = new ListUsersCharactersRequest
            {
                PlayFabId = playfabID
            };

            //Have to get all characters - playfab can't just return a single character
            PlayFabServerAPI.GetAllUsersCharacters(listUsersCharactersRequest,
            result =>
            {
                foreach (CharacterResult character in result.Characters)
                {
                    if (character.CharacterId == characterID)
                    {
                        TryRetrieveCharacterData(clientIdOfCaller, characterID);
                    }
                }
            },
            error =>
            {
                Debug.Log("Error retrieving character");
            });
        }
        void TryRetrieveCharacterData(ushort clientIdOfCaller, string characterID)
        {
            GetEntityTokenRequest requestToken = GetTokenForRequest();

            PlayFabAuthenticationAPI.GetEntityToken(requestToken,
            result =>
            {
                Debug.Log("GetEntityToken call for GetCharacterData worked");
                RetrieveCharacterData(clientIdOfCaller, characterID);
            },
            error =>
            {
                Debug.Log($"Failed to get entity token");
            });
        }
        void RetrieveCharacterData(ushort ConnectedClientID, string characterID)
        {
            PlayFab.DataModels.EntityKey characterEntityKey = CreateKeyForEntity(characterID, "character");
            GetObjectsRequest getObjectsRequest = CreateGetCharacterObjectRequestEscaped(characterEntityKey);

            PlayFabDataAPI.GetObjects(getObjectsRequest,
            result =>
            {
                PlayFabCharacterData characterData = PlayFabSimpleJson.DeserializeObject<PlayFabCharacterData>(result.Objects["CharacterData"].EscapedDataObject);
                Debug.Log($"character position for retrieved character: {characterData.WorldPositionX}, {characterData.WorldPositionY}, {characterData.WorldPositionZ}");
                characterData.SetWorldPosition(characterData.WorldPositionX, characterData.WorldPositionY, characterData.WorldPositionZ);
                Debug.Log($"character position AS VECTOR 3 for retrieved character: {characterData.WorldPosition.ToString()}");

                SetCurrentCharacterDataForConnectedClient(ConnectedClientID, characterData);


            }, error =>
            {
                Debug.Log("Error setting player info from PlayFab result object");
                Debug.Log(error.ErrorMessage);
            });

        }
        #endregion

        #region Save Character Data
        //Save character data
        public void TrySaveCharacterData(PlayFabCharacterData characterData)
        {
            GetEntityTokenRequest requestToken = GetTokenForRequest();

            PlayFabAuthenticationAPI.GetEntityToken(requestToken,
            result =>
            {
                SaveCharacterData(characterData);
            },
            error =>
            {
                Debug.Log($"Failed to get entity token");
            });   
        }
        void SaveCharacterData(PlayFabCharacterData characterData)
        {
            
            var dataList = new List<SetObject>();
            dataList.Add(ReturnNewSetObject("CharacterData", SetCharacterInfoData(characterData)));

            PlayFab.DataModels.EntityKey characterEntityKey = CreateKeyForEntity(characterData.CharacterID, "character");

            SetObjectsRequest setCharacterObjectDataRequest = new SetObjectsRequest()
            {
                Entity = characterEntityKey,
                Objects = dataList
            };

            PlayFabDataAPI.SetObjects(setCharacterObjectDataRequest,
                result =>
                {
                    if (characterData.IsInitialCharacterData)
                    {   
                        ushort clientID = ReturnClientConnectionByPlayFabCharacterID(characterData);
                        
                        if (clientID != 9999)
                        {
                            ServerManager.Instance.SendToClient(clientID, Tags.RegisterNewCharacterResponse, new RegisterNewCharacterResponseData(true));
                        } else
                        {
                            ServerManager.Instance.SendToClient(clientID, Tags.RegisterNewCharacterResponse, new RegisterNewCharacterResponseData(false));
                        }
                    }
                },
                error =>
                {
                    Debug.Log($"Failed to save character state data");
                    Debug.Log(error.ErrorMessage);
                    Debug.Log(error.ErrorDetails);
                });
        }
        #endregion

        #region PlayFabAPICallComponents
        //UTILS

        //Retruns a new GetObjectsRequest with with EscapedDataObject set
        private static GetObjectsRequest CreateGetCharacterObjectRequestEscaped(PlayFab.DataModels.EntityKey characterEntityKey)
        {
            return new GetObjectsRequest
            {
                Entity = characterEntityKey,
                EscapeObject = true
            };
        }

        //Returns a new SetObject with EscapedDataObject set
        SetObject ReturnNewSetObject(string objectName, Dictionary<string, object> objectData)
        {
            return new SetObject()
            {
                ObjectName = objectName,
                DataObject = objectData
            };
        } 

        //Returns a new EntityKey for an entity based on ID and Type
        PlayFab.DataModels.EntityKey CreateKeyForEntity(string entityId, string entityType)
        {
            return new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType };
        }

        //Creates, sets, and returns <string, object> dictionary required by PlayFabs object list base on existing values held in characterData
        Dictionary<string, object> SetCharacterInfoData(PlayFabCharacterData characterData)
        {
            return new Dictionary<string, object>()
            {
                {"CharacterID", characterData.CharacterID},
                {"PlayFabID", characterData.PlayFabID},
                {"CharacterName", characterData.CharacterName},

                {"CharacterLevel", characterData.CharacterLevel},
                {"CharacterXP", characterData.CharacterXP},
                {"CharacterGold", characterData.CharacterGold},

                {"WorldPositionX", characterData.WorldPosition.x},
                {"WorldPositionY", characterData.WorldPosition.y},
                {"WorldPositionZ", characterData.WorldPosition.z},

                {"InitialCharacterSave", characterData.IsInitialCharacterData}
            };
        }

        //Returns the request token the server needs to act on an entity
        GetEntityTokenRequest GetTokenForRequest()
        {
            return new GetEntityTokenRequest();
        }

        //Uses the PlayFab account ID for that player to return the correct connected client for this character
        private ushort ReturnClientConnectionByPlayFabCharacterID(PlayFabCharacterData characterData)
        {
            foreach (KeyValuePair<ushort, ConnectedClient> kvp in ServerManager.Instance.ConnectedClients)
            {
                if (kvp.Value != null)
                {
                    if (kvp.Value.PlayFabID == characterData.PlayFabID)
                    {
                        return kvp.Value.ClientID;
                    }
                }
            }

            return 9999; // there will NEVER be this many players on a single server
        }

        //Assigns CurrentCharacterData inside a client connection for a connected player
        private void SetCurrentCharacterDataForConnectedClient(ushort ConnectedClientID, PlayFabCharacterData characterData)
        {
            bool ConnectedClientIDIsValid = IsConnectedClientIDValid(ConnectedClientID);
            if (ConnectedClientIDIsValid)
            {
                ServerManager.Instance.ConnectedClients[ConnectedClientID].CurrentCharacterData = characterData;
                SpawnCharacterOnServer(ConnectedClientID, characterData);

            }
        }
        
        //Safety check to make sure client is still valid at time of call
        private bool IsConnectedClientIDValid(ushort ConnectedClientID)
        {
            ConnectedClient cc;
            ServerManager.Instance.ConnectedClients.TryGetValue(ConnectedClientID, out cc);

            if (cc != null)
            {
                return true;
            }
            return false;
        }

        #endregion

        void SpawnCharacterOnServer(ushort clientID, PlayFabCharacterData characterData)
        {
            PlayerManager.Instance.SpawnPlayerOnServer(clientID, characterData);
        }


    }
}

