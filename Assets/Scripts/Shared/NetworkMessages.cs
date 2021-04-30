using UnityEngine;
using DarkRift;

namespace DarkRiftRPG
{
    public enum Tags
    {
        JoinServerRequest,
        JoinServerResponse,

        RegisterNewCharacterRequest,
        RegisterNewCharacterResponse,

        JoinGameAsCharacterRequest,
        JoinGameAsCharacterResponse,

        PlayerMovementRequest,
        PlayerMovementUpdate,

        SpawnPlayer,
        DespawnPlayer
    }

    public struct JoinServerRequestData : IDarkRiftSerializable
    {
        public string PlayFabID;

        public JoinServerRequestData(string playfabID)
        {
            PlayFabID = playfabID;
        }

        public void Deserialize(DeserializeEvent e)
        {
            PlayFabID = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(PlayFabID);
        }
    }
    public struct JoinServerResponseData : IDarkRiftSerializable
    {
        public bool JoinServerRequestAccepted;

        public JoinServerResponseData(bool accepted)
        {
            JoinServerRequestAccepted = accepted;
        }
        public void Deserialize(DeserializeEvent e)
        {
            JoinServerRequestAccepted = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(JoinServerRequestAccepted);
        }
    }

    public struct RegisterNewCharacterResponseData : IDarkRiftSerializable
    {
        public bool RegisteredSuccessfully;

        public RegisterNewCharacterResponseData(bool registeredSuccessfully)
        {
            RegisteredSuccessfully = registeredSuccessfully;
        }
        public void Deserialize(DeserializeEvent e)
        {
            RegisteredSuccessfully = e.Reader.ReadBoolean();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(RegisteredSuccessfully);
        }
    }
    public struct RegisterNewCharacterRequestData : IDarkRiftSerializable
    {
        public string CharacterName;

        public RegisterNewCharacterRequestData(string name)
        {
            CharacterName = name;
        }
        public void Deserialize(DeserializeEvent e)
        {
            CharacterName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(CharacterName);
        }
    }

    public struct SpawnLocalPlayerResponseData : IDarkRiftSerializable
    {
        public ushort ID;

        public SpawnLocalPlayerResponseData(ushort id)
        {
            ID = id;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
        }
    }
    public struct JoinGameAsCharacterRequestData : IDarkRiftSerializable
    {
        public string CharacterID;

        public JoinGameAsCharacterRequestData(string characterID)
        {
            CharacterID = characterID;
        }
        public void Deserialize(DeserializeEvent e)
        {
            CharacterID = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(CharacterID);
        }
    }
    
    public struct PlayerMovementRequestData : IDarkRiftSerializable
    {
        public Vector3 PlayerClickLocation;

        public PlayerMovementRequestData(Vector3 clickPos)
        {
            PlayerClickLocation = clickPos;
        }
        public void Deserialize(DeserializeEvent e)
        {
            PlayerClickLocation = e.Reader.ReadVector3();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.WriteVector3(PlayerClickLocation);
        }
    }
    public struct PlayerPositionInputData : IDarkRiftSerializable
    {
        public ushort ID;
        public Vector3 Pos;

        public PlayerPositionInputData(ushort id, Vector3 pos)
        {
            ID = id;
            Pos = pos;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt16();
            Pos = e.Reader.ReadVector3();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.WriteVector3(Pos);
        }
    }
    public struct ProccessedPlayerMovementData : IDarkRiftSerializable
    {
        public PlayerPositionInputData[] ProccessedMovementUpdate;

        public ProccessedPlayerMovementData(PlayerPositionInputData[] newPlayerPositions)
        {
            ProccessedMovementUpdate = newPlayerPositions;
        }
        public void Deserialize(DeserializeEvent e)
        {
            ProccessedMovementUpdate = e.Reader.ReadSerializables<PlayerPositionInputData>();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ProccessedMovementUpdate);
        }
    }

    public struct PlayerSpawnData : IDarkRiftSerializable
    {
        public ushort ID;
        public Vector3 Position;
        public string PlayerCharacterName;

        public PlayerSpawnData(ushort id, Vector3 position, string name)
        {
            ID = id;
            Position = position;
            PlayerCharacterName = name;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt16();
            Position = e.Reader.ReadVector3();
            PlayerCharacterName = e.Reader.ReadString();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
            e.Writer.WriteVector3(Position);
            e.Writer.Write(PlayerCharacterName);
        }
    }
    public struct PlayerDespawnData : IDarkRiftSerializable
    {
        public ushort ID;

        public PlayerDespawnData(ushort id)
        {
            ID = id;
        }

        public void Deserialize(DeserializeEvent e)
        {
            ID = e.Reader.ReadUInt16();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(ID);
        }
    }
}
