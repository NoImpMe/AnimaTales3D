using System;
using UnityEngine;

/// <summary>
/// 전투 대형 배치 규칙 — 최대 3:3, 최소 1:1, 좌측 아군/우측 적군 (사람 지시, LOG #16).
/// 2D 원본(AllyBattleSetting.SpawnAlly/EnemyBattleSetting.SpawnEnemy)은 인원수별 가로 간격 공식
/// (3명: i*3.5-3.5, 2명: i*3.5-1.75, 1명: 0)으로 아군은 아래 줄(y=-2.2), 적군은 위 줄(y=1.2)에
/// 배치했다 — "간격 공식"은 그대로 재사용하고, "줄(행) 구분 축"만 3D 쿼터뷰에 맞춰 새로 매핑했다:
/// 원본의 가로 간격 축(X)은 여기서 진영 내 슬롯이 늘어서는 축(Z)에, 원본의 상/하 행 구분 축(Y)은
/// 여기서 좌/우 진영 구분 축(X)에 대응한다.
/// </summary>
public static class BattleFormation
{
    public const int MinUnitsPerSide = 1;
    public const int MaxUnitsPerSide = 3;

    private const float SlotSpacing = 3.5f;

    /// <summary>count명 중 슬롯 index(0-based)의 진영 내 간격축 오프셋. 2D 원본 간격 공식과 동일.</summary>
    public static float GetSlotOffset(int index, int count)
    {
        if (count < MinUnitsPerSide || count > MaxUnitsPerSide)
            throw new ArgumentOutOfRangeException(nameof(count), $"전투 인원은 {MinUnitsPerSide}~{MaxUnitsPerSide}명이어야 합니다.");
        if (index < 0 || index >= count)
            throw new ArgumentOutOfRangeException(nameof(index));

        return count switch
        {
            3 => index * SlotSpacing - 3.5f,
            2 => index * SlotSpacing - 1.75f,
            _ => 0f,
        };
    }

    /// <summary>아군 슬롯의 로컬 위치 — 진영 중심에서 좌측(-X)으로 sideOffset만큼 떨어진 자리.</summary>
    public static Vector3 GetAllySlotPosition(int index, int count, float sideOffset)
        => new Vector3(-sideOffset, 0f, GetSlotOffset(index, count));

    /// <summary>적군 슬롯의 로컬 위치 — 진영 중심에서 우측(+X)으로 sideOffset만큼 떨어진 자리.</summary>
    public static Vector3 GetEnemySlotPosition(int index, int count, float sideOffset)
        => new Vector3(sideOffset, 0f, GetSlotOffset(index, count));
}
