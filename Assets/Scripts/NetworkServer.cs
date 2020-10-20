﻿using UnityEngine;
using UnityEngine.Assertions;
using Unity.Collections;
using Unity.Networking.Transport;
using NetworkMessages;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;

public class NetworkServer : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public ushort serverPort;
    private NativeList<NetworkConnection> m_Connections;

    private List<NetworkObjects.NetworkPlayer> allCubes;

    float elapsedTime = 0.0f;
    float updateTime = 1.0f;

    void Start ()
    {
        m_Driver = NetworkDriver.Create();
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = serverPort;
        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Failed to bind to port " + serverPort);
        else
            m_Driver.Listen();

        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);

        allCubes = new List<NetworkObjects.NetworkPlayer>();
    }

    void SendToClient(string message, NetworkConnection c){
        var writer = m_Driver.BeginSend(NetworkPipeline.Null, c);
        NativeArray<byte> bytes = new NativeArray<byte>(Encoding.ASCII.GetBytes(message),Allocator.Temp);
        writer.WriteBytes(bytes);
        m_Driver.EndSend(writer);
    }


    public void OnDestroy()
    {
        m_Driver.Dispose();
        m_Connections.Dispose();
    }

    void OnConnect(NetworkConnection c){
        m_Connections.Add(c);
        Debug.Log("Accepted a connection");

        //// Example to send a handshake message:
        // HandshakeMsg m = new HandshakeMsg();
        // m.player.id = c.InternalId.ToString();
        // SendToClient(JsonUtility.ToJson(m),c);        

        SendCharacterToNewClient(c);

    }

    void SendCharacterToNewClient(NetworkConnection nc)
    {
        CreatePlayerMsg m = new CreatePlayerMsg(nc);
        m.player.id = nc.InternalId.ToString();
        allCubes.Add(m.player);
        SendToClient(JsonUtility.ToJson(m), nc);
    }

    void PopulateClient(NetworkConnection nc)
    {
        PopulateMsg m = new PopulateMsg();
        m.players = allCubes;
        SendToClient(JsonUtility.ToJson(m), nc);
    }

    void UpdateClient(NetworkConnection nc)
    {
        ServerUpdateMsg m = new ServerUpdateMsg();
        foreach(NetworkObjects.NetworkPlayer p in allCubes)
        {
            p.R = UnityEngine.Random.Range(0, 255);
            p.G = UnityEngine.Random.Range(0, 255);
            p.B = UnityEngine.Random.Range(0, 255);

        }
        m.players = allCubes;
        SendToClient(JsonUtility.ToJson(m), nc);
    }

    void OnData(DataStreamReader stream, int i){
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
                foreach(NetworkObjects.NetworkPlayer p  in allCubes)
                {
                    if(p.id == puMsg.player.id)
                    {
                        p.X = puMsg.player.X;
                        p.Y = puMsg.player.Y;
                        p.Z = puMsg.player.Z;
                    }
                }
            Debug.Log("Player update message received!");
            break;
            case Commands.SERVER_UPDATE:
            ServerUpdateMsg suMsg = JsonUtility.FromJson<ServerUpdateMsg>(recMsg);
            Debug.Log("Server update message received!");
            break;
            default:
            Debug.Log("SERVER ERROR: Unrecognized message received!");
            break;
        }
    }

    void OnDisconnect(int i){
        Debug.Log("Client disconnected from server");

        DisconnectedPlayerMsg m = new DisconnectedPlayerMsg(m_Connections[i].InternalId.ToString());

        SendToClient(JsonUtility.ToJson(m), m_Connections[i]);
        
        m_Connections[i] = default(NetworkConnection);
    }

    void Update ()
    {
        m_Driver.ScheduleUpdate().Complete();

        // CleanUpConnections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            if (!m_Connections[i].IsCreated)
            {

                m_Connections.RemoveAtSwapBack(i);
                --i;
            }
        }

        // AcceptNewConnections
        NetworkConnection c = m_Driver.Accept();
        while (c  != default(NetworkConnection))
        {            
            OnConnect(c);

            // Check if there is another new connection
            c = m_Driver.Accept();
        }


        // Read Incoming Messages
        DataStreamReader stream;
        for (int i = 0; i < m_Connections.Length; i++)
        {
            Assert.IsTrue(m_Connections[i].IsCreated);
            
            NetworkEvent.Type cmd;
            cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream);
            while (cmd != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    OnData(stream, i);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    OnDisconnect(i);

                    foreach (NetworkObjects.NetworkPlayer p in allCubes)
                    {
                        if(p.id == m_Connections[i].InternalId.ToString())
                        {
                            allCubes.RemoveSwapBack(p);
                        }
                    }
                }

                cmd = m_Driver.PopEventForConnection(m_Connections[i], out stream);
            }
        }

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= updateTime)
        {
            elapsedTime = 0;

            for (int i = 0; i < m_Connections.Length; i++)
            {
                UpdateClient(m_Connections[i]);
            }
        }
    }
}