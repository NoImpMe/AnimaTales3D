using NUnit.Framework;
using UnityEngine;

/// <summary>
/// DungeonGenerationConfig(ScriptableObject)의 기본값이 안전한 범위 안에 있는지 확인한다.
/// 이 Config는 DungeonGridManager가 이전에 직접 들고 있던 hexSize/zoneRadius/wallChance/battleChance를
/// 옮겨받은 것으로, 기본값은 리팩터링 전 씬 인스펙터의 실제 오버라이드 값과 같아야 한다.
/// </summary>
public class DungeonGenerationConfigTests
{
    private DungeonGenerationConfig CreateDefault()
    {
        return ScriptableObject.CreateInstance<DungeonGenerationConfig>();
    }

    [Test]
    public void Defaults_MatchPreviousSceneOverrideValues()
    {
        var config = CreateDefault();

        Assert.AreEqual(1.2f, config.hexSize, 0.0001f);
        Assert.AreEqual(3, config.zoneRadius);
        Assert.AreEqual(0.25f, config.wallChance, 0.0001f);
        Assert.AreEqual(0.45f, config.battleChance, 0.0001f);

        Object.DestroyImmediate(config);
    }

    [Test]
    public void Defaults_HexSizeIsPositive()
    {
        var config = CreateDefault();
        Assert.Greater(config.hexSize, 0f);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void Defaults_ZoneRadiusIsAtLeastOne()
    {
        var config = CreateDefault();
        Assert.GreaterOrEqual(config.zoneRadius, 1);
        Object.DestroyImmediate(config);
    }

    [Test]
    public void Defaults_ChancesAreWithinZeroToOneAndDoNotExceedTotalOne()
    {
        var config = CreateDefault();

        Assert.GreaterOrEqual(config.wallChance, 0f);
        Assert.LessOrEqual(config.wallChance, 1f);
        Assert.GreaterOrEqual(config.battleChance, 0f);
        Assert.LessOrEqual(config.battleChance, 1f);
        // DecideTileType은 wallChance + battleChance 순서로 누적 판정하므로,
        // 합이 1을 넘으면 Empty 타일이 절대 나오지 않는 의도치 않은 상태가 된다.
        Assert.LessOrEqual(config.wallChance + config.battleChance, 1f);

        Object.DestroyImmediate(config);
    }
}
