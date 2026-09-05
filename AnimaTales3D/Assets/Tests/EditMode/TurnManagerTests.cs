using System.Collections.Generic;
using NUnit.Framework;

public class TurnManagerTests
{
    private class TestUnit : IBattleUnit
    {
        public float Speed { get; set; }
        public bool TurnCheck { get; set; }
        public string Label; // 디버깅/단언 편의용, 로직에는 관여하지 않음

        public TestUnit(string label, float speed, bool turnCheck = false)
        {
            Label = label;
            Speed = speed;
            TurnCheck = turnCheck;
        }
    }

    [Test]
    public void UpdateTurnList_SortsBySpeedDescending()
    {
        var manager = new TurnManager<TestUnit>();
        var slow = new TestUnit("slow", 3f);
        var fast = new TestUnit("fast", 10f);
        var mid = new TestUnit("mid", 5f);

        manager.InsertUnit(slow);
        manager.InsertUnit(fast);
        manager.InsertUnit(mid);

        var result = manager.UpdateTurnList();

        Assert.AreEqual(new[] { fast, mid, slow }, result);
    }

    [Test]
    public void ResetTurnList_ClearsQueue()
    {
        var manager = new TurnManager<TestUnit>();
        manager.InsertUnit(new TestUnit("a", 1f));
        manager.ResetTurnList();

        var result = manager.UpdateTurnList();

        Assert.IsEmpty(result);
    }

    [Test]
    public void CheckChanged_FalseRightAfterSort()
    {
        var manager = new TurnManager<TestUnit>();
        manager.InsertUnit(new TestUnit("a", 1f));
        manager.InsertUnit(new TestUnit("b", 5f));
        manager.UpdateTurnList();

        Assert.IsFalse(manager.CheckChanged());
    }

    [Test]
    public void CheckChanged_TrueAfterSpeedChangesOrderInvalid()
    {
        var manager = new TurnManager<TestUnit>();
        var a = new TestUnit("a", 1f);
        var b = new TestUnit("b", 5f);
        manager.InsertUnit(a);
        manager.InsertUnit(b);
        manager.UpdateTurnList(); // [b(5), a(1)]

        a.Speed = 100f; // 버프 등으로 정렬 기준이 바뀌었지만 아직 재정렬은 안 한 상태

        Assert.IsTrue(manager.CheckChanged());
    }

    [Test]
    public void OnLevelUpTurnChanged_MovesNotYetActedUnitsToEndSortedBySpeed()
    {
        // 원본(TurnManager.OnLevelUpTurnChanged)은 "아직 행동 안 한(turnCheck=false) 유닛 개수만큼
        // 큐의 뒤쪽 인덱스를 잘라낸다" 방식이라, 그 유닛들이 실제로 큐 뒤쪽에 있을 때만 안전하게 동작한다
        // (원본 그대로 이식 — 인덱스 기반 제거, 식별 기반 제거가 아님).
        var manager = new TurnManager<TestUnit>();
        var actedA = new TestUnit("actedA", 5f, turnCheck: true);
        var actedB = new TestUnit("actedB", 3f, turnCheck: true);
        var pendingLow = new TestUnit("pendingLow", 1f, turnCheck: false);
        var pendingHigh = new TestUnit("pendingHigh", 8f, turnCheck: false);

        manager.InsertUnit(actedA);
        manager.InsertUnit(actedB);
        manager.InsertUnit(pendingLow);
        manager.InsertUnit(pendingHigh);

        var result = manager.OnLevelUpTurnChanged();

        Assert.AreEqual(new List<TestUnit> { actedA, actedB, pendingHigh, pendingLow }, result);
    }
}
