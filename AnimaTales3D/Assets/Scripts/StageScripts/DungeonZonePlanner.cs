using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DungeonGridManager의 순수 좌표/배치 계산 로직만 분리한 클래스.
/// MonoBehaviour나 실제 GameObject/Instantiate에 의존하지 않아 EditMode 테스트가 가능하다.
/// 알고리즘 자체는 기존 DungeonGridManager 구현과 동일하며, 위치만 옮겼다.
/// </summary>
public class DungeonZonePlanner
{
    private readonly int zoneRadius;

    public DungeonZonePlanner(int zoneRadius)
    {
        this.zoneRadius = zoneRadius;
    }

    /// <summary>
    /// 보스를 배치할 방향을 고른다. 진입 방향(들어온 쪽)은 제외해서
    /// "들어온 쪽으로 다시 보스가 붙는" 어색한 상황을 피한다.
    /// (첫 구역처럼 entryCoord가 없으면 완전 랜덤)
    /// </summary>
    public HexCoord PickBossDirection(HexCoord center, HexCoord? entryCoord)
    {
        var directions = HexCoord.Directions;

        if (entryCoord == null)
        {
            return directions[Random.Range(0, directions.Length)];
        }

        // entryCoord는 항상 "center + 어떤방향*radius" 형태로 생성되므로 나눗셈으로 정확히 역산 가능.
        HexCoord diff = entryCoord.Value - center;
        HexCoord towardEntry = new HexCoord(
            zoneRadius != 0 ? diff.q / zoneRadius : diff.q,
            zoneRadius != 0 ? diff.r / zoneRadius : diff.r
        );

        HexCoord picked;
        int guard = 0; // 무한루프 방지
        do
        {
            picked = directions[Random.Range(0, directions.Length)];
            guard++;
        }
        while (picked == towardEntry && guard < 20);

        return picked;
    }

    /// <summary>
    /// 좌표 하나의 타일 타입을 결정한다. bossCoord/entryCoord/center 우선순위는 원본과 동일.
    /// </summary>
    public HexTileType DecideTileType(HexCoord coord, HexCoord center, HexCoord bossCoord, HexCoord? entryCoord, float wallChance, float battleChance)
    {
        if (coord == bossCoord) return HexTileType.Boss;
        if (entryCoord.HasValue && coord == entryCoord.Value) return HexTileType.Empty; // 진입로는 항상 통행 가능
        if (!entryCoord.HasValue && coord == center) return HexTileType.Start; // 첫 구역의 시작점

        float roll = Random.value;
        if (roll < wallChance) return HexTileType.Wall;
        if (roll < wallChance + battleChance) return HexTileType.Battle;
        return HexTileType.Empty;
    }

    /// <summary>
    /// candidateCenter를 중심으로 radius 범위의 타일들이 occupied(이미 점유된 좌표)와 하나라도 겹치는지 검사한다.
    /// </summary>
    public bool ZoneRangeOverlaps(HexCoord candidateCenter, int radius, ICollection<HexCoord> occupied)
    {
        foreach (var coord in HexCoord.GetRange(candidateCenter, radius))
        {
            if (occupied.Contains(coord)) return true;
        }
        return false;
    }

    /// <summary>
    /// target이 center로부터 "정확히 radius칸 만큼 6방향 중 하나"로 떨어져 있을 때, 그 방향의 인덱스를 반환한다.
    /// 해당하지 않으면 -1.
    /// </summary>
    public int FindDirectionIndex(HexCoord center, HexCoord target, int radius)
    {
        if (radius == 0) return -1;

        HexCoord diff = target - center;
        if (diff.q % radius != 0 || diff.r % radius != 0) return -1;

        HexCoord unit = new HexCoord(diff.q / radius, diff.r / radius);
        var directions = HexCoord.Directions;
        for (int i = 0; i < directions.Length; i++)
        {
            if (directions[i] == unit) return i;
        }
        return -1;
    }

    public static void ShuffleInPlace(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[i], list[j]);
        }
    }

    /// <summary>
    /// 보스 타일을 기준으로 occupied(기존에 생성된 모든 구역의 점유 좌표)와 절대 겹치지 않는
    /// 다음 구역의 중심 좌표를 탐색한다.
    ///
    /// 1차 시도: 보스가 바라보는 방향 기준 좌우 60도 이내(같은 방향 포함 3방향)로만 확장.
    /// 이 범위는 직전 구역과 수학적으로 겹치지 않음이 보장되는 방향이라 대부분 여기서 바로 성공한다.
    /// 혹시 다른 구역과 우연히 겹치는 경우를 대비해, 실패 시 gap을 늘리고 6방향 전체로 넓혀가며
    /// "반드시" 겹치지 않는 자리를 찾을 때까지 재시도한다.
    /// </summary>
    public bool TryFindNonOverlappingZonePlacement(
        HexCoord bossCoord, HexCoord currentZoneCenter, int currentZoneRadius,
        ICollection<HexCoord> occupied,
        out HexCoord newCenter, out HexCoord entryCoord)
    {
        var directions = HexCoord.Directions;
        int bossDirIndex = FindDirectionIndex(currentZoneCenter, bossCoord, currentZoneRadius);

        List<int> candidateIndices = new List<int>();
        if (bossDirIndex >= 0)
        {
            candidateIndices.Add(bossDirIndex);
            candidateIndices.Add((bossDirIndex + 1) % 6);
            candidateIndices.Add((bossDirIndex + 5) % 6);
        }
        else
        {
            for (int i = 0; i < 6; i++) candidateIndices.Add(i);
        }
        ShuffleInPlace(candidateIndices);

        const int maxGapExtension = 6;
        for (int gap = currentZoneRadius + 1; gap <= currentZoneRadius + 1 + maxGapExtension; gap++)
        {
            foreach (var dirIndex in candidateIndices)
            {
                HexCoord dir = directions[dirIndex];
                HexCoord candidateCenter = bossCoord + dir * gap;

                if (!ZoneRangeOverlaps(candidateCenter, currentZoneRadius, occupied))
                {
                    newCenter = candidateCenter;
                    entryCoord = bossCoord + dir;
                    return true;
                }
            }

            // 선호 방향(±60도)에서 자리를 못 찾으면 이후로는 6방향 전체로 넓혀서 재시도.
            if (candidateIndices.Count < 6)
            {
                candidateIndices.Clear();
                for (int i = 0; i < 6; i++) candidateIndices.Add(i);
                ShuffleInPlace(candidateIndices);
            }
        }

        newCenter = default;
        entryCoord = default;
        return false;
    }
}
