using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TreeVisualizer : MonoBehaviour
{
    [Header("Prefabs and UI")]
    [SerializeField] private GameObject nodeButtonPrefab;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private Transform visualizationParent;
    [SerializeField] private GameObject visualizationPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private Transform treePanelsParent;
    [SerializeField] private TextMeshProUGUI actionDescriptionText;


    [Header("Animation and Colors")]
    [SerializeField] private float animationSpeed = 1.5f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color visitedColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;

    [Header("Spacing")]
    [SerializeField] private float verticalSpacing = 200f;
    [SerializeField] private float horizontalSpacing = 200f;


    private List<TreeNodeObject> nodes = new List<TreeNodeObject>();
    private List<GameObject> connections = new List<GameObject>();
    private TreeNodeObject rootNode;
    private bool isVisualizing = false;
    private string currentOperation = "";

    [System.Serializable]
    public class TreeNodeObject
    {
        public GameObject nodeObject;
        public int value;
        public TreeNodeObject left;
        public TreeNodeObject right;
        public Vector3 position;
        public TextMeshProUGUI valueText;
        public Image nodeImage;
        public int depth;

        public TreeNodeObject(GameObject obj, int val, Vector3 pos, int nodeDepth = 0)
        {
            nodeObject = obj;
            value = val;
            position = pos;
            nodeImage = obj.GetComponent<Image>();
            valueText = obj.GetComponentInChildren<TextMeshProUGUI>();
            valueText.text = val.ToString();
            left = null;
            right = null;
            depth = nodeDepth;
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

    public void VisualizeInorderTraversal()
    {
        StartVisualization("InorderTraversal");
        StartCoroutine(InorderTraversal(rootNode));
    }

    public void VisualizePreorderTraversal()
    {
        StartVisualization("PreorderTraversal");
        StartCoroutine(PreorderTraversal(rootNode));
    }

    public void VisualizePostorderTraversal()
    {
        StartVisualization("PostorderTraversal");
        StartCoroutine(PostorderTraversal(rootNode));
    }

    public void VisualizeInsertNode()
    {
        StartVisualization("InsertNode");
        StartCoroutine(InsertNodeCoroutine(65)); // Example insert 65
    }

    public void VisualizeDeleteNode()
    {
        StartVisualization("DeleteNode");
        StartCoroutine(DeleteNodeCoroutine(30)); // Example delete 30
    }

    private void StartVisualization(string operation)
    {
        if (isVisualizing) return;

        // Close info panels first
        CloseActiveInfoPanels();

        // Clear tree and create a fresh one
        ClearTree();
        CreateSampleBST();

        // Show visualization
        visualizationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        quizManager.quizPanel.SetActive(false);
        quizManager.blurPanel.gameObject.SetActive(false);

        isVisualizing = true;
        currentOperation = operation;
    }

    private void CloseActiveInfoPanels()
    {
        if (treePanelsParent == null) return;

        foreach (Transform child in treePanelsParent)
        {
            if (child.gameObject.activeSelf)
                child.gameObject.SetActive(false);
        }
    }

    public void ShowInfoPanelForAlgorithm(string algorithmName)
    {
        CloseActiveInfoPanels();

        Transform panel = treePanelsParent.Find(algorithmName + "Panel");
        if (panel != null)
            panel.gameObject.SetActive(true);
        else
            Debug.LogWarning("Information panel not found for " + algorithmName);
    }

    private void CreateSampleBST()
    {
        int[] values = { 50, 30, 70, 20, 40, 60, 80 };

        GameObject rootObj = CreateNodeObject(values[0], Vector3.zero);
        rootNode = new TreeNodeObject(rootObj, values[0], rootObj.transform.localPosition);
        nodes.Add(rootNode);

        for (int i = 1; i < values.Length; i++)
        {
            InsertNodeWithoutAnimation(values[i]);
        }

        RecalculateNodePositions();
        DrawConnections();
    }

    private void ClearTree()
    {
        foreach (var node in nodes)
            if (node.nodeObject != null)
                Destroy(node.nodeObject);

        foreach (var conn in connections)
            if (conn != null)
                Destroy(conn);

        nodes.Clear();
        connections.Clear();
        rootNode = null;
    }

    private GameObject CreateNodeObject(int value, Vector3 position)
    {
        GameObject nodeObj = Instantiate(nodeButtonPrefab, visualizationParent);
        nodeObj.transform.localPosition = position;
        return nodeObj;
    }

    private void InsertNodeWithoutAnimation(int value)
    {
        if (rootNode == null) return;

        TreeNodeObject current = rootNode;
        TreeNodeObject parent = null;
        bool isLeftChild = false;

        while (current != null)
        {
            parent = current;
            if (value < current.value)
            {
                current = current.left;
                isLeftChild = true;
            }
            else
            {
                current = current.right;
                isLeftChild = false;
            }
        }

        Vector3 tempPosition = parent.position + new Vector3(isLeftChild ? -1 : 1, -1, 0);
        GameObject newNodeObj = CreateNodeObject(value, tempPosition);
        TreeNodeObject newNode = new TreeNodeObject(newNodeObj, value, tempPosition);
        nodes.Add(newNode);

        if (isLeftChild)
            parent.left = newNode;
        else
            parent.right = newNode;
    }

    private void RecalculateNodePositions()
    {
        if (rootNode == null) return;

        float startX = -(nodes.Count / 2f); // Shift starting X left based on total nodes
        PositionSubtree(rootNode, 0, ref startX);

        foreach (var node in nodes)
            node.nodeObject.transform.localPosition = node.position;
    }


    private void PositionSubtree(TreeNodeObject node, int depth, ref float x)
    {
        if (node == null) return;

        PositionSubtree(node.left, depth + 1, ref x);

        float xPos = x * horizontalSpacing;
        float yPos = 4f - depth * verticalSpacing;
        node.position = new Vector3(xPos, yPos, 0);
        node.depth = depth;
        x += 1f;

        PositionSubtree(node.right, depth + 1, ref x);
    }

    private void DrawConnections()
    {
        foreach (var conn in connections)
            Destroy(conn);
        connections.Clear();

        foreach (var node in nodes)
        {
            if (node.left != null)
                connections.Add(CreateConnection(node.position, node.left.position));
            if (node.right != null)
                connections.Add(CreateConnection(node.position, node.right.position));
        }
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

    // ============================== Traversal Animations ==============================

    private IEnumerator InorderTraversal(TreeNodeObject node)
    {
        if (node == null) yield break;

        yield return InorderTraversal(node.left);
        HighlightNode(node);
        UpdateActionDescription($"Visiting Node {node.value}");
        yield return new WaitForSeconds(animationSpeed);
        yield return InorderTraversal(node.right);

        if (node == rootNode)
            StartQuizAfterTraversal();
    }

    private IEnumerator PreorderTraversal(TreeNodeObject node)
    {
        if (node == null) yield break;

        HighlightNode(node);
        UpdateActionDescription($"Visiting Node {node.value}");
        yield return new WaitForSeconds(animationSpeed);
        yield return PreorderTraversal(node.left);
        yield return PreorderTraversal(node.right);

        if (node == rootNode)
            StartQuizAfterTraversal();
    }

    private IEnumerator PostorderTraversal(TreeNodeObject node)
    {
        if (node == null) yield break;

        yield return PostorderTraversal(node.left);
        yield return PostorderTraversal(node.right);
        UpdateActionDescription($"Visiting Node {node.value}");
        HighlightNode(node);
        
        yield return new WaitForSeconds(animationSpeed);

        if (node == rootNode)
            StartQuizAfterTraversal();
    }
   
    private void HighlightNode(TreeNodeObject node)
    {
        if (node != null && node.nodeImage != null)
        {
            node.nodeImage.color = highlightColor;
        }
    }

    private void MarkVisitedNode(TreeNodeObject node)
    {
        if (node != null && node.nodeImage != null)
        {
            node.nodeImage.color = visitedColor;
        }
    }

    private void UpdateActionDescription(string message)
    {
        if (actionDescriptionText != null)
        {
            actionDescriptionText.text = message;
        }
    }

    private IEnumerator InsertNodeCoroutine(int value)
    {
        if (rootNode == null)
        {
            yield break;
        }

        TreeNodeObject current = rootNode;
        TreeNodeObject parent = null;
        bool isLeftChild = false;

        while (current != null)
        {
            HighlightNode(current);
            UpdateActionDescription($"Searching at Node {current.value} for Insert Position");
            yield return new WaitForSeconds(animationSpeed);
            MarkVisitedNode(current);
            yield return new WaitForSeconds(animationSpeed * 0.5f);

            parent = current;
            if (value < current.value)
            {
                current = current.left;
                isLeftChild = true;
            }
            else
            {
                current = current.right;
                isLeftChild = false;
            }
        }

        Vector3 tempPosition = parent.position + new Vector3(isLeftChild ? -1 : 1, -1, 0);
        GameObject newNodeObj = CreateNodeObject(value, tempPosition);
        TreeNodeObject newNode = new TreeNodeObject(newNodeObj, value, tempPosition);
        nodes.Add(newNode);

        if (isLeftChild)
            parent.left = newNode;
        else
            parent.right = newNode;

        UpdateActionDescription($"Inserted Node {value} under Node {parent.value}");

        RecalculateNodePositions();
        DrawConnections();
        yield return new WaitForSeconds(animationSpeed);

        StartQuizAfterTraversal();
    }

    private IEnumerator DeleteNodeCoroutine(int value)
    {
        if (rootNode == null)
        {
            yield break;
        }

        TreeNodeObject current = rootNode;
        TreeNodeObject parent = null;
        bool isLeftChild = false;

        // Find the node to delete
        while (current != null && current.value != value)
        {
            HighlightNode(current);
            UpdateActionDescription($"Searching Node {current.value} for Deletion");
            yield return new WaitForSeconds(animationSpeed);
            MarkVisitedNode(current);
            yield return new WaitForSeconds(animationSpeed * 0.5f);

            parent = current;
            if (value < current.value)
            {
                current = current.left;
                isLeftChild = true;
            }
            else
            {
                current = current.right;
                isLeftChild = false;
            }
        }

        if (current == null)
        {
            UpdateActionDescription($"Node {value} not found!");
            StartQuizAfterTraversal();
            yield break;
        }

        HighlightNode(current);
        UpdateActionDescription($"Found Node {current.value}, Deleting...");
        yield return new WaitForSeconds(animationSpeed);

        // 3 Cases of Deletion
        if (current.left == null && current.right == null)
        {
            Destroy(current.nodeObject);
            nodes.Remove(current);

            if (parent != null)
            {
                if (isLeftChild)
                    parent.left = null;
                else
                    parent.right = null;
            }
            else
            {
                rootNode = null;
            }
        }
        else if (current.left == null || current.right == null)
        {
            TreeNodeObject child = (current.left != null) ? current.left : current.right;

            if (parent != null)
            {
                if (isLeftChild)
                    parent.left = child;
                else
                    parent.right = child;
            }
            else
            {
                rootNode = child;
            }

            Destroy(current.nodeObject);
            nodes.Remove(current);
        }
        else
        {
            TreeNodeObject successorParent = current;
            TreeNodeObject successor = current.right;
            bool successorIsLeftChild = false;

            while (successor.left != null)
            {
                HighlightNode(successor);
                UpdateActionDescription($"Finding Inorder Successor at Node {successor.value}");
                yield return new WaitForSeconds(animationSpeed);
                MarkVisitedNode(successor);
                yield return new WaitForSeconds(animationSpeed * 0.5f);

                successorParent = successor;
                successor = successor.left;
                successorIsLeftChild = true;
            }

            current.value = successor.value;
            current.valueText.text = successor.value.ToString();

            if (successorIsLeftChild)
                successorParent.left = successor.right;
            else
                successorParent.right = successor.right;

            Destroy(successor.nodeObject);
            nodes.Remove(successor);
        }

        UpdateActionDescription($"Deleted Node {value} Successfully");

        RecalculateNodePositions();
        DrawConnections();
        yield return new WaitForSeconds(animationSpeed);

        StartQuizAfterTraversal();
    }

    private void StartQuizAfterTraversal()
    {
        visualizationPanel.SetActive(false);

        // Show blur panel first
        quizManager.blurPanel.gameObject.SetActive(true);

        // Then start quiz
        quizManager.StartQuiz(currentOperation);

        isVisualizing = false;
    }
    public void OnBackButtonPressed()
    {
        StopAllCoroutines();
        ClearTree();
        visualizationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        isVisualizing = false;
    }

    public void ClearTreeExternally()
    {
        ClearTree();
        visualizationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }


}
