using UnityEngine;

public class AttackState : PlayerBaseState
{
    private int comboIndex = 0;
    private bool comboInputReceived = false;
    private bool isContinuingCombo = false; // 콤보 연속 수행 여부 플래그

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        // 새로운 타격 시작 시 플래그 초기화
        isContinuingCombo = false;
        comboInputReceived = false;

        player.animator.applyRootMotion = true;

        string stateName = $"Attack{comboIndex + 1}";
        player.animator.CrossFadeInFixedTime(stateName, 0.1f);
    }

    public override void Update()
    {
        // 1. 콤보 전환 로직
        if (comboInputReceived && player.canNextAttack)
        {
            if (comboIndex + 1 < player.currentWeapon.comboClips.Length)
            {
                comboIndex++;
                isContinuingCombo = true; // 다음 콤보로 이어짐을 표시
                stateMachine.ChangeState(this);
                return;
            }
        }

        // 2. 이동/회피 캔슬 로직
        if (player.canCancel)
        {
            if (player.moveInput != Vector2.zero)
            {
                stateMachine.ChangeState(player.moveState);
                return;
            }
            // (추가 제안) 여기서 구르기 입력 시 바로 전환되도록 구조를 확장할 수 있습니다.
        }

        // 3. 자연 종료
        AnimatorStateInfo stateInfo = player.animator.GetCurrentAnimatorStateInfo(0);
        if (!player.animator.IsInTransition(0) && stateInfo.normalizedTime >= 0.95f)
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
        // 콤보가 이어지는 상황이 아닐 때만(공격 세트가 완전히 끝났을 때만) 초기화
        if (!isContinuingCombo)
        {
            comboIndex = 0;
            player.animator.applyRootMotion = false;
            
        }
    }

    public override void OnDashInput() { /* 무시 */ }
}
