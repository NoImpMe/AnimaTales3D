using System.Collections.Generic;

/// <summary>
/// 유닛(아니마) 1종의 기본 스탯 템플릿 — 2D 원본이 BGDatabase "Anima" 테이블에서 읽던 값(HP/Weight/
/// AP/DP/SP/DropRate/DropGold/Objectfile/Type/Attack/Skill/IsBoss)을 그대로 옮긴 데이터.
/// BGDatabase 대신 <see cref="AnimaDatabase"/>가 Resources/Anima/AnimaList.json에서 읽어들인다.
/// 필드명은 원본 BGDatabase 컬럼명과 그대로 맞춰(JsonUtility 대소문자 일치 요구 + 추적 용이성),
/// 실제 전투에 쓰이는 값만 포함했다(원본 테이블의 _id/Description/Meeted는 AnimaDataSO.LoadFromTable이
/// 읽지 않는 미사용 컬럼이라 제외).
/// </summary>
[System.Serializable]
public class AnimaTemplate
{
    public string name;
    public float HP;
    public float Weight;
    public float AP;
    public float DP;
    public float SP;
    public float DropRate;
    public int DropGold;
    public string Objectfile;
    public string Type;
    public string Attack;
    public List<string> Skill;
    public bool IsBoss;
}
