using UnityEngine;

public class SmoothMove : MonoBehaviour
{
    public Vector3 finalPosition;  // The final position to move to
    public float duration = 2.0f;  // Duration in seconds
    private Vector3 startPosition; // Starting position
    private float elapsedTime = 0.0f; // Time elapsed

    void Start()
    {
        startPosition = transform.position; // Initialize starting position
    }

    void Update()
    {
        if (elapsedTime < duration)
        {
            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Calculate the percentage of completion
            float t = elapsedTime / duration;

            // Apply the ease-in-out curve
            t = Mathf.SmoothStep(0, 1, t);

            // Interpolate between the start and final positions
            transform.position = Vector3.Lerp(startPosition, finalPosition, t);
        }
        else
        {
            // Ensure the final position is set exactly at the end
            transform.position = finalPosition;
        }
    }
}
