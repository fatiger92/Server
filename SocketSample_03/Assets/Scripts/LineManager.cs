using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LineManager : MonoBehaviour
{
    static public LineManager Instance;

    public Canvas m_canvas;
    public Material m_defaultMat;
    public Transform m_drawObjs;

    private LineRenderer m_curLine = null;
    private List<GameObject> m_lineRenderObjs;
    private int m_numClicks = 0;

    LinePacket packet = null;
    List<LineInfo> lineInfoDataList = null;

    GraphicRaycaster m_gr;
    PointerEventData m_ped;

    public System.Action<string> OnSend;

    void Awake() => Instance = this;
    void Start()
    {
        m_gr = m_canvas.GetComponent<GraphicRaycaster>();
        m_ped = new PointerEventData(null);

        m_lineRenderObjs = new List<GameObject>();
    }
    void Update() => Drawing();

    public void Drawing()
    {
        m_ped.position = Input.mousePosition;

        Vector3 linePos = Camera.main.ScreenToWorldPoint(new Vector3(m_ped.position.x, m_ped.position.y, 3f));
        
        List<RaycastResult> results = new List<RaycastResult>();
        m_gr.Raycast(m_ped, results);

        if (results.Count > 0)
        {
            if (results[0].gameObject.CompareTag("DrawingCanvas"))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    packet = new LinePacket();
                    lineInfoDataList = new List<LineInfo>();
                    //

                    GameObject temp = new GameObject();
                    temp.name = "garbage";
                    temp.transform.SetParent(m_drawObjs);

                    m_curLine = temp.AddComponent<LineRenderer>();

                    m_curLine.startWidth = 0.02f;
                    m_curLine.endWidth = 0.02f;
                    m_curLine.material = m_defaultMat;
                    m_curLine.material.SetColor("_Color", Color.black);

                    m_lineRenderObjs.Add(temp);
                    m_numClicks = 0;
                }
                else if (Input.GetMouseButton(0))
                {
                    if (m_curLine == null) return;

                    m_curLine.positionCount = m_numClicks + 1;
                    m_curLine.SetPosition(m_numClicks, linePos);

                    m_numClicks++;

                    ///
                    LineInfo lineInfo = new LineInfo();
                    lineInfo.mousePosVt = linePos;
                    lineInfoDataList.Add(lineInfo);
                    ///

                    print("마우스 이동중");
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    m_numClicks = 0;
                    m_curLine = null;

                    packet.receive_dataList = lineInfoDataList;
                    string json = JsonUtility.ToJson(packet);
                    OnSend($"&POSITION|{json}");
                    print($"LineManager :: Json ::{ lineInfoDataList.Count}");
                }
            }
            else if (!results[0].gameObject.CompareTag("DrawingCanvas"))
            {
                m_curLine = null;
                return;
            }
        }
        else return;
    }

    public void OnClickBtnUndo() => OnSend("&UNDO");

    public void Undo()
    {
        if (m_lineRenderObjs.Count == 0) return;

        Destroy(m_lineRenderObjs[m_lineRenderObjs.Count - 1]);

        m_lineRenderObjs.Remove(m_lineRenderObjs[m_lineRenderObjs.Count - 1]); 
    }

    public void Clear()
    {
        if (m_lineRenderObjs.Count == 0) return;

        foreach (var go in m_lineRenderObjs)
        {
            Destroy(go);
        }

        m_lineRenderObjs.Clear();

        OnSend("&CLEAR");
    }

    public void BuildLine(LinePacket lineData)
    {
        print("만들자!");

        GameObject temp = new GameObject();
        temp.name = "garbage";
        temp.transform.SetParent(m_drawObjs);

        LineRenderer lineRenderer = temp.AddComponent<LineRenderer>();

        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;
        lineRenderer.material = m_defaultMat;
        lineRenderer.material.SetColor("_Color", Color.black);

        lineRenderer.positionCount = lineData.receive_dataList.Count;

        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, lineData.receive_dataList[i].mousePosVt);
        }

        m_lineRenderObjs.Add(temp);
    }
}
