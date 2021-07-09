using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;
using Newtonsoft.Json;

public class Client : MonoBehaviour
{
	string clientName; 

	bool socketReady;  
	TcpClient socket;  
	NetworkStream stream; 
	StreamWriter writer; 
	StreamReader reader;

	PositionPacket packet = null;
	List<MousePosData> mousePosDatasList = null;

	int clientDataidx;

	public void ConnectToServer() 
	{
		// 이미 연결되었다면 함수 무시
		if (socketReady) return;

		// 기본 호스트 / 포트번호
		string ip ="172.30.1.8";
		int port = 35000;

		// 소켓 생성
		try
		{
			socket = new TcpClient(ip, port); 
			stream = socket.GetStream(); 
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);
			socketReady = true;
		}
		catch (Exception e)
		{
			print($"소켓에러 : {e.Message}");
		}
	}

	void Update()
	{
		if (socketReady && stream.DataAvailable)
		{
			string data = reader.ReadLine();

			if (data != null) OnIncomingData(data);
		}

		if (Input.GetMouseButtonDown(0))
		{
			packet = new PositionPacket();
			mousePosDatasList = new List<MousePosData>();
		}
		else if (Input.GetMouseButton(0))
		{
			MousePosData mousePosData = new MousePosData();

			mousePosData.mousePosX = Input.mousePosition.x;
			mousePosData.mousePosY = Input.mousePosition.y;
			mousePosData.mousePosZ = Input.mousePosition.z;

			mousePosDatasList.Add(mousePosData);
			print("Mouse Moving...");
		}
		else if (Input.GetMouseButtonUp(0))
		{
			packet.receive_dataList = mousePosDatasList;
			string json = JsonUtility.ToJson(packet);
            //print($"Client :: Json :: {json} // Mouse Pos :: {Input.mousePosition}");
            print($"Client :: Json :: {packet.receive_dataList.Count}");

			PositionPacket tempPacket = JsonUtility.FromJson<PositionPacket>(json);

			//foreach (var data in tempPacket.receive_dataList)
			//{
			//	print($"Client :: X : {data.mousePosX}, Y : {data.mousePosY}, Z : {data.mousePosZ}");
			//}

			//print($"Client :: Count {tempPacket.receive_dataList.Count}");

			Send($"&POSITION|{json}");
		}

	}

	void OnIncomingData(string data)
	{
		print($"Client :: OnIncomingData :: Call Count {clientDataidx++}");

        if (data == "%NAME")
        {
            clientName = "Guest" + UnityEngine.Random.Range(1000, 10000);
            Send($"&NAME|{clientName}");
            return;
        }

		if (data.Contains("%POSITION"))
		{
			string json = data.Split('|')[1];
			PositionPacket packet = JsonUtility.FromJson<PositionPacket>(json);

			print($"Client :: Json ::{ packet.receive_dataList.Count}");
			return;
		}
		
	}

	void Send(string data)
	{
		if (!socketReady) return;

		writer.WriteLine(data);
		writer.Flush();
	}

	public void OnSendButton(InputField SendInput)
	{
#if (UNITY_EDITOR || UNITY_STANDALONE)
		if (!Input.GetButtonDown("Submit")) return;
		SendInput.ActivateInputField();
#endif
		if (SendInput.text.Trim() == "") return;

		string message = SendInput.text;
		SendInput.text = "";
		Send(message);
	}
	void OnApplicationQuit()
	{
		CloseSocket();
	}

	void CloseSocket()
	{
		if (!socketReady) return;

		writer.Close();
		reader.Close();
		socket.Close();
		socketReady = false;
	}
}
