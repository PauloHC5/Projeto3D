using UnityEngine;
using UnityEngine.UI;

public class ButtomBehaviour : MonoBehaviour
{
    private Canvas canvasAttached;
    private Vector3 originalScale;
    private Image[] buttonImages;

    private void Awake()
    {
        canvasAttached = GetComponentInParent<Canvas>();

        if (canvasAttached == null)
        {
            Debug.LogError("No Canvas found in parent hierarchy. Please attach this script to a GameObject that is a child of a Canvas.");
        }

        buttonImages = GetComponentsInChildren<Image>(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalScale = transform.localScale; // Store the original scale of the button        
    }

    // Update is called once per frame
    void Update()
    {
        MouseOverResumeButton();
    }

    private void MouseOverResumeButton()
    {

        if (IsMouseOverResumeButton())
        {
            // If the mouse is over the resume button, change its color                        
            foreach (var image in buttonImages)
            {
                image.color = Color.yellow; // Change to your desired color
            }

            // Set its scale to indicate it's hovered
            transform.localScale = originalScale + new Vector3(0.2f, 0.2f, 0.2f); // Slightly increase size
        }
        else
        {
            // If the mouse is not over the resume button, reset its color
            // Get the Image component of the button and reset its color
            var buttonImages = GetComponentsInChildren<UnityEngine.UI.Image>();
            foreach (var image in buttonImages)
            {
                if (image) image.color = Color.white; // Change to your desired color
            }

            // Reset its scale to normal
            transform.localScale = originalScale; // Reset to original scale
        }

    }

    private bool IsMouseOverResumeButton()
    {        

        // Obtém o RectTransform do botão
        RectTransform rectTransform = GetComponent<RectTransform>();        

        // Pega a posição do mouse na tela
        Vector2 mousePosition = Input.mousePosition;

        

        // Verifica se o mouse está sobre o botão
        return RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            mousePosition,
            canvasAttached.worldCamera
        );
    }
}
