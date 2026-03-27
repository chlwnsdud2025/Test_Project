using UnityEngine;

public class AttackState : PlayerBaseState
{
    private float timer;
    private float duration = 0.5f;

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Attack");
        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration) // 공격 끝남
        {
            if (player.moveInput != Vector2.zero) stateMachine.ChangeState(player.moveState);
            else stateMachine.ChangeState(player.idleState);
        }
    }

    //예외 처리: 공격 중에는 또 공격하거나 대쉬할 수 없도록 부모의 기능을 무시(Override)합니다!
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
}
