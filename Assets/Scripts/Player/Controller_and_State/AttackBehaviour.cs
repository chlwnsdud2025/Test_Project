using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    [Header("타이밍 설정 (0.0 ~ 1.0)")]
    public float comboWindowStart = 0.2f;
    public float comboWindowEnd = 0.7f;
    public float comboTransitionPoint = 0.6f; // 실제 다음 공격으로 넘어가는 시점
    public float cancelWindowStart = 0.7f;

    private PlayerController player;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) player = animator.GetComponent<PlayerController>();

        // 상태 진입 시 플래그 초기화
        player.canCombo = false;
        player.canNextAttack = false;
        player.canCancel = false;
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        float progress = stateInfo.normalizedTime;

        // 1. 콤보 입력 가능 구간 (입력 버퍼링용)
        player.canCombo = (progress >= comboWindowStart && progress <= comboWindowEnd);

        // 2. 실제 다음 공격으로 전환 가능한 시점
        player.canNextAttack = (progress >= comboTransitionPoint);

        // 3. 이동/구르기 캔슬 가능 구간
        player.canCancel = (progress >= cancelWindowStart);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (player == null) return;

        // 애니메이션이 끝나거나 끊길 때 안전하게 초기화
        player.canCombo = false;
        player.canNextAttack = false;
        player.canCancel = false;

        // Root Motion 및 물리 초기화 (필요 시)
        player.animator.applyRootMotion = false;
        player.ResetVelocity();
    }
}