/// <summary>
/// 전투 데미지/회복/실드/버프 계산 공식 — 2D 원본(AnimaActions.cs/EnemyActions.cs)의 Calc* 메서드를
/// 순수 함수로 이식. 원본은 UnityEngine.Random.Range를 내부에서 직접 호출했지만, 여기서는 그 결과값을
/// randomRoll 매개변수로 받는다(호출부에서 Random.Range(0.95f, 1.11f)를 넘겨주면 원본과 동일하게 동작
/// — 회귀 테스트가 가능하도록 순수 함수로 분리).
///
/// 원본은 아군/적 스킬 데미지·회복량 공식에 비대칭(적 스킬은 weight 미적용, 적 회복은 weight 대신
/// 고정값 1.13)이 있었는데, 이는 버그로 보여 사람 확인 후 통일했다(2026-09-05, LOG #14) —
/// 모든 weight는 이제 <see cref="SkillData.Weight"/>(SkillList.json)에서 오는 값 하나로 아군/적
/// 구분 없이 동일하게 적용된다.
/// </summary>
public static class BattleMath
{
    /// <summary>기본 공격 데미지(스킬 아님). 아군/적 버전 모두 동일한 공식.</summary>
    public static float CalcAttackDamage(float attackerDamage, float defenderDefense, float randomRoll)
        => attackerDamage * (1000f / (1000f + defenderDefense)) * randomRoll;

    /// <summary>스킬 데미지. weight는 항상 SkillData.Weight에서 온 값을 곱한다(아군/적 동일).</summary>
    public static float CalcSkillDamage(float attackerDamage, float defenderDefense, float weight, float randomRoll)
        => attackerDamage * (900f / (900f + defenderDefense)) * randomRoll * weight;

    /// <summary>회복량. weight는 항상 SkillData.Weight에서 온 값(아군/적 동일). 최대 스태미나의 40%로 상한.</summary>
    public static float CalcHealAmount(float healerDamage, float targetMaxStamina, float weight, float randomRoll)
    {
        float a = healerDamage * randomRoll * weight;
        float b = targetMaxStamina * 0.4f;
        return a >= b ? b : a;
    }

    /// <summary>실드량. 아군/적 버전 모두 동일한 공식.</summary>
    public static float CalcShieldAmount(float healerDamage, float weight, float randomRoll)
        => healerDamage * randomRoll * weight;

    /// <summary>버프 배율 — buffer(시전자)의 최근 데미지 값 기반.</summary>
    public static float CalcBuffRatio(float casterRecentDamage, float weight)
        => 0.0002f * casterRecentDamage + weight;

    /// <summary>
    /// 디버프 배율. 원본 시그니처는 (stat, damage, weight)였으나 stat 매개변수는 본문에서 전혀 쓰이지
    /// 않는 죽은 매개변수라 제거했다(반환값은 원본과 동일).
    /// </summary>
    public static float CalcDebuffRatio(float debufferDamage, float weight)
        => (debufferDamage * -0.0002f + (weight - 1f)) * debufferDamage;
}
