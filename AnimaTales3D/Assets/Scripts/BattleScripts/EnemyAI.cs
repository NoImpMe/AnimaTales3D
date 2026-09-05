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
