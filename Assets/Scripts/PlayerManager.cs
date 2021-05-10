using System.Collections.Generic;
using UnityEngine;
using DarkRift;

namespace DarkRiftRPG
{
    //This class is concerned with spawning new players, handling player movement, and setting the data for the current player character
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;

        public GameObject ServerPlayerPrefab;

        public Dictionary<ushort, GameObject> CurrentPlayers = new Dictionary<ushort, GameObject>();

        List<PlayerPositionInputData> UnprocessedPlayerMovementInput = new List<PlayerPositionInputData>();
        List<PlayerPositionInputData> ProccessedPlayerMovementInput = new List<PlayerPositionInputData>();

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SpawnPlayerOnServer(ushort clientID, PlayFabCharacterData characterData)
        {
            if (!CurrentPlayers.ContainsKey(clientID))
            {

                InstantiateAndAddPlayer(clientID, characterData.WorldPosition);

                PlayerSpawnData spawnData = new PlayerSpawnData(
                    clientID,
                    characterData.WorldPosition,
                    characterData.CharacterName
                    );

                SyncronizePlayerCharacterOnClients(spawnData);

            }
            else
            {
                Debug.Log("Player already spawned for this client");
            }
        }

        private void SyncronizePlayerCharacterOnClients(PlayerSpawnData spawnData)
        {
            ServerManager.Instance.SendToClient(spawnData.ID, Tags.SpawnPlayer, spawnData);

            ServerManager.Instance.SendNewPlayerToOthers(spawnData.ID, spawnData.Position, spawnData.PlayerCharacterName);

            ServerManager.Instance.SendOthersToNewPlayer(spawnData.ID, CurrentPlayers);
        }

        private GameObject InstantiateAndAddPlayer(ushort clientID, Vector3 spawnPosition)
        {
            if (CurrentPlayers.ContainsKey(clientID)) return new GameObject();
            Debug.Log("Instantiting player on server at position: " + spawnPosition.ToString());
            GameObject go = Instantiate(ServerPlayerPrefab, spawnPosition, Quaternion.identity);
            CurrentPlayers.Add(clientID, go);

            return go;
        }

        public void HandlePlayerMovementRequest(ushort clientID, Vector3 playerClickLocation)
        {
            PlayerPositionInputData input = new PlayerPositionInputData(clientID, playerClickLocation);
            UnprocessedPlayerMovementInput.Add(input);
        }

        private void FixedUpdate()
        {
            foreach (PlayerPositionInputData input in UnprocessedPlayerMovementInput)
            {
                ServerPlayerController controller = CurrentPlayers[input.ID].GetComponent<ServerPlayerController>();
                controller.UpdateNavTarget(input.Pos);

                ProccessedPlayerMovementInput.Add(input);
            }

            ProccessedPlayerMovementData proccessedMovement = new ProccessedPlayerMovementData(ProccessedPlayerMovementInput.ToArray());
            ServerManager.Instance.SendToAll(Tags.PlayerMovementUpdate, proccessedMovement);

            UnprocessedPlayerMovementInput.Clear();
            ProccessedPlayerMovementInput.Clear();

        }
    }
}

