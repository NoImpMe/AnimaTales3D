using UnityEngine;

/// <summary>
/// 전투 유닛 1개의 임시 비주얼 — 2D 원본 스프라이트를 그대로 재사용한 2.5D 빌보드(타일 전투 아트와
/// 동일한 방식, LOG #11 참고). 실제 3D 모델로 교체되기 전까지의 자리표시자(placeholder)다.
/// </summary>
public class BattleUnitVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    public AnimaUnit BoundUnit { get; private set; }

    public void Bind(AnimaUnit unit)
    {
        BoundUnit = unit;
        Sprite sprite = Resources.Load<Sprite>($"Anima/{unit.Objectfile}");
        if (sprite == null)
        {
            Debug.LogWarning($"[BattleUnitVisual] '{unit.Objectfile}' 스프라이트를 찾을 수 없습니다(아직 이식되지 않은 유닛일 수 있음). 비주얼 없이 진행합니다.");
            return;
        }
        spriteRenderer.sprite = sprite;
    }
}
