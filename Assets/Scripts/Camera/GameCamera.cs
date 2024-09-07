using UnityEngine;

public class GameCamera : Singleton<GameCamera>
{
    [SerializeField] private Transform Target;
    [SerializeField] private float Distance = 5f;
    [SerializeField] private float MinDistance = 2f;
    [SerializeField] private float MaxDistance = 10f;
    [SerializeField] private float SmoothSpeed = 0.125f;
    [SerializeField] private float RotationSpeed = 150f;
    [SerializeField] private float ZoomSpeed = 2f;
    [SerializeField] private float MinVerticalAngle = -45f;
    [SerializeField] private float MaxVerticalAngle = 45f;
    [SerializeField] private LayerMask CollisionLayers;
    [SerializeField] private float CollisionOffset = 0.2f;

    private float CurrentX = 0f;
    private float CurrentY = 0f;
    private float CurrentDistance;
    private Vector3 DesiredPosition;
    private Vector3 SmoothVelocity = Vector3.zero;
    private Vector3 LastValidPosition;

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

    private void HandleInput()
    {
        CurrentX += Input.GetAxis("Mouse X") * RotationSpeed * Time.deltaTime;
        CurrentY -= Input.GetAxis("Mouse Y") * RotationSpeed * Time.deltaTime;
        CurrentY = Mathf.Clamp(CurrentY, MinVerticalAngle, MaxVerticalAngle);
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        CurrentDistance = Mathf.Clamp(CurrentDistance - scrollInput * ZoomSpeed, MinDistance, MaxDistance);
    }

    private void HandleCameraMovement()
    {
        Quaternion rotation = Quaternion.Euler(CurrentY, CurrentX, 0);
        Vector3 direction = rotation * Vector3.back;
        DesiredPosition = Target.position + direction * CurrentDistance;
        transform.position = Vector3.SmoothDamp(transform.position, DesiredPosition, ref SmoothVelocity, SmoothSpeed);
        transform.LookAt(Target.position);
    }

    private void HandleCameraCollision()
    {
        RaycastHit hit;
        if (Physics.Linecast(Target.position, transform.position, out hit, CollisionLayers))
        {
            float hitDistance = Vector3.Distance(Target.position, hit.point) - CollisionOffset;
            Vector3 collisionPosition = Target.position - transform.forward * hitDistance;
            LastValidPosition = collisionPosition;
            transform.position = Vector3.Lerp(transform.position, collisionPosition, SmoothSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, LastValidPosition, SmoothSpeed);
            LastValidPosition = transform.position;
        }
    }
}
