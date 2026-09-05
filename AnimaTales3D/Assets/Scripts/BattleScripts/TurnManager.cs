using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 전투 턴 큐 — 2D 원본(BattleScript/TurnManager.cs)을 MonoBehaviour/ScriptableObject 밖 순수 클래스로 이식.
/// Speed 내림차순 정렬 큐라는 동작은 그대로 유지한다.
/// </summary>
public class TurnManager<TUnit> where TUnit : IBattleUnit
{
    private readonly List<TUnit> turnList = new();

    public void ResetTurnList() => turnList.Clear();

    public void InsertUnit(TUnit unit) => turnList.Add(unit);

    public List<TUnit> UpdateTurnList()
    {
        turnList.Sort((a, b) => b.Speed.CompareTo(a.Speed));
        return turnList;
    }

    public bool CheckChanged()
    {
        var check = turnList.ToList();
        check.Sort((a, b) => b.Speed.CompareTo(a.Speed));
        return !check.SequenceEqual(turnList);
    }

    /// <summary>
    /// 레벨업 등으로 스탯이 바뀐 뒤, 아직 행동하지 않은(<see cref="IBattleUnit.TurnCheck"/>가 false인) 유닛만
    /// 골라내 다시 Speed 정렬해 큐 뒤쪽에 이어붙인다.
    /// 원본과 동일하게, "아직 행동 안 한 유닛 개수만큼 큐의 뒤쪽 인덱스를 잘라낸다"는 방식으로 제거한다
    /// (조건에 맞는 유닛을 식별해 개별 제거하는 것이 아님 — 원본 동작을 그대로 보존).
    /// </summary>
    public List<TUnit> OnLevelUpTurnChanged()
    {
        var partTurnList = new List<TUnit>();
        for (int i = 0; i < turnList.Count; i++)
        {
            if (turnList[i].TurnCheck) continue;
            partTurnList.Add(turnList[i]);
        }

        int lastIndex = turnList.Count - partTurnList.Count;
        for (int i = turnList.Count - 1; i >= lastIndex; i--)
        {
            turnList.RemoveAt(i);
        }

        partTurnList.Sort((a, b) => b.Speed.CompareTo(a.Speed));
        turnList.AddRange(partTurnList);
        return turnList;
    }
}
