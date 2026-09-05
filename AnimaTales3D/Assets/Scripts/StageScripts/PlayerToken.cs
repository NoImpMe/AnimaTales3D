using System.Collections;
using UnityEngine;

/// <summary>
/// 던전 그리드 위의 플레이어 위치를 나타내는 토큰.
/// 그레이박스 단계에서는 Capsule에 부착해서 사용한다.
/// 나중에 3D 파티 대표 캐릭터 모델로 교체할 때도 이 컴포넌트는 그대로 유지 가능.
/// </summary>
public class PlayerToken : MonoBehaviour
{
    private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");

    [SerializeField] private float moveDuration = 0.35f;
    [SerializeField] private float hopHeight = 0.4f; // 타일 이동 시 살짝 튀는 느낌 (연출용)

    private Coroutine moveRoutine;
    private Animator animator;

    private void Awake()
    {
        // Animator가 없는 모델(그레이박스 Capsule 등)에서도 동작해야 하므로 null 허용.
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// 즉시 위치 이동 (던전 최초 생성 시 배치용).
    /// </summary>
    public void WarpTo(Vector3 tilePosition)
    {
        transform.position = tilePosition + Vector3.up * 0.5f;
    }

    /// <summary>
    /// 부드럽게 타일 간 이동. Animator가 있으면 이동 시작/종료에 맞춰 "IsMoving"을 토글한다.
    /// </summary>
    public void MoveTo(Vector3 tilePosition)
    {
        // 비활성 오브젝트에서는 코루틴을 시작할 수 없으므로 안전장치로 즉시 이동 처리.
        // (Player Token 필드에 Project 창의 프리팹 에셋을 그대로 연결했을 때 발생하는
        //  "Coroutine couldn't be started because the game object is inactive" 방지)
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[PlayerToken] 비활성 상태에서 MoveTo 호출됨. Player Token이 " +
                "Hierarchy(씬)의 오브젝트가 아니라 Project 창의 프리팹을 참조하고 있는지 확인하세요.");
            transform.position = tilePosition + Vector3.up * 0.5f;
            return;
        }

        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine(tilePosition + Vector3.up * 0.5f));
    }

    private IEnumerator MoveRoutine(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        if (animator != null) animator.SetBool(IsMovingParam, true);

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;

            Vector3 flatPos = Vector3.Lerp(startPos, targetPos, t);
            flatPos.y += Mathf.Sin(t * Mathf.PI) * hopHeight; // 포물선 형태로 살짝 튐

            transform.position = flatPos;
            yield return null;
        }

        transform.position = targetPos;

        if (animator != null) animator.SetBool(IsMovingParam, false);
    }
}
