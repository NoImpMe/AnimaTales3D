/// <summary>
/// 전투 데미지/회복/실드/버프 계산 공식 — 2D 원본(AnimaActions.cs/EnemyActions.cs)의 Calc* 메서드를
/// 순수 함수로 이식. 원본은 UnityEngine.Random.Range를 내부에서 직접 호출했지만, 여기서는 그 결과값을
/// randomRoll 매개변수로 받는다(호출부에서 Random.Range(0.95f, 1.11f)를 넘겨주면 원본과 동일하게 동작
/// — 회귀 테스트가 가능하도록 순수 함수로 분리).
///
/// 원본에 존재하던 아군/적 간 비대칭(버그로 보이는 동작 포함)은 고치지 않고 그대로 보존했다:
/// - 적 스킬 데미지는 weight를 받고도 곱하지 않음(EnemyActions.CalcSkillDamage)
/// - 적 회복량은 weight 대신 고정값 1.13을 곱함(EnemyActions.CalcHealAmount)
/// </summary>
public static class BattleMath
{
    /// <summary>기본 공격 데미지. 아군/적 버전 모두 동일한 공식.</summary>
    public static float CalcAttackDamage(float attackerDamage, float defenderDefense, float randomRoll)
        => attackerDamage * (1000f / (1000f + defenderDefense)) * randomRoll;

    /// <summary>아군 스킬 데미지 — weight 배율 포함.</summary>
    public static float CalcAllySkillDamage(float attackerDamage, float defenderDefense, float weight, float randomRoll)
        => attackerDamage * (900f / (900f + defenderDefense)) * randomRoll * weight;

    /// <summary>적 스킬 데미지 — 원본 그대로 weight를 곱하지 않는다.</summary>
    public static float CalcEnemySkillDamage(float attackerDamage, float defenderDefense, float randomRoll)
        => attackerDamage * (900f / (900f + defenderDefense)) * randomRoll;

    /// <summary>아군 회복량. 최대 스태미나의 40%로 상한.</summary>
    public static float CalcAllyHealAmount(float healerDamage, float targetMaxStamina, float weight, float randomRoll)
    {
        float a = healerDamage * randomRoll * weight;
        float b = targetMaxStamina * 0.4f;
        return a >= b ? b : a;
    }

    /// <summary>적 회복량 — 원본 그대로 weight 대신 고정값 1.13을 곱한다.</summary>
    public static float CalcEnemyHealAmount(float healerDamage, float targetMaxStamina, float randomRoll)
    {
        float a = healerDamage * randomRoll * 1.13f;
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
