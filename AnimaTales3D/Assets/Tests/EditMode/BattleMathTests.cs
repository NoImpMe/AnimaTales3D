using NUnit.Framework;

/// <summary>
/// 2D 원본(AnimaActions.cs/EnemyActions.cs)의 Calc* 메서드 원본 수식을 그대로 재현한 참조 구현과
/// 비교해, BattleMath로 이식한 결과가 완전히 같은 값을 내는지 확인하는 회귀 테스트.
/// 원본은 UnityEngine.Random.Range를 내부 호출했으므로, randomRoll을 여러 대표값으로 고정해 비교한다.
/// </summary>
public class BattleMathTests
{
    private static readonly float[] RandomRollSamples = { 0.95f, 1.0f, 1.11f };

    [Test]
    public void CalcAttackDamage_MatchesOriginalFormula()
    {
        foreach (var roll in RandomRollSamples)
        {
            // 원본: damage * (1000f / (1000f + enemy.animaData.Defense)) * roll
            float expected = 120f * (1000f / (1000f + 40f)) * roll;
            float actual = BattleMath.CalcAttackDamage(120f, 40f, roll);
            Assert.AreEqual(expected, actual, 0.0001f);
        }
    }

    [Test]
    public void CalcAllySkillDamage_MatchesOriginalFormula()
    {
        foreach (var roll in RandomRollSamples)
        {
            // 원본(AnimaActions.CalcSkillDamage): damage * (900/(900+Defense)) * roll * weight
            float expected = 120f * (900f / (900f + 40f)) * roll * 1.5f;
            float actual = BattleMath.CalcAllySkillDamage(120f, 40f, 1.5f, roll);
            Assert.AreEqual(expected, actual, 0.0001f);
        }
    }

    [Test]
    public void CalcEnemySkillDamage_IgnoresWeightLikeOriginal()
    {
        foreach (var roll in RandomRollSamples)
        {
            // 원본(EnemyActions.CalcSkillDamage)은 weight 매개변수를 받지만 리턴식에 쓰지 않는다.
            float expected = 120f * (900f / (900f + 40f)) * roll;
            float actual = BattleMath.CalcEnemySkillDamage(120f, 40f, roll);
            Assert.AreEqual(expected, actual, 0.0001f);
        }
    }

    [Test]
    public void CalcAllyHealAmount_CappedAt40PercentMaxStamina()
    {
        // a = damage*roll*weight = 100*1.11*2 = 222, b = maxStamina*0.4 = 500*0.4 = 200 → b가 더 작음
        float result = BattleMath.CalcAllyHealAmount(100f, 500f, 2f, 1.11f);
        Assert.AreEqual(200f, result, 0.0001f);
    }

    [Test]
    public void CalcAllyHealAmount_BelowCap_ReturnsRawAmount()
    {
        // a = 50*1.0*1 = 50, b = 1000*0.4 = 400 → a가 더 작음
        float result = BattleMath.CalcAllyHealAmount(50f, 1000f, 1f, 1.0f);
        Assert.AreEqual(50f, result, 0.0001f);
    }

    [Test]
    public void CalcEnemyHealAmount_UsesFixedMultiplierLikeOriginal()
    {
        // 원본(EnemyActions.CalcHealAmount)은 weight 대신 고정값 1.13f를 곱한다.
        float expected = 80f * 1.0f * 1.13f; // < maxStamina*0.4 이므로 캡에 안 걸림
        float result = BattleMath.CalcEnemyHealAmount(80f, 1000f, 1.0f);
        Assert.AreEqual(expected, result, 0.0001f);
    }

    [Test]
    public void CalcShieldAmount_MatchesOriginalFormula()
    {
        float expected = 60f * 1.05f * 1.2f;
        float result = BattleMath.CalcShieldAmount(60f, 1.2f, 1.05f);
        Assert.AreEqual(expected, result, 0.0001f);
    }

    [Test]
    public void CalcBuffRatio_MatchesOriginalFormula()
    {
        // 원본: 0.0002f * damage + weight
        float expected = 0.0002f * 300f + 1.5f;
        float result = BattleMath.CalcBuffRatio(300f, 1.5f);
        Assert.AreEqual(expected, result, 0.0001f);
    }

    [Test]
    public void CalcDebuffRatio_MatchesOriginalFormula()
    {
        // 원본: (damage * -0.0002f + (weight - 1)) * damage  (첫 번째 stat 매개변수는 원본에서도 미사용)
        float damage = 250f;
        float weight = 0.7f;
        float expected = (damage * -0.0002f + (weight - 1f)) * damage;
        float result = BattleMath.CalcDebuffRatio(damage, weight);
        Assert.AreEqual(expected, result, 0.0001f);
    }
}
