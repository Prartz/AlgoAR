using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphVisualizer : MonoBehaviour
{
    [Header("Prefabs and UI")]
    [SerializeField] private GameObject nodeButtonPrefab;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private Transform visualizationParent;
    [SerializeField] private GameObject visualizationPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private TextMeshProUGUI actionDescriptionText;
    [SerializeField] private GameObject informationPanelsParent;  // parent that contains all 5 panels


    [Header("Animation and Colors")]
    [SerializeField] private float animationSpeed = 1.5f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color visitedColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    [Header("Layout Settings")]
    [SerializeField] private float radius = 300f;

    private List<GraphNodeObject> nodes = new List<GraphNodeObject>();
    private List<GameObject> connections = new List<GameObject>();
    private bool isVisualizing = false;
    private string currentOperation = "";

    [System.Serializable]
    public class GraphNodeObject
    {
        public GameObject nodeObject;
        public int value;
        public Vector3 position;
        public TextMeshProUGUI valueText;
        public Image nodeImage;
        public List<GraphNodeObject> neighbors;

        public GraphNodeObject(GameObject obj, int val, Vector3 pos)
        {
            nodeObject = obj;
            value = val;
            position = pos;
            nodeImage = obj.GetComponent<Image>();
            valueText = obj.GetComponentInChildren<TextMeshProUGUI>();
            valueText.text = val.ToString();
            neighbors = new List<GraphNodeObject>();
        }
    }

    private void Start()
    {
        if (quizManager == null)
            quizManager = FindObjectOfType<QuizManager>();

        visualizationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        quizManager.quizPanel.SetActive(false);
        quizManager.blurPanel.gameObject.SetActive(false);
    }

    public void VisualizeBFS()
    {
        StartVisualization("BFS");
        StartCoroutine(BFSTraversal(nodes[0]));
    }

    public void VisualizeDFS()
    {
        StartVisualization("DFS");
        StartCoroutine(DFSTraversal(nodes[0]));
    }

    public void VisualizeDijkstra()
    {
        StartVisualization("Dijkstra");
        StartCoroutine(DijkstraTraversal(nodes[0]));
    }

    public void VisualizeAStar()
    {
        StartVisualization("AStar");
        StartCoroutine(AStarTraversal(nodes[0], nodes[nodes.Count - 1]));
    }

    public void VisualizeBellmanFord()
    {
        StartVisualization("BellmanFord");
        StartCoroutine(BellmanFordTraversal(nodes[0]));
    }

    public void StartVisualization(string operation)
    {
        if (isVisualizing) return;

        ClearGraph();
        CreateSampleGraph();

        HideActiveInformationPanel(); 
        visualizationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        quizManager.quizPanel.SetActive(false);
        quizManager.blurPanel.gameObject.SetActive(false);

        isVisualizing = true;
        currentOperation = operation;
    }

    private void CreateSampleGraph()
    {
        int nodeCount = 6;

        for (int i = 0; i < nodeCount; i++)
        {
            float angle = i * Mathf.PI * 2f / nodeCount;
            Vector3 position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            GameObject nodeObj = Instantiate(nodeButtonPrefab, visualizationParent);
            nodeObj.transform.localPosition = position;
            GraphNodeObject newNode = new GraphNodeObject(nodeObj, i + 1, position);
            nodes.Add(newNode);
        }

        ConnectNodes(nodes[0], nodes[1]);
        ConnectNodes(nodes[0], nodes[2]);
        ConnectNodes(nodes[1], nodes[3]);
        ConnectNodes(nodes[2], nodes[3]);
        ConnectNodes(nodes[2], nodes[4]);
        ConnectNodes(nodes[3], nodes[5]);
        ConnectNodes(nodes[4], nodes[5]);
    }

    private void ConnectNodes(GraphNodeObject a, GraphNodeObject b)
    {
        if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
        if (!b.neighbors.Contains(a)) b.neighbors.Add(a);

        GameObject conn = CreateConnection(a.position, b.position);
        connections.Add(conn);
    }

    private GameObject CreateConnection(Vector3 start, Vector3 end)
    {
        GameObject lineObj = Instantiate(connectionPrefab, visualizationParent);
        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();
        Vector3 midPoint = (start + end) / 2f;
        rectTransform.localPosition = midPoint;

        float distance = Vector3.Distance(start, end);
        rectTransform.sizeDelta = new Vector2(distance, 5f);

        Vector3 direction = (end - start).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        return lineObj;
    }

    private IEnumerator BFSTraversal(GraphNodeObject startNode)
    {
        Queue<GraphNodeObject> queue = new Queue<GraphNodeObject>();
        HashSet<GraphNodeObject> visited = new HashSet<GraphNodeObject>();

        queue.Enqueue(startNode);
        visited.Add(startNode);

        while (queue.Count > 0)
        {
            GraphNodeObject current = queue.Dequeue();
            HighlightNode(current);
            UpdateActionDescription($"Visiting Node {current.value}");
            yield return new WaitForSeconds(animationSpeed);

            MarkNodeAsVisited(current);

            foreach (var neighbor in current.neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);
                }
            }
        }

        StartQuizAfterTraversal();
    }

    private IEnumerator DFSTraversal(GraphNodeObject startNode)
    {
        Stack<GraphNodeObject> stack = new Stack<GraphNodeObject>();
        HashSet<GraphNodeObject> visited = new HashSet<GraphNodeObject>();

        stack.Push(startNode);

        while (stack.Count > 0)
        {
            GraphNodeObject current = stack.Pop();

            if (!visited.Contains(current))
            {
                visited.Add(current);
                HighlightNode(current);
                UpdateActionDescription($"Visiting Node {current.value}");
                yield return new WaitForSeconds(animationSpeed);
                MarkNodeAsVisited(current);

                foreach (var neighbor in current.neighbors)
                {
                    if (!visited.Contains(neighbor))
                        stack.Push(neighbor);
                }
            }
        }

        StartQuizAfterTraversal();
    }

    private IEnumerator DijkstraTraversal(GraphNodeObject startNode)
    {
        Dictionary<GraphNodeObject, float> distance = new Dictionary<GraphNodeObject, float>();
        HashSet<GraphNodeObject> visited = new HashSet<GraphNodeObject>();
        PriorityQueue<GraphNodeObject> pq = new PriorityQueue<GraphNodeObject>();

        foreach (var node in nodes)
        {
            distance[node] = Mathf.Infinity;
        }
        distance[startNode] = 0f;
        pq.Enqueue(startNode, 0f);

        while (pq.Count > 0)
        {
            var current = pq.Dequeue();
            if (visited.Contains(current)) continue;

            visited.Add(current);
            HighlightNode(current);
            UpdateActionDescription($"Visiting Node {current.value} with distance {distance[current]}");
            yield return new WaitForSeconds(animationSpeed);

            MarkNodeAsVisited(current);

            foreach (var neighbor in current.neighbors)
            {
                float edgeWeight = 1f;
                float newDist = distance[current] + edgeWeight;

                if (newDist < distance[neighbor])
                {
                    distance[neighbor] = newDist;
                    pq.Enqueue(neighbor, newDist);
                }
            }
        }

        StartQuizAfterTraversal();
    }

    private IEnumerator AStarTraversal(GraphNodeObject startNode, GraphNodeObject goalNode)
    {
        Dictionary<GraphNodeObject, float> gScore = new Dictionary<GraphNodeObject, float>();
        PriorityQueue<GraphNodeObject> openSet = new PriorityQueue<GraphNodeObject>();

        foreach (var node in nodes)
        {
            gScore[node] = Mathf.Infinity;
        }
        gScore[startNode] = 0f;

        openSet.Enqueue(startNode, Heuristic(startNode, goalNode));

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            HighlightNode(current);
            UpdateActionDescription($"Visiting Node {current.value}");
            yield return new WaitForSeconds(animationSpeed);

            if (current == goalNode)
            {
                UpdateActionDescription($"Reached Goal Node {goalNode.value}");
                yield return new WaitForSeconds(animationSpeed);
                MarkNodeAsVisited(current);

                break;
            }

            foreach (var neighbor in current.neighbors)
            {
                float tentativeGScore = gScore[current] + 1f;

                if (tentativeGScore < gScore[neighbor])
                {
                    gScore[neighbor] = tentativeGScore;
                    float fScore = tentativeGScore + Heuristic(neighbor, goalNode);
                    openSet.Enqueue(neighbor, fScore);
                }
            }
        }

        StartQuizAfterTraversal();
    }

    private IEnumerator BellmanFordTraversal(GraphNodeObject startNode)
    {
        Dictionary<GraphNodeObject, float> distance = new Dictionary<GraphNodeObject, float>();

        foreach (var node in nodes)
        {
            distance[node] = Mathf.Infinity;
        }
        distance[startNode] = 0f;

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            foreach (var node in nodes)
            {
                foreach (var neighbor in node.neighbors)
                {
                    float edgeWeight = 1f;
                    if (distance[node] + edgeWeight < distance[neighbor])
                    {
                        distance[neighbor] = distance[node] + edgeWeight;

                        // ✨ Highlight the updated neighbor node
                        HighlightNode(neighbor);
                        UpdateActionDescription($"Updated distance for Node {neighbor.value} to {distance[neighbor]}");
                        yield return new WaitForSeconds(animationSpeed * 0.6f);
                        MarkNodeAsVisited(neighbor);
                    }
                }
            }
            UpdateActionDescription($"Iteration {i + 1} completed");
            yield return new WaitForSeconds(animationSpeed);
            
        }

        UpdateActionDescription("Bellman-Ford completed!");
        yield return new WaitForSeconds(animationSpeed);
       

        StartQuizAfterTraversal();
    }

    private float Heuristic(GraphNodeObject a, GraphNodeObject b)
    {
        return Vector3.Distance(a.position, b.position);
    }

    private void HighlightNode(GraphNodeObject node)
    {
        if (node != null && node.nodeImage != null)
        {
            node.nodeImage.color = highlightColor;
        }
    }

    private void UpdateActionDescription(string message)
    {
        if (actionDescriptionText != null)
        {
            actionDescriptionText.text = message;
        }
    }

    private void ClearGraph()
    {
        foreach (var node in nodes)
            if (node.nodeObject != null)
                Destroy(node.nodeObject);

        foreach (var conn in connections)
            if (conn != null)
                Destroy(conn);

        nodes.Clear();
        connections.Clear();
    }

    private void HideActiveInformationPanel()
    {
        if (informationPanelsParent != null)
        {
            foreach (Transform child in informationPanelsParent.transform)
            {
                if (child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);  // ✨ hide the visible panel
                }
            }
        }
    }

    public void ShowInfoPanelForAlgorithm(string algorithmName)
    {
        HideActiveInformationPanel();  // First hide any visible panels

        Transform panel = informationPanelsParent.transform.Find(algorithmName + "Panel");
        if (panel != null)
            panel.gameObject.SetActive(true);
        else
            Debug.LogWarning("Information panel not found for " + algorithmName);
    }

    private void StartQuizAfterTraversal()
    {
        visualizationPanel.SetActive(false);

        quizManager.blurPanel.gameObject.SetActive(true);
        quizManager.StartQuiz(currentOperation);

        isVisualizing = false;
    }

    private void MarkNodeAsVisited(GraphNodeObject node)
    {
        if (node != null && node.nodeImage != null)
        {
            node.nodeImage.color = visitedColor;
        }
    }

    public void OnBackButtonPressed()
    {
        StopAllCoroutines();
        ClearGraph();
        visualizationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        isVisualizing = false;
    }

    public void ClearGraphExternally()
    {
        ClearGraph();
        visualizationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }



}
