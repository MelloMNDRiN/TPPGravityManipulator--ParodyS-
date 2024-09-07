using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class Character : MonoBehaviour
{
    #region MOVEMENT

    private float MovementSpeed;
    private float RotationSpeed;

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

    [SerializeField] private Transform FootTransform;

    public ICharacterState CurrentState { get; private set; }


    private Animator _animator;
    private GameCamera _gameCamera;
    private Rigidbody _rigidbody;
    public Animator Animator => _animator = _animator != null ? _animator : GetComponent<Animator>();
    public Rigidbody Rigidbody => _rigidbody = _rigidbody != null ? _rigidbody : GetComponent<Rigidbody>();
    public GameCamera GameCamera => _gameCamera = _gameCamera != null ? _gameCamera : GameCamera.Instance;


    [SerializeField] private float GroundCheckDistance = 0.1f;
    [SerializeField] private LayerMask GroundLayer;


    #region GRAVITY 

    public Vector3 GravityDirection = Vector3.down;
    public float GravityStrength = 9.81f;

    #endregion



    public bool IsGrounded
    {
        get
        {

            Vector3 raycastStart = FootTransform.position;

            return Physics.Raycast(raycastStart, Vector3.down, GroundCheckDistance, GroundLayer) ||
                   Physics.Raycast(raycastStart + Vector3.right * 0.1f, Vector3.down, GroundCheckDistance, GroundLayer) ||
                   Physics.Raycast(raycastStart - Vector3.right * 0.1f, Vector3.down, GroundCheckDistance, GroundLayer) ||
                   Physics.Raycast(raycastStart + Vector3.forward * 0.1f, Vector3.down, GroundCheckDistance, GroundLayer) ||
                   Physics.Raycast(raycastStart - Vector3.forward * 0.1f, Vector3.down, GroundCheckDistance, GroundLayer);
        }
    }
    public bool IsMoving => Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Epsilon || Mathf.Abs(Input.GetAxis("Vertical")) > Mathf.Epsilon;
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

        defaultRotation = transform.rotation;
    }
    private void Update()
    {
        CurrentState?.UpdateState(this);

        if (IsChangingGravity)
        {
            SetState(new GravityManipulationState());
        }
        else
        {
            HandleMovement();
        }
        
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

        StartCoroutine(AlignPlayerWithGravityCoroutine());
    }
    private IEnumerator AlignPlayerWithGravityCoroutine()
    {
        Vector3 upDirection = - GravityDirection.normalized;
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, upDirection) * transform.rotation;

        float elapsedTime = 0f;
        Quaternion initialRotation = transform.rotation;

        while (elapsedTime < 1f)
        {
            transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime);
            elapsedTime += Time.deltaTime * RotationSpeed;
            yield return null;
        }
        transform.rotation = targetRotation;
    }
    public void SetState(ICharacterState state)
    {

        CurrentState?.ExitState(this);
        CurrentState = state;
        CurrentState?.EnterState(this);

    }
    private void HandleMovement()
    {
        if(IsMoving && IsGrounded)
        {
            SetState(new WalkingState());
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            SetState(new JumpingState());
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
            HologramParent.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / HoloSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        HologramParent.rotation = targetRotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawSphere(FootTransform.position, 0.1f);
    }
}
