
using System.Collections;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    #region MOVEMENT

    // Speed at which the character moves
    [field: SerializeField] public float MovementSpeed { get; private set; }

    // Speed at which the character rotates
    [field: SerializeField] public float RotationSpeed { get; private set; }

    // Power of the character's jump
    public float JumpPower;

    // Transform representing the character's feet
    public Transform FootTransform;

    #endregion

    #region HOLOGRAM

    [SerializeField] private Transform HologramParent;  // Parent object of the hologram
    [SerializeField] private GameObject gHologram;      // Hologram GameObject
    [SerializeField] private float HoloSpeed = 0.5f;    // Speed of hologram rotation

    [Header("Gravity Directions")]
    [Space(5)]
    [SerializeField] private Quaternion directionRight;
    [SerializeField] private Quaternion directionLeft;
    [SerializeField] private Quaternion directionBack;
    [SerializeField] private Quaternion directionFront;

    private Quaternion defaultRotation;  // The default rotation of the character

    #endregion

    public ICharacterState CurrentState { get; private set; }
    public ICharacterState OldState { get; private set; }

    [SerializeField] private float GroundCheckDistance = 0.1f;  // Distance to check for the ground
    [SerializeField] private LayerMask GroundLayer;  // Layer used for ground detection

    #region COMPONENTS
    private Animator _animator;
    private GameCamera _gameCamera;
    private Rigidbody _rigidbody;

    // Lazy loading of Animator component
    public Animator Animator => _animator = _animator != null ? _animator : GetComponent<Animator>();

    // Lazy loading of Rigidbody component
    public Rigidbody Rigidbody => _rigidbody = _rigidbody != null ? _rigidbody : GetComponent<Rigidbody>();

    // Access to GameCamera instance
    public GameCamera GameCamera => _gameCamera = _gameCamera != null ? _gameCamera : GameCamera.Instance;

    #endregion

    #region GRAVITY 

    public Vector3 GravityDirection = Vector3.down;  
    public float GravityStrength = 9.81f;            

    #endregion

    [Space(5)]
    public Timer FallTimer;  // Timer used to track the character's fall duration
    [Space(5)]
    [Header("Events")]
    public UnityEvent<Vector3> OnGravitChange;  // Event invoked when gravity changes
    public UnityEvent<int> OnCollect;           // Event invoked when an item is collected

    // Check if the character is grounded by raycasting from the foot position
    public bool IsGrounded
    {
        get
        {
            return Physics.Raycast(FootTransform.position, GravityDirection, GroundCheckDistance, GroundLayer);
        }
    }

    // Check if the character is moving based on player input
    public bool IsMoving => (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Epsilon || Mathf.Abs(Input.GetAxis("Vertical")) > Mathf.Epsilon);

    // Check if the player is trying to change gravity direction
    public bool IsChangingGravity
    {
        get
        {
            return (Input.GetKeyDown(KeyCode.UpArrow) ||
                    Input.GetKeyDown(KeyCode.DownArrow) ||
                    Input.GetKeyDown(KeyCode.LeftArrow) ||
                    Input.GetKeyDown(KeyCode.RightArrow)) &&
                    !(CurrentState is GravityManipulationState);
        }
    }

    private void Awake()
    {
        // Deactivate the hologram initially
        gHologram.SetActive(false);
    }

    private void Start()
    {
       
        SetState(new IdleState());

        FallTimer = new Timer(3f);
        FallTimer.StartTimer();

        // Subscribe GameOver method to the timer's finish event
        FallTimer.OnTimerFinished += GameManager.Instance.GameOver;

        // Store the default rotation of the character
        defaultRotation = transform.rotation;
    }

    private void Update()
    {
        // Handle gravity change when the player presses arrow keys and the character is not jumping
        if (IsChangingGravity && CurrentState is not JumpingState)
        {
            SetState(new GravityManipulationState());
        }
        else
        {
            HandleMovement();
        }

        // If grounded, reset the fall timer
        if (IsGrounded)
        {
            FallTimer.ResetTimer();
        }
        else
        {
            // If not grounded and the timer is not running, start it
            if (!FallTimer.IsRunning) FallTimer.StartTimer();

            FallTimer.Update(Time.deltaTime);
        }

        
        CurrentState?.UpdateState(this);
    }

    private void FixedUpdate()
    {
        // Simulate gravity by applying a force towards the gravity direction
        SimulateGravity();
    }

    private void SimulateGravity()
    {
        Rigidbody.AddForce(GravityDirection.normalized * GravityStrength, ForceMode.Acceleration);
    }

    public void ChangeGravityDirection(Vector3 GravityDirection)
    {
        // Change the gravity direction and invoke the event
        this.GravityDirection = GravityDirection;
        Debug.Log($"Changing Gravity Direction to {GravityDirection}");
        OnGravitChange?.Invoke(GravityDirection);

        // Start coroutine to align the player with the new gravity direction
        StartCoroutine(AlignPlayerWithGravityCoroutine());
    }

    private IEnumerator AlignPlayerWithGravityCoroutine()
    {
        Vector3 upDirection = -GravityDirection.normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, upDirection) * transform.rotation;

        float elapsedTime = 0f;
        Quaternion initialRotation = transform.rotation;

        while (elapsedTime < 1f)
        {
            // Smoothly rotate the player to align with the new gravity direction
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime);
            elapsedTime += Time.deltaTime * 10f;
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    public void SetState(ICharacterState state)
    {
        // Exit the current state and enter the new state
        CurrentState?.ExitState(this);
        OldState = CurrentState;
        CurrentState = state;
        CurrentState?.EnterState(this);
    }

    private void HandleMovement()
    {
        // If moving and grounded, set the character state to walking
        if (IsMoving && IsGrounded)
        {
            SetState(new WalkingState());
        }

        // If the space key is pressed and grounded, set the character state to jumping
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded)
        {
            SetState(new JumpingState());
        }
    }

    public void Move(float speed)
    {
       
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);

        // Adjust movement based on the current gravity direction
        Vector3 adjustedMovement = Quaternion.FromToRotation(Vector3.up, -GravityDirection) * movement;

        // Apply movement to the character's position
        transform.position += adjustedMovement * speed * Time.deltaTime;
    }

    public void Rotate(float rotationSpeed)
    {
        // Get player input for rotation
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);

        // Adjust rotation based on the current gravity direction
        Vector3 adjustedMovement = Quaternion.FromToRotation(Vector3.up, -GravityDirection) * movement;

        if (adjustedMovement != Vector3.zero)
        {
            // Rotate the character smoothly towards the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(adjustedMovement.normalized, -GravityDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void ToggleHologram(bool tru)
    {
        // Toggle the hologram visibility and reset its rotation
        gHologram.SetActive(tru);
        HologramParent.transform.rotation = defaultRotation;
    }

    public void RotateHoloToTarget(Quaternion targetRotation)
    {
        // Rotate the hologram towards the target rotation
        StartCoroutine(IRotateHoloToTarget(targetRotation));
    }

    private IEnumerator IRotateHoloToTarget(Quaternion targetRotation)
    {
        Quaternion initialRotation = HologramParent.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < HoloSpeed)
        {
            // Smoothly rotate the hologram to the target rotation
            Quaternion intermediateRotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / HoloSpeed);
            HologramParent.rotation = Quaternion.Euler(intermediateRotation.eulerAngles);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the hologram reaches the exact target rotation
        HologramParent.rotation = Quaternion.Euler(targetRotation.eulerAngles);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Collectable"))
        {
            Debug.Log("Collecting Item");
            Destroy(collision.gameObject);
            OnCollect?.Invoke(1);
        }
    }

    public void Collect(GameObject Go)
    {
        OnCollect?.Invoke(1);
        StartCoroutine(ICollect(Go));
    }
    private IEnumerator ICollect(GameObject go)
    {

        float duration = 1.5f;
        Vector3 startPosition = go.transform.position;
        Vector3 targetPosition = transform.position;
        float elapsedTime = 0f;


        Vector3 initialScale = go.transform.localScale;

        while (elapsedTime < duration)
        {

            float t = elapsedTime / duration;

            if (go)
            {
                go.transform.Rotate(Vector3.up, 360 * Time.deltaTime / duration, Space.World);

                go.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

                go.transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            }
            else
            {
                yield break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }


        go.transform.position = targetPosition;
        go.transform.localScale = Vector3.zero;


        Destroy(go);
        yield break;
    }

    //Idk why i did this
    public void SetVictoryDance()
    {
        SetState(new DanceState());
        enabled = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(FootTransform.position, 0.1f);
    }
}
