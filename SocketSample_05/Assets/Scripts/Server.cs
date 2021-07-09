using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;

public class Server : MonoBehaviour
{
    List<ServerClient> clients;
    List<ServerClient> disconnectList;

    TcpListener server;
    bool serverStarted;

    int serverDataidx;

    public void ServerCreate()
    {
        clients = new List<ServerClient>();
        disconnectList = new List<ServerClient>();

        try
        {
            int port = 35000;
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            StartListening();
            serverStarted = true;

            print($"서버가 {port}번에서 열렸습니다.");
        }
        catch (Exception e)
        {
            print($"Socket error: {e.Message}");
        }
    }

    void Update()
    {
        if (!serverStarted) return;

        foreach (ServerClient cl in clients)
        {
            if (!IsConnected(cl.tcp))
            {
                cl.tcp.Close();
                disconnectList.Add(cl);
                continue;
            }
            else
            {
                NetworkStream stream = cl.tcp.GetStream();

                if (stream.DataAvailable)
                {
                    print("계속 대기");
                    string data = new StreamReader(stream, true).ReadLine();

                    if (data != null) OnIncomingData(cl, data);
                }
            }
        }

        for (int i = 0; i < disconnectList.Count - 1; i++)
        {
            Broadcast($"{disconnectList[i].clientName} 연결이 끊어졌습니다", clients);

            clients.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }

    bool IsConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    //print($"TimeDelta :: {Time.deltaTime}, 실행이 되는 건가? {client.Client.Receive(new byte[1], SocketFlags.Peek)}");
                    return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                }

                return true;
            }
            else return false;
        }
        catch
        {
            return false;
        }
    }

    void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));

        StartListening();

        Broadcast("%NAME", new List<ServerClient>() { clients[clients.Count - 1] });
    }

    void OnIncomingData(ServerClient client, string data)
    {
        print($"Server :: OnIncomingData :: 몇번불렀냐 {serverDataidx++}");

        if (data.Contains("&NAME"))
        {
            client.clientName = data.Split('|')[1];

            Broadcast(client, $"{client.clientName} 이 연결되었습니다", clients);

            return;
        }

        if (data.Contains("&POSITION"))
        {
            string json = data.Split('|')[1];
            PositionPacket packet = JsonUtility.FromJson<PositionPacket>(json);

            //foreach (var element in packet.receive_dataList)
            //{
            //    print($"Server :: X : {element.mousePosX}, Y : {element.mousePosY}, Z : {element.mousePosZ}");
            //}

            print($"Server :: Json ::{ packet.receive_dataList.Count}");

            Broadcast($"%POSITION|{json}", clients);

            return;
        }

        Broadcast($"{client.clientName} : {data}", clients);
    }

    void Broadcast(string data, List<ServerClient> clients)
    {
        foreach (var cl in clients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(cl.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                print($"쓰기 에러 : {e.Message} 를 클라이언트에게 {cl.clientName}");
            }
        }
    }

    void Broadcast(ServerClient sender, string data, List<ServerClient> clients)
    {
        foreach (var cl in clients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(cl.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                print($"쓰기 에러 : {e.Message} 를 클라이언트에게 {cl.clientName}");
            }
        }
    }
}


public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }
}
