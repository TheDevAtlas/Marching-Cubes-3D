using UnityEngine;

public class SmoothMove : MonoBehaviour
{
    public Vector3 finalPosition;    // The final position to move to
    public Vector3 finalScale;       // The final scale to achieve
    public Quaternion finalRotation; // The final rotation to achieve
    public float duration = 2.0f;    // Duration in seconds
    public float delay = 0.0f;       // Delay before starting the transition

    private Vector3 startPosition;   // Starting position
    private Vector3 startScale;      // Starting scale
    private Quaternion startRotation;// Starting rotation
    private float elapsedTime = 0.0f;// Time elapsed

    void Start()
    {
        startPosition = transform.position; // Initialize starting position
        startScale = transform.localScale;  // Initialize starting scale
        startRotation = transform.rotation; // Initialize starting rotation
    }

    void Update()
    {
        // Check if the delay has passed before starting the transition
        if (elapsedTime < duration + delay)
        {
            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Proceed only if the delay has passed
            if (elapsedTime >= delay)
            {
                // Calculate the percentage of completion excluding the delay time
                float t = (elapsedTime - delay) / duration;

                // Apply the ease-in-out curve
                t = Mathf.SmoothStep(0, 1, t);

                // Interpolate between the start and final positions
                transform.position = Vector3.Lerp(startPosition, finalPosition, t);
                // Interpolate between the start and final scales
                transform.localScale = Vector3.Lerp(startScale, finalScale, t);
                // Interpolate between the start and final rotations
                transform.rotation = Quaternion.Slerp(startRotation, finalRotation, t);
            }
        }
        else
        {
            // Ensure the final position, scale, and rotation are set exactly at the end
            transform.position = finalPosition;
            transform.localScale = finalScale;
            transform.rotation = finalRotation;
        }
    }
}
