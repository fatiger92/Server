using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class MousePosData
{
    public float mousePosX;
    public float mousePosY;
    public float mousePosZ;
}

public class PositionPacket
{
    //public MousePosData receive_data = new MousePosData();
    public List<MousePosData> receive_dataList = new List<MousePosData>();

    public static string ToByteArraySerialize(PositionPacket packet)
    {
        MemoryStream ms = new MemoryStream();
        BinaryFormatter bf = new BinaryFormatter();

        try
        {
            bf.Serialize(ms, packet.receive_dataList);
        }
        catch (Exception e)
        {
            Debug.LogError($"Serialize error :: {e}");
        }

        ms.Close();

        return Convert.ToBase64String(ms.ToArray());
    }
}
