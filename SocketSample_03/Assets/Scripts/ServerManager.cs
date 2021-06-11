using System.Collections;
using System.Collections.Generic;
using System.Net; // 네트워크 사용해야 되니까 Net
using System.Net.Sockets; // 네트워크 소켓사용 Net.Sockets
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public class ServerManager : MonoBehaviour
{
    public InputField PortInput; // 포트 넣어 주는 인풋필드

    List<ServerClient> clients; // 연결된 클라이언트 관리
    List<ServerClient> disconnectList; // 연결해제된 클라이언트 관리

    TcpListener server; // 리스너 소켓
    bool serverStarted; // 서버가 시작되었는지 아닌지 판단 bool 변수


    public void ServerCreate() // 서버생성 메서드
    {
        clients = new List<ServerClient>(); // 연결된 클라이언트들 관리 리스트 초기화
        disconnectList = new List<ServerClient>(); // 연결해제된 클라이언트들 관리 리스트 초기화 

        try // 일단 실행
        {
            int port = PortInput.text == "" ? 35000 : int.Parse(PortInput.text); // 포트 인풋 필드에 아무것도 없으면 35000 넣음

            // TcpListener 리스너 소켓을 초기화하면서 IPAdress와 port 번호를 넣어줌.
            // IPAddress.Any는 서버자체에 존재하는 IP를 사용하겠다는 것(자기자신 0.0.0.0)
            server = new TcpListener(IPAddress.Any, port); 
            
            server.Start(); // 초기화된 리스너 소켓 시작 메서드

            StartListening(); // 리스너 초기화후 리스닝 시작 메서드.

            serverStarted = true; // 서버 시작.

            ChatManager.instance.ShowMessage($"서버가 {port}번에서 열렸습니다.");
        }
        catch (Exception e)
        {
            ChatManager.instance.ShowMessage($"Socket error: {e.Message}");
        }
    }

    void Update()
    {
        if (!serverStarted) return; // 서버가 열리면 하위 실행

        foreach (ServerClient cl in clients) //현재 연결된 녀석 전부 가져옴
        {
            // 클라이언트가 여전히 연결되있나? 리스트요소 하나하나 체크
            if (!IsConnected(cl.tcp))
            {
                cl.tcp.Close();
                disconnectList.Add(cl);
                continue;
            }
            // 클라이언트로부터 체크 메시지를 받는다
            else
            {
                NetworkStream stream = cl.tcp.GetStream(); // 네트워크에서 데이터흐름을 담당함.

                if (stream.DataAvailable)
                {
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

    bool IsConnected(TcpClient client) // 클라가 연결되었는지 안되었는지 판단하는 메서드.
    {
        try
        {
            // 조건에 충족하면 poll을 해줌
            // 조건은 매개변수로 받은 클라가 null이 아니고, 클라의 Socket client가 null이 아니고, 그 Socket client가 connected 되어있을경우
            if (client != null && client.Client != null && client.Client.Connected) 
            {
                // Poll() 메소드는 간단한 boolean 값을 반환하는데, 수행하고자 하는 행동이 블록을 걸지 않고 완료할 수 있으면, true를, 블록을 걸면 false를 반환하다.
                // int 파라미터는 Poll() 메소드가 특정 이벤트에 대해서 소켓을 주시하는 시간을 microseconds 단위로 나타낸 것이다. SelectMode 파라미터는 주시해야 할 행동을 나타낸다.

                // 여기 if문이 없으면 데이터 전송이 안됨..
                // SelectMode 클래스의 SelectRead 값을 사용할 경우, 다음과 같은 조건을 만족하면 Poll() 메소드가 true를 반환
                // Accept() 메소드가 성공, 소켓에 읽어들일 데이터가 있을 때, 연결이 종료되었을 떄
                if (client.Client.Poll(0, SelectMode.SelectRead)) 
                {
                    print($"TimeDelta :: {Time.deltaTime}, 실행이 되는 건가? {client.Client.Receive(new byte[1], SocketFlags.Peek)}");

                    // client.Client.Receive(byte[] buffer, SocketFlags socketFlags)
                    // Receive 메서드로 1 byte를 보내서 0 이외의 값이 반환된다면 !(false) == true다.
                    // 0이아니면 현재 Connect 되어 있다는 것, 따라서 return 으로 true를 반환해야만 한다.
                    // SocketFlags.Peek = 들어오는 메세지를 미리본다.

                    return !(client.Client.Receive(new byte[1], SocketFlags.Peek) == 0); // 아..false가 나올 경우도 있구나...
                }
                print("어차피 이것도 될꺼면"); // 위와 다름.

                return true; // 현재 커넥트 되어 있음.
            }
            else return false; // 커넥트 안되어 있음.
        }
        catch
        {
            return false; // 커넥트 안되어있음.
        }
    }

    void StartListening()
    {
        // BeginAcceptTcpClient :: 들어오는 연결 시도를 받아들이는 비동기 작업 시작
        // BeginAcceptTcpClient의 첫번째 파라미터는 public delegate void AsyncCallback(IAsyncResult ar);

        // 비동기로 듣기를 시작하고 다음걸 받기 위해 준비.
        // 동기로 했을경우에는 아래 구문이 진행될 동안에 다음 것이 실행되지 않고 멈추는 현상이 발생함.

        server.BeginAcceptTcpClient(AcceptTcpClient, server);
        // 이 메서드가 두번 실행되는데... ServerCreate에서 한번 AcceptTcpClient에서 한번
    }

    void AcceptTcpClient(IAsyncResult ar)
    {
        // IAsyncResult는 이 비동기 작업에 대한 상태 정보 및 사용자 정의 데이터를 저장함
        // 넘겨진 추가 정보를 가져옵니다.
        // AsyncState 속성의 자료형은 Object 형식이기 때문에 형 변환이 필요.
        TcpListener listener = (TcpListener)ar.AsyncState;

        // 클라이언트 리스트에 추가해준다.
        // EndAcceptTcpClient(IAsyncResult)는 들어오는 연결 시도를 비동기적으로 받아들이고 원격 호스트 통신을 처리할 새로운 TcpClient을 만듬
        clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        
        StartListening(); // 비동기 재귀

        // 메시지를 연결된 모두에게 보냄
        // 클라이언트에 Data를 보낼때는 
        Broadcast("%NAME", new List<ServerClient>() { clients[clients.Count - 1] });
    }

    void OnIncomingData(ServerClient client, string data) 
    {
        print($"Server :: OnIncomingData :: {data}");

        if (data.Contains("&NAME"))
        {
            client.clientName = data.Split('|')[1];

            Broadcast($"{client.clientName}이 연결되었습니다", clients);
            
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
                ChatManager.instance.ShowMessage($"쓰기 에러 : {e.Message}를 클라이언트에게 {cl.clientName}");
            }
        }
    }
}


public class ServerClient // TCP 클라이언트 만들어주는 클래스. 
{
    public TcpClient tcp; // Tcp 클라
    public string clientName; // 이름

    public ServerClient(TcpClient clientSocket) 
    {
        // 생성자로 clientSocket가 들어오면 처음에는 이름이 없으니 아래 Guest를 넣어줌
        clientName = "Guest";

        // 들어온 소켓 넣어줌
        tcp = clientSocket;
    }
}
