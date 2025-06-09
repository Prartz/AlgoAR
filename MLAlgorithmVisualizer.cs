using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MLVisualizer : MonoBehaviour
{
    [Header("Prefabs and UI")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private Transform visualizationParent;
    [SerializeField] private GameObject visualizationPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private Transform algorithmPanelsParent;
    [SerializeField] private TextMeshProUGUI actionDescriptionText;
    [SerializeField] private Button nextAlgorithmButton;

    [Header("Animation and Colors")]
    [SerializeField] private float animationSpeed = 1.5f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color inputColor = Color.cyan;
    [SerializeField] private Color hiddenColor = Color.yellow;
    [SerializeField] private Color outputColor = Color.red;

    [Header("Node and Line Sizes")]
    [SerializeField] private float nodeSize = 50f;
    [SerializeField] private float connectionWidth = 8f;


    private List<GameObject> nodes = new List<GameObject>();
    private List<GameObject> connections = new List<GameObject>();
    private bool isVisualizing = false;
    private string currentAlgorithm = "";
    private int currentAlgorithmIndex = 0;

    private string[] algorithmOrder = { "LinearRegression", "KMeansClustering", "DecisionTree", "NeuralNetwork", "RandomForest" };

    private void Start()
    {
        if (quizManager == null)
            quizManager = FindObjectOfType<QuizManager>();

        visualizationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        quizManager.quizPanel.SetActive(false);
        quizManager.blurPanel.gameObject.SetActive(false);

        if (nextAlgorithmButton != null)
            nextAlgorithmButton.onClick.AddListener(LoadNextAlgorithm);
    }

    public void OpenAlgorithmInfoPanel(string algorithmName)
    {
        CloseActiveInfoPanels();
        currentAlgorithm = algorithmName;

        for (int i = 0; i < algorithmOrder.Length; i++)
        {
            if (algorithmOrder[i] == algorithmName)
            {
                currentAlgorithmIndex = i;
                break;
            }
        }

        Transform panel = algorithmPanelsParent.Find(algorithmName + "Panel");
        if (panel != null)
            panel.gameObject.SetActive(true);
        else
            Debug.LogWarning("Information panel not found for " + algorithmName);
    }

    public void VisualizeButtonPressed(string algorithmName)
    {
        switch (algorithmName)
        {
            case "LinearRegression": VisualizeLinearRegression(); break;
            case "KMeansClustering": VisualizeKMeansClustering(); break;
            case "DecisionTree": VisualizeDecisionTree(); break;
            case "NeuralNetwork": VisualizeNeuralNetwork(); break;
            case "RandomForest": VisualizeRandomForest(); break;
            default: Debug.LogWarning("Unknown algorithm " + algorithmName); break;
        }
    }

    private void StartVisualization(string algorithm)
    {
        if (isVisualizing) return;

        CloseActiveInfoPanels();
        ClearVisualization();

        visualizationPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        quizManager.quizPanel.SetActive(false);
        quizManager.blurPanel.gameObject.SetActive(false);

        isVisualizing = true;
        currentAlgorithm = algorithm;
    }

    private void ClearVisualization()
    {
        foreach (var node in nodes)
            if (node != null)
                Destroy(node);

        foreach (var conn in connections)
            if (conn != null)
                Destroy(conn);

        nodes.Clear();
        connections.Clear();
    }

    private void CloseActiveInfoPanels()
    {
        if (algorithmPanelsParent == null) return;

        foreach (Transform child in algorithmPanelsParent)
        {
            if (child.gameObject.activeSelf)
                child.gameObject.SetActive(false);
        }
    }

    private void StartQuizAfterVisualization()
    {
        visualizationPanel.SetActive(false);
        quizManager.blurPanel.gameObject.SetActive(true);
        quizManager.StartQuiz(currentAlgorithm);
        isVisualizing = false;
    }

    private void LoadNextAlgorithm()
    {
        quizManager.blurPanel.gameObject.SetActive(false);

        currentAlgorithmIndex++;
        if (currentAlgorithmIndex >= algorithmOrder.Length)
        {
            UpdateActionDescription("All algorithms completed! Good job!");
            mainMenuPanel.SetActive(true);
            return;
        }

        OpenAlgorithmInfoPanel(algorithmOrder[currentAlgorithmIndex]);
    }

    private void UpdateActionDescription(string message)
    {
        if (actionDescriptionText != null)
            actionDescriptionText.text = message;
    }

    private void HighlightObject(GameObject obj, Color color)
    {
        Image image = obj.GetComponent<Image>();
        if (image != null)
            image.color = color;
    }

    private GameObject CreateNode(Vector3 position, float size = 50f, Color color = default)
    {
        GameObject nodeObj = Instantiate(nodePrefab, visualizationParent);
        nodeObj.transform.localPosition = position;

        RectTransform rect = nodeObj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size, size);

        Image image = nodeObj.GetComponent<Image>();
        if (image != null)
            image.color = color != default ? color : defaultColor;

        return nodeObj;
    }

    private GameObject CreateConnection(Vector3 start, Vector3 end, float width = 5f, Color color = default)
    {
        GameObject lineObj = Instantiate(connectionPrefab, visualizationParent);
        RectTransform rectTransform = lineObj.GetComponent<RectTransform>();

        Vector3 midPoint = (start + end) / 2f;
        rectTransform.localPosition = midPoint;

        float distance = Vector3.Distance(start, end);
        rectTransform.sizeDelta = new Vector2(distance, width);

        Vector3 direction = (end - start).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        rectTransform.rotation = Quaternion.Euler(0, 0, angle);

        Image lineImage = lineObj.GetComponent<Image>();
        if (lineImage != null && color != default)
            lineImage.color = color;

        return lineObj;
    }

    public void VisualizeLinearRegression()
    {
        StartVisualization("LinearRegression");
        StartCoroutine(LinearRegressionCoroutine());
    }

    public void VisualizeKMeansClustering()
    {
        StartVisualization("KMeansClustering");
        StartCoroutine(KMeansClusteringCoroutine());
    }

    public void VisualizeDecisionTree()
    {
        StartVisualization("DecisionTree");
        StartCoroutine(DecisionTreeCoroutine());
    }

    public void VisualizeNeuralNetwork()
    {
        StartVisualization("NeuralNetwork");
        StartCoroutine(NeuralNetworkCoroutine());
    }

    public void VisualizeRandomForest()
    {
        StartVisualization("RandomForest");
        StartCoroutine(RandomForestCoroutine());
    }


    // ------------- Visualization Coroutines (put your existing ones like LinearRegressionCoroutine, etc.)

    private IEnumerator LinearRegressionCoroutine()
    {
        UpdateActionDescription("Initializing Linear Regression Model");
        yield return new WaitForSeconds(animationSpeed);

        // Create data points (x,y)
        List<Vector2> dataPoints = new List<Vector2>
        {
            new Vector2(-200, 100),
            new Vector2(-150, 50),
            new Vector2(-100, 0),
            new Vector2(-50, -20),
            new Vector2(0, -50),
            new Vector2(50, -70),
            new Vector2(100, -100),
            new Vector2(150, -120),
            new Vector2(200, -150)
        };

        // Create visual representation of data points
        List<GameObject> pointObjects = new List<GameObject>();
        foreach (var point in dataPoints)
        {
            GameObject pointObj = CreateNode(point, 20f, Color.blue);
            pointObjects.Add(pointObj);
            nodes.Add(pointObj);
            yield return new WaitForSeconds(animationSpeed * 0.2f);
        }

        UpdateActionDescription("Calculating Best Fit Line");
        yield return new WaitForSeconds(animationSpeed);

        // Draw initial guess line
        Vector3 startPoint = new Vector3(-250, 150, 0);
        Vector3 endPoint = new Vector3(250, -200, 0);
        GameObject lineObj = CreateConnection(startPoint, endPoint, 5f, Color.red);
        connections.Add(lineObj);

        // Simulate gradient descent iterations
        for (int i = 0; i < 5; i++)
        {
            UpdateActionDescription($"Gradient Descent Iteration {i+1}");
            yield return new WaitForSeconds(animationSpeed);
            
            // Update line position to simulate converging
            Destroy(lineObj);
            connections.Remove(lineObj);
            
            // Adjust line to better fit
            startPoint.y -= 20;
            endPoint.y += 5;
            
            lineObj = CreateConnection(startPoint, endPoint, 5f, Color.red);
            connections.Add(lineObj);
        }

        UpdateActionDescription("Linear Regression Complete");
        yield return new WaitForSeconds(animationSpeed);

        StartQuizAfterVisualization();
    }

    private IEnumerator KMeansClusteringCoroutine()
    {
        UpdateActionDescription("Initializing K-Means Clustering");
        yield return new WaitForSeconds(animationSpeed);

        // Create random data points
        List<Vector3> dataPoints = new List<Vector3>();
        List<GameObject> pointObjects = new List<GameObject>();
        
        // Cluster 1
        for (int i = 0; i < 8; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-200, -100), Random.Range(-100, 100), 0);
            dataPoints.Add(pos);
            GameObject point = CreateNode(pos, 20f, Color.white);
            pointObjects.Add(point);
            nodes.Add(point);
        }
        
        // Cluster 2
        for (int i = 0; i < 8; i++)
        {
            Vector3 pos = new Vector3(Random.Range(100, 200), Random.Range(-100, 100), 0);
            dataPoints.Add(pos);
            GameObject point = CreateNode(pos, 20f, Color.white);
            pointObjects.Add(point);
            nodes.Add(point);
        }
        
        // Cluster 3
        for (int i = 0; i < 8; i++)
        {
            Vector3 pos = new Vector3(Random.Range(-50, 50), Random.Range(-200, -100), 0);
            dataPoints.Add(pos);
            GameObject point = CreateNode(pos, 20f, Color.white);
            pointObjects.Add(point);
            nodes.Add(point);
        }

        yield return new WaitForSeconds(animationSpeed);
        
        // Create initial centroids
        Vector3[] centroids = new Vector3[3];
        GameObject[] centroidObjects = new GameObject[3];
        
        centroids[0] = new Vector3(-150, 0, 0);
        centroids[1] = new Vector3(150, 0, 0);
        centroids[2] = new Vector3(0, -150, 0);
        
        Color[] clusterColors = { new Color(1, 0.5f, 0.5f), new Color(0.5f, 1, 0.5f), new Color(0.5f, 0.5f, 1) };
        
        for (int i = 0; i < centroids.Length; i++)
        {
            centroidObjects[i] = CreateNode(centroids[i], 40f, Color.yellow);
            nodes.Add(centroidObjects[i]);
        }
        
        UpdateActionDescription("Initialized Centroids");
        yield return new WaitForSeconds(animationSpeed);
        
        // Run 3 iterations of k-means
        for (int iteration = 0; iteration < 3; iteration++)
        {
            UpdateActionDescription($"K-Means Iteration {iteration+1}: Assigning Points to Clusters");
            yield return new WaitForSeconds(animationSpeed);
            
            // Assign points to clusters based on nearest centroid
            int[] assignments = new int[dataPoints.Count];
            
            for (int i = 0; i < dataPoints.Count; i++)
            {
                float minDist = float.MaxValue;
                int closestCentroid = 0;
                
                for (int j = 0; j < centroids.Length; j++)
                {
                    float dist = Vector3.Distance(dataPoints[i], centroids[j]);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestCentroid = j;
                    }
                }
                
                assignments[i] = closestCentroid;
                HighlightObject(pointObjects[i], clusterColors[closestCentroid]);
                yield return new WaitForSeconds(animationSpeed * 0.1f);
            }
            
            UpdateActionDescription($"K-Means Iteration {iteration+1}: Updating Centroids");
            yield return new WaitForSeconds(animationSpeed);
            
            // Update centroids
            for (int i = 0; i < centroids.Length; i++)
            {
                Vector3 sum = Vector3.zero;
                int count = 0;
                
                for (int j = 0; j < assignments.Length; j++)
                {
                    if (assignments[j] == i)
                    {
                        sum += dataPoints[j];
                        count++;
                    }
                }
                
                if (count > 0)
                {
                    Vector3 newPos = sum / count;
                    centroids[i] = newPos;
                    centroidObjects[i].transform.localPosition = newPos;
                }
            }
            
            yield return new WaitForSeconds(animationSpeed);
        }
        
        UpdateActionDescription("K-Means Clustering Complete");
        yield return new WaitForSeconds(animationSpeed);
        
        StartQuizAfterVisualization();
    }

    private IEnumerator DecisionTreeCoroutine()
    {
        UpdateActionDescription("Building Decision Tree");
        yield return new WaitForSeconds(animationSpeed);

        // Create root node
        GameObject rootNode = CreateNode(new Vector3(0, 150, 0), 60f, highlightColor);
        nodes.Add(rootNode);
        
        GameObject textObj = new GameObject("RootText");
        textObj.transform.SetParent(rootNode.transform, false);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Age > 30?";
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 16;
        text.color = Color.black;
        
        yield return new WaitForSeconds(animationSpeed);
        
        // Create left branch (Yes)
        GameObject leftNode = CreateNode(new Vector3(-150, 50, 0), 60f);
        nodes.Add(leftNode);
        
        GameObject leftTextObj = new GameObject("LeftText");
        leftTextObj.transform.SetParent(leftNode.transform, false);
        TextMeshProUGUI leftText = leftTextObj.AddComponent<TextMeshProUGUI>();
        leftText.text = "Income > 50K?";
        leftText.alignment = TextAlignmentOptions.Center;
        leftText.fontSize = 16;
        leftText.color = Color.black;
        
        // Create connection to left node
        GameObject leftConn = CreateConnection(rootNode.transform.localPosition, leftNode.transform.localPosition);
        connections.Add(leftConn);
        
        // Add "Yes" label to left connection
        GameObject leftConnTextObj = new GameObject("LeftConnText");
        leftConnTextObj.transform.SetParent(visualizationParent, false);
        TextMeshProUGUI leftConnText = leftConnTextObj.AddComponent<TextMeshProUGUI>();
        leftConnText.text = "Yes";
        leftConnText.alignment = TextAlignmentOptions.Center;
        leftConnText.fontSize = 16;
        leftConnText.color = Color.white;
        leftConnTextObj.transform.localPosition = (rootNode.transform.localPosition + leftNode.transform.localPosition) / 2 + new Vector3(-20, 0, 0);
        
        yield return new WaitForSeconds(animationSpeed);
        
        // Create right branch (No)
        GameObject rightNode = CreateNode(new Vector3(150, 50, 0), 60f);
        nodes.Add(rightNode);
        
        GameObject rightTextObj = new GameObject("RightText");
        rightTextObj.transform.SetParent(rightNode.transform, false);
        TextMeshProUGUI rightText = rightTextObj.AddComponent<TextMeshProUGUI>();
        rightText.text = "Education > 12yrs?";
        rightText.alignment = TextAlignmentOptions.Center;
        rightText.fontSize = 16;
        rightText.color = Color.black;
        
        // Create connection to right node
        GameObject rightConn = CreateConnection(rootNode.transform.localPosition, rightNode.transform.localPosition);
        connections.Add(rightConn);
        
        // Add "No" label to right connection
        GameObject rightConnTextObj = new GameObject("RightConnText");
        rightConnTextObj.transform.SetParent(visualizationParent, false);
        TextMeshProUGUI rightConnText = rightConnTextObj.AddComponent<TextMeshProUGUI>();
        rightConnText.text = "No";
        rightConnText.alignment = TextAlignmentOptions.Center;
        rightConnText.fontSize = 16;
        rightConnText.color = Color.white;
        rightConnTextObj.transform.localPosition = (rootNode.transform.localPosition + rightNode.transform.localPosition) / 2 + new Vector3(20, 0, 0);
        
        yield return new WaitForSeconds(animationSpeed);
        
        // Add leaf nodes for left branch
        GameObject leftYesNode = CreateNode(new Vector3(-200, -50, 0), 60f, Color.green);
        nodes.Add(leftYesNode);
        
        GameObject leftYesTextObj = new GameObject("LeftYesText");
        leftYesTextObj.transform.SetParent(leftYesNode.transform, false);
        TextMeshProUGUI leftYesText = leftYesTextObj.AddComponent<TextMeshProUGUI>();
        leftYesText.text = "Class A";
        leftYesText.alignment = TextAlignmentOptions.Center;
        leftYesText.fontSize = 16;
        leftYesText.color = Color.black;
        
        GameObject leftNoNode = CreateNode(new Vector3(-100, -50, 0), 60f, Color.red);
        nodes.Add(leftNoNode);
        
        GameObject leftNoTextObj = new GameObject("LeftNoText");
        leftNoTextObj.transform.SetParent(leftNoNode.transform, false);
        TextMeshProUGUI leftNoText = leftNoTextObj.AddComponent<TextMeshProUGUI>();
        leftNoText.text = "Class B";
        leftNoText.alignment = TextAlignmentOptions.Center;
        leftNoText.fontSize = 16;
        leftNoText.color = Color.black;
        
        // Create connections
        GameObject leftYesConn = CreateConnection(leftNode.transform.localPosition, leftYesNode.transform.localPosition);
        connections.Add(leftYesConn);
        
        GameObject leftNoConn = CreateConnection(leftNode.transform.localPosition, leftNoNode.transform.localPosition);
        connections.Add(leftNoConn);
        
        // Add "Yes/No" labels
        GameObject leftYesConnTextObj = new GameObject("LeftYesConnText");
        leftYesConnTextObj.transform.SetParent(visualizationParent, false);
        TextMeshProUGUI leftYesConnText = leftYesConnTextObj.AddComponent<TextMeshProUGUI>();
        leftYesConnText.text = "Yes";
        leftYesConnText.alignment = TextAlignmentOptions.Center;
        leftYesConnText.fontSize = 16;
        leftYesConnText.color = Color.white;
        leftYesConnTextObj.transform.localPosition = (leftNode.transform.localPosition + leftYesNode.transform.localPosition) / 2 + new Vector3(-20, 0, 0);
        
        GameObject leftNoConnTextObj = new GameObject("LeftNoConnText");
        leftNoConnTextObj.transform.SetParent(visualizationParent, false);
        TextMeshProUGUI leftNoConnText = leftNoConnTextObj.AddComponent<TextMeshProUGUI>();
        leftNoConnText.text = "No";
        leftNoConnText.alignment = TextAlignmentOptions.Center;
        leftNoConnText.fontSize = 16;
        leftNoConnText.color = Color.white;
        leftNoConnTextObj.transform.localPosition = (leftNode.transform.localPosition + leftNoNode.transform.localPosition) / 2 + new Vector3(20, 0, 0);
        
        yield return new WaitForSeconds(animationSpeed);
        
        // Add leaf nodes for right branch
        GameObject rightYesNode = CreateNode(new Vector3(100, -50, 0), 60f, Color.green);
        nodes.Add(rightYesNode);
        
        GameObject rightYesTextObj = new GameObject("RightYesText");
        rightYesTextObj.transform.SetParent(rightYesNode.transform, false);
        TextMeshProUGUI rightYesText = rightYesTextObj.AddComponent<TextMeshProUGUI>();
        rightYesText.text = "Class A";
        rightYesText.alignment = TextAlignmentOptions.Center;
        rightYesText.fontSize = 16;
        rightYesText.color = Color.black;
        
        GameObject rightNoNode = CreateNode(new Vector3(200, -50, 0), 60f, Color.red);
        nodes.Add(rightNoNode);
        
        GameObject rightNoTextObj = new GameObject("RightNoText");
        rightNoTextObj.transform.SetParent(rightNoNode.transform, false);
        TextMeshProUGUI rightNoText = rightNoTextObj.AddComponent<TextMeshProUGUI>();
        rightNoText.text = "Class B";
        rightNoText.alignment = TextAlignmentOptions.Center;
        rightNoText.fontSize = 16;
        rightNoText.color = Color.black;
        
        // Create connections
        GameObject rightYesConn = CreateConnection(rightNode.transform.localPosition, rightYesNode.transform.localPosition);
        connections.Add(rightYesConn);
        
        GameObject rightNoConn = CreateConnection(rightNode.transform.localPosition, rightNoNode.transform.localPosition);
        connections.Add(rightNoConn);
        
        // Add "Yes/No" labels
        GameObject rightYesConnTextObj = new GameObject("RightYesConnText");
        rightYesConnTextObj.transform.SetParent(visualizationParent, false);
        TextMeshProUGUI rightYesConnText = rightYesConnTextObj.AddComponent<TextMeshProUGUI>();
        rightYesConnText.text = "Yes";
        rightYesConnText.alignment = TextAlignmentOptions.Center;
        rightYesConnText.fontSize = 16;
        rightYesConnText.color = Color.white;
        rightYesConnTextObj.transform.localPosition = (rightNode.transform.localPosition + rightYesNode.transform.localPosition) / 2 + new Vector3(-20, 0, 0);
        
        GameObject rightNoConnTextObj = new GameObject("RightNoConnText");
        rightNoConnTextObj.transform.SetParent(visualizationParent, false);
        TextMeshProUGUI rightNoConnText = rightNoConnTextObj.AddComponent<TextMeshProUGUI>();
        rightNoConnText.text = "No";
        rightNoConnText.alignment = TextAlignmentOptions.Center;
        rightNoConnText.fontSize = 16;
        rightNoConnText.color = Color.white;
        rightNoConnTextObj.transform.localPosition = (rightNode.transform.localPosition + rightNoNode.transform.localPosition) / 2 + new Vector3(20, 0, 0);
        
        // Show animation of decision path
        UpdateActionDescription("Decision Path Example: Age=25, Education=16yrs");
        yield return new WaitForSeconds(animationSpeed);
        
        HighlightObject(rootNode, Color.yellow);
        yield return new WaitForSeconds(animationSpeed);
        
        HighlightObject(rightNode, Color.yellow);
        HighlightObject(rightConn.gameObject, Color.yellow);
        yield return new WaitForSeconds(animationSpeed);
        
        HighlightObject(rightYesNode, Color.yellow);
        HighlightObject(rightYesConn.gameObject, Color.yellow);
        yield return new WaitForSeconds(animationSpeed);
        
        UpdateActionDescription("Decision Tree Classification Complete");
        yield return new WaitForSeconds(animationSpeed);
        
        StartQuizAfterVisualization();
    }

    private IEnumerator NeuralNetworkCoroutine()
    {
        UpdateActionDescription("Building Neural Network");
        yield return new WaitForSeconds(animationSpeed);

        // Create input layer (3 nodes)
        List<GameObject> inputLayer = new List<GameObject>();
        for (int i = 0; i < 3; i++)
        {
            GameObject node = CreateNode(new Vector3(-200, 100 - i * 100, 0), 40f, inputColor);
            nodes.Add(node);
            inputLayer.Add(node);
        }

        // Create hidden layer (4 nodes)
        List<GameObject> hiddenLayer = new List<GameObject>();
        for (int i = 0; i < 4; i++)
        {
            GameObject node = CreateNode(new Vector3(0, 150 - i * 100, 0), 40f, hiddenColor);
            nodes.Add(node);
            hiddenLayer.Add(node);
        }

        // Create output layer (2 nodes)
        List<GameObject> outputLayer = new List<GameObject>();
        for (int i = 0; i < 2; i++)
        {
            GameObject node = CreateNode(new Vector3(200, 50 - i * 100, 0), 40f, outputColor);
            nodes.Add(node);
            outputLayer.Add(node);
        }

        yield return new WaitForSeconds(animationSpeed);

        UpdateActionDescription("Creating Connections");
        yield return new WaitForSeconds(animationSpeed);

        // Connect input to hidden layer
        foreach (var inputNode in inputLayer)
        {
            foreach (var hiddenNode in hiddenLayer)
            {
                GameObject conn = CreateConnection(inputNode.transform.localPosition, hiddenNode.transform.localPosition, 2f, new Color(0, 0.8f, 1f, 0.5f));
                connections.Add(conn);
                yield return new WaitForSeconds(0.05f);
            }
        }

        // Connect hidden to output layer
        foreach (var hiddenNode in hiddenLayer)
        {
            foreach (var outputNode in outputLayer)
            {
                GameObject conn = CreateConnection(hiddenNode.transform.localPosition, outputNode.transform.localPosition, 2f, new Color(0, 0.8f, 1f, 0.5f));
                connections.Add(conn);
                yield return new WaitForSeconds(0.05f);
            }
        }

        yield return new WaitForSeconds(animationSpeed);

        UpdateActionDescription("Forward Propagation");
        yield return new WaitForSeconds(animationSpeed);

        // Highlight input layer
        foreach (var inputNode in inputLayer)
        {
            HighlightObject(inputNode, Color.cyan);
            yield return new WaitForSeconds(animationSpeed * 0.3f);
        }

        // Highlight connections input -> hidden
        foreach (var inputNode in inputLayer)
        {
            foreach (var hiddenNode in hiddenLayer)
            {
                foreach (var conn in connections)
                {
                    if (Vector3.Distance(conn.transform.localPosition, (inputNode.transform.localPosition + hiddenNode.transform.localPosition) / 2) < 5f)
                    {
                        HighlightObject(conn, Color.cyan);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(animationSpeed * 0.2f);
        }

        // Highlight hidden layer
        foreach (var hiddenNode in hiddenLayer)
        {
            HighlightObject(hiddenNode, Color.yellow);
            yield return new WaitForSeconds(animationSpeed * 0.3f);
        }

        // Highlight connections hidden -> output
        foreach (var hiddenNode in hiddenLayer)
        {
            foreach (var outputNode in outputLayer)
            {
                foreach (var conn in connections)
                {
                    if (Vector3.Distance(conn.transform.localPosition, (hiddenNode.transform.localPosition + outputNode.transform.localPosition) / 2) < 5f)
                    {
                        HighlightObject(conn, Color.yellow);
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(animationSpeed * 0.2f);
        }

        // Highlight output layer
        foreach (var outputNode in outputLayer)
        {
            HighlightObject(outputNode, Color.red);
            yield return new WaitForSeconds(animationSpeed * 0.3f);
        }

        yield return new WaitForSeconds(animationSpeed);

        UpdateActionDescription("Backpropagation");
        yield return new WaitForSeconds(animationSpeed);

        UpdateActionDescription("Neural Network Training Complete!");
        yield return new WaitForSeconds(animationSpeed);

        StartQuizAfterVisualization();
    }

    private IEnumerator RandomForestCoroutine()
    {
        UpdateActionDescription("Building Random Forest");
        yield return new WaitForSeconds(animationSpeed);

        int treeCount = 3; // Number of trees
        float startX = -200f;
        float spacingX = 200f;

        for (int t = 0; t < treeCount; t++)
        {
            // Root
            GameObject rootNode = CreateNode(new Vector3(startX + t * spacingX, 100, 0), 50f, highlightColor);
            nodes.Add(rootNode);

            // Left and Right
            GameObject leftNode = CreateNode(new Vector3(startX + t * spacingX - 50, 0, 0), 40f, Color.green);
            GameObject rightNode = CreateNode(new Vector3(startX + t * spacingX + 50, 0, 0), 40f, Color.red);
            nodes.Add(leftNode);
            nodes.Add(rightNode);

            connections.Add(CreateConnection(rootNode.transform.localPosition, leftNode.transform.localPosition, 4f, Color.white));
            connections.Add(CreateConnection(rootNode.transform.localPosition, rightNode.transform.localPosition, 4f, Color.white));

            yield return new WaitForSeconds(animationSpeed * 0.5f);
        }

        UpdateActionDescription("Aggregating Trees...");
        yield return new WaitForSeconds(animationSpeed);

        // Create ensemble vote node
        GameObject voteNode = CreateNode(new Vector3(0, -150, 0), 60f, Color.magenta);
        nodes.Add(voteNode);

        // Connect last tree nodes to vote node
        foreach (var node in nodes)
        {
            if (node.transform.localPosition.y == 0)
            {
                connections.Add(CreateConnection(node.transform.localPosition, voteNode.transform.localPosition, 3f, Color.magenta));
                yield return new WaitForSeconds(animationSpeed * 0.2f);
            }
        }

        UpdateActionDescription("Random Forest Prediction Ready!");
        yield return new WaitForSeconds(animationSpeed);

        StartQuizAfterVisualization();
    } 

    public void OnBackButtonPressed()
    {
        StopAllCoroutines();
        ClearVisualization();
        visualizationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
        isVisualizing = false;
    }

    public void ClearVisualizationExternally()
    {
        ClearVisualization();
        visualizationPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }



}
