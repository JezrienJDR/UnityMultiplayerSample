using UnityEngine;
using Unity.Collections;
using Unity.Networking.Transport;
using NetworkMessages;
using NetworkObjects;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;


public class NetworkClient : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;
    public string serverIP;
    public ushort serverPort;

    public GameObject localCube;
    public GameObject remoteCube;

    public List<GameObject> activeCubes;

    float timeElapsed;
    public float updateTime;

    bool CubeInstantiated = false;
    
    void Start ()
    {
        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.Parse(serverIP,serverPort);
        m_Connection = m_Driver.Connect(endpoint);
    }
    
    IEnumerator SendRepeatHandshakeToServer()
    {
        while(true)
        {
            yield return new WaitForSeconds(2);
            Debug.Log("Sending Handshake");
            HandshakeMsg m = new HandshakeMsg();
            m.player.id = m_Connection.InternalId.ToString();
            SendToServer(JsonUtility.ToJson(m));
        }
    }
    void SendToServer(string message){
        var writer = m_Driver.BeginSend(m_Connection);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message),Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }

    void OnConnect(){
        Debug.Log("We are now connected to the server");

        //// Example to send a handshake message:
        // HandshakeMsg m = new HandshakeMsg();
        // m.player.id = m_Connection.InternalId.ToString();
        // SendToServer(JsonUtility.ToJson(m));

        StartCoroutine(SendRepeatHandshakeToServer());
    }

    void OnData(DataStreamReader stream){
        NativeArray<byte> bytes = new NativeArray<byte>(stream.Length,Allocator.Temp);
        stream.ReadBytes(bytes);
        string recMsg = Encoding.ASCII.GetString(bytes.ToArray());
        NetworkHeader header = JsonUtility.FromJson<NetworkHeader>(recMsg);

        switch(header.cmd){
            case Commands.HANDSHAKE:
            HandshakeMsg hsMsg = JsonUtility.FromJson<HandshakeMsg>(recMsg);
            Debug.Log("Handshake message received!");
            break;
            case Commands.PLAYER_UPDATE:
            PlayerUpdateMsg puMsg = JsonUtility.FromJson<PlayerUpdateMsg>(recMsg);
            Debug.Log("Player update message received!");
            break;
            case Commands.SERVER_UPDATE:
            ServerUpdateMsg suMsg = JsonUtility.FromJson<ServerUpdateMsg>(recMsg);
                for (int i = 0; i < suMsg.players.Count; i++)
                {
                    bool spawned = false;
                    foreach(GameObject p in activeCubes)
                    {
                        if(p.GetComponent<CubeBase>().id == suMsg.players[i].id)
                        {
                            spawned = true;

                            p.GetComponent<CubeBase>().ColourChange(suMsg.players[i].R, suMsg.players[i].G, suMsg.players[i].B);

                            if (p.GetComponent<PlayerCube>() == null)
                            {
                                p.transform.position = new Vector3(suMsg.players[i].X, suMsg.players[i].Y, suMsg.players[i].Z);
                            }
                        }
                    }

                    if(!spawned)
                    {
                        AddCube(suMsg.players[i]);
                    }
                }
            //Debug.Log("Server update message received!");
            break;
            case Commands.CREATE_PLAYER:
                // Create local cube
                Debug.Log("CREATE PLAYER message received!");
            CreatePlayerMsg myCubeMsg = JsonUtility.FromJson<CreatePlayerMsg>(recMsg);
            SpawnMyCube(myCubeMsg);
            break;
            case Commands.DESTROY_PLAYER:
                // 
                Debug.Log("DESTROY_PLAYER message received!");
                DisconnectedPlayerMsg dpMsg = JsonUtility.FromJson<DisconnectedPlayerMsg>(recMsg);
                for(int i = 0; i < activeCubes.Count; i++)
                {
                    if (activeCubes[i].GetComponent<CubeBase>().id == dpMsg.droppedID)
                    {
                        activeCubes[i].SetActive(false);
                        DestroyImmediate(activeCubes[i]);
                        activeCubes.RemoveAt(i);
                        i--;
                    }
                }
            break;
            default:
            Debug.Log("Unrecognized message received!");
            break;
        }
    }

    void SpawnMyCube(CreatePlayerMsg m)
    {
        activeCubes = new List<GameObject>();

        Vector3 pos = new Vector3(m.player.X, m.player.Y, m.player.Z);
        Color newCol = new Color(m.player.R, m.player.G, m.player.B);
        GameObject p1 = Instantiate(localCube, pos, new Quaternion(0, 0, 0, 0));
        p1.GetComponent<CubeBase>().id = m.player.id;
        activeCubes.Add(p1);

        CubeInstantiated = true;
    }

    void AddCube(NetworkObjects.NetworkPlayer p)
    {
        Vector3 pos = new Vector3(p.X, p.Y, p.Z);
        Color newCol = new Color(p.R, p.G, p.B);
        GameObject p1 = Instantiate(remoteCube, pos, new Quaternion(0, 0, 0, 0));
        p1.GetComponent<CubeBase>().id = p.id;
        activeCubes.Add(p1);
    }

    void SendPosition()
    {
        if (CubeInstantiated)
        {
            PlayerUpdateMsg m = new PlayerUpdateMsg();

            m.player.X = activeCubes[0].transform.position.x;
            m.player.Y = activeCubes[0].transform.position.y;
            m.player.Z = activeCubes[0].transform.position.z;

            m.player.id = activeCubes[0].GetComponent<CubeBase>().id;

            SendToServer(JsonUtility.ToJson(m));
        }
    }

    void Populate()
    {

    }
    void Disconnect(){
        m_Connection.Disconnect(m_Driver);
        m_Connection = default(NetworkConnection);
    }

    void OnDisconnect(){
        Debug.Log("Client got disconnected from server");
        m_Connection = default(NetworkConnection);
    }

    public void OnDestroy()
    {
        m_Driver.Dispose();
    }   
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if(timeElapsed >= updateTime)
        {
            timeElapsed = 0;

            SendPosition();
        }

        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }

        DataStreamReader stream;
        NetworkEvent.Type cmd;
        cmd = m_Connection.PopEvent(m_Driver, out stream);
        while (cmd != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                OnConnect();
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                OnData(stream);
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                OnDisconnect();
            }

            cmd = m_Connection.PopEvent(m_Driver, out stream);
        }
    }
}