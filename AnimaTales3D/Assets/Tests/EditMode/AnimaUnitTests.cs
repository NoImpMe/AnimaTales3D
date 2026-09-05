using System.Collections.Generic;
using NUnit.Framework;

/// <summary>AnimaUnit(원본 AnimaDataSO 이식)의 회귀 테스트.</summary>
public class AnimaUnitTests
{
    private static AnimaTemplate MakeTemplate(bool isBoss = false, List<string> skill = null)
    {
        return new AnimaTemplate
        {
            name = "felix1",
            HP = 20f,
            Weight = 1.1f,
            AP = 10f,
            DP = 5.01f,
            SP = 5.4f,
            DropRate = 85f,
            DropGold = 100,
            Objectfile = "felix1",
            Type = "Felix",
            Attack = "FelixAttack",
            Skill = skill ?? new List<string> { "FelixBuff" },
            IsBoss = isBoss,
        };
    }

    [Test]
    public void CreateFromTemplate_StatsMatchAnimaStatFormulas()
    {
        var template = MakeTemplate();
        AnimaUnit unit = AnimaUnit.CreateFromTemplate(template, level: 5, staminaFraction: 1f, isAlly: true);

        Assert.AreEqual(AnimaStatFormulas.CalcStat(5, 1.1f, 20f), unit.Maxstamina, 0.0001f);
        Assert.AreEqual(AnimaStatFormulas.CalcStat(5, 1.1f, 10f), unit.Damage, 0.0001f);
        Assert.AreEqual(AnimaStatFormulas.CalcStat(5, 1.1f, 5.01f), unit.Defense, 0.0001f);
        Assert.AreEqual(AnimaStatFormulas.CalcStat(5, 1.1f, 5.4f), unit.Speed, 0.0001f);
    }

    [Test]
    public void CreateFromTemplate_CopiesIdentityAndBehaviorFields()
    {
        var template = MakeTemplate(isBoss: true);
        AnimaUnit unit = AnimaUnit.CreateFromTemplate(template, level: 3, staminaFraction: 1f, isAlly: false);

        Assert.AreEqual("felix1", unit.UnitName);
        Assert.IsFalse(unit.IsAlly);
        Assert.IsTrue(unit.IsBoss);
        Assert.AreEqual("Felix", unit.Type);
        Assert.AreEqual("FelixAttack", unit.AttackName);
        CollectionAssert.AreEqual(new[] { "FelixBuff" }, unit.SkillNames);
        Assert.AreEqual(100, unit.DropGold);
        Assert.AreEqual(85f, unit.DropRate, 0.0001f);
    }

    [Test]
    public void Initialize_StartsAtFullStamina()
    {
        AnimaUnit unit = AnimaUnit.Initialize(MakeTemplate(), level: 5, isAlly: true);
        Assert.AreEqual(unit.Maxstamina, unit.Stamina, 0.0001f);
    }

    [Test]
    public void GetAnima_StartsAt40PercentStamina()
    {
        AnimaUnit unit = AnimaUnit.GetAnima(MakeTemplate(), level: 5, isAlly: true);
        Assert.AreEqual(unit.Maxstamina * 0.4f, unit.Stamina, 0.0001f);
    }

    [Test]
    public void Speed_SatisfiesIBattleUnitContract()
    {
        AnimaUnit unit = AnimaUnit.Initialize(MakeTemplate(), level: 5, isAlly: true);
        IBattleUnit asInterface = unit;
        Assert.AreEqual(unit.Speed, asInterface.Speed, 0.0001f);
    }

    [Test]
    public void TurnCheck_ReflectsTurnCheckFlag()
    {
        AnimaUnit unit = AnimaUnit.Initialize(MakeTemplate(), level: 1, isAlly: true);
        Assert.IsFalse(unit.TurnCheck);
        unit.TurnCheckFlag = true;
        Assert.IsTrue(unit.TurnCheck);
    }

    [Test]
    public void LevelUp_BelowCap_IncrementsLevelAndRecalculatesStats()
    {
        AnimaUnit unit = AnimaUnit.Initialize(MakeTemplate(), level: 1, isAlly: true);
        unit.LevelUp();

        Assert.AreEqual(2, unit.Level);
        Assert.AreEqual(AnimaStatFormulas.CalcStat(2, 1.1f, 20f), unit.Maxstamina, 0.0001f);
        Assert.AreEqual(AnimaStatFormulas.CalcStat(2, 1.1f, 10f), unit.Damage, 0.0001f);
        Assert.AreEqual(AnimaStatFormulas.CalcStat(2, 1.1f, 5.4f), unit.Speed, 0.0001f);
    }

    [Test]
    public void LevelUp_AtCap_DoesNothing()
    {
        // mood는 항상 0(데이터 소스에 Mood 컬럼이 없어 미확보) → 상한 maxLevel[0]=14
        AnimaUnit unit = AnimaUnit.Initialize(MakeTemplate(), level: 14, isAlly: true);
        unit.LevelUp();
        Assert.AreEqual(14, unit.Level);
    }
}
