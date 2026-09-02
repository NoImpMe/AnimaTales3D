using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// DungeonZonePlanner(구역 배치 순수 로직)의 EditMode 테스트.
/// DungeonGridManager에서 분리되기 전에는 MonoBehaviour/Instantiate에 묶여 있어 테스트가 불가능했다.
/// </summary>
public class DungeonZonePlannerTests
{
    [Test]
    public void PickBossDirection_FirstZone_ReturnsOneOfSixDirections()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        Random.InitState(42);
        var dir = planner.PickBossDirection(new HexCoord(0, 0), entryCoord: null);
        Assert.Contains(dir, HexCoord.Directions);
    }

    [Test]
    public void PickBossDirection_WithEntry_NeverPointsBackToEntry()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var center = new HexCoord(0, 0);
        var entryDir = HexCoord.Directions[2];
        var entryCoord = center + entryDir * 3;

        // 여러 시드로 반복 검증 (가드 로직이 진입 방향을 피하는지 확인)
        for (int seed = 0; seed < 50; seed++)
        {
            Random.InitState(seed);
            var bossDir = planner.PickBossDirection(center, entryCoord);
            Assert.AreNotEqual(entryDir, bossDir, $"seed={seed}에서 보스 방향이 진입 방향과 같으면 안 됨");
        }
    }

    [Test]
    public void DecideTileType_BossCoordIsAlwaysBoss()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var boss = new HexCoord(2, 1);
        var type = planner.DecideTileType(boss, new HexCoord(0, 0), boss, entryCoord: null, wallChance: 1f, battleChance: 0f);
        Assert.AreEqual(HexTileType.Boss, type);
    }

    [Test]
    public void DecideTileType_EntryCoordIsAlwaysEmpty()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var entry = new HexCoord(1, 0);
        var type = planner.DecideTileType(entry, new HexCoord(0, 0), new HexCoord(5, 5), entryCoord: entry, wallChance: 1f, battleChance: 0f);
        Assert.AreEqual(HexTileType.Empty, type);
    }

    [Test]
    public void DecideTileType_FirstZoneCenterIsStart()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var center = new HexCoord(0, 0);
        var type = planner.DecideTileType(center, center, new HexCoord(5, 5), entryCoord: null, wallChance: 1f, battleChance: 0f);
        Assert.AreEqual(HexTileType.Start, type);
    }

    [Test]
    public void ZoneRangeOverlaps_DetectsOverlap()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var occupied = new HashSet<HexCoord>(HexCoord.GetRange(new HexCoord(0, 0), 2));
        bool overlaps = planner.ZoneRangeOverlaps(new HexCoord(1, 0), 1, occupied);
        Assert.IsTrue(overlaps);
    }

    [Test]
    public void ZoneRangeOverlaps_NoOverlapWhenFarAway()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var occupied = new HashSet<HexCoord>(HexCoord.GetRange(new HexCoord(0, 0), 2));
        bool overlaps = planner.ZoneRangeOverlaps(new HexCoord(20, 20), 1, occupied);
        Assert.IsFalse(overlaps);
    }

    [Test]
    public void FindDirectionIndex_FindsExactDirection()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var center = new HexCoord(0, 0);
        int radius = 3;
        for (int i = 0; i < HexCoord.Directions.Length; i++)
        {
            var target = center + HexCoord.Directions[i] * radius;
            Assert.AreEqual(i, planner.FindDirectionIndex(center, target, radius));
        }
    }

    [Test]
    public void FindDirectionIndex_ReturnsMinusOneForNonAlignedTarget()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var center = new HexCoord(0, 0);
        var offCoord = new HexCoord(1, 1); // radius 3 방향 어디에도 안 맞음
        Assert.AreEqual(-1, planner.FindDirectionIndex(center, offCoord, 3));
    }

    [Test]
    public void TryFindNonOverlappingZonePlacement_FindsNonOverlappingCenter_WhenSpaceIsEmpty()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 3);
        var currentZoneCenter = new HexCoord(0, 0);
        int radius = 3;
        var occupied = new HashSet<HexCoord>(HexCoord.GetRange(currentZoneCenter, radius));
        var bossCoord = currentZoneCenter + HexCoord.Directions[0] * radius;

        Random.InitState(1);
        bool found = planner.TryFindNonOverlappingZonePlacement(
            bossCoord, currentZoneCenter, radius, occupied,
            out HexCoord newCenter, out HexCoord entryCoord);

        Assert.IsTrue(found);
        Assert.IsFalse(planner.ZoneRangeOverlaps(newCenter, radius, occupied),
            "새로 찾은 구역 중심은 기존 점유 좌표와 겹치면 안 된다");
    }

    [Test]
    public void TryFindNonOverlappingZonePlacement_ReturnsFalse_WhenCompletelySurrounded()
    {
        var planner = new DungeonZonePlanner(zoneRadius: 1);
        var currentZoneCenter = new HexCoord(0, 0);
        int radius = 1;
        var bossCoord = currentZoneCenter + HexCoord.Directions[0] * radius;

        // 보스 주변 아주 넓은 범위를 전부 점유된 것으로 채워서 자리를 못 찾게 만든다.
        var occupied = new HashSet<HexCoord>(HexCoord.GetRange(new HexCoord(0, 0), 20));

        Random.InitState(1);
        bool found = planner.TryFindNonOverlappingZonePlacement(
            bossCoord, currentZoneCenter, radius, occupied,
            out HexCoord newCenter, out HexCoord entryCoord);

        Assert.IsFalse(found);
    }
}
