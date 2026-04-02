using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 5f;

    // 입력 시스템
    private InputSystem_Actions controls;
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector2 mouseMoveInput;


    [Header("Look Settings")]
    public float mouseSensitivity = 10f; // 마우스 감도
    public Transform cameraTransform;    // 상하 회전을 시킬 카메라(또는 머리) 오브젝트

    [Header("Components")]
    public Animator animator;
    private Rigidbody rb;
    //무기 교체
    public weapon_1_ob currentWeapon;

    private AnimatorOverrideController overrideController;

    [HideInInspector] public bool canCombo;      // 현재 클릭 시 예약 가능 여부
    [HideInInspector] public bool canNextAttack; // 실제 다음 애니메이션 전환 가능 시점
    [HideInInspector] public bool canCancel;     // 이동/회피로 캔슬 가능 여부

    // 핵심: 분리된 상태 머신 객체
    public StateMachine stateMachine { get; private set; }

    // 생성해둔 상태들 (캐싱)
    public IdleState idleState { get; private set; }
    public MoveState moveState { get; private set; }
    public AttackState attackState { get; private set; }
    public DodgeState dashState { get; private set; }

    private void Awake()
    {
        controls = new InputSystem_Actions();
        stateMachine = new StateMachine();

        // 1. 상태 인스턴스화 (자신과 상태머신을 넘겨줌)
        idleState = new IdleState(this, stateMachine);
        moveState = new MoveState(this, stateMachine);
        attackState = new AttackState(this, stateMachine);
        dashState = new DodgeState(this, stateMachine);

        // 2. New Input System 이벤트 연결
        // Controller가 직접 상태를 바꾸지 않고, 현재 상태에게 "공격버튼 눌렸어!"라고 전달만 합니다.
        controls.PlayerMovement.Attack.performed += ctx => stateMachine.CurrentState.OnAttackInput();

        // (만약 Dash 액션을 만드셨다면 아래처럼 연결합니다)
        controls.PlayerMovement.Dodge.performed += ctx => stateMachine.CurrentState.OnDashInput();

        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;
    }

    private void Start()
    {
        if (currentWeapon != null) EquipWeapon(currentWeapon);
        // 게임 시작 시 초기 상태 지정
        stateMachine.Initialize(idleState);
    }

    private void Update()
    {
        // 매 프레임 입력값 읽기
        moveInput = controls.PlayerMovement.Move.ReadValue<Vector2>();
        mouseMoveInput = controls.PlayerMovement.Look.ReadValue<Vector2>();


        // 상태 머신 실행
        stateMachine.Update();
    }

    public void EquipWeapon(weapon_1_ob newData)
    {
        currentWeapon = newData;

        // 무기 데이터에 있는 클립들을 애니메이터 노드에 매핑
        // "Attack1"이라는 노드 이름을 실제 파일(newData.comboClips[0])로 교체
        for (int i = 0; i < newData.comboClips.Length; i++)
        {
            overrideController[$"Attack{i + 1}"] = newData.comboClips[i];
        }
    }

    public void ResetVelocity()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnEnable() { controls.Enable(); }
    private void OnDisable() { controls.Disable(); }
}
