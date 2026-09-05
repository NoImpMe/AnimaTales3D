/// <summary>
/// <see cref="TurnManager{TUnit}"/>가 턴 순서를 정렬하는 데 필요한 최소 스탯 계약.
/// 실제 유닛 데이터 모델(2D 원본의 AnimaDataSO에 대응)은 별도 작업에서 이 인터페이스를 구현하면 된다.
/// </summary>
public interface IBattleUnit
{
    float Speed { get; }

    /// <summary>
    /// 레벨업 등으로 턴 큐 재계산이 필요할 때, 이미 이번 라운드에 행동을 마친 유닛인지 구분하는 플래그.
    /// 2D 원본 AnimaDataSO.turnCheck에 대응.
    /// </summary>
    bool TurnCheck { get; }
}
