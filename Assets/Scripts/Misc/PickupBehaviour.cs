using UnityEngine;

public class PickupBehaviour : MonoBehaviour
{
    public float spinSpeed = 90f; // degrees per second
    public float moveAmplitude = 0.5f; // units
    public float moveFrequency = 1f; // cycles per second

    private float initialY;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, spinSpeed * Time.deltaTime, 0);

        // Move up and down
        Vector3 pos = transform.position;
        pos.y = initialY + Mathf.Sin(Time.time * moveFrequency * Mathf.PI * 2) * moveAmplitude;
        transform.position = pos;
    }
}
