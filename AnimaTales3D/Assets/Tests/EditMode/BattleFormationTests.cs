using NUnit.Framework;
using UnityEngine;

/// <summary>
/// BattleFormation(최대 3:3, 최소 1:1, 좌측 아군/우측 적군)의 회귀 테스트.
/// 간격 공식은 2D 원본(AllyBattleSetting/EnemyBattleSetting)의 실제 값과 직접 비교한다.
/// </summary>
public class BattleFormationTests
{
    [Test]
    public void GetSlotOffset_ThreeUnits_MatchesOriginalSpacingFormula()
    {
        // 원본: (i * 3.5f) - 3.5f
        Assert.AreEqual(-3.5f, BattleFormation.GetSlotOffset(0, 3), 0.0001f);
        Assert.AreEqual(0f, BattleFormation.GetSlotOffset(1, 3), 0.0001f);
        Assert.AreEqual(3.5f, BattleFormation.GetSlotOffset(2, 3), 0.0001f);
    }

    [Test]
    public void GetSlotOffset_TwoUnits_MatchesOriginalSpacingFormula()
    {
        // 원본: (i * 3.5f) - 1.75f
        Assert.AreEqual(-1.75f, BattleFormation.GetSlotOffset(0, 2), 0.0001f);
        Assert.AreEqual(1.75f, BattleFormation.GetSlotOffset(1, 2), 0.0001f);
    }

    [Test]
    public void GetSlotOffset_OneUnit_IsCentered()
    {
        Assert.AreEqual(0f, BattleFormation.GetSlotOffset(0, 1), 0.0001f);
    }

    [Test]
    public void GetSlotOffset_CountBelowMinOrAboveMax_Throws()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => BattleFormation.GetSlotOffset(0, 0));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => BattleFormation.GetSlotOffset(0, 4));
    }

    [Test]
    public void GetSlotOffset_IndexOutOfRangeForCount_Throws()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => BattleFormation.GetSlotOffset(2, 2));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => BattleFormation.GetSlotOffset(-1, 3));
    }

    [Test]
    public void GetAllySlotPosition_IsOnNegativeXSide()
    {
        Vector3 pos = BattleFormation.GetAllySlotPosition(1, 3, sideOffset: 5f);
        Assert.AreEqual(-5f, pos.x, 0.0001f);
        Assert.AreEqual(0f, pos.z, 0.0001f); // 3명 중 가운데(index 1)는 오프셋 0
    }

    [Test]
    public void GetEnemySlotPosition_IsOnPositiveXSide()
    {
        Vector3 pos = BattleFormation.GetEnemySlotPosition(1, 3, sideOffset: 5f);
        Assert.AreEqual(5f, pos.x, 0.0001f);
        Assert.AreEqual(0f, pos.z, 0.0001f);
    }

    [Test]
    public void GetAllyAndEnemySlotPositions_MirrorEachOtherAcrossCenter()
    {
        for (int count = BattleFormation.MinUnitsPerSide; count <= BattleFormation.MaxUnitsPerSide; count++)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 ally = BattleFormation.GetAllySlotPosition(i, count, sideOffset: 4f);
                Vector3 enemy = BattleFormation.GetEnemySlotPosition(i, count, sideOffset: 4f);
                Assert.AreEqual(-ally.x, enemy.x, 0.0001f);
                Assert.AreEqual(ally.z, enemy.z, 0.0001f);
            }
        }
    }
}
