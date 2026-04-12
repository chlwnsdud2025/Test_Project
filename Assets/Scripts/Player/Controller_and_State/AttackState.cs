using UnityEngine;
using static Attack_SO_Data;

public class AttackState : PlayerBaseState
{
    private int comboIndex = 0;
    private bool comboInputReceived = false;
    private bool isContinuingCombo = false;

    private float trackingWindow = 0.3f;

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        isContinuingCombo = false;
        comboInputReceived = false;

        // 진입할 때 모든 플래그 초기화
        player.canCombo = false;
        player.canNextAttack = false;
        player.canCancel = false;

        player.animator.applyRootMotion = true;

        string stateName = $"Attack{comboIndex + 1}";
        player.animator.CrossFadeInFixedTime(stateName, 0.1f);
    }

    public override void Update()
    {
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        float progress = stateInfo.normalizedTime;

        // 현재 재생 중인 공격의 타이밍 데이터를 가져옵니다.
        AttackData currentAttackData = player.currentWeapon.comboAttacks[comboIndex];

        // 트랜지션 중이 아닐 때만 플래그 업데이트 (기존 AttackBehaviour의 역할 대체)
        if (!player.animator.IsInTransition(0))
        {
            player.canCombo = (progress >= currentAttackData.comboWindowStart && progress <= currentAttackData.comboWindowEnd);
            player.canNextAttack = (progress >= currentAttackData.comboTransitionPoint);
            player.canCancel = (progress >= currentAttackData.cancelWindowStart);
        }
        else
        {
            player.canCombo = false;
            player.canNextAttack = false;
            player.canCancel = false;
        }

        // 1. 공격 초반 방향 보정 (Attack Tracking)
        if (player.animator.IsInTransition(0) || progress <= trackingWindow)
        {
            if (player.moveInput != Vector2.zero)
            {
                Vector3 camForward = player.cameraTransform.forward;
                Vector3 camRight = player.cameraTransform.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();

                Vector3 attackDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;

                if (attackDir != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(attackDir);
                    player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 15f * Time.deltaTime);
                    player.model.transform.localRotation = Quaternion.identity;
                }
            }
        }

        // 2. 콤보 전환 로직
        if (comboInputReceived && player.canNextAttack)
        {
            if (comboIndex + 1 < player.currentWeapon.comboAttacks.Length)
            {
                comboIndex++;
                isContinuingCombo = true;
                stateMachine.ChangeState(this);
                return;
            }
        }

        // 3. 이동/회피 캔슬 로직
        if (player.canCancel)
        {
            if (player.moveInput != Vector2.zero)
            {
                stateMachine.ChangeState(player.moveState);
                return;
            }
        }

        // 4. 자연 종료
        if (!player.animator.IsInTransition(0) && progress >= 0.95f)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void OnAttackInput()
    {
        if (player.canCombo)
        {
            comboInputReceived = true;
            Debug.Log($"{comboIndex + 1}타 중 다음 콤보 예약됨!");
        }
    }

    public override void Exit()
    {
        if (!isContinuingCombo)
        {
            comboIndex = 0;
            player.animator.applyRootMotion = false;
        }
    }

    public override void OnDashInput() { /* 무시 */ }
}
