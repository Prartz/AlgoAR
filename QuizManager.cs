using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Panel")]
    
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject nextAlgorithmButton;
    [SerializeField] public GameObject quizPanel;
    [SerializeField] public Image blurPanel;

    
    [Header("Answer Buttons")]
    [SerializeField] private Button answerButtonA;
    [SerializeField] private Button answerButtonB;
    [SerializeField] private Button answerButtonC;
    [SerializeField] private Button answerButtonD;
    [SerializeField] private TMP_Text answerTextA;
    [SerializeField] private TMP_Text answerTextB;
    [SerializeField] private TMP_Text answerTextC;
    [SerializeField] private TMP_Text answerTextD;
    [SerializeField] private Button submitButton;
    
    [Header("Stats")]
    [SerializeField] private TMP_Text correctAnswersText;
    [SerializeField] private TMP_Text wrongAnswersText;
    [SerializeField] private TMP_Text timeText;
    
    [Header("Algorithm Info")]
    [SerializeField] private string currentAlgorithm;
    [SerializeField] private Button nextButton;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip correctSound;
    [SerializeField] private AudioClip wrongSound;
    [SerializeField] private AudioClip noAnswerSound;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource correctAudioSource;
    [SerializeField] private AudioSource wrongAudioSource;
    [SerializeField] private AudioSource noAnswerAudioSource;


    
    private string currentTopic;

    // Firebase references
    private DatabaseReference dbReference;
    private FirebaseAuth auth;
    private FirebaseUser user;
    
    // Quiz variables
    private List<QuizQuestion> questions;
    private int currentQuestionIndex = 0;
    private int correctAnswers = 0;
    private int wrongAnswers = 0;
    private float quizTimer = 0f;
    private bool isQuizActive = false;
    private string selectedAnswer;
    private string currentCorrectAnswer;
    
    // Algorithm sequence
    private string[] sortingAlgorithmSequence = { "BubbleSort", "SelectionSort", "InsertionSort", "QuickSort", "MergeSort", "HeapSort"};
    private string[] treeOperationSequence = { "InorderTraversal", "PreorderTraversal", "PostorderTraversal", "InsertNode", "DeleteNode" };
    private string[] graphAlgorithmSequence = { "Dijkstra", "BFS", "DFS", "AStar", "BellmanFord" };
    private string[] mlAlgorithmSequence = { "LinearRegression", "KMeansClustering", "DecisionTree", "NeuralNetwork", "RandomForest" };


    private int currentAlgorithmIndex = 0;
    
    [System.Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] options;
        public string correctAnswer;
    }
    
    private void Start()
    {
        InitializeFirebase();
        
        // Listeners for quiz question options
        answerButtonA.onClick.AddListener(() => SelectAnswer("A"));
        answerButtonB.onClick.AddListener(() => SelectAnswer("B"));
        answerButtonC.onClick.AddListener(() => SelectAnswer("C"));
        answerButtonD.onClick.AddListener(() => SelectAnswer("D"));
        submitButton.onClick.AddListener(SubmitAnswer);

        // ✅ For next question inside quiz
        nextButton.onClick.AddListener(NextQuestion);

        // ✅ For next algorithm after completing quiz
        nextAlgorithmButton.GetComponent<Button>().onClick.AddListener(NextAlgorithm);

        // Panels setup
        quizPanel.SetActive(false);
        nextAlgorithmButton.SetActive(false);
        blurPanel.gameObject.SetActive(false);

        answerButtonA.gameObject.SetActive(true);
        answerButtonB.gameObject.SetActive(true);
        answerButtonC.gameObject.SetActive(true);
        answerButtonD.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (isQuizActive)
        {
            quizTimer += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }
    
    void InitializeFirebase()
    {
        // Initialize Firebase App with database URL
        FirebaseApp.DefaultInstance.Options.DatabaseUrl = 
            new Uri("https://algoar-app-default-rtdb.asia-southeast1.firebasedatabase.app/");
        
        // Now initialize Auth and Database
        auth = FirebaseAuth.DefaultInstance;
        user = auth.CurrentUser;
        
        // Get database reference
        FirebaseDatabase database = FirebaseDatabase.GetInstance("https://algoar-app-default-rtdb.asia-southeast1.firebasedatabase.app/");
        dbReference = database.RootReference;
    }


    
    public void StartQuiz(string topic)
    {
        SetAlgorithmCategory(topic); // use this for both sorting and tree
        currentAlgorithm = topic;
        LoadQuestions(topic);

        quizPanel.SetActive(true);
        blurPanel.gameObject.SetActive(false);
        nextAlgorithmButton.SetActive(false);

        currentQuestionIndex = 0;
        correctAnswers = 0;
        wrongAnswers = 0;
        quizTimer = 0f;
        isQuizActive = true;

        scoreText.text = "Score: 0";
        correctAnswersText.text = "0";
        wrongAnswersText.text = "0";

        DisplayQuestion(0);
    }

    
    void LoadQuestions(string topic)
    {
        // Here we're hard-coding questions, but you could load them from a JSON file
        // or from Firebase for more flexibility
        questions = new List<QuizQuestion>();
        
        if (topic == "BubbleSort")
        {
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of Bubble Sort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "Which statement is true about Bubble Sort?",
                options = new string[] { "It's the fastest sorting algorithm", "It's stable", "It has O(1) space complexity", "All of the above" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "How many swaps are required to sort [5,1,4,2,8] using Bubble Sort?",
                options = new string[] { "4", "5", "6", "7" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "Bubble Sort is also known as:",
                options = new string[] { "Exchange sort", "Sinking sort", "Both A and B", "None of the above" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the best case time complexity of Bubble Sort?",
                options = new string[] { "O(n²)", "O(n log n)", "O(n)", "O(1)" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "How many passes are needed to sort n elements using Bubble Sort in the worst case?",
                options = new string[] { "n", "n-1", "n²", "log n" },
                correctAnswer = "B"
            });
        }
        else if (topic == "SelectionSort")
        {
            // Selection Sort questions
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of Selection Sort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "C"
            });
            
            // Add more Selection Sort questions...
            questions.Add(new QuizQuestion {
                question = "What is the space complexity of Selection Sort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "D"
            });
            
            questions.Add(new QuizQuestion {
                question = "How many swaps are performed in Selection Sort for n elements?",
                options = new string[] { "n", "n-1", "n²", "0" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "Is Selection Sort stable?",
                options = new string[] { "Yes", "No", "Depends on implementation", "None of the above" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is Selection Sort good for?",
                options = new string[] { "Large datasets", "Almost sorted data", "Minimizing write operations", "All of the above" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "How does Selection Sort find the position for an element?",
                options = new string[] { "Binary search", "Linear search for minimum", "Heap data structure", "Hash table" },
                correctAnswer = "B"
            });
        }
        
        // Add questions for other algorithms similarly
        else if (topic == "InsertionSort")
        {
            // Insertion Sort questions
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of Insertion Sort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the best case time complexity of Insertion Sort?",
                options = new string[] { "O(n²)", "O(n log n)", "O(n)", "O(1)" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "Is Insertion Sort stable?",
                options = new string[] { "Yes", "No", "Depends on implementation", "None of the above" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "Insertion Sort is efficient for:",
                options = new string[] { "Large random arrays", "Small arrays", "Nearly sorted arrays", "Both B and C" },
                correctAnswer = "D"
            });
            
            questions.Add(new QuizQuestion {
                question = "Which sorting algorithm is similar to how people sort playing cards?",
                options = new string[] { "Bubble Sort", "Insertion Sort", "Selection Sort", "Quick Sort" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the space complexity of Insertion Sort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "D"
            });
        }
        else if (topic == "QuickSort")
        {
            // Quick Sort questions
            questions.Add(new QuizQuestion {
                question = "What is the average time complexity of QuickSort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the worst case time complexity of QuickSort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(n³)" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "What causes QuickSort's worst-case performance?",
                options = new string[] { "Random data", "Already sorted data", "Many duplicate elements", "Choosing first/last element as pivot" },
                correctAnswer = "D"
            });
            
            questions.Add(new QuizQuestion {
                question = "Is QuickSort stable?",
                options = new string[] { "Yes", "No", "Depends on implementation", "None of the above" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the divide-and-conquer step in QuickSort?",
                options = new string[] { "Merging", "Partitioning", "Heapifying", "Insertion" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the typical space complexity of QuickSort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(log n)" },
                correctAnswer = "D"
            });
        }
        else if (topic == "HeapSort")
        {
            // Heap Sort questions
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of HeapSort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(log n)" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What data structure is used in HeapSort?",
                options = new string[] { "Binary Search Tree", "Graph", "Binary Heap", "Hash Table" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "Is HeapSort stable?",
                options = new string[] { "Yes", "No", "Depends on implementation", "None of the above" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the space complexity of HeapSort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "D"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the first step in HeapSort?",
                options = new string[] { "Building the heap", "Sorting", "Partitioning", "Merging" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "How many phases does HeapSort consist of?",
                options = new string[] { "1", "2", "3", "4" },
                correctAnswer = "B"
            });
        }
        else if (topic == "MergeSort")
        {
            // Merge Sort questions
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of MergeSort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the space complexity of MergeSort?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(1)" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "Is MergeSort stable?",
                options = new string[] { "Yes", "No", "Depends on implementation", "None of the above" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "Which strategy does MergeSort use?",
                options = new string[] { "Greedy", "Divide and Conquer", "Dynamic Programming", "Backtracking" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the main disadvantage of MergeSort?",
                options = new string[] { "Slow speed", "High time complexity", "Extra space requirement", "Complex implementation" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "MergeSort works well for which data structure?",
                options = new string[] { "Arrays only", "Linked Lists only", "Both Arrays and Linked Lists", "None of the above" },
                correctAnswer = "C"
            });
        }
        else if (topic == "InorderTraversal")
        {
            questions.Add(new QuizQuestion {
                question = "In a binary search tree, what order does inorder traversal visit nodes?",
                options = new string[] { "Root, Left, Right", "Left, Root, Right", "Left, Right, Root", "Right, Root, Left" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "The inorder traversal of a binary search tree gives:",
                options = new string[] { "Nodes in descending order", "Nodes in ascending order", "Nodes in level order", "Nodes in random order" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the inorder traversal of the following tree: 50(30(20,40),70(60,80))?",
                options = new string[] { "50,30,20,40,70,60,80", "20,30,40,50,60,70,80", "20,40,30,60,80,70,50", "50,20,30,40,60,70,80" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of inorder traversal?",
                options = new string[] { "O(1)", "O(log n)", "O(n)", "O(n²)" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "Which data structure is typically used for non-recursive implementation of inorder traversal?",
                options = new string[] { "Queue", "Stack", "Linked List", "Array" },
                correctAnswer = "B"
            });

            questions.Add(new QuizQuestion {
                question = "What problem can be solved using inorder traversal?",
                options = new string[] { "Finding the height of a tree", "Checking if a tree is balanced", "Creating a sorted list from a BST", "Counting leaf nodes" },
                correctAnswer = "C"
            });
        }
        else if (topic == "PreorderTraversal")
        {
            questions.Add(new QuizQuestion {
                question = "What is the order of nodes visited in preorder traversal?",
                options = new string[] { "Root, Left, Right", "Left, Root, Right", "Left, Right, Root", "Right, Root, Left" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the preorder traversal of the following tree: 50(30(20,40),70(60,80))?",
                options = new string[] { "50,30,20,40,70,60,80", "20,30,40,50,60,70,80", "20,40,30,60,80,70,50", "50,20,30,40,60,70,80" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "What can preorder traversal be used for?",
                options = new string[] { "Copying a tree", "Sorting values in a tree", "Finding the minimum value", "Checking if a tree is complete" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "If you have the preorder traversal result of a tree, can you uniquely reconstruct the tree?",
                options = new string[] { "Yes, always", "No, never", "Yes, if it's a binary search tree", "Yes, if you also have the inorder traversal" },
                correctAnswer = "D"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the space complexity of recursive preorder traversal?",
                options = new string[] { "O(1)", "O(log n) average, O(n) worst", "O(n)", "O(n²)" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "Preorder traversal is also known as:",
                options = new string[] { "Depth-first traversal", "Breadth-first traversal", "Level-order traversal", "None of the above" },
                correctAnswer = "A"
            });
        }
        else if (topic == "PostorderTraversal")
        {
            questions.Add(new QuizQuestion {
                question = "What is the order of nodes visited in postorder traversal?",
                options = new string[] { "Root, Left, Right", "Left, Root, Right", "Left, Right, Root", "Right, Root, Left" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the postorder traversal of the following tree: 50(30(20,40),70(60,80))?",
                options = new string[] { "50,30,20,40,70,60,80", "20,30,40,50,60,70,80", "20,40,30,60,80,70,50", "50,20,30,40,60,70,80" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "In which scenario is postorder traversal particularly useful?",
                options = new string[] { "Finding the tree height", "Deleting a tree", "Searching for a value", "Balancing a tree" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of postorder traversal?",
                options = new string[] { "O(1)", "O(log n)", "O(n)", "O(n²)" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "Postorder traversal is used in calculating:",
                options = new string[] { "Size of a tree", "Height of a tree", "Both A and B", "None of the above" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "Which traversal visits the root node last?",
                options = new string[] { "Inorder", "Preorder", "Postorder", "Level-order" },
                correctAnswer = "C"
            });
        }
        else if (topic == "InsertNode")
        {
            questions.Add(new QuizQuestion {
                question = "In a binary search tree, where should a new node with value less than the root be inserted?",
                options = new string[] { "Left subtree", "Right subtree", "As a sibling of root", "Depends on tree balance" },
                correctAnswer = "A"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of insertion in a balanced binary search tree?",
                options = new string[] { "O(1)", "O(log n)", "O(n)", "O(n²)" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the worst-case time complexity of insertion in a binary search tree?",
                options = new string[] { "O(1)", "O(log n)", "O(n)", "O(n²)" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "When inserting a new node in a BST, the property that must be maintained is:",
                options = new string[] { "Root has maximum value", "Left child < Parent < Right child", "All levels must be filled", "Tree must be balanced" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "Inserting nodes in sorted order into a BST creates what kind of tree?",
                options = new string[] { "Balanced tree", "Complete tree", "Full tree", "Skewed tree" },
                correctAnswer = "D"
            });
            
            questions.Add(new QuizQuestion {
                question = "After inserting a new node, what property might be violated in an AVL tree?",
                options = new string[] { "Search property", "Balance factor", "Completeness", "Connectedness" },
                correctAnswer = "B"
            });
        }
        else if (topic == "DeleteNode")
        {
            questions.Add(new QuizQuestion {
                question = "What is the most complex case when deleting a node from a BST?",
                options = new string[] { "Deleting a leaf node", "Deleting a node with one child", "Deleting a node with two children", "Deleting the root" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "When deleting a node with two children, what node typically replaces it?",
                options = new string[] { "Left child", "Right child", "Inorder successor", "Parent node" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the inorder successor of a node?",
                options = new string[] { "The node's parent", "The leftmost node in right subtree", "The rightmost node in left subtree", "The root of the tree" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "What is the time complexity of deletion in a balanced binary search tree?",
                options = new string[] { "O(1)", "O(log n)", "O(n)", "O(n²)" },
                correctAnswer = "B"
            });
            
            questions.Add(new QuizQuestion {
                question = "When deleting a leaf node, what needs to be updated?",
                options = new string[] { "The node's value", "The node's children", "The parent's pointer", "The root pointer" },
                correctAnswer = "C"
            });
            
            questions.Add(new QuizQuestion {
                question = "After deleting a node, what property might need to be restored in an AVL tree?",
                options = new string[] { "Search property", "Balance factor", "Completeness", "Node color (red/black)" },
                correctAnswer = "B"
            });
        }
        else if (topic == "BFS")
        {
            questions.Add(new QuizQuestion {
                question = "What data structure is primarily used in BFS?",
                options = new string[] { "Stack", "Queue", "Heap", "Tree" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "BFS is typically used for finding the _____ path in an unweighted graph.",
                options = new string[] { "Shortest", "Longest", "Random", "Minimum spanning tree" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "BFS traverses nodes _____",
                options = new string[] { "Depth-first", "Level-by-level", "Randomly", "Preorder" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Time complexity of BFS for a graph with V vertices and E edges is?",
                options = new string[] { "O(V)", "O(E)", "O(V + E)", "O(VE)" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "In BFS, each vertex is visited:",
                options = new string[] { "Twice", "Once", "Thrice", "Four times" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "BFS is useful for solving:",
                options = new string[] { "Finding connected components", "Cycle detection", "Shortest path", "All of the above" },
                correctAnswer = "D"
            });
        }
        else if (topic == "DFS")
        {
            questions.Add(new QuizQuestion {
                question = "What data structure is primarily used in DFS?",
                options = new string[] { "Queue", "Stack", "Heap", "Graph" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "DFS can be implemented using?",
                options = new string[] { "Recursion", "Iteration with Stack", "Both A and B", "None of the above" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "DFS is useful for finding:",
                options = new string[] { "Shortest path", "Connected components", "Cycle detection", "Both B and C" },
                correctAnswer = "D"
            });
            questions.Add(new QuizQuestion {
                question = "DFS traversal is classified as:",
                options = new string[] { "Breadth-first", "Depth-first", "Level-order", "Zigzag" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Time complexity of DFS for a graph with V vertices and E edges is?",
                options = new string[] { "O(V)", "O(E)", "O(V+E)", "O(VE)" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "Which traversal visits child before siblings?",
                options = new string[] { "BFS", "DFS", "Level Order", "None" },
                correctAnswer = "B"
            });
        }
        else if (topic == "Dijkstra")
        {
            questions.Add(new QuizQuestion {
                question = "Dijkstra's Algorithm finds:",
                options = new string[] { "Minimum Spanning Tree", "Shortest paths from a source", "Longest path", "Maximum flow" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Dijkstra's algorithm cannot handle graphs with:",
                options = new string[] { "Positive weights", "Negative weights", "Zero weights", "None of the above" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Which data structure optimizes Dijkstra's performance?",
                options = new string[] { "Queue", "Stack", "Priority Queue (Min-Heap)", "Graph" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "Time complexity of Dijkstra with a binary heap?",
                options = new string[] { "O(V)", "O(V log V + E)", "O(E log V)", "O(VE)" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Which traversal does Dijkstra resemble the most?",
                options = new string[] { "DFS", "BFS with priorities", "Bellman-Ford", "A*" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Dijkstra is guaranteed to find the shortest path in a graph with:",
                options = new string[] { "Negative weights", "Positive weights only", "Self loops", "Disconnected components" },
                correctAnswer = "B"
            });
        }
        else if (topic == "AStar")
        {
            questions.Add(new QuizQuestion {
                question = "A* algorithm uses:",
                options = new string[] { "Only cost from start", "Only estimated cost to goal", "Sum of actual and estimated cost", "Random guess" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "In A*, the heuristic function is used to:",
                options = new string[] { "Overestimate cost", "Underestimate cost", "Estimate distance to goal", "None" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "A* guarantees shortest path if:",
                options = new string[] { "Heuristic is admissible", "Graph is weighted", "All nodes are visited", "Heap is balanced" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "A* can behave like Dijkstra if:",
                options = new string[] { "Heuristic is always zero", "Heuristic overestimates", "All edges have zero weight", "Start equals goal" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "What priority is used in A* open set?",
                options = new string[] { "gScore only", "hScore only", "fScore (g + h)", "Random selection" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "A* is best suited for:",
                options = new string[] { "Finding all cycles", "Finding minimum spanning tree", "Finding path to goal efficiently", "Finding longest path" },
                correctAnswer = "C"
            });
        }
        else if (topic == "BellmanFord")
        {
            questions.Add(new QuizQuestion {
                question = "Bellman-Ford algorithm finds:",
                options = new string[] { "Shortest paths from source", "Longest path", "Cycle detection", "MST" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "Bellman-Ford can handle graphs with:",
                options = new string[] { "Negative weights", "Only positive weights", "Only zero weights", "Self loops only" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "How many times are edges relaxed in Bellman-Ford?",
                options = new string[] { "V", "V-1", "E", "E-1" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Bellman-Ford time complexity is:",
                options = new string[] { "O(V)", "O(V+E)", "O(VE)", "O(E log V)" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "Bellman-Ford can detect:",
                options = new string[] { "Positive cycles", "Negative weight cycles", "Both", "None" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "If a negative cycle is found, Bellman-Ford should:",
                options = new string[] { "Continue updating", "Stop and report cycle", "Restart", "Ignore" },
                correctAnswer = "B"
            });
        }

        else if (topic == "LinearRegression")
        {
            questions.Add(new QuizQuestion {
                question = "What is the goal of Linear Regression?",
                options = new string[] { "Classification", "Clustering", "Predicting continuous values", "Dimensionality reduction" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "What is the equation form in Linear Regression?",
                options = new string[] { "y = mx + b", "y = ax^2 + bx + c", "y = sin(x)", "y = log(x)" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "Which technique is used to minimize error in Linear Regression?",
                options = new string[] { "Maximization", "Gradient Descent", "Random Search", "Brute Force" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Which metric is commonly used to evaluate Linear Regression models?",
                options = new string[] { "Accuracy", "Mean Squared Error", "F1 Score", "Recall" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "What happens if features are highly correlated in Linear Regression?",
                options = new string[] { "Better prediction", "Overfitting", "Multicollinearity", "Underfitting" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "Linear Regression assumes:",
                options = new string[] { "Independence of errors", "Multicollinearity", "Heteroscedasticity", "Non-linearity" },
                correctAnswer = "A"
            });
        }
        else if (topic == "KMeansClustering")
        {
            questions.Add(new QuizQuestion {
                question = "K-Means is a type of:",
                options = new string[] { "Supervised Learning", "Unsupervised Learning", "Reinforcement Learning", "Semi-supervised Learning" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "What does K in K-Means represent?",
                options = new string[] { "Number of Clusters", "Number of Neighbors", "Kernel", "Knowledge" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "What distance metric is commonly used in K-Means?",
                options = new string[] { "Manhattan", "Euclidean", "Cosine", "Jaccard" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "What problem may occur in K-Means if K is chosen badly?",
                options = new string[] { "Poor clustering", "Overfitting", "Underfitting", "Nothing" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "Which initialization method improves K-Means?",
                options = new string[] { "Random Sampling", "K-Means++", "Gradient Descent", "PCA" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "K-Means can fail when:",
                options = new string[] { "Clusters are spherical", "Clusters have same density", "Clusters are non-convex", "All of the above" },
                correctAnswer = "C"
            });
        }
        else if (topic == "DecisionTree")
        {
            questions.Add(new QuizQuestion {
                question = "Decision Trees are used for:",
                options = new string[] { "Regression only", "Classification only", "Both Regression and Classification", "Neither" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "What splitting criteria is used for classification trees?",
                options = new string[] { "MSE", "Gini Index", "Euclidean Distance", "Cosine Similarity" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "A fully grown decision tree often suffers from:",
                options = new string[] { "Underfitting", "Overfitting", "Bias", "Variance reduction" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "What is pruning in decision trees?",
                options = new string[] { "Growing a tree", "Removing leaves", "Making deeper trees", "Adding nodes" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Which algorithm is a popular method for building decision trees?",
                options = new string[] { "SVM", "ID3", "Naive Bayes", "PCA" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "What is the time complexity to build a decision tree?",
                options = new string[] { "O(n)", "O(n log n)", "O(n²)", "O(n*m*log n)" },
                correctAnswer = "D"
            });
        }
        else if (topic == "NeuralNetwork")
        {
            questions.Add(new QuizQuestion {
                question = "Neural Networks are inspired by:",
                options = new string[] { "Animal brains", "Plant cells", "Cloud computing", "Electric circuits" },
                correctAnswer = "A"
            });
            questions.Add(new QuizQuestion {
                question = "Which function is commonly used in the output layer of classification networks?",
                options = new string[] { "Sigmoid", "ReLU", "Tanh", "Softmax" },
                correctAnswer = "D"
            });
            questions.Add(new QuizQuestion {
                question = "Backpropagation updates:",
                options = new string[] { "Biases", "Weights", "Both weights and biases", "Neither" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "What is a common problem in deep networks?",
                options = new string[] { "Vanishing gradients", "Exploding gradients", "Overfitting", "All of the above" },
                correctAnswer = "D"
            });
            questions.Add(new QuizQuestion {
                question = "Dropout is used for:",
                options = new string[] { "Speeding training", "Regularization", "Reducing data", "Feature scaling" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "The Rectified Linear Unit (ReLU) is defined as:",
                options = new string[] { "max(0,x)", "x²", "sin(x)", "exp(x)" },
                correctAnswer = "A"
            });
        }
        else if (topic == "RandomForest")
        {
            questions.Add(new QuizQuestion {
                question = "Random Forests combine:",
                options = new string[] { "Linear models", "Multiple decision trees", "Neural networks", "Support vectors" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Random Forest reduces:",
                options = new string[] { "Bias", "Variance", "Overfitting", "Training time" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "What is bagging?",
                options = new string[] { "Boosting models", "Random sampling with replacement", "Removing features", "Normalizing data" },
                correctAnswer = "B"
            });
            questions.Add(new QuizQuestion {
                question = "Which statement is true for Random Forest?",
                options = new string[] { "It always overfits", "It is unstable", "It is less interpretable than a single tree", "It has high bias" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "Random Forest handles missing values by:",
                options = new string[] { "Dropping rows", "Mean imputation", "Surrogate splits", "Stopping training" },
                correctAnswer = "C"
            });
            questions.Add(new QuizQuestion {
                question = "What is the out-of-bag (OOB) score?",
                options = new string[] { "Validation accuracy using unused samples", "Overfitting score", "Random error", "Bagging index" },
                correctAnswer = "A"
            });
        }
    }
    
    void DisplayQuestion(int index)
    {
        if (index < questions.Count)
        {
            QuizQuestion question = questions[index];
            
            questionText.text = question.question;
            answerTextA.text = question.options[0];
            answerTextB.text = question.options[1];
            answerTextC.text = question.options[2];
            answerTextD.text = question.options[3];
            
            // Reset button colors
            ResetButtonColors();
            
            // Store correct answer for later checking
            currentCorrectAnswer = question.correctAnswer;
            selectedAnswer = "";
            submitButton.interactable = false;
        
        }
        else
        {
            // Quiz completed for this algorithm
            CompleteQuiz();
        }

        answerButtonA.gameObject.SetActive(true);
        answerButtonB.gameObject.SetActive(true);
        answerButtonC.gameObject.SetActive(true);
        answerButtonD.gameObject.SetActive(true);
    }
    
    void SelectAnswer(string answer)
    {
        selectedAnswer = answer;
        submitButton.interactable = true;
        
        // Highlight selected button
        ResetButtonColors();
        switch (answer)
        {
            case "A":
                answerButtonA.GetComponent<Image>().color = new Color(0.7f, 0.7f, 1f);
                break;
            case "B":
                answerButtonB.GetComponent<Image>().color = new Color(0.7f, 0.7f, 1f);
                break;
            case "C":
                answerButtonC.GetComponent<Image>().color = new Color(0.7f, 0.7f, 1f);
                break;
            case "D":
                answerButtonD.GetComponent<Image>().color = new Color(0.7f, 0.7f, 1f);
                break;
        }
    }
    
    void ResetButtonColors()
    {
        answerButtonA.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.4f);
        answerButtonB.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.4f);
        answerButtonC.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.4f);
        answerButtonD.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.4f);
    }
    
    void SubmitAnswer()
    {
        if (string.IsNullOrEmpty(selectedAnswer))
        {
            // No option selected
            if (noAnswerAudioSource != null)
                noAnswerAudioSource.Play();

            Debug.Log("No option selected!");
            return;
        }

        if (selectedAnswer == currentCorrectAnswer)
        {
            // Correct answer
            correctAnswers++;
            correctAnswersText.text = correctAnswers.ToString();
            scoreText.text = "Score: " + correctAnswers;

            if (correctAudioSource != null)
                correctAudioSource.Play();

            HighlightButton(selectedAnswer, Color.green);
        }
        else
        {
            // Wrong answer
            wrongAnswers++;
            wrongAnswersText.text = wrongAnswers.ToString();

            if (wrongAudioSource != null)
                wrongAudioSource.Play();

            HighlightButton(selectedAnswer, Color.red);
            HighlightButton(currentCorrectAnswer, Color.green);
        }

        // Disable buttons
        answerButtonA.interactable = false;
        answerButtonB.interactable = false;
        answerButtonC.interactable = false;
        answerButtonD.interactable = false;
        submitButton.interactable = false;

        // Enable next button
        nextButton.gameObject.SetActive(true);
    }

    
    void HighlightButton(string buttonId, Color color)
    {
        switch (buttonId)
        {
            case "A":
                answerButtonA.GetComponent<Image>().color = color;
                break;
            case "B":
                answerButtonB.GetComponent<Image>().color = color;
                break;
            case "C":
                answerButtonC.GetComponent<Image>().color = color;
                break;
            case "D":
                answerButtonD.GetComponent<Image>().color = color;
                break;
        }
    }
    
    void NextQuestion()
    {
        currentQuestionIndex++;
        
        // Enable answer buttons
        answerButtonA.interactable = true;
        answerButtonB.interactable = true;
        answerButtonC.interactable = true;
        answerButtonD.interactable = true;
        
        // Hide next button
        nextButton.gameObject.SetActive(false);
        
        DisplayQuestion(currentQuestionIndex);
    }
    
    void CompleteQuiz()
    {
        isQuizActive = false;
        
        // Save quiz results to Firebase
        SaveQuizResultsToFirebase();
        
        // Show blur panel and next algorithm button
        quizPanel.SetActive(false);   
        blurPanel.gameObject.SetActive(true);
        nextAlgorithmButton.SetActive(true);
        
        // Update UI to show final score
        // This could display a summary of performance
    }
    
   

    void SaveQuizResultsToFirebase()
    {
        if (user != null)
        {
            string userId = user.UserId;
            float score = (float)correctAnswers / questions.Count * 100;
            string timestamp = System.DateTime.Now.ToString();
            string activityKey = dbReference.Child("users").Child(userId).Child("recentActivity").Push().Key;

            Dictionary<string, object> activityData = new Dictionary<string, object>
            {
                { "type", "Quiz" },
                { "description", "Completed " + currentAlgorithm + " quiz with score " + score.ToString("0") + "%" },
                { "timestamp", timestamp }
            };

            // ✅ Save under different Firebase nodes based on category
            if (IsTreeOperation(currentAlgorithm))
            {
                dbReference.Child("users").Child(userId).Child("treeOperationProgress").Child(currentAlgorithm).SetValueAsync(score);
            }
            else
            {
                dbReference.Child("users").Child(userId).Child("algorithmProgress").Child(currentAlgorithm).SetValueAsync(score);
            }

            dbReference.Child("users").Child(userId).Child("recentActivity").Child(activityKey).SetValueAsync(activityData);
        }
    }

    bool IsTreeOperation(string topic)
    {
        return topic.Contains("Traversal") || topic.Contains("Node");
    }


    void UpdateTimerDisplay()
    {
        int minutes = (int)(quizTimer / 60);
        int seconds = (int)(quizTimer % 60);
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
        timeText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }
    
    void NextAlgorithm()
    {
        // Hide quiz panel and blur
        quizPanel.SetActive(false);
        blurPanel.gameObject.SetActive(false);
        nextAlgorithmButton.SetActive(false);

        string nextAlgo = "";

        if (!IsTreeOperation(currentAlgorithm))
        {
            currentAlgorithmIndex = Array.IndexOf(sortingAlgorithmSequence, currentAlgorithm);
            Debug.Log($"Current algorithm: {currentAlgorithm}, Index: {currentAlgorithmIndex}");

            currentAlgorithmIndex++;

            if (currentAlgorithmIndex < sortingAlgorithmSequence.Length)
            {
                nextAlgo = sortingAlgorithmSequence[currentAlgorithmIndex];
                Debug.Log($"Next algorithm to show: {nextAlgo}");

                SortingVisualizer visualizer = FindObjectOfType<SortingVisualizer>();
                if (visualizer != null)
                {
                    visualizer.ShowInfoPanelForAlgorithm(nextAlgo);
                }
                else
                {
                    Debug.LogError("SortingVisualizer not found in the scene!");
                }
            }
            else
            {
                Debug.Log("All sorting algorithms completed!");
            }
        }

        else if (IsGraphOperation(currentAlgorithm))
        {
            currentAlgorithmIndex = Array.IndexOf(graphAlgorithmSequence, currentAlgorithm);
            currentAlgorithmIndex++;

            if (currentAlgorithmIndex < graphAlgorithmSequence.Length)
            {
                nextAlgo = graphAlgorithmSequence[currentAlgorithmIndex];

                GraphVisualizer visualizer = FindObjectOfType<GraphVisualizer>();
                if (visualizer != null)
                {
                    visualizer.ShowInfoPanelForAlgorithm(nextAlgo);
                }
            }
            else
            {
                Debug.Log("All graph algorithms completed!");
            }
        }

        else if (IsMLAlgorithm(currentAlgorithm))   // ✅ ADD THIS
        {
            currentAlgorithmIndex = Array.IndexOf(mlAlgorithmSequence, currentAlgorithm);
            currentAlgorithmIndex++;

            if (currentAlgorithmIndex < mlAlgorithmSequence.Length)
            {
                nextAlgo = mlAlgorithmSequence[currentAlgorithmIndex];
                MLVisualizer visualizer = FindObjectOfType<MLVisualizer>();
                if (visualizer != null)
                {
                    visualizer.OpenAlgorithmInfoPanel(nextAlgo);
                }
            }
            else
            {
                Debug.Log("All ML algorithms completed!");
            }
        }
        
        else
        {
            currentAlgorithmIndex = Array.IndexOf(sortingAlgorithmSequence, currentAlgorithm);
            currentAlgorithmIndex++;

            if (currentAlgorithmIndex < sortingAlgorithmSequence.Length)
            {
                nextAlgo = sortingAlgorithmSequence[currentAlgorithmIndex];

                SortingVisualizer visualizer = FindObjectOfType<SortingVisualizer>();
                if (visualizer != null)
                {
                    visualizer.ShowInfoPanelForAlgorithm(nextAlgo);
                }
            }
            else
            {
                Debug.Log("All sorting algorithms completed!");
            }
        }
    }

    bool IsGraphOperation(string topic)
    {
        return topic == "Dijkstra" || topic == "BFS" || topic == "DFS" || topic == "AStar" || topic == "BellmanFord";
    }

    bool IsMLAlgorithm(string topic)
    {
        return topic == "LinearRegression" || topic == "KMeansClustering" || topic == "DecisionTree" || topic == "NeuralNetwork" || topic == "RandomForest";
    }

    public void SetAlgorithmCategory(string algorithm)
    {
        currentAlgorithm = algorithm;
        
        // Reset the algorithm index based on the category
        if (IsTreeOperation(algorithm))
        {
            // Find the index in the tree operation sequence
            currentAlgorithmIndex = Array.IndexOf(treeOperationSequence, algorithm);
            if (currentAlgorithmIndex < 0) currentAlgorithmIndex = 0;
        }
        else
        {
            // Find the index in the sorting algorithm sequence
            currentAlgorithmIndex = Array.IndexOf(sortingAlgorithmSequence, algorithm);
            if (currentAlgorithmIndex < 0) currentAlgorithmIndex = 0;
        }
    }

}