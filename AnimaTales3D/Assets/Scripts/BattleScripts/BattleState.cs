/// <summary>
/// 전투 진행 5단계. 2D 원본(BattleScript/BattleState.cs)의 상태를 그대로 이식.
/// 명시적 전이 콜백 없이 여러 곳(턴 진행/승패 판정)에서 직접 대입되는 원본 구조도 그대로 유지한다.
/// </summary>
public enum BattleState
{
    Start,
    PlayerTurn,
    EnemyTurn,
    Win,
    Defeat,
}
