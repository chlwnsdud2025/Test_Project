using UnityEngine;

public class DodgeState : PlayerBaseState
{
    private readonly int dodgeAnimHash = Animator.StringToHash("Roll");

    public DodgeState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Roll");
        player.animator.applyRootMotion = true;
        player.animator.CrossFadeInFixedTime(dodgeAnimHash, 0.1f);

    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);

        // 1. 여기서 상태 전환 조건을 체크합니다.
        if (stateInfo.shortNameHash == dodgeAnimHash && !player.animator.IsInTransition(0))
        {
            if (stateInfo.normalizedTime >= 0.75f)
            {
                // 여기서 딱 한 번만 ChangeState를 호출합니다.
                if (player.moveInput != Vector2.zero)
                    stateMachine.ChangeState(player.moveState);
                else
                    stateMachine.ChangeState(player.idleState);
            }
        }
    }

    // 2. Exit()는 StateMachine.ChangeState()에 의해 '자동으로' 호출됩니다.
    public override void Exit()
    {
        // 여기서는 정리 작업만 합니다. 절대 ChangeState를 또 부르면 안 됩니다!
        player.animator.applyRootMotion = false;

        if (player.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // 대쉬 중에도 공격/대쉬 중복 입력 방지
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
}
