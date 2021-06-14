using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Linq;

public class Client : MonoBehaviour
{
    static public Client Instance;
    void Awake() => Instance = this;

    //local IP
    public string serverIp = "172.30.1.8";
    public Material test_material;
    public Color test_color;

    private Socket clientSocket = null;

    SimplePacket newPacket = null; // test
    List<float[]> testLines = null;

    // Start is called before the first frame update
    void Start() { }

    public void ConnectToServer()
    {
        //클라이언트에서 사용할 소켓 준비
        this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //클라이언트는 바인딩할 필요 없음

        //접속할 서버의 통신지점(목적지)
        IPAddress serverIPAdress = IPAddress.Parse(this.serverIp);
        IPEndPoint serverEndPoint = new IPEndPoint(serverIPAdress, Server.PortNumb);

        //서버로 연결 요청
        try
        {
            Debug.Log("Connecting to Server");
            this.clientSocket.Connect(serverEndPoint);
        }
        catch (SocketException e)
        {
            Debug.Log("Connection Failed:" + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        if (this.clientSocket != null)
        {
            this.clientSocket.Close();
            this.clientSocket = null;
        }
    }

    public static void Send(SimplePacket packet)
    {
        if (Client.Instance.clientSocket == null) return;

        byte[] sendData = SimplePacket.ToByteArray(packet);
        byte[] prefSize = new byte[1];
        prefSize[0] = (byte)sendData.Length;    //버퍼의 가장 앞부분에 이 버퍼의 길이에 대한 정보가 있는데 이것을 먼저 보낸다.
        Client.Instance.clientSocket.Send(prefSize);
        Client.Instance.clientSocket.Send(sendData);

        Debug.Log("Send Packet from Client :" + packet.mouseLinePosList.ToString());

        //for (int i = 0; i < packet.mouseLinePosList.Count; i++)
        //{
        //    print($"data[i] :: {packet.mouseLinePosList[i].ToString()}");

        //    for (int j = 0; j < packet.mouseLinePosList[i].Length; j++)
        //    {
        //        print($"packet.mouseLinePosList[i] :: {packet.mouseLinePosList[j].ToString()}");
        //    }  
        //}



        print($"packet.mouseLinePosList.Count :: {packet.mouseLinePosList.Count}");
    }

    // Update is called once per frame
    void Update()
    {
        #region 기존코드

        //기존 코드
        //if (Input.GetMouseButtonDown(0))
        //{
        //    print("한번만");

        //    SimplePacket newPacket = new SimplePacket();
        //    newPacket.mouseX = Input.mousePosition.x;
        //    newPacket.mouseY = Input.mousePosition.y;
        //    Send(newPacket);
        //}
        #endregion

        //마우스 왼쪽 클리할 때마다 패킷 클래스를 이용해서 위치정보를 서버에 전송.
        if (Input.GetMouseButtonDown(0))
        {
            print("한번만");
            newPacket = new SimplePacket();
            testLines = new List<float[]>();
        }
        //마우스를 클릭 후 계속 위치정보 서버에 전송 그리고 마우스 버튼 뗄 시에 배열자체를 보냄
        else if (Input.GetMouseButton(0))
        {
            Vector3 linePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 3f));

            //print($"마우스 포지션 x :: {Input.mousePosition.x}, 마우스 포지션 y :: {Input.mousePosition.y}");
            //print($"Client :: 가공된 라인 포지션 {linePos}");

            float[] arrLines = new float[3];
            arrLines[0] = linePos.x;
            arrLines[1] = linePos.y;
            arrLines[2] = linePos.z;

            testLines.Add(arrLines);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            newPacket.mouseLinePosList = testLines;
            //여기까지 잘들어오는 것 확인

            Send(newPacket);

            testLines.Clear();
        }
    }
}
