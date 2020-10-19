using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Networking.Transport;

namespace NetworkMessages
{
    public enum Commands{
        PLAYER_UPDATE,
        SERVER_UPDATE,
        HANDSHAKE,
        PLAYER_INPUT,
        POPULATE_CLIENT,
        CREATE_PLAYER,
        DESTROY_PLAYER
    }

    [System.Serializable]
    public class NetworkHeader{
        public Commands cmd;
    }

    [System.Serializable]
    public class HandshakeMsg:NetworkHeader{
        public NetworkObjects.NetworkPlayer player;
        public HandshakeMsg(){      // Constructor
            cmd = Commands.HANDSHAKE;
            player = new NetworkObjects.NetworkPlayer();
        }
    }
    
    [System.Serializable]
    public class PlayerUpdateMsg:NetworkHeader{
        public NetworkObjects.NetworkPlayer player;
        public PlayerUpdateMsg(){      // Constructor
            cmd = Commands.PLAYER_UPDATE;
            player = new NetworkObjects.NetworkPlayer();
        }
    };

    [System.Serializable]
    public class PlayerInputMsg:NetworkHeader{
        public Input myInput;
        public PlayerInputMsg(){
            cmd = Commands.PLAYER_INPUT;
            myInput = new Input();
        }
    }
    [System.Serializable]
    public class  ServerUpdateMsg:NetworkHeader{
        public List<NetworkObjects.NetworkPlayer> players;
        public ServerUpdateMsg(){      // Constructor
            cmd = Commands.SERVER_UPDATE;
            players = new List<NetworkObjects.NetworkPlayer>();
        }
    }

    public class PopulateMsg : NetworkHeader
    {
        public List<NetworkObjects.NetworkPlayer> players;
        public PopulateMsg()
        {      // Constructor
            cmd = Commands.POPULATE_CLIENT;
            players = new List<NetworkObjects.NetworkPlayer>();
        }
    }


    // Creates a new player object on the server, sends it to its own client to spawn the local player cube.
    [System.Serializable]
    public class CreatePlayerMsg : NetworkHeader
    {
        public NetworkObjects.NetworkPlayer player;
        public CreatePlayerMsg(NetworkConnection nc)
        {      // Constructor
            cmd = Commands.CREATE_PLAYER;
            player = new NetworkObjects.NetworkPlayer();
            player.id = nc.InternalId.ToString();
            
        }
    }
} 

namespace NetworkObjects
{
    [System.Serializable]
    public class NetworkObject{
        public string id;
    }
    [System.Serializable]
    public class NetworkPlayer : NetworkObject{
        

        public float R;
        public float G;
        public float B;

        public float X;
        public float Y;
        public float Z;

        public NetworkPlayer(){
            R = Random.Range(0,255);
            G = Random.Range(0, 255);
            B = Random.Range(0, 255);

            X = Random.Range(-10, 10);
            Y = Random.Range(-10, 10);
            Z = Random.Range(-10, 10);
        }
    }
}
