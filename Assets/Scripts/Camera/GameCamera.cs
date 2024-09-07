using UnityEngine;

public class GameCamera : Singleton<GameCamera>
{
    [SerializeField] private Transform Target;  
    [SerializeField] private float Distance = 5f;  // Default distance from the target
    [SerializeField] private float MinDistance = 2f;  // Minimum allowed distance
    [SerializeField] private float MaxDistance = 10f;  // Maximum allowed distance
    [SerializeField] private float SmoothSpeed = 0.125f;  // How smooth the camera movement is
    [SerializeField] private float RotationSpeed = 150f;  // Speed of rotation based on mouse movement
    [SerializeField] private float ZoomSpeed = 2f;  // Speed of zooming in and out
    [SerializeField] private float MinVerticalAngle = -45f;  // Minimum vertical angle for camera
    [SerializeField] private float MaxVerticalAngle = 45f;  // Maximum vertical angle for camera
    [SerializeField] private LayerMask CollisionLayers;  // Layers to check for collisions
    [SerializeField] private float CollisionOffset = 0.2f;  // Offset to avoid clipping
    [SerializeField] private Vector3 GravityDirection = Vector3.down;  // Current gravity direction

    private float CurrentX = 0f;  // Current X rotation
    private float CurrentY = 0f;  // Current Y rotation
    private float CurrentDistance;  // Current distance from the target
    private Vector3 DesiredPosition;  // Where the camera should be
    private Vector3 SmoothVelocity = Vector3.zero;  // For smooth movement
    private Vector3 LastValidPosition;  // Last position before collision

    void Start()
    {
        CurrentDistance = Distance; 
    }

    void Update()
    {
        HandleInput();
    }

    void LateUpdate()
    {
        HandleCameraMovement();
        HandleCameraCollision();
    }

    public void OnGravityChange(Vector3 direction)
    {
        GravityDirection = direction;  // Update gravity direction
    }

    private void HandleInput()
    {
        // Rotate based on mouse movement
        CurrentX += Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime;
        CurrentY -= Input.GetAxis("Mouse Y") * RotationSpeed * Time.deltaTime;
        CurrentY = Mathf.Clamp(CurrentY, MinVerticalAngle, MaxVerticalAngle);

        // Zoom based on scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        CurrentDistance = Mathf.Clamp(CurrentDistance - scrollInput * ZoomSpeed, MinDistance, MaxDistance);
    }

    private void HandleCameraMovement()
    {
        // Create rotation based on gravity and input
        Quaternion targetRotation = Quaternion.Euler(CurrentY, CurrentX, 0);
        Quaternion gravityRotation = Quaternion.FromToRotation(Vector3.up, -GravityDirection);
        Quaternion finalRotation = gravityRotation * targetRotation;

        // Calculate desired position and move camera
        Vector3 direction = finalRotation * Vector3.back;
        DesiredPosition = Target.position + direction * CurrentDistance;

        transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition, ref SmoothVelocity, SmoothSpeed);
        transform.rotation = finalRotation;
    }

    private void HandleCameraCollision()
    {
        RaycastHit hit;
        // Check for obstacles between camera and target
        if (Physics.Linecast(Target.position, transform.position, out hit, CollisionLayers))
        {
            float hitDistance = Vector3.Distance(Target.position, hit.point) - CollisionOffset;
            Vector3 collisionPosition = Target.position - transform.forward * hitDistance;
            LastValidPosition = collisionPosition;
            transform.position = Vector3.Lerp(transform.position, collisionPosition, SmoothSpeed);
        }
        else
        {
            // Move camera to last valid position if no collision
            transform.position = Vector3.Lerp(transform.position, LastValidPosition, SmoothSpeed);
            LastValidPosition = transform.position;
        }
    }
}
