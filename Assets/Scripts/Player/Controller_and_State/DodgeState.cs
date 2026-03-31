using UnityEngine;

public class DodgeState : PlayerBaseState
{
    private readonly int dodgeAnimHash = Animator.StringToHash("Dodge");
    private float timer;
    private float duration = 0.8f;

    public DodgeState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter()
    {
        Debug.Log("상태: Dash");
        timer = 0f;
        player.animator.applyRootMotion = true;
        player.animator.CrossFade(dodgeAnimHash, 0.05f);

    }

    public override void Update()
    {
        timer += Time.deltaTime;
        if (timer >= duration) // 대쉬 끝남
        {
            player.animator.applyRootMotion = false;
            if (player.moveInput != Vector2.zero) stateMachine.ChangeState(player.moveState);
            else stateMachine.ChangeState(player.idleState);
        }
    }

    // 대쉬 중에도 공격/대쉬 중복 입력 방지
    public override void OnAttackInput() { /* 무시 */ }
    public override void OnDashInput() { /* 무시 */ }
}
