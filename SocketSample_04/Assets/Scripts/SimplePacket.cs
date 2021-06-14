using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;   //요게 바이너리 포매터임!

[Serializable]
public class SimplePacket
{
    public List<float[]> mouseLinePosList;

    #region 기존의 코드
    ////쏘는거
    //public static byte[] ToByteArray(SimplePacket packet)
    //{
    //    //스트림생성 한다.  물흘려보내기
    //    MemoryStream stream = new MemoryStream();

    //    //스트림으로 건너온 패킷을 포맷으로 바이너리 묶어준다.
    //    BinaryFormatter formatter = new BinaryFormatter();

    //    formatter.Serialize(stream, packet.mouseX);       //스트림에 담는다. 시리얼라이즈는 담는다는 뜻임.
    //    formatter.Serialize(stream, packet.mouseY);

    //    return stream.ToArray();
    //}

    ////받는거
    //public static SimplePacket FromByteArray(byte[] input)
    //{
    //    //스트림 생성
    //    MemoryStream stream = new MemoryStream(input);
    //    //바이너리 포매터로 스트림에 떠내려온 데이터를 건져낸다.
    //    BinaryFormatter formatter = new BinaryFormatter();
    //    //패킷을 생성해서
    //    SimplePacket packet = new SimplePacket();
    //    //생성한 패킷에 디이터를 디시리얼 라이즈해서 담는다.
    //    packet.mouseX = (float)formatter.Deserialize(stream);
    //    packet.mouseY = (float)formatter.Deserialize(stream);

    //    return packet;
    //}
    #endregion

    //쏘는거
    public static byte[] ToByteArray(SimplePacket packet)
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(ms, packet);
        return ms.ToArray();
    }

    //받는거
    public static SimplePacket FromByteArray(byte[] bytes)
    {
        //스트림 생성
        MemoryStream ms = new MemoryStream(bytes);
        //바이너리 포매터로 스트림에 떠내려온 데이터를 건져낸다.
       
        //Debug.LogFormat($"data :: {stream}");
        BinaryFormatter bf = new BinaryFormatter();

        //패킷을 생성해서
        SimplePacket packet = new SimplePacket();
        //생성한 패킷에 데이터를 디시리얼 라이즈해서 담는다.

        //bf.Deserialize(ms);

        //foreach (var data in bf)
        //{
        //    Debug.LogFormat($"received data :: {data}");
        //}
        
        return packet;
    }

}

