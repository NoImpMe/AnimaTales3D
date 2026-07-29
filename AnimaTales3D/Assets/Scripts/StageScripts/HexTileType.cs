/// <summary>
/// 포트폴리오 원본 로직 기준 타일 종류.
/// Start: 구역 시작지점 / Battle: 전투 타일 / Wall: 이동 불가 타일
/// 이후 Shop, Rest, Boss 등 추가 확장 가능.
/// </summary>
public enum HexTileType
{
    Start,
    Battle,
    Wall,
    Empty,   // 아무 이벤트 없는 통로용 타일 (필요 시 사용)
    Boss,
}
