using System.Collections.Generic;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Unity;
using UnityEngine;

namespace DarkRiftRPG
{
    public class ServerManager : MonoBehaviour
    {
        public static ServerManager Instance;

        public Dictionary<ushort, ConnectedClient> ConnectedClients = new Dictionary<ushort, ConnectedClient>();

        private XmlUnityServer xmlServer;
        private DarkRiftServer server;
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
        void Start()
        {
            xmlServer = GetComponent<XmlUnityServer>();
            server = xmlServer.Server;
            server.ClientManager.ClientConnected += OnClientConnected;
            server.ClientManager.ClientDisconnected += OnClientDisconnected;
        }
       
        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            RemovePlayerFromServer(e);
            SendToAllExcept(e.Client.ID, Tags.DespawnPlayer, new PlayerDespawnData(e.Client.ID));
        }
        private void RemovePlayerFromServer(ClientDisconnectedEventArgs e)
        {
            if (ConnectedClients.ContainsKey(e.Client.ID))
            {
                ConnectedClients.Remove(e.Client.ID);
            }

            if (PlayerManager.Instance.CurrentPlayers.ContainsKey(e.Client.ID))
            {
                Destroy(PlayerManager.Instance.CurrentPlayers[e.Client.ID]);
                PlayerManager.Instance.CurrentPlayers.Remove(e.Client.ID);
            }
            
            
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            e.Client.MessageReceived += OnMessage;
        }

        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            IClient client = (IClient)sender;
            using (Message message = e.GetMessage())
            {
                switch ((Tags)message.Tag)
                {
                    case Tags.JoinServerRequest:
                        OnPlayerJoinServerRequest(client, message.Deserialize<JoinServerRequestData>());
                        break;
                }
            }
        }
        private void OnPlayerJoinServerRequest(IClient client, JoinServerRequestData data)
        {
            JoinServerResponseData responseData = new JoinServerResponseData();

            if (ConnectedClients.ContainsKey(client.ID))
            {
                responseData.JoinServerRequestAccepted = false;

                using (Message message = Message.Create((ushort)Tags.JoinServerResponse, data))
                {
                    client.SendMessage(message, SendMode.Reliable);
                }
                return;
            }
            else
            {
                responseData.JoinServerRequestAccepted = true;

                client.MessageReceived -= OnMessage;

                ConnectedClient c = new ConnectedClient(client, data.PlayFabID);
                ConnectedClients.Add(c.ClientID, c);
                Debug.Log(c.PlayFabID);
                using (Message message = Message.Create((ushort)Tags.JoinServerResponse, responseData))
                {
                    client.SendMessage(message, SendMode.Reliable);
                }
            }
        }

        public void SendNewPlayerToOthers(ushort clientID, Vector3 position, string name)
        {
            PlayerSpawnData spawnData = new PlayerSpawnData(clientID, position, name);
            SendToAllExcept(clientID, Tags.SpawnPlayer, spawnData);
        }
        public void SendOthersToNewPlayer(ushort clientID, Dictionary<ushort, GameObject> players)
        {
            foreach (KeyValuePair<ushort, GameObject> player in players)
            {
                if (player.Key != clientID)
                {
                    PlayerSpawnData spawnData = new PlayerSpawnData(player.Key, player.Value.transform.position, ConnectedClients[player.Key].CurrentCharacterData.CharacterName);
                    SendToClient(clientID, Tags.SpawnPlayer, spawnData);
                }
            }
        }

        public void SendToClient(ushort id, Tags t, IDarkRiftSerializable obj, SendMode mode = SendMode.Reliable)
        {
            using (Message m = Message.Create<IDarkRiftSerializable>((ushort)t, obj))
            {
                ConnectedClients[id].Client.SendMessage(m, mode);
            }
        }
        public void SendToAll(Tags t, IDarkRiftSerializable obj, SendMode mode = SendMode.Reliable)
        {
            foreach (KeyValuePair<ushort, ConnectedClient> connectedClient in ConnectedClients)
            {
                using (Message m = Message.Create<IDarkRiftSerializable>((ushort)t, obj))
                {
                    connectedClient.Value.Client.SendMessage(m, mode);
                }
            }
        }
        public void SendToAllExcept(ushort id, Tags t, IDarkRiftSerializable obj, SendMode mode = SendMode.Reliable)
        {
            using (Message m = Message.Create<IDarkRiftSerializable>((ushort)t, obj))
            {
                foreach (KeyValuePair<ushort, ConnectedClient> connectedClient in ConnectedClients)
                {
                    if (connectedClient.Key != id)
                    {
                        connectedClient.Value.Client.SendMessage(m, mode);
                    }
                }
            }
        }

        void OnDestroy()
        {
            server.ClientManager.ClientConnected -= OnClientConnected;
            server.ClientManager.ClientDisconnected -= OnClientDisconnected;
        }

    }
}

