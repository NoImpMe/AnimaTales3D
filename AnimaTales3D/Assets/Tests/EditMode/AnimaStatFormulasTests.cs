using NUnit.Framework;

/// <summary>
/// AnimaStatFormulas.CalcStat의 회귀 테스트. 기대값은 2D 원본 공식을 Python으로 독립 계산해
/// 고정된 상수로 박아뒀다(같은 식을 테스트에서 다시 유도하는 대신, 별도 계산으로 교차 검증).
/// </summary>
public class AnimaStatFormulasTests
{
    [Test]
    public void CalcStat_Felix1Level5HP_MatchesIndependentlyComputedValue()
    {
        // weight=1.1, baseHP=20, level=5 → 317 (Python math.ceil로 독립 계산)
        Assert.AreEqual(317f, AnimaStatFormulas.CalcStat(5, 1.1f, 20f), 0.5f);
    }

    [Test]
    public void CalcStat_Felix1Level5AP_MatchesIndependentlyComputedValue()
    {
        // weight=1.1, baseAP=10, level=5 → 159
        Assert.AreEqual(159f, AnimaStatFormulas.CalcStat(5, 1.1f, 10f), 0.5f);
    }

    [Test]
    public void CalcStat_Felix1Level1HP_MatchesIndependentlyComputedValue()
    {
        // weight=1.1, baseHP=20, level=1 → 109
        Assert.AreEqual(109f, AnimaStatFormulas.CalcStat(1, 1.1f, 20f), 0.5f);
    }

    [Test]
    public void CalcStat_Felix1Level10AP_MatchesIndependentlyComputedValue()
    {
        // weight=1.1, baseAP=10, level=10 → 261
        Assert.AreEqual(261f, AnimaStatFormulas.CalcStat(10, 1.1f, 10f), 0.5f);
    }

    [Test]
    public void CalcStat_Amare1Level3DP_MatchesIndependentlyComputedValue()
    {
        // weight=1.1, baseDP=5.04, level=3 → 56
        Assert.AreEqual(56f, AnimaStatFormulas.CalcStat(3, 1.1f, 5.04f), 0.5f);
    }

    [Test]
    public void CalcStat_ZeroBaseStat_ReturnsZero()
    {
        // tombstone0/inanis* 처럼 base stat이 0이면 결과도 0 (0의 거듭제곱/곱셈은 전부 0)
        Assert.AreEqual(0f, AnimaStatFormulas.CalcStat(5, 0f, 0f), 0.0001f);
    }

    [Test]
    public void CalcStat_ResultIsAlwaysCeiled()
    {
        float result = AnimaStatFormulas.CalcStat(3, 1.16f, 10.2f);
        Assert.AreEqual(result, UnityEngine.Mathf.Ceil(result), 0.0001f);
    }
}
