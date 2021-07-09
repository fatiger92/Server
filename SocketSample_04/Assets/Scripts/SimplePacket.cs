using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class FloatList
{
    public List<float> data = new List<float>();
}

public class SimplePacket : MonoBehaviour
{
    public FloatList list = new FloatList();
    public FloatList receive_list = new FloatList();

    //public void Awake()
    //{
    //    list.data.Add(1.2f);
    //    list.data.Add(2.2f);
    //    list.data.Add(3.2f);
    //    list.data.Add(4.2f);
    //    list.data.Add(5.2f);
    //}

    //public void Start()
    //{
    //    FromByteArrayDeserialize(ToByteArraySerialize(this));

    //    foreach (float _f in receive_list.data)
    //        Debug.Log("data -> " + _f.ToString());
    //}


    public static byte[] ToByteArraySerialize(SimplePacket packet)
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();

        try
        {
            bf.Serialize(ms, packet.list);
        }
        catch (Exception e)
        {
            Debug.LogError($"Serialize error :: {e}");
        }

        ms.Close();

        return ms.ToArray();
    }

    public static SimplePacket FromByteArrayDeserialize(byte[] _byte) //byte[] bytes
    {
        MemoryStream ms = new MemoryStream(_byte);
        BinaryFormatter bf = new BinaryFormatter();

        //receive_list = (FloatList)bf.Deserialize(ms);
        SimplePacket packet = (SimplePacket)bf.Deserialize(ms);
        
        ms.Close();

        return packet;
    }

}

//public class SimplePacket
//{
//public List<float> mouseLinePosList;

//public static byte[] ToByteArraySerialize(SimplePacket packet)
//{
//    // packet의 데이터는 잘들어온다.
//    // 그럼문제는 직렬화할때..
//    MemoryStream ms = new MemoryStream();
//    BinaryFormatter bf = new BinaryFormatter();

//    try
//    {
//        bf.Serialize(ms, packet);
//    }
//    catch (Exception e)
//    {
//        Debug.LogError($"Serialize error :: {e}");
//    }

//    Debug.LogFormat($"Serialize data Count :: {packet.mouseLinePosList.Count}");

//    return ms.ToArray();
//}

//public static SimplePacket FromByteArrayDeserialize(byte[] bytes)
//{
//    MemoryStream ms = new MemoryStream(bytes);
//    BinaryFormatter bf = new BinaryFormatter();
//    SimplePacket packet = new SimplePacket();

//    packet = (SimplePacket)bf.Deserialize(ms);

//    foreach (var data in packet.mouseLinePosList)
//    {
//        Debug.LogFormat($"Serialize data :: {data}");
//    }

//    return packet;
//}
//}



