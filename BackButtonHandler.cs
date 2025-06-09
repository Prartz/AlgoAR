using UnityEngine;

public class BackButtonHandler : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;

    public void OnBackPressed()
    {
        // Handle all known visualizers
        SortingVisualizer sorting = FindObjectOfType<SortingVisualizer>();
        GraphVisualizer graph = FindObjectOfType<GraphVisualizer>();
        TreeVisualizer tree = FindObjectOfType<TreeVisualizer>();
        MLVisualizer ml = FindObjectOfType<MLVisualizer>();

        if (sorting != null)
        {
            sorting.StopAllCoroutines();
            sorting.ReturnToMainMenu(); // Built-in method
        }

        if (graph != null)
        {
            graph.StopAllCoroutines();
            graph.ClearGraphExternally(); // ← Add this method in GraphVisualizer
            graph.gameObject.SetActive(false);
        }

        if (tree != null)
        {
            tree.StopAllCoroutines();
            tree.ClearTreeExternally(); // ← Add this method in TreeVisualizer
            tree.gameObject.SetActive(false);
        }

        if (ml != null)
        {
            ml.StopAllCoroutines();
            ml.ClearVisualizationExternally(); // ← Add this method in MLVisualizer
            ml.gameObject.SetActive(false);
        }

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        else
            Debug.LogWarning("Main menu panel is not assigned!");
    }
}
