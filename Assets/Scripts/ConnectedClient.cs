using DarkRift.Server;
using DarkRift;

namespace DarkRiftRPG
{
    public class ConnectedClient
    {
        public ushort ClientID;
        public IClient Client;
        public string PlayFabID;

        public PlayFabCharacterData CurrentCharacterData;
        public ConnectedClient(IClient client, string playfabID)
        {
            Client = client;
            ClientID = client.ID;
            PlayFabID = playfabID;

            Client.MessageReceived += OnMessage;
        }

        private void OnMessage(object sender, MessageReceivedEventArgs e)
        {
            IClient client = (IClient)sender;
            using (Message m = e.GetMessage())
            {
                switch ((Tags)m.Tag)
                {
                    case Tags.JoinGameAsCharacterRequest:
                        OnJoinGameAsCharacterRequest(m.Deserialize<JoinGameAsCharacterRequestData>());
                        break;
                    case Tags.PlayerMovementRequest:
                        OnPlayerMovementRequest(m.Deserialize<PlayerMovementRequestData>());
                        break;
                    case Tags.RegisterNewCharacterRequest:
                        OnPlayerRegisterNewCharacterRequest(m.Deserialize<RegisterNewCharacterRequestData>());
                        break;
                }
            }
        }

        private void OnPlayerRegisterNewCharacterRequest(RegisterNewCharacterRequestData data)
        {
            PlayFabAPICaller.Instance.TryCreateNewCharacter(data.CharacterName, PlayFabID, ClientID);
        }

        private void OnJoinGameAsCharacterRequest(JoinGameAsCharacterRequestData data)
        {
            PlayFabAPICaller.Instance.TryRetrieveCharacterData(ClientID, PlayFabID, data.CharacterID);
        }

        private void OnPlayerMovementRequest(PlayerMovementRequestData data)
        {
            PlayerManager.Instance.HandlePlayerMovementRequest(ClientID, data.PlayerClickLocation);
        }

        public void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            e.Client.MessageReceived -= OnMessage;
        }
    }
}

