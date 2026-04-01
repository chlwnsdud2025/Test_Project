using UnityEngine;

public class AttackState : PlayerBaseState
{
    private readonly int attackAnimHash = Animator.StringToHash("Attack");
    private readonly int attack2AnimHash = Animator.StringToHash("Attack2");

    private bool comboInputReceived = false; // 콤보 입력 여부 저장

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Attack1");
        comboInputReceived = false;
        player.animator.applyRootMotion = true;

        player.animator.CrossFadeInFixedTime(attackAnimHash, 0.1f);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.shortNameHash == attackAnimHash && !player.animator.IsInTransition(0))
        {
            float progress = stateInfo.normalizedTime;

            // --- 핵심: 콤보 전환 로직 ---
            // 입력은 OnAttackInput에서 미리 받아두고(comboInputReceived),
            // 실제 전환은 애니메이션이 최소 0.6f(타격 이후)만큼은 재생되었을 때 수행함
            if (progress >= 0.7f && progress <= 0.8f)
            {
                if (comboInputReceived)
                {
                    Debug.Log("공격2!");
                    //stateMachine.ChangeState(player.);
                    return;
                }
            }

            // --- 이동 캔슬 로직 (콤보보다 뒤에 배치) ---
            if (progress >= 0.5f && progress < 0.95f)
            {
                if (player.moveInput != Vector2.zero)
                {
                    stateMachine.ChangeState(player.moveState);
                    return;
                }
            }

            // --- 자연 종료 ---
            if (progress >= 0.95f)
            {
                stateMachine.ChangeState(player.idleState);
            }
        }
    }
    public override void Exit()
    {
        player.animator.applyRootMotion = false;

        if (player.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // 공격 중 공격 버튼을 눌렀을 때 처리
    public override void OnAttackInput()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        float progress = stateInfo.normalizedTime;

        // 0.2f부터 0.7f 사이에 클릭했다면 "예약"만 해둠
        if (progress >= 0.2f && progress <= 0.8f)
        {
            comboInputReceived = true;
            Debug.Log("다음 콤보 예약됨!");
        }
    }
    public override void OnDashInput() { /* 무시 */ }
}
