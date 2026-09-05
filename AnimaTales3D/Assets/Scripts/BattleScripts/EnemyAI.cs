using System.Collections.Generic;

/// <summary>2D 원본 EnemyActions.ActionType과 동일.</summary>
public enum BattleActionType
{
    Attack,
    UseSkill,
}

/// <summary>가중치가 부여된 행동 선택지 하나. 2D 원본 EnemyActions.ActionWeight에 대응.</summary>
public readonly struct WeightedAction
{
    public readonly BattleActionType ActionType;
    public readonly float Weight;

    public WeightedAction(BattleActionType actionType, float weight)
    {
        ActionType = actionType;
        Weight = weight;
    }
}

/// <summary>
/// 적 행동 결정 — 2D 원본(EnemyActions.DecideAction)의 가중 랜덤 로직을 순수 함수로 이식.
/// 원본은 UnityEngine.Random.Range(0f, totalWeight)를 내부에서 호출했지만, 여기서는 그 결과값을
/// randomRoll 매개변수(0~가중치 합 범위)로 받는다 — 회귀 테스트가 가능하도록 순수 함수로 분리.
/// </summary>
public static class EnemyAI
{
    public static BattleActionType DecideAction(IReadOnlyList<WeightedAction> weights, string unitType, float randomRoll)
    {
        // 원본: type == "Irascor"인 유닛은 가중치 계산 없이 무조건 공격.
        if (unitType == "Irascor") return BattleActionType.Attack;

        float cumulativeWeight = 0f;
        foreach (var action in weights)
        {
            cumulativeWeight += action.Weight;
            if (randomRoll <= cumulativeWeight) return action.ActionType;
        }

        // 원본은 가중치 합을 못 채우면(부동소수 오차 등) 아무 것도 반환하지 않고 조용히 끝나지만,
        // 순수 함수는 값을 반환해야 하므로 안전한 기본값(Attack)으로 폴백한다.
        return BattleActionType.Attack;
    }
}

/// <summary>
/// 적 AI가 상황 판단에 쓰는 입력값. 5개 비-Irascor 테마(Amare/Felix/Havet/Lacrima/Phobia)마다
/// 실제로 쓰는 필드는 하나씩뿐이지만(테마별 스킬 성격이 다르므로), 호출부 단순화를 위해 구조체 하나로 통합했다.
/// 값을 모르거나 해당 없는 필드는 기본값(모두 "AI를 더 똑똑하게 만들지 않는 중립값")으로 둬도 안전하다.
/// </summary>
public readonly struct BattleSituation
{
    /// <summary>Amare용 — 자기 팀(적 팀) 중 가장 낮은 HP 비율(0~1). 낮을수록 회복/실드 스킬을 써야 함.</summary>
    public readonly float AllyLowestHpRatio;
    /// <summary>Felix용 — 자기 팀이 이미 버프 상태인지. 아직이면 버프 스킬을 먼저 써야 함.</summary>
    public readonly bool SelfTeamBuffed;
    /// <summary>Havet용 — 상대 팀 중 가장 낮은 HP 비율(0~1). 낮을수록 스킬로 처치를 노려야 함.</summary>
    public readonly float TargetLowestHpRatio;
    /// <summary>Lacrima용 — 공격 가능한(생존한) 상대 수. 광역 스킬은 2명 이상일 때만 효율적임.</summary>
    public readonly int AliveTargetCount;
    /// <summary>Phobia용 — 주 타겟이 이미 디버프 상태인지. 아직이면 디버프 스킬을 먼저 써야 함.</summary>
    public readonly bool TargetDebuffed;

    public BattleSituation(
        float allyLowestHpRatio = 1f,
        bool selfTeamBuffed = false,
        float targetLowestHpRatio = 1f,
        int aliveTargetCount = 1,
        bool targetDebuffed = false)
    {
        AllyLowestHpRatio = allyLowestHpRatio;
        SelfTeamBuffed = selfTeamBuffed;
        TargetLowestHpRatio = targetLowestHpRatio;
        AliveTargetCount = aliveTargetCount;
        TargetDebuffed = targetDebuffed;
    }
}

/// <summary>
/// 원본에는 없던 신규 로직 — 사람 지시("나머지 5개를 각 상황에 맞게 적 AI를 조금 똑똑하게 만들고 싶어")로 추가.
/// Irascor를 제외한 5개 테마(SkillList.json 기준 실제 스킬 성격: Amare=회복/실드, Felix=버프,
/// Havet=단일공격, Lacrima=광역공격, Phobia=디버프)마다, 상황에 따라 UseSkill 가중치를 올리거나 낮춘 뒤
/// 그 결과를 <see cref="DecideAction"/>에 그대로 넘기면 된다. Attack 가중치는 건드리지 않는다.
/// Irascor는 어차피 DecideAction에서 무조건 공격으로 처리되므로 여기서 별도 분기가 필요 없다(기본 배율 1).
/// </summary>
public static class EnemySituationalAI
{
    private const float HealNeededThreshold = 0.5f;
    private const float ExecuteThreshold = 0.3f;
    private const float BoostMultiplier = 2.5f;
    private const float SuppressMultiplier = 0.4f;

    public static List<WeightedAction> ApplySituationalModifiers(string unitType, IReadOnlyList<WeightedAction> baseWeights, BattleSituation situation)
    {
        float skillMultiplier = unitType switch
        {
            // Amare: 팀원이 절반 이하로 다치면 회복/실드 스킬을 우선, 아니면 굳이 아끼고 공격 위주로.
            "Amare" => situation.AllyLowestHpRatio <= HealNeededThreshold ? BoostMultiplier : SuppressMultiplier,
            // Felix: 아직 버프를 안 걸었다면 먼저 걸고, 이미 걸려 있으면 다시 걸 필요 없이 공격.
            "Felix" => situation.SelfTeamBuffed ? SuppressMultiplier : BoostMultiplier,
            // Havet: 상대가 처치권 안에 들어오면 스킬로 확정 처치를 노림, 아니면 기본 확률 유지.
            "Havet" => situation.TargetLowestHpRatio <= ExecuteThreshold ? BoostMultiplier : 1f,
            // Lacrima: 상대가 2명 이상 생존해 있어야 광역 스킬이 제 몫을 함, 1명뿐이면 낭비이므로 공격 위주로.
            "Lacrima" => situation.AliveTargetCount >= 2 ? BoostMultiplier : SuppressMultiplier,
            // Phobia: 주 타겟이 아직 디버프 안 걸렸으면 먼저 걸고, 이미 걸려 있으면 다시 걸 필요 없이 공격.
            "Phobia" => situation.TargetDebuffed ? SuppressMultiplier : BoostMultiplier,
            _ => 1f,
        };

        var adjusted = new List<WeightedAction>(baseWeights.Count);
        foreach (var action in baseWeights)
        {
            float weight = action.ActionType == BattleActionType.UseSkill ? action.Weight * skillMultiplier : action.Weight;
            adjusted.Add(new WeightedAction(action.ActionType, weight));
        }
        return adjusted;
    }
}
