using UnityEngine;

public class DodgeState : PlayerBaseState
{
    private readonly int dodgeAnimHash = Animator.StringToHash("Roll");

    // 선입력(버퍼) 상태를 저장할 변수들
    private bool attackBuffered = false;
    private bool dodgeBuffered = false;

    public DodgeState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Roll");

        // 상태 진입 시 무적 플래그 및 선입력 데이터 초기화
        player.isInvincible = false;
        attackBuffered = false;
        dodgeBuffered = false;

        // 방향 전환 로직 (기존과 동일)
        if (player.moveInput != Vector2.zero)
        {
            Vector3 camForward = player.cameraTransform.forward;
            Vector3 camRight = player.cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 rollDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;

            if (rollDir != Vector3.zero)
            {
                player.transform.rotation = Quaternion.LookRotation(rollDir);
                player.model.transform.localRotation = Quaternion.identity;
            }
        }

        player.animator.applyRootMotion = true;
        player.animator.CrossFadeInFixedTime(dodgeAnimHash, 0.1f);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        float progress = stateInfo.normalizedTime;
        Roll_SO_Data dodgeData = player.currentDodgeData;

        if (stateInfo.shortNameHash == dodgeAnimHash && !player.animator.IsInTransition(0))
        {
            // 1. 무적(I-Frame) 판정 로직
            player.isInvincible = (progress >= dodgeData.iframeStart && progress <= dodgeData.iframeEnd);

            // 2. 선입력 실행 및 캔슬 로직 (cancelWindow 도달 시)
            if (progress >= dodgeData.cancelWindow)
            {
                // 예약된 공격이 있다면 공격 상태로 전환!
                if (attackBuffered)
                {
                    stateMachine.ChangeState(player.attackState);
                    return;
                }
                // 예약된 구르기가 있다면 연속 구르기 실행!
                else if (dodgeBuffered)
                {
                    stateMachine.ChangeState(player.dashState);
                    return;
                }
                // 예약된 버튼은 없지만 이동 키를 누르고 있다면 이동 상태로 전환
                else if (player.moveInput != Vector2.zero)
                {
                    stateMachine.ChangeState(player.moveState);
                    return;
                }
            }

            // 3. 자연 종료 로직 (아무 입력도 없이 animationEnd 도달 시 Idle로 전환)
            if (progress >= dodgeData.animationEnd)
            {
                stateMachine.ChangeState(player.idleState);
            }
        }
    }

    public override void Exit()
    {
        player.isInvincible = false;
        player.animator.applyRootMotion = false;
    }

    // [핵심 변경점] 입력을 무시하지 않고 선입력 기간인지 체크하여 예약합니다.
    public override void OnAttackInput()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        float progress = stateInfo.normalizedTime;

        // 현재 애니메이션이 선입력을 받을 수 있는 구간이라면 버퍼에 저장
        if (progress >= player.currentDodgeData.preInputWindowStart)
        {
            attackBuffered = true;
            Debug.Log("구르기 중 공격 선입력 완료!");
        }
    }

    public override void OnDashInput()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        float progress = stateInfo.normalizedTime;

        // 구르기 연타 시 다음 구르기 예약
        if (progress >= player.currentDodgeData.preInputWindowStart)
        {
            dodgeBuffered = true;
            Debug.Log("구르기 연속 선입력 완료!");
        }
    }
}
