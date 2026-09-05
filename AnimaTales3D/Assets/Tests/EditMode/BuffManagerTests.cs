using System.Collections.Generic;
using NUnit.Framework;

public class BuffManagerTests
{
    private class TestUnit
    {
        public string Label;
        public TestUnit(string label) => Label = label;
    }

    [Test]
    public void AddOrRenewBuff_NewBuff_IsAdded()
    {
        var manager = new BuffManager<TestUnit>();
        var target = new TestUnit("a");
        var types = new List<string> { "strengthdown" };

        manager.AddOrRenewBuff(new Buff<TestUnit>(types, 0.8f, 3, target, 1));

        Assert.AreEqual(1, manager.GetBuffList().Count);
    }

    [Test]
    public void AddOrRenewBuff_SameTypesListReferenceAndTarget_RenewsInsteadOfAdding()
    {
        // 원본(BuffManager.IsExistAndRenewBuff)은 type 리스트를 내용이 아니라 참조로 비교한다 —
        // 같은 List<string> 인스턴스를 재사용해야 "같은 버프"로 인식되는 원본 동작을 그대로 검증한다.
        var manager = new BuffManager<TestUnit>();
        var target = new TestUnit("a");
        var sharedTypes = new List<string> { "strengthdown" };

        manager.AddOrRenewBuff(new Buff<TestUnit>(sharedTypes, 0.8f, 3, target, 1));
        manager.AddOrRenewBuff(new Buff<TestUnit>(sharedTypes, 0.8f, 5, target, 1));

        var buffs = manager.GetBuffList();
        Assert.AreEqual(1, buffs.Count);
        Assert.AreEqual(5, buffs[0].RemainingTurns);
    }

    [Test]
    public void AddOrRenewBuff_SameContentButDifferentListInstance_IsTreatedAsDifferentBuff()
    {
        var manager = new BuffManager<TestUnit>();
        var target = new TestUnit("a");

        manager.AddOrRenewBuff(new Buff<TestUnit>(new List<string> { "strengthdown" }, 0.8f, 3, target, 1));
        manager.AddOrRenewBuff(new Buff<TestUnit>(new List<string> { "strengthdown" }, 0.8f, 3, target, 1));

        Assert.AreEqual(2, manager.GetBuffList().Count);
    }

    [Test]
    public void TickOne_DecrementsOnlyTargetUnitsBuffs()
    {
        var manager = new BuffManager<TestUnit>();
        var target = new TestUnit("a");
        var other = new TestUnit("b");
        manager.AddOrRenewBuff(new Buff<TestUnit>(new List<string> { "strengthdown" }, 0.8f, 2, target, 1));
        manager.AddOrRenewBuff(new Buff<TestUnit>(new List<string> { "speeddown" }, 0.8f, 2, other, 1));

        manager.TickOne(target);

        var buffs = manager.GetBuffList();
        Assert.AreEqual(1, buffs.Find(b => ReferenceEquals(b.Target, target)).RemainingTurns);
        Assert.AreEqual(2, buffs.Find(b => ReferenceEquals(b.Target, other)).RemainingTurns);
    }

    [Test]
    public void TickOne_RemovesBuffAndReturnsTypeWhenExpired()
    {
        var manager = new BuffManager<TestUnit>();
        var target = new TestUnit("a");
        manager.AddOrRenewBuff(new Buff<TestUnit>(new List<string> { "strengthdown" }, 0.8f, 1, target, 1));

        var expired = manager.TickOne(target);

        Assert.AreEqual(new List<string> { "strengthdown" }, expired);
        Assert.IsEmpty(manager.GetBuffList());
    }

    [Test]
    public void TickClear_ExpiresImmediatelyWithoutRemoving()
    {
        var manager = new BuffManager<TestUnit>();
        var target = new TestUnit("a");
        manager.AddOrRenewBuff(new Buff<TestUnit>(new List<string> { "strengthdown" }, 0.8f, 5, target, 1));

        var expired = manager.TickClear(target);

        Assert.AreEqual(new List<string> { "strengthdown" }, expired);
        // 원본(BuffManager.TickClear)은 만료된 버프를 리스트에서 지우지 않는다 — 그대로 보존.
        Assert.AreEqual(1, manager.GetBuffList().Count);
    }
}
