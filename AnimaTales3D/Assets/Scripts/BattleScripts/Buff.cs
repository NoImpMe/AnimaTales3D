using System.Collections.Generic;

/// <summary>
/// 버프/디버프 인스턴스 — 2D 원본(BattleScript/Buff.cs)을 순수 클래스로 이식.
/// TUnit은 대상 유닛 타입(참조 동일성 비교로만 쓰이므로 class 제약만 건다).
/// </summary>
public class Buff<TUnit> where TUnit : class
{
    public List<string> Types { get; }
    public float Weight { get; }
    public int RemainingTurns { get; private set; }
    public TUnit Target { get; }
    public int Distinct { get; }

    public Buff(List<string> types, float weight, int remainingTurns, TUnit target, int distinct)
    {
        Types = types;
        Weight = weight;
        RemainingTurns = remainingTurns;
        Target = target;
        Distinct = distinct;
    }

    public void Tick() => RemainingTurns--;

    public void Renew(Buff<TUnit> other) => RemainingTurns = other.RemainingTurns;

    public void Clear() => RemainingTurns = 0;

    public bool IsExpired() => RemainingTurns <= 0;
}
