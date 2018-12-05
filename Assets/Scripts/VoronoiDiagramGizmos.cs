﻿using UnityEngine;
using Voronoi;

public class VoronoiDiagramGizmos : MonoBehaviour
{
    [Header("Gizmos")]
    [SerializeField] private bool m_showBorder = true;
    [SerializeField] private bool m_showInputPoints = true;
    [SerializeField] private bool m_showSweepLine = true;
    [SerializeField] private bool m_showBeachLine = true;
    [SerializeField] private bool m_showEdges = true;
    [SerializeField] private bool m_showVertices = true;
    [SerializeField] private bool m_showDelaunayTriangulation = true;
    private Color m_gridColor = Color.black;
    private Color m_sweepLineColor = Color.red;
    private Color m_inputPointColor = Color.red;
    private Color m_edgeColor = Color.blue;
    private Color m_vertexColor = Color.blue;
    private Color m_delaunayTriangulationColor = Color.green;
    private Color m_beachLineColor = Color.red;
    private int m_beachLineDivision = 1000;

    [Space]
    [Header("Voronoi Diagram Parameters")]
    [SerializeField] private float m_width = 20;
    [SerializeField] private float m_height = 20;
    [SerializeField] private float m_sweepLine = 10;
    [SerializeField] private Vector3[] m_inputPoints =
                                                    {
                                                        new Vector3(-6, 6, 0),
                                                        new Vector3(6, 5, 0),
                                                        new Vector3(-3, 0, 0),
                                                        new Vector3(3, -3, 0),
                                                        new Vector3(-8, -5, 0),
                                                        new Vector3(8, -9, 0)
                                                    };

    private float HalfWidth { get { return m_width / 2; } }
    private float HalfHeight { get { return m_height / 2; } }

    [Space]
    [Header("Inspector Buttons")]
    [SerializeField] private int m_inputPointNumbers = 5;
    [InspectorMethod]
    private void ResetSweepLine()
    {
        m_sweepLine = HalfHeight;
        m_voronoiDiagram = null;

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif

    }

    private bool m_playingAnimation;
    [InspectorMethod]
    private void PlaySweepLineAnimation()
    {
        if(!Application.isPlaying)
        {
            return;
        }

        m_sweepLine = HalfHeight;
        m_playingAnimation = true;
    }

    [InspectorMethod]
    public void GenerateRandomInputPoints()
    {
        ResetSweepLine();

        m_inputPoints = new Vector3[m_inputPointNumbers];
        for (int i = 0; i < m_inputPoints.Length; i++)
        {
            m_inputPoints[i] = new Vector3(Random.Range(-HalfWidth, HalfWidth), Random.Range(-HalfHeight, HalfHeight), 0);
        }

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    [InspectorMethod]
    private void GenerateVoronoiDiagram()
    {
        if (m_inputPoints == null || m_inputPoints.Length == 0)
        {
            return;
        }

        m_voronoiDiagram = new VoronoiDiagram(m_inputPoints, new VBorder(m_width, m_height));
        m_sweepLine = -HalfHeight * HalfHeight;

#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    [InspectorMethod]
    public void GenerateRandomVoronoiDiagram()
    {
        GenerateRandomInputPoints();
        GenerateVoronoiDiagram();
    }

    private void Update()
    {
        if (m_playingAnimation)
        {
            m_sweepLine -= Time.deltaTime * 2;
        }
    }

    [SerializeField] private VoronoiDiagram m_voronoiDiagram;

    private void OnDrawGizmos()
    {
        DrawBorder();
        DrawInputPoints();
        DrawSweepLine();
        DrawBeachLine();
        DrawEdges();
        DrawVertices();
        DrawDelaunayTriangulation();
    }

    private void DrawBorder()
    {
        if (!m_showBorder)
        {
            return;
        }

        Gizmos.color = m_gridColor;
        DrawThickLine(new Vector3(-HalfWidth, HalfHeight, 0), new Vector3(HalfWidth, HalfHeight, 0));
        DrawThickLine(new Vector3(HalfWidth, HalfHeight, 0), new Vector3(HalfWidth, -HalfHeight, 0));
        DrawThickLine(new Vector3(HalfWidth, -HalfHeight, 0), new Vector3(-HalfWidth, -HalfHeight, 0));
        DrawThickLine(new Vector3(-HalfWidth, -HalfHeight, 0), new Vector3(-HalfWidth, HalfHeight, 0));
    }

    private void DrawInputPoints()
    {
        if (m_inputPoints == null || m_inputPoints.Length == 0)
        {
            return;
        }

        if (!m_showInputPoints)
        {
            return;
        }

#if UNITY_EDITOR
        UnityEditor.Handles.color = m_inputPointColor;
#endif
        for (int i = 0; i < m_inputPoints.Length; i++)
        {
            DrawSolidDisc(m_inputPoints[i], 0.2f);
        }
    }

    private void DrawSweepLine()
    {
        if (!m_showSweepLine)
        {
            return;
        }

        if(m_sweepLine > HalfHeight)
        {
            m_sweepLine = HalfHeight;
        }

        Gizmos.color = m_sweepLineColor;
        DrawThickLine(new Vector3(-HalfWidth, m_sweepLine, 0), new Vector3(HalfWidth, m_sweepLine, 0));
    }

    private void DrawBeachLine()
    {
        if (!m_showBeachLine)
        {
            return;
        }

        if (m_inputPoints == null || m_inputPoints.Length == 0)
        {
            return;
        }

        m_voronoiDiagram = new VoronoiDiagram(m_inputPoints, new VBorder(m_width, m_height), m_sweepLine);

        Gizmos.color = m_beachLineColor;
        Vector3 point1;
        Vector3 point2;
        for (int x = 0; x < m_beachLineDivision; x++)
        {
            point1 = m_voronoiDiagram.GetBeachLine(GetDivisionX(x), m_sweepLine);
            point2 = m_voronoiDiagram.GetBeachLine(GetDivisionX(x + 1), m_sweepLine);
            point1.y = Mathf.Clamp(point1.y, -HalfHeight, HalfHeight);
            point2.y = Mathf.Clamp(point2.y, -HalfHeight, HalfHeight);

            if (point1.y == HalfHeight && point2.y == HalfHeight)
            {
                continue;
            }

            if (point1.y == -HalfHeight && point2.y == -HalfHeight)
            {
                continue;
            }

            DrawThickLine(point1, point2);
        }
    }

    private float GetDivisionX(int x)
    {
        return -HalfWidth + 2 * HalfWidth / m_beachLineDivision * x;
    }

    private void DrawEdges()
    {
        if (!m_showEdges)
        {
            return;
        }

        if (m_voronoiDiagram == null)
        {
            return;
        }

        if(m_voronoiDiagram.Edges == null || m_voronoiDiagram.Edges.Count == 0)
        {
            return;
        }

        Gizmos.color = m_edgeColor;
        for (int i = 0; i < m_voronoiDiagram.Edges.Count; i++)
        {
            if (m_showBeachLine)
            {
                m_voronoiDiagram.Edges[i].UpdateDirectrix(m_sweepLine);

                if (m_voronoiDiagram.Edges[i].StartPoint.x > HalfWidth || m_voronoiDiagram.Edges[i].StartPoint.x < -HalfWidth ||
                    m_voronoiDiagram.Edges[i].StartPoint.y > HalfHeight || m_voronoiDiagram.Edges[i].StartPoint.y < -HalfHeight ||
                    m_voronoiDiagram.Edges[i].EndPoint.x > HalfWidth || m_voronoiDiagram.Edges[i].EndPoint.x < -HalfWidth ||
                    m_voronoiDiagram.Edges[i].EndPoint.y > HalfHeight || m_voronoiDiagram.Edges[i].EndPoint.y < -HalfHeight)
                {
                    continue;
                }

                DrawThickLine(m_voronoiDiagram.Edges[i].StartPoint, m_voronoiDiagram.Edges[i].EndPoint);
            }
            else
            {
                DrawThickLine(m_voronoiDiagram.Edges[i].StartPoint, m_voronoiDiagram.Edges[i].VertexPoint);
            }
        }
    }

    private void DrawVertices()
    {
        if (!m_showVertices)
        {
            return;
        }

        if (m_voronoiDiagram == null)
        {
            return;
        }

        if (m_voronoiDiagram.Vertices == null || m_voronoiDiagram.Vertices.Count == 0)
        {
            return;
        }

#if UNITY_EDITOR
        UnityEditor.Handles.color = m_vertexColor;
#endif
        for (int i = 0; i < m_voronoiDiagram.Vertices.Count; i++)
        {
            DrawSolidDisc(m_voronoiDiagram.Vertices[i], 0.2f);
        }
    }


    private void DrawDelaunayTriangulation()
    {
        if(!m_showDelaunayTriangulation)
        {
            return;
        }

        if (m_voronoiDiagram == null)
        {
            return;
        }

        if (m_voronoiDiagram.DelaunayEdges == null || m_voronoiDiagram.DelaunayEdges.Count == 0)
        {
            return;
        }

        Gizmos.color = m_delaunayTriangulationColor;
        for (int i = 0; i < m_voronoiDiagram.DelaunayEdges.Count; i++)
        {
            DrawThickLine(m_voronoiDiagram.DelaunayEdges[i].LeftSite, m_voronoiDiagram.DelaunayEdges[i].RightSite);
        }
    }


    private void DrawSolidDisc(Vector3 center, float radius)
    {
#if UNITY_EDITOR
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.back, radius);
#endif
    }

    private void DrawThickLine(Vector3 from, Vector3 to, int thick = 10)
    {
        Gizmos.DrawLine(from, to);

        if(thick == 1)
        {
            return;
        }

        Camera currentCamera = Camera.current;
        if (currentCamera == null)
        {
            return;
        }

        Vector3 v1 = (to - from).normalized;
        Vector3 v2 = (currentCamera.transform.position - from).normalized;
        Vector3 normal = Vector3.Cross(v1, v2);
        for (int i = 1; i < thick; i++)
        {
            Vector3 o = 0.01f * thick * normal * ((float)i / thick);
            Gizmos.DrawLine(from + o, to + o);
            Gizmos.DrawLine(from - o, to - o);
        }
    }
}
