using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LineManager : MonoBehaviour
{
    static public LineManager Instance;

    public Canvas m_canvas;
    GraphicRaycaster m_gr;
    PointerEventData m_ped;

    public Material m_defaultMat;

    private LineRenderer m_curLine = null;
    private List<GameObject> m_lineRenderObjs;

    private int m_numClicks = 0;

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
                    GameObject temp = new GameObject();
                    temp.name = "garbage";

                    m_curLine = temp.AddComponent<LineRenderer>();

                    m_curLine.startWidth = 0.1f;
                    m_curLine.endWidth = 0.1f;
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

                    Debug.LogFormat($"라인 포지션 :: {linePos}");
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    m_numClicks = 0;
                    m_curLine = null;
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
}
