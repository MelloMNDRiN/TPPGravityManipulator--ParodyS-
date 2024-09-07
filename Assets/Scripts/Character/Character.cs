using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    #region MOVEMENT

    [field: SerializeField] public float MovementSpeed { get; private set; }
    [field: SerializeField] public float RotationSpeed { get; private set; }

    public float JumpPower;

    public Transform FootTransform;

    #endregion

    #region HOLOGRAM

    [SerializeField] private Transform HologramParent;
    [SerializeField] private GameObject gHologram;

    [SerializeField] private float HoloSpeed = 0.5f;

    [Header("Gravity Directions")]
    [Space(5)]
    [SerializeField] private Quaternion directionRight;
    [SerializeField] private Quaternion directionLeft;
    [SerializeField] private Quaternion directionBack;
    [SerializeField] private Quaternion directionFront;

    private Quaternion defaultRotation;

    #endregion


  
    public ICharacterState CurrentState { get; private set; }
    public ICharacterState OldState { get; private set; }

    [SerializeField] private float GroundCheckDistance = 0.1f;
    [SerializeField] private LayerMask GroundLayer;

    #region COMPONENTS
    private Animator _animator;
    private GameCamera _gameCamera;
    private Rigidbody _rigidbody;
    public Animator Animator => _animator = _animator != null ? _animator : GetComponent<Animator>();
    public Rigidbody Rigidbody => _rigidbody = _rigidbody != null ? _rigidbody : GetComponent<Rigidbody>();
    public GameCamera GameCamera => _gameCamera = _gameCamera != null ? _gameCamera : GameCamera.Instance;
    #endregion

   


 
    #region GRAVITY 

    public Vector3 GravityDirection = Vector3.down;
    public float GravityStrength = 9.81f;

    #endregion

    [Space(5)]
    public Timer FallTimer;
    [Space(5)]
    [Header("Events")]
    public UnityEvent<Vector3> OnGravitChange;
    public UnityEvent<int> OnCollect;

    public bool IsGrounded
    {
        get
        {
            return Physics.Raycast(FootTransform.position, GravityDirection, GroundCheckDistance, GroundLayer);
        }
    }
    public bool IsMoving => (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Epsilon || Mathf.Abs(Input.GetAxis("Vertical")) > Mathf.Epsilon);
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
        gHologram.SetActive(false);
    }

    private void Start()
    {
        SetState(new IdleState());

        FallTimer = new Timer(3f);
        FallTimer.StartTimer();

        FallTimer.OnTimerFinished += GameManager.Instance.GameOver;

        defaultRotation = transform.rotation;
    }
    private void Update()
    {
        
        if (IsChangingGravity && CurrentState is not JumpingState)
        {
            SetState(new GravityManipulationState());
        }
        else
        {
            HandleMovement();
        }

        if (IsGrounded)
        {
            FallTimer.ResetTimer();
        }
        else
        {
            if (!FallTimer.IsRunning) FallTimer.StartTimer();

            FallTimer.Update(Time.deltaTime);
        }

        CurrentState?.UpdateState(this);

    }
    private void FixedUpdate()
    {
        SimulateGravity();
    }
    private void SimulateGravity()
    {
        Rigidbody.AddForce(GravityDirection.normalized * GravityStrength, ForceMode.Acceleration);
    }

    public void ChangeGravityDirection(Vector3 GravityDirection)
    {
        this.GravityDirection = GravityDirection;

        Debug.Log($"Changing Gravity Direction to {GravityDirection}");

        OnGravitChange?.Invoke(GravityDirection);

       
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
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime);
            elapsedTime += Time.deltaTime * 10f;
            yield return null;
        }
        transform.rotation = targetRotation;
    }

    public void SetState(ICharacterState state)
    {
        CurrentState?.ExitState(this);
        OldState = CurrentState;
        CurrentState = state;
        CurrentState?.EnterState(this);
    }
    private void HandleMovement()
    {
        if (IsMoving && IsGrounded)
        {
            SetState(new WalkingState());
           
        }

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

        Vector3 adjustedMovement = Quaternion.FromToRotation(Vector3.up, -GravityDirection) * movement;

        transform.position += adjustedMovement * speed * Time.deltaTime;
    }

    public void Rotate(float rotationSpeed)
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput);

        Vector3 adjustedMovement = Quaternion.FromToRotation(Vector3.up, -GravityDirection) * movement;

        if (adjustedMovement != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(adjustedMovement.normalized, -GravityDirection);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void ToggleHologram(bool tru)
    {
        gHologram.SetActive(tru);
        HologramParent.transform.rotation = defaultRotation;
    }
    public void RotateHoloToTarget(Quaternion targetRotation)
    {
        StartCoroutine(IRotateHoloToTarget(targetRotation));
    }

    private IEnumerator IRotateHoloToTarget(Quaternion targetRotation)
    {
        Quaternion initialRotation = HologramParent.rotation;
        float elapsedTime = 0f;

        while (elapsedTime < HoloSpeed)
        {
            Quaternion intermediateRotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / HoloSpeed);

            Vector3 eulerAngles = intermediateRotation.eulerAngles;
        
            HologramParent.rotation = Quaternion.Euler(eulerAngles);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Vector3 finalEulerAngles = targetRotation.eulerAngles;
      
        HologramParent.rotation = Quaternion.Euler(finalEulerAngles);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Collectable") && GameManager.Instance.CollectableCubes.Contains(collision.gameObject))
        {
            GameManager.Instance.CollectableCubes.Remove(collision.gameObject);
            Collect(collision.gameObject);
            collision.gameObject.GetComponent<Collider>().enabled = false;
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
