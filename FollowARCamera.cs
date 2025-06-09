using UnityEngine;

public class FollowARCamera : MonoBehaviour
{
    public float followDistance = 3.0f;
    private Vector3 initialCameraForward;
    private Quaternion initialRotation;
    private bool initialized = false;
    
    void Start()
    {
        // Remember initial forward direction
        initialCameraForward = Camera.main.transform.forward;
        initialRotation = transform.rotation;
        initialized = true;
    }
    
    void LateUpdate()
    {
        if (!initialized) return;
        
        // Move with camera while maintaining the same relative position
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * followDistance;
        
        // Keep facing the camera
        transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward, Camera.main.transform.up);
        
        // Keep the bars upright
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
    }
}