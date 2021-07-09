using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Net.Sockets;
using System.IO;
using System;

public class Client : MonoBehaviour
{
	public InputField IPInput, PortInput, NickInput; // 데이터를 입력하기 위한 인풋필드
	string clientName;	// 클라이름

	bool socketReady;	// 소켓이 붙어 있는지 아닌지 확인하는 bool 변수
	TcpClient socket;	// 소켓 클라
	NetworkStream stream; // 네트워크 스트림
	StreamWriter writer; // 파일 쓰는 놈
	StreamReader reader; // 파일 읽는 놈

    private void Start()
    {
		LineManager.Instance.OnSend = (data) => Send(data);
	}

    public void ConnectToServer() // 클라이언트로 접속 클릭시 실행되는 메서드
	{
		// 이미 연결되었다면 함수 무시
		if (socketReady) return;

		// 기본 호스트 / 포트번호
		string ip = IPInput.text == "" ? "192.168.1.37" : IPInput.text.ToString();
		int port = PortInput.text == "" ? 35000 : int.Parse(PortInput.text.ToString());

		// 소켓 생성
		try
		{
			socket = new TcpClient(ip, port); // 소켓 클라 생성
			stream = socket.GetStream(); // 현재 연결된 클라의 소켓 네트워크 스트림을 가져옴
			writer = new StreamWriter(stream); // 새로운 파일 쓰는 놈 생성
			reader = new StreamReader(stream); // 새로운 파일 읽는 놈 생성
			socketReady = true;
		}
		catch (Exception e)
		{
			ChatManager.instance.ShowMessage($"소켓에러 : {e.Message}");
		}
	}

	void Update()
	{
		if (socketReady && stream.DataAvailable)
		{
			string data = reader.ReadLine();

			if (data != null) OnIncomingData(data);
		}
	}

	void OnIncomingData(string data)
	{
		print($"Client :: OnIncomingData :: {data}");

		if (data.Contains("%NAME"))
		{
			clientName = NickInput.text == "" ? "Guest" + UnityEngine.Random.Range(1000, 10000) : NickInput.text;
			Send($"&NAME|{clientName}");
			return;
		}
		else if (data.Contains("%MESSAGE"))
		{
			string msg = data.Split('|')[1];
			ChatManager.instance.ShowMessage(msg);
			return;
		}
		else if (data.Contains("%POSITION"))
		{
			string jsonData = data.Split('|')[1];
			LinePacket packet = JsonUtility.FromJson<LinePacket>(jsonData);

			print($"Client :: Json ::{ packet.receive_dataList.Count}");
			LineManager.Instance.BuildLine(packet);
			return;
		}
		else if (data.Contains("%UNDO"))
		{
			print("언도우");
			LineManager.Instance.Undo();
			return;
		}
		else if (data.Contains("%CLEAR"))
		{
			print("클리어");
			LineManager.Instance.Clear();
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
		Send($"&MESSAGE|{message}");
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
