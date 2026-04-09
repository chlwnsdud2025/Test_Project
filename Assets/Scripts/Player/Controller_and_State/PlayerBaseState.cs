using Unity.VisualScripting;
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



    //  부모의 회전 함수를 3D 액션(카메라 기준)에 맞게 덮어씁니다.
    protected virtual void HandleRotation()
    {
        if (player.moveInput != Vector2.zero)
        {
            // 카메라가 바라보는 방향 중 수평 방향(Y=0)만 추출
            Vector3 camForward = player.cameraTransform.forward;
            Vector3 camRight = player.cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            // 입력에 따른 절대적인 이동 방향 계산
            Vector3 moveDir = (camRight * player.moveInput.x + camForward * player.moveInput.y).normalized;

            if (moveDir != Vector3.zero)
            {
                // 모델(또는 플레이어 전체)이 이동 방향을 부드럽게 바라보게 함
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                player.model.transform.rotation = Quaternion.Slerp(
                    player.model.transform.rotation,
                    targetRotation,
                    15f * Time.deltaTime // 회전 속도 조절
                );
            }
        }
    }


}