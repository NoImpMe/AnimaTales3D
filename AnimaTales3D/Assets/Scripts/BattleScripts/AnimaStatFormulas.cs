using UnityEngine;

/// <summary>
/// 아니마 스탯 계산 공식 — 2D 원본(AnimaDataSO.CalcStat)을 순수 함수로 이식.
/// 원본 주석에 남아있던 원식(수학 표기): math.ceil(((2*j)*(j+0.9))*(k*math.sqrt(math.sqrt(pow(i,3)))
/// + k*math.sqrt(math.sqrt(pow(j, i))))) — i=level, j=weight, k=stat.
/// </summary>
public static class AnimaStatFormulas
{
    public static float CalcStat(int level, float weight, float baseStat)
    {
        return Mathf.Ceil(
            ((2f * weight) * (weight + 0.9f)) * (baseStat * Mathf.Sqrt(Mathf.Sqrt(Mathf.Pow(level, 3f))))
            + baseStat * Mathf.Sqrt(Mathf.Sqrt(Mathf.Pow(weight, level))));
    }
}
