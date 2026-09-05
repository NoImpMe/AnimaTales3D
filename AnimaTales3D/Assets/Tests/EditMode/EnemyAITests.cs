using System.Collections.Generic;
using NUnit.Framework;

/// <summary>2D 원본(EnemyActions.DecideAction) 가중 랜덤 로직의 회귀 테스트.</summary>
public class EnemyAITests
{
    private static readonly WeightedAction[] EqualWeights =
    {
        new(BattleActionType.Attack, 1f),
        new(BattleActionType.UseSkill, 1f),
    };

    [Test]
    public void DecideAction_IrascorType_AlwaysAttacksRegardlessOfRoll()
    {
        // 원본: type == "Irascor"이면 가중치 계산 없이 무조건 공격.
        var result = EnemyAI.DecideAction(EqualWeights, "Irascor", randomRoll: 1.9f);
        Assert.AreEqual(BattleActionType.Attack, result);
    }

    [Test]
    public void DecideAction_RollWithinFirstWeightRange_PicksFirstAction()
    {
        // totalWeight=2, roll=0.5 → 누적 1.0을 넘지 않아 Attack
        var result = EnemyAI.DecideAction(EqualWeights, "Havet", randomRoll: 0.5f);
        Assert.AreEqual(BattleActionType.Attack, result);
    }

    [Test]
    public void DecideAction_RollPastFirstWeight_PicksSecondAction()
    {
        // totalWeight=2, roll=1.5 → 누적 1.0을 넘어 두 번째(UseSkill)
        var result = EnemyAI.DecideAction(EqualWeights, "Havet", randomRoll: 1.5f);
        Assert.AreEqual(BattleActionType.UseSkill, result);
    }

    [Test]
    public void DecideAction_RollExactlyAtCumulativeBoundary_PicksThatAction()
    {
        // 원본 비교 연산자가 <=이므로 경계값(정확히 누적 가중치)도 그 액션으로 판정된다.
        var result = EnemyAI.DecideAction(EqualWeights, "Havet", randomRoll: 1.0f);
        Assert.AreEqual(BattleActionType.Attack, result);
    }

    [Test]
    public void DecideAction_UnevenWeights_RespectsProportions()
    {
        var weights = new List<WeightedAction>
        {
            new(BattleActionType.Attack, 3f),
            new(BattleActionType.UseSkill, 1f),
        };

        // totalWeight=4. roll=2.9 → 누적 3.0 이하이므로 Attack
        Assert.AreEqual(BattleActionType.Attack, EnemyAI.DecideAction(weights, "Phobia", 2.9f));
        // roll=3.5 → 누적 3.0 초과, 누적 4.0 이하이므로 UseSkill
        Assert.AreEqual(BattleActionType.UseSkill, EnemyAI.DecideAction(weights, "Phobia", 3.5f));
    }
}
