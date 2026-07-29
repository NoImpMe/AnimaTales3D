using UnityEngine;

/// <summary>
/// ZoneTheme와 관련된 보조 기능 모음 (그레이박스 색상, 랜덤 테마 선택).
/// </summary>
public static class ZoneThemeUtility
{
    /// <summary>
    /// 그레이박스 단계에서 구역을 시각적으로 구분하기 위한 테마별 틴트 컬러.
    /// 나중에 실제 아트(스카이박스, 포스트프로세싱 컬러그레이딩)로 교체될 자리.
    /// </summary>
    public static Color GetColor(ZoneTheme theme)
    {
        return theme switch
        {
            ZoneTheme.Amare => new Color(1f, 0.85f, 0.4f),   // 사랑 - 따뜻한 골드
            ZoneTheme.Felix => new Color(0.5f, 0.9f, 0.6f),  // 기쁨 - 연둣빛
            ZoneTheme.Havet => new Color(0.55f, 0.4f, 0.2f), // 욕망 - 어두운 갈색/금
            ZoneTheme.Irascor => new Color(0.95f, 0.4f, 0.2f), // 분노 - 주황/붉은
            ZoneTheme.Lacrima => new Color(0.4f, 0.7f, 0.95f), // 슬픔 - 하늘색
            ZoneTheme.Phobia => new Color(0.4f, 0.15f, 0.45f), // 공포 - 어두운 보라
            _ => Color.white,
        };
    }

    /// <summary>
    /// exclude로 지정한 테마를 제외하고 무작위 테마를 반환한다.
    /// "이전 구역과 다른 테마" 요구사항 처리용.
    /// </summary>
    public static ZoneTheme GetRandomTheme(ZoneTheme? exclude = null)
    {
        var values = (ZoneTheme[])System.Enum.GetValues(typeof(ZoneTheme));

        ZoneTheme picked;
        do
        {
            picked = values[Random.Range(0, values.Length)];
        }
        while (exclude.HasValue && picked == exclude.Value && values.Length > 1);

        return picked;
    }
}
