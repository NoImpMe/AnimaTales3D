using System.Collections.Generic;

/// <summary>
/// 버프/디버프 관리 — 2D 원본(BattleScript/BuffManager.cs)을 순수 클래스로 이식.
/// 원본은 Dictionary&lt;Buff,int&gt;를 썼지만 값(distinct)이 어디서도 읽히지 않는 죽은 값이라
/// List&lt;Buff&gt;로 단순화했다 (동작은 동일 — 전부 순회하며 참조 동일성으로만 비교).
/// </summary>
public class BuffManager<TUnit> where TUnit : class
{
    private readonly List<Buff<TUnit>> buffs = new();

    /// <summary>
    /// 같은 타입의 버프가 이미 대상에게 걸려 있으면 지속시간만 갱신하고, 없으면 새로 추가한다.
    /// 원본과 동일하게 "같은 타입"은 문자열 내용이 아니라 <see cref="Buff{TUnit}.Types"/> 리스트의
    /// 참조 동일성으로 판정한다 — 호출부가 버프 타입별로 같은 List&lt;string&gt; 인스턴스를 재사용한다는
    /// 원본의 전제를 그대로 따른다.
    /// </summary>
    public void AddOrRenewBuff(Buff<TUnit> buff)
    {
        if (TryRenewExisting(buff)) return;
        buffs.Add(buff);
    }

    /// <summary>대상 유닛의 턴이 끝날 때마다 호출 — 그 유닛에 걸린 버프만 1틱 감소시키고, 만료된 것은 제거한다.</summary>
    public List<string> TickOne(TUnit target)
    {
        var expired = new List<string>();
        var toRemove = new List<Buff<TUnit>>();

        foreach (var buff in buffs)
        {
            if (!ReferenceEquals(buff.Target, target)) continue;
            buff.Tick();
            if (buff.IsExpired())
            {
                toRemove.Add(buff);
                expired.AddRange(buff.Types);
            }
        }

        foreach (var buff in toRemove) buffs.Remove(buff);
        return expired;
    }

    /// <summary>대상 유닛의 모든 버프를 즉시 만료시킨다(제거는 하지 않고 만료 타입만 반환).</summary>
    public List<string> TickClear(TUnit target)
    {
        var expired = new List<string>();
        foreach (var buff in buffs)
        {
            if (!ReferenceEquals(buff.Target, target)) continue;
            buff.Clear();
            if (buff.IsExpired()) expired.AddRange(buff.Types);
        }
        return expired;
    }

    public List<Buff<TUnit>> GetBuffList() => new(buffs);

    private bool TryRenewExisting(Buff<TUnit> buff)
    {
        foreach (var existing in buffs)
        {
            if (ReferenceEquals(existing.Types, buff.Types) && ReferenceEquals(existing.Target, buff.Target))
            {
                existing.Renew(buff);
                return true;
            }
        }
        return false;
    }
}
