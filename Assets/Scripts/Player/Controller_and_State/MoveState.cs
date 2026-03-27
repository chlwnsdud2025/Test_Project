using UnityEngine;

public class MoveState : PlayerBaseState
{
    private readonly int runAnimHash = Animator.StringToHash("Run");
    public MoveState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public override void Enter() {
        Debug.Log("ป๓ลย: Move");

        player.animator.CrossFadeInFixedTime(runAnimHash, 0.1f);

    }

    public override void Update()
    {
        if (player.moveInput == Vector2.zero)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        HandleRotation();

        Vector3 moveDir = new Vector3(player.moveInput.x, 0f, player.moveInput.y);
        player.transform.Translate(moveDir * player.moveSpeed * Time.deltaTime, Space.World);
    }
}
