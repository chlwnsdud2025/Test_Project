using UnityEngine;

public class AttackState : PlayerBaseState
{
    private int comboIndex = 0;
    private bool comboInputReceived = false;

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        comboInputReceived = false;
        player.animator.applyRootMotion = true;

        // "Attack1", "Attack2" 등의 이름으로 재생 (OverrideController에 의해 실제 Clip이 재생됨)
        string stateName = $"Attack{comboIndex + 1}";
        player.animator.CrossFadeInFixedTime(stateName, 0.1f);
    }

    public override void Update()
    {
        // 1. 콤보 전환 로직
        if (comboInputReceived && player.canNextAttack)
        {
            // 다음 타수가 있는지 무기 데이터 확인
            if (comboIndex + 1 < player.currentWeapon.comboClips.Length)
            {
                comboIndex++;
                stateMachine.ChangeState(this); // 자기 자신으로 재진입 (Enter 호출)
                return;
            }
        }

        // 2. 이동 캔슬 로직
        if (player.moveInput != Vector2.zero && player.canCancel)
        {
            comboIndex = 0; // 콤보 초기화
            stateMachine.ChangeState(player.moveState);
            return;
        }

        // 3. 자연 종료 (애니메이션 완료)
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        if (!player.animator.IsInTransition(0) && stateInfo.normalizedTime >= 0.95f)
        {
            comboIndex = 0;
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void OnAttackInput()
    {
        // 비헤이비어가 허용한 구간에서만 입력 예약
        if (player.canCombo)
        {
            comboInputReceived = true;
            Debug.Log($"{comboIndex + 1}타 중 다음 콤보 예약됨!");
        }
    }

    public override void Exit()
    {
        // 상태를 완전히 빠져나갈 때 (공격이 끝났을 때)만 초기화되지 않도록 주의
        // (자기 자신으로 재진입할 때는 Index를 유지해야 함)
    }
    public override void OnDashInput() { /* 무시 */ }
}
