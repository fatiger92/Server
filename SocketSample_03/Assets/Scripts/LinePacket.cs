using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LineInfo
{
    public Vector3 mousePosVt;
    public Color colorR;

    public int materialIdx;
}
public class LinePacket
{
    public List<LineInfo> receive_dataList = new List<LineInfo>();
}
