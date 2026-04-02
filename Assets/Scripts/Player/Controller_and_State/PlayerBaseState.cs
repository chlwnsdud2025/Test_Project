using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerController player;
    protected StateMachine stateMachine;

    public PlayerBaseState(PlayerController player, StateMachine stateMachine)
    {
        this.player = player;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }

    public virtual void OnAttackInput()
    {
        stateMachine.ChangeState(player.attackState);
    }

    public virtual void OnDashInput()
    {
        stateMachine.ChangeState(player.dashState);
    }

    protected virtual void HandleRotation()
    {
        if (player.moveInput != Vector2.zero)
        {
            // 입력한 방향(moveInput)을 바라보도록 부드럽게 회전시키는 로직
            Vector3 targetDirection = new Vector3(player.moveInput.x, 0f, player.moveInput.y);
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, 30f * Time.deltaTime);
        }
    }


}