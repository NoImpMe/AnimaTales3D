using System;
using UnityEngine;

/// <summary>
/// 턴 순서를 좌우로 회전하는 회전체(캐러셀)로 표시하기 위한 배치 계산 — 원본에는 없던 신규 UI 로직
/// (사람 지시, LOG #17). 턴 큐(TurnManager가 정렬한 순서)의 슬롯들을 원형 호를 따라 배치하고,
/// 현재 턴인 유닛이 항상 정면(각도 0)에 오도록 회전체 전체를 돌리는 방식이다.
/// 실제 MonoBehaviour/UI 연결(회전 애니메이션, 아이콘 프리팹 등)은 아직 미착수 — 유닛 데이터 모델과
/// 씬 연결 단계에서 진행할 예정이며, 여기서는 좌표/각도 계산만 순수 함수로 분리해 테스트 가능하게 했다.
/// </summary>
public static class TurnOrderCarouselLayout
{
    /// <summary>
    /// 슬롯 index가 현재 턴(currentIndex)을 기준으로 몇 도 떨어져 있는지.
    /// 양수는 아직 차례가 안 온 뒤쪽(오른쪽) 유닛, 음수는 이미 지나간(왼쪽) 유닛을 뜻한다.
    /// </summary>
    public static float GetSlotAngleOffset(int index, int currentIndex, int totalCount, float angleStepDegrees)
    {
        if (totalCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(totalCount), "전체 슬롯 수는 1 이상이어야 합니다.");

        return (index - currentIndex) * angleStepDegrees;
    }

    /// <summary>
    /// 슬롯의 원형 호 위 로컬 위치. 각도 0(정면)은 -Z(카메라 쪽), 양의 각도는 +X(오른쪽)로 휘어진다.
    /// </summary>
    public static Vector3 GetSlotLocalPosition(int index, int currentIndex, int totalCount, float radius, float angleStepDegrees)
    {
        float angleRad = GetSlotAngleOffset(index, currentIndex, totalCount, angleStepDegrees) * Mathf.Deg2Rad;
        return new Vector3(radius * Mathf.Sin(angleRad), 0f, -radius * Mathf.Cos(angleRad));
    }

    /// <summary>현재 턴 유닛이 항상 정면(0도)에 오도록 회전체 전체에 적용해야 할 Y축 회전각.</summary>
    public static float GetCarouselYRotation(int currentIndex, float angleStepDegrees)
        => -currentIndex * angleStepDegrees;
}
