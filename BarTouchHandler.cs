using UnityEngine;
using TMPro;

public class BarTouchHandler : MonoBehaviour
{
    public TMP_Text infoText; // Assign this in the inspector or dynamically
    private float originalHeight;

    void Start()
    {
        originalHeight = transform.localScale.y;
    }

    void OnMouseDown()
    {
        Debug.Log("Touched bar with height: " + originalHeight);

        if (infoText != null)
        {
            infoText.text = $"Height: {originalHeight:F2}";
        }

        // Optional: visually highlight the bar
        LeanTween.color(gameObject, Color.magenta, 0.2f);
    }
}
