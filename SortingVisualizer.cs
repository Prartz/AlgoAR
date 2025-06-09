using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SortingVisualizer : MonoBehaviour
{
    [SerializeField] private int numberOfBars;
    [SerializeField] private float speed;
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private Transform sortingPanelsParent; 
    [SerializeField] private GameObject visualizationPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private QuizManager quizManager;
    [SerializeField] private TextMeshProUGUI sortingActionText;


    private List<GameObject> bars;
    private float[] barHeights;
    private int barCount;

    private bool isPause = false;
    private bool isSorting = false;
    private string currentAlgorithm = "";
    private Dictionary<string, GameObject> algorithmPanels;


    // For bug fixing
    GameObject temp;
    GameObject[] tempList;

    private void Start()
    {
        numberOfBars = 10;
        barCount = numberOfBars;
        speed = 0.5f;
        bars = new List<GameObject>();
        
        // Get reference to quiz manager if not set in inspector
        if (quizManager == null)
            quizManager = FindObjectOfType<QuizManager>();

        algorithmPanels = new Dictionary<string, GameObject>();

        // Make sure sortingPanelsParent is not null
        if (sortingPanelsParent == null)
        {
            Debug.LogError("sortingPanelsParent is not assigned in the inspector!");
            return;
        }

        // Ensure we have panels for all required sorting algorithms
        string[] requiredAlgorithms = new string[] { "BubbleSort", "SelectionSort", "InsertionSort", "QuickSort", "HeapSort", "MergeSort" };
        
        // First, add all existing panels
        foreach (Transform child in sortingPanelsParent)
        {
            // Fix: Normalize the algorithm name to ensure consistent casing
            string algorithmName = child.name.Replace("Panel", "");
            algorithmPanels[algorithmName] = child.gameObject;
            
            // Log panels being added for debugging
            Debug.Log($"Added panel: {child.name} with key: {algorithmName}");
        }
        
        // Check if any required algorithm panels are missing
        foreach (string algorithm in requiredAlgorithms)
        {
            if (!algorithmPanels.ContainsKey(algorithm))
            {
                Debug.LogWarning($"Missing panel for {algorithm}! Creating a placeholder panel.");
                
                // Create a placeholder panel
                GameObject placeholderPanel = new GameObject($"{algorithm}Panel");
                placeholderPanel.transform.SetParent(sortingPanelsParent);
                
                // Add a simple text component to explain this is a placeholder
                GameObject textObject = new GameObject("PlaceholderText");
                textObject.transform.SetParent(placeholderPanel.transform);
                
                TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
                text.text = $"{algorithm} Information\n\nThis is a placeholder panel. Please create a proper {algorithm}Panel in the Unity Editor.";
                text.fontSize = 24;
                text.alignment = TextAlignmentOptions.Center;
                
                RectTransform rectTransform = textObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = new Vector2(20, 20);
                rectTransform.offsetMax = new Vector2(-20, -20);
                
                // Deactivate it initially
                placeholderPanel.SetActive(false);
                
                // Add to dictionary
                algorithmPanels[algorithm] = placeholderPanel;
            }
        }
    }

    private void Update()
    {
        // No panel UI updates needed
    }

    void InitializeARBars()
    {
        StopAllCoroutines();

        if (bars != null)
        {
            for (int i = 0; i < bars.Count; i++)
                Destroy(bars[i].gameObject);
            bars.Clear();
        }

        Destroy(temp);
        bars = new List<GameObject>();
        barHeights = new float[barCount];

        GameObject barsParent = new GameObject("BarsParent");
        FollowARCamera followScript = barsParent.AddComponent<FollowARCamera>();
        followScript.followDistance = 3.5f;  // Increased from 2.0f for better framing

        // Position the bars parent with a downward offset
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraUp = Camera.main.transform.up;
        
        // Add a downward offset (negative Y direction relative to camera)
        Vector3 downwardOffset = -cameraUp * 0.1f;  // Adjust this value as needed
        
        barsParent.transform.position = cameraPosition + cameraForward * followScript.followDistance + downwardOffset;
        barsParent.transform.rotation = Quaternion.LookRotation(cameraForward);

        float totalWidth = 1.8f;
        float barWidth = 0.1f;
        float spacing = totalWidth / (barCount - 1);

        for (int i = 0; i < barCount; i++)
        {
            barHeights[i] = Random.Range(0.5f, 2f);
            Vector3 localPosition = new Vector3((i * spacing) - (totalWidth / 2), barHeights[i] / 2 - 0.5f, 0);

            GameObject bar = Instantiate(barPrefab, barsParent.transform);
            bar.transform.localPosition = localPosition;
            bar.transform.localRotation = Quaternion.identity;
            bar.transform.localScale = new Vector3(barWidth, barHeights[i], 0.1f);

            bars.Add(bar);
        }
    }

    public void VisualizeBubbleSort()
    {
        if (visualizationPanel != null)
        visualizationPanel.SetActive(true); // ✅ Activate only on click

        currentAlgorithm = "BubbleSort";
        CloseActivePanel();                   
        if (visualizationPanel != null)       
            visualizationPanel.SetActive(true);

        InitializeARBars();
        if (IsSorted(bars))
            InitializeARBars();
        isSorting = true;
        StartCoroutine(BubbleSort(bars));
    }

    public void VisualizeSelectionSort()
    {
        if (visualizationPanel != null)
        visualizationPanel.SetActive(true); // ✅ Activate only on click

        currentAlgorithm = "SelectionSort";
        CloseActivePanel();                   
        if (visualizationPanel != null)      
            visualizationPanel.SetActive(true);

        InitializeARBars();
        if (IsSorted(bars))
            InitializeARBars();
        isSorting = true;
        StartCoroutine(SelectionSort(bars));
    }

    public void VisualizeInsertionSort()
    {
        if (visualizationPanel != null)
        visualizationPanel.SetActive(true); // ✅ Activate only on click

        currentAlgorithm = "InsertionSort";
        CloseActivePanel();                    
        if (visualizationPanel != null)       
            visualizationPanel.SetActive(true);

        InitializeARBars();
        if (IsSorted(bars))
            InitializeARBars();
        isSorting = true;
        StartCoroutine(InsertionSort(bars));
    }

    public void VisualizeQuickSort()
    {
        if (visualizationPanel != null)
        visualizationPanel.SetActive(true); // ✅ Activate only on click

        currentAlgorithm = "QuickSort";
        CloseActivePanel();                   
        if (visualizationPanel != null)      
            visualizationPanel.SetActive(true);

        InitializeARBars();
        if (IsSorted(bars))
            InitializeARBars();
        isSorting = true;
        StartCoroutine(QuickSort(bars, 0, bars.Count - 1));
    }

    public void VisualizeHeapSort()
    {
        if (visualizationPanel != null)
        visualizationPanel.SetActive(true); // ✅ Activate only on click

        currentAlgorithm = "HeapSort";
        CloseActivePanel();                   
        if (visualizationPanel != null)      
            visualizationPanel.SetActive(true);

        InitializeARBars();
        if (IsSorted(bars))
            InitializeARBars();
        isSorting = true;
        StartCoroutine(HeapSort(bars));
    }

    public void VisualizeMergeSort()
    {
        if (visualizationPanel != null)
        visualizationPanel.SetActive(true);

        currentAlgorithm = "MergeSort";
        CloseActivePanel();                   
        if (visualizationPanel != null)      
            visualizationPanel.SetActive(true);

        InitializeARBars();
        if (IsSorted(bars))
            InitializeARBars();
        isSorting = true;
        StartCoroutine(MergeSort(bars, 0, bars.Count - 1));
    }

    IEnumerator BubbleSort(List<GameObject> c)
    {
        for (int i = 0; i < c.Count; i++)
        {
            for (int j = 0; j < c.Count - i - 1; j++)
            {
                yield return new WaitForSeconds(speed);

                UpdateSortingAction($"Comparing Bar {j} and Bar {j + 1}");

                if (c[j].transform.localScale.y > c[j + 1].transform.localScale.y)
                {
                    UpdateSortingAction($"Swapping Bar {j} and Bar {j + 1}");

                    temp = c[j];
                    yield return new WaitForSeconds(speed);

                    LeanTween.moveLocalX(c[j], c[j + 1].transform.localPosition.x, speed);
                    LeanTween.moveLocalZ(c[j], -1.5f, speed).setLoopPingPong(1);
                    c[j] = c[j + 1];

                    LeanTween.moveLocalX(c[j + 1], temp.transform.localPosition.x, speed);
                    LeanTween.moveLocalZ(c[j + 1], 1.5f, speed).setLoopPingPong(1);
                    c[j + 1] = temp;

                    yield return new WaitForSeconds(speed);
                }
                else
                {
                    UpdateSortingAction($"No Swap Needed for Bar {j} and Bar {j + 1}");
                }

                yield return new WaitForSeconds(speed);
            }
        }

        if (IsSorted(c))
        {
            UpdateSortingAction("Array Sorted Successfully!");
            StartCoroutine(Complete());
        }
    }
 
    // Quick Sort
    IEnumerator QuickSort(List<GameObject> c, int left, int right)
    {
        if (left < right)
        {
            int pivot = (int)(c[right].transform.localScale.y * 10);
            LeanTween.color(c[right], Color.red, 0.01f);

            UpdateSortingAction($"Choosing Pivot at Bar {right}");

            int i = left - 1;

            for (int j = left; j < right; j++)
            {
                yield return new WaitForSeconds(speed);

                UpdateSortingAction($"Comparing Bar {j} with Pivot");

                if ((int)(c[j].transform.localScale.y * 10) < pivot)
                {
                    yield return new WaitForSeconds(speed);
                    i++;
                    UpdateSortingAction($"Swapping Bar {i} with Bar {j}");

                    temp = c[i];
                    Vector3 tempPosition = c[i].transform.localPosition;

                    LeanTween.moveLocalX(c[i], c[j].transform.localPosition.x, speed);
                    LeanTween.moveZ(c[i], -1.5f, speed).setLoopPingPong(1);
                    c[i] = c[j];

                    LeanTween.moveLocalX(c[j], tempPosition.x, speed);
                    LeanTween.moveZ(c[j], 1.5f, speed).setLoopPingPong(1);
                    c[j] = temp;
                }
            }

            yield return new WaitForSeconds(speed);
            UpdateSortingAction($"Placing Pivot at correct position");

            temp = c[i + 1];
            Vector3 tP = c[i + 1].transform.localPosition;

            LeanTween.moveLocalX(c[i + 1], c[right].transform.localPosition.x, speed);
            LeanTween.moveZ(c[i + 1], -1.5f, speed).setLoopPingPong(1);
            c[i + 1] = c[right];

            LeanTween.moveLocalX(c[right], tP.x, speed);
            LeanTween.moveZ(c[right], 1.5f, speed).setLoopPingPong(1);
            c[right] = temp;

            int p = i + 1;
            yield return new WaitForSeconds(speed);

            // ✅ Fix: Recursive calls must be awaited
            yield return StartCoroutine(QuickSort(c, left, p - 1));
            yield return StartCoroutine(QuickSort(c, p + 1, right));
        }

        if (IsSorted(bars))
        {
            UpdateSortingAction("Array Sorted Successfully!");
            yield return new WaitForSeconds(speed);
            StartCoroutine(Complete());
        }
    }


    // Selection Sort
    IEnumerator SelectionSort(List<GameObject> c)
    {
        for (int i = 0; i < c.Count; i++)
        {
            int min_index = i;
            UpdateSortingAction($"Finding minimum from position {i}");

            for (int j = i + 1; j < c.Count; j++)
            {
                yield return new WaitForSeconds(speed);
                UpdateSortingAction($"Comparing Bar {j} with current minimum Bar {min_index}");

                if (c[j].transform.localScale.y < c[min_index].transform.localScale.y)
                {
                    UpdateSortingAction($"New minimum found at Bar {j}");
                    min_index = j;
                }
            }

            yield return new WaitForSeconds(speed);
            UpdateSortingAction($"Swapping minimum Bar {min_index} with Bar {i}");

            temp = c[i];

            LeanTween.moveLocalX(c[i], c[min_index].transform.localPosition.x, speed);
            LeanTween.moveLocalZ(c[i], -1.5f, speed).setLoopPingPong(1);
            c[i] = c[min_index];

            LeanTween.moveLocalX(c[min_index], temp.transform.localPosition.x, speed);
            LeanTween.moveLocalZ(c[min_index], +1.5f, speed).setLoopPingPong(1);
            c[min_index] = temp;

            yield return new WaitForSeconds(speed);
        }

        if (IsSorted(bars))
        {
            UpdateSortingAction("Array Sorted Successfully!");
            StartCoroutine(Complete());
        }
    }

    // Insertion Sort
    IEnumerator InsertionSort(List<GameObject> c)
    {
        for (int i = 1; i < c.Count; i++)
        {
            yield return new WaitForSeconds(speed);

            GameObject key = c[i];
            LeanTween.color(key, Color.red, 0.01f);
            // Move the key element up to indicate it's being compared
            LeanTween.moveLocalY(key, key.transform.localPosition.y + 0.5f, speed);

            UpdateSortingAction($"Inserting Bar {i} into the sorted part");

            int j = i - 1;

            // Store original positions for animation
            Dictionary<int, Vector3> originalPositions = new Dictionary<int, Vector3>();
            for (int k = 0; k <= i; k++)
            {
                originalPositions[k] = c[k].transform.localPosition;
            }

            // Find position for key
            while (j >= 0 && c[j].transform.localScale.y > key.transform.localScale.y)
            {
                yield return new WaitForSeconds(speed);
                UpdateSortingAction($"Moving Bar {j} to position {j+1}");
                
                // Move element to the right
                Vector3 newPos = new Vector3(originalPositions[j+1].x, c[j].transform.localPosition.y, c[j].transform.localPosition.z);
                LeanTween.moveLocal(c[j], newPos, speed);
                
                // Shift in array
                c[j + 1] = c[j];
                j--;
            }

            // Insert key at correct position
            yield return new WaitForSeconds(speed);
            UpdateSortingAction($"Placing key element at position {j+1}");
            
            // Move key to its correct position
            Vector3 keyPos = new Vector3(originalPositions[j+1].x, originalPositions[j+1].y, originalPositions[j+1].z);
            LeanTween.moveLocal(key, keyPos, speed);
            
            // Reset color
            LeanTween.color(key, Color.white, 0.01f);
            
            // Update array
            c[j + 1] = key;
        }

        yield return new WaitForSeconds(0.1f);
        if (IsSorted(c))
        {
            UpdateSortingAction("Array Sorted Successfully!");
            StartCoroutine(Complete());
        }
    }

    // Heap Sort
    IEnumerator HeapSort(List<GameObject> c)
    {
        int n = c.Count;

        for (int i = n / 2 - 1; i >= 0; i--)
        {
            yield return BuildHeap(c, n, i);
        }

        for(int i=n-1; i>=0; i--)
        {
            yield return new WaitForSeconds(speed);
            temp = c[0];
            float tempX = c[0].transform.localPosition.x;

            LeanTween.color(c[0], Color.cyan, 0.01f);
            LeanTween.moveLocalX(c[0], c[i].transform.localPosition.x, speed);
            LeanTween.moveLocalZ(c[0], -1.5f, speed).setLoopPingPong(1);
            c[0] = c[i];

            LeanTween.color(c[i], Color.white, 0.01f);
            LeanTween.moveLocalX(c[i], tempX, speed);
            LeanTween.moveLocalZ(c[i], 1.5f, speed).setLoopPingPong(1);
            c[i] = temp;

            yield return BuildHeap(c, i, 0);
        }

        if(IsSorted(bars))
        {
            yield return new WaitForSeconds(speed);
            StartCoroutine(Complete());
        }
    }   

    IEnumerator BuildHeap(List<GameObject> c, int n, int i)
    {
        int largest = i;
        int l = i * 2 + 1;
        int r = i * 2 + 2;

        UpdateSortingAction($"Heapifying at index {i}");

        LeanTween.color(c[largest], Color.red, 0.01f);

        yield return new WaitForSeconds(speed);
        if (l < n && c[l].transform.localScale.y > c[largest].transform.localScale.y)
        {
            UpdateSortingAction($"Left child at {l} is bigger");
            largest = l;
            LeanTween.color(c[largest], Color.yellow, 0.01f);
        }

        if (r < n && c[r].transform.localScale.y > c[largest].transform.localScale.y)
        {
            UpdateSortingAction($"Right child at {r} is bigger");
            largest = r;
            LeanTween.color(c[largest], Color.yellow, 0.01f);
        }

        if (largest != i)
        {
            UpdateSortingAction($"Swapping {i} with {largest}");

            temp = c[i];
            float tempX = c[i].transform.localPosition.x;

            LeanTween.moveLocalX(c[i], c[largest].transform.localPosition.x, speed);
            LeanTween.moveLocalZ(c[i], -1.5f, speed).setLoopPingPong(1);
            c[i] = c[largest];

            LeanTween.moveLocalX(c[largest], tempX, speed);
            LeanTween.moveLocalZ(c[largest], 1.5f, speed).setLoopPingPong(1);
            c[largest] = temp;

            yield return BuildHeap(c, n, largest);
        }
    }


    // // Merge Sort
    IEnumerator MergeSort(List<GameObject> c, int low, int high)
    {
        if(low < high)
        {
            yield return new WaitForSeconds(speed);
            int mid = (low + high) / 2;

            LeanTween.color(c[mid], Color.red, 0.01f);
            yield return new WaitForSeconds(speed);
            LeanTween.color(c[mid], Color.white, 0.01f);
            
            yield return MergeSort(c, low, mid);
            yield return MergeSort(c, mid+1, high);

            yield return Merge(c, low, high, mid);
        }

        if(IsSorted(bars))
        {
            yield return new WaitForSeconds(0.001f);
            StartCoroutine(Complete());
        }
    }

    IEnumerator Merge(List<GameObject> c, int low, int high, int mid)
    {
        UpdateSortingAction($"Merging from {low} to {high}");

        int leftIndex = low;
        int rightIndex = mid + 1;
        int mergeIndex = low;

        // ✅ FIX: Make tempList local to avoid overwrites in recursion
        GameObject[] tempList = new GameObject[barCount];

        while (leftIndex <= mid && rightIndex <= high)
        {
            yield return new WaitForSeconds(speed);

            UpdateSortingAction($"Comparing Bar {leftIndex} and Bar {rightIndex}");

            if (c[leftIndex].transform.localScale.y < c[rightIndex].transform.localScale.y)
            {
                tempList[mergeIndex] = c[leftIndex];
                leftIndex++;
            }
            else
            {
                tempList[mergeIndex] = c[rightIndex];
                rightIndex++;
            }
            mergeIndex++;
        }

        while (leftIndex <= mid)
        {
            tempList[mergeIndex++] = c[leftIndex++];
        }

        while (rightIndex <= high)
        {
            tempList[mergeIndex++] = c[rightIndex++];
        }

        for (int i = low; i < mergeIndex; i++)
        {
            yield return new WaitForSeconds(speed);
            LeanTween.moveLocalX(tempList[i], c[low].transform.localPosition.x + (i - low) * 0.2f, speed);
            c[i] = tempList[i];
        }
    }

    private bool IsSorted(List<GameObject> o)
    {
        for (int i = 1; i < o.Count; i++)
        {
            float front = o[i - 1].transform.localScale.y;
            float back = o[i].transform.localScale.y;

            if (front > back)
                return false;
        }
        return true;
    }

    IEnumerator Complete()
    {
        for (int i = 0; i < bars.Count; i++)
        {
            yield return new WaitForSeconds(0.03f);
            LeanTween.color(bars[i], Color.green, 0.01f);
            LeanTween.moveLocalZ(bars[i], 0, speed);
        }

        yield return new WaitForSeconds(1.5f);

        for (int i = 0; i < bars.Count; i++)
            Destroy(bars[i].gameObject);
        bars.Clear();

        GameObject barsParent = GameObject.Find("BarsParent");
        if (barsParent != null)
            Destroy(barsParent);

        isSorting = false;
        StopAllCoroutines();
        
        // Start the quiz for the current algorithm
        if (quizManager != null)
        {
            quizManager.StartQuiz(currentAlgorithm);
        }
        else
        {
            Debug.LogError("QuizManager reference is missing!");
        }
    }

    void CloseActivePanel()
    {
        if (sortingPanelsParent == null) return;

        foreach (Transform child in sortingPanelsParent)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                break;
            }
        }
    }

    public void SpeedFromSlider(float speed) => this.speed = speed;

    public void MaxBarsFromSlider(float number)
    {
        this.numberOfBars = (int)number;
        barCount = numberOfBars;
        InitializeARBars();
    }

    public void OnExitButton()
    {
        Application.Quit();
    }

    public void OnPauseButton()
    {
        isPause = !isPause;
        Time.timeScale = isPause ? 0 : 1;
    }

    // Helper method for returning to main menu
    public void ReturnToMainMenu()
    {
        // Stop ongoing processes
        StopAllCoroutines();
        isSorting = false;
        isPause = false;
        Time.timeScale = 1;

        // Clean up bars
        if (bars != null)
        {
            for (int i = 0; i < bars.Count; i++)
                Destroy(bars[i].gameObject);
            bars.Clear();
        }

        // Clean up bars parent
        GameObject barsParent = GameObject.Find("BarsParent");
        if (barsParent != null)
            Destroy(barsParent);
            
        // First deactivate visualization panel
        if (visualizationPanel != null)
        {
            visualizationPanel.SetActive(false);
        }
        
        // Then make sure the main menu panel is active
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("MainMenuPanel reference is null! Please assign it in the Inspector.");
        }
    }

    private void UpdateSortingAction(string message)
    {
        if (sortingActionText != null)
        {
            sortingActionText.text = message;
        }
    }

    public void ShowInfoPanelForAlgorithm(string algorithm)
    {
        CloseActivePanel();

        if (visualizationPanel != null)
            visualizationPanel.SetActive(false);

        // Debug info
        Debug.Log($"Showing info panel for algorithm: {algorithm}");
        Debug.Log($"Available keys in dictionary: {string.Join(", ", algorithmPanels.Keys)}");

        // Make sure the algorithm panel exists
        if (algorithmPanels.ContainsKey(algorithm))
        {
            algorithmPanels[algorithm].SetActive(true);
            Debug.Log($"Successfully activated panel for {algorithm}");
        }
        else
        {
            // Try case-insensitive search as a fallback
            string algorithmKey = null;
            foreach (var key in algorithmPanels.Keys)
            {
                if (key.Equals(algorithm, System.StringComparison.OrdinalIgnoreCase))
                {
                    algorithmKey = key;
                    break;
                }
            }

            if (algorithmKey != null)
            {
                algorithmPanels[algorithmKey].SetActive(true);
                Debug.Log($"Successfully activated panel for {algorithmKey} (case-insensitive match)");
            }
            else
            {
                Debug.LogWarning($"Panel not found for {algorithm}, creating a basic placeholder panel");
                
                // Create a placeholder panel if it doesn't exist yet
                GameObject placeholderPanel = new GameObject($"{algorithm}Panel");
                placeholderPanel.transform.SetParent(sortingPanelsParent);
                
                // Add a simple text component to explain this is a placeholder
                GameObject textObject = new GameObject("PlaceholderText");
                textObject.transform.SetParent(placeholderPanel.transform);
                
                TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
                text.text = $"{algorithm} Information\n\nThis is a placeholder panel. Please create a proper {algorithm}Panel in the Unity Editor.";
                text.fontSize = 24;
                text.alignment = TextAlignmentOptions.Center;
                
                RectTransform rectTransform = textObject.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = new Vector2(20, 20);
                rectTransform.offsetMax = new Vector2(-20, -20);
                
                // Make it active
                placeholderPanel.SetActive(true);
                
                // Add to dictionary for future reference
                algorithmPanels[algorithm] = placeholderPanel;
                
                Debug.Log($"Created and activated placeholder panel for {algorithm}");
            }
        }
    }
}