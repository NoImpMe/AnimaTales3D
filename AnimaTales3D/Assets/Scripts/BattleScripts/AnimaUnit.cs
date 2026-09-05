using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 유닛 1개의 런타임 인스턴스 — 2D 원본 AnimaDataSO(ScriptableObject, BGDatabase "Anima" 테이블
/// 연동)를 이식. BGDatabase 대신 <see cref="AnimaTemplate"/>(AnimaDatabase가 JSON에서 읽은 값)을
/// 소스로 쓴다는 점만 다르고, 필드 구성과 스탯 계산 흐름은 원본과 동일하다.
/// <see cref="TurnManager{TUnit}"/>가 요구하는 <see cref="IBattleUnit"/>(Speed/TurnCheck)를 구현해
/// 이미 이식된 순수 로직 레이어(TurnManager/BuffManager/EnemyAI/BattleMath)와 바로 연결 가능하다.
///
/// 원본 AnimaDataSO의 필드 중 전투 순수 로직과 무관한 것(스킬 아이콘 스프라이트, 인벤토리 슬롯 카운터,
/// 오버월드 위치 인덱스 등)은 아직 스코프 밖이라 제외했다 — 필요해지면 그때 추가한다.
/// </summary>
public class AnimaUnit : ScriptableObject, IBattleUnit
{
    // 원본 maxLevel 배열(무드별 레벨 상한). BGDatabase "Anima" 테이블 내보내기(AnimaList.json)에는
    // 원본 AnimaDataSO.LoadFromTable이 읽던 "Mood" 컬럼이 존재하지 않아, mood는 항상 기본값 0으로
    // 남는다 — 즉 현재는 LevelUp()이 항상 maxLevel[0]=14를 상한으로 취급한다. 메타/레벨업 시스템을
    // 실제로 포팅할 때 Mood 데이터 소스를 다시 확인할 것(이 단계의 스코프 밖).
    private static readonly int[] MaxLevelByMood = { 14, 20, 27, 35, 43, 52, 60, 70, 80, 100 };

    public bool Animadie;
    public bool IsAlly;
    public bool TurnCheckFlag;
    public bool IsBoss;
    public int Level = 1;
    public string UnitName;
    public float Maxstamina = 1f;
    public float Stamina = 1f;
    public float Shield;
    public float Damage = 1f;
    public float Defense;
    public string Objectfile;
    public string Type = "";
    public string AttackName = "";
    public List<string> SkillNames = new();
    public int DropGold;
    public float DropRate;
    public Dictionary<string, float> TmpAbility = new();

    private float speed = 1f;
    private float weight;
    private float baseHP;
    private float baseAP;
    private float baseDP;
    private float baseSP;
    private int mood;

    public float Speed => speed;
    public bool TurnCheck => TurnCheckFlag;

    /// <summary>버프/디버프가 Speed를 바꿀 때 쓰는 세터. Speed는 IBattleUnit 계약상 읽기 전용 프로퍼티라 직접 대입이 안 되므로 필요.</summary>
    public void SetSpeed(float value) => speed = value;

    /// <summary>스태미나 비율(0~1)을 지정해 템플릿으로부터 유닛을 생성한다. 2D 원본 LoadFromTable에 대응.</summary>
    public static AnimaUnit CreateFromTemplate(AnimaTemplate template, int level, float staminaFraction, bool isAlly)
    {
        var unit = CreateInstance<AnimaUnit>();
        unit.UnitName = template.name;
        unit.IsAlly = isAlly;
        unit.IsBoss = template.IsBoss;
        unit.Level = level;
        unit.weight = template.Weight;
        unit.baseHP = template.HP;
        unit.baseAP = template.AP;
        unit.baseDP = template.DP;
        unit.baseSP = template.SP;
        unit.mood = 0;

        unit.Maxstamina = AnimaStatFormulas.CalcStat(level, template.Weight, template.HP);
        unit.Stamina = unit.Maxstamina * staminaFraction;
        unit.Damage = AnimaStatFormulas.CalcStat(level, template.Weight, template.AP);
        unit.Defense = AnimaStatFormulas.CalcStat(level, template.Weight, template.DP);
        unit.speed = AnimaStatFormulas.CalcStat(level, template.Weight, template.SP);

        unit.DropGold = template.DropGold;
        unit.DropRate = template.DropRate;
        unit.Objectfile = template.Objectfile;
        unit.Type = template.Type;
        unit.AttackName = template.Attack;
        unit.SkillNames = template.Skill != null ? new List<string>(template.Skill) : new List<string>();

        return unit;
    }

    /// <summary>원본 AnimaDataSO.Initialize 대응 — 스태미나 100%로 시작.</summary>
    public static AnimaUnit Initialize(AnimaTemplate template, int level, bool isAlly)
        => CreateFromTemplate(template, level, staminaFraction: 1f, isAlly);

    /// <summary>원본 AnimaDataSO.GetAnima 대응 — 스태미나 40%로 시작(포획 등 특수 상황용).</summary>
    public static AnimaUnit GetAnima(AnimaTemplate template, int level, bool isAlly)
        => CreateFromTemplate(template, level, staminaFraction: 0.4f, isAlly);

    /// <summary>원본 AnimaDataSO.LevelUp 대응. mood가 항상 0으로 고정돼(위 주석 참고) 현재는 상한이 14로 고정된다.</summary>
    public void LevelUp()
    {
        if (MaxLevelByMood[mood] <= Level) return;

        Level++;
        Maxstamina = AnimaStatFormulas.CalcStat(Level, weight, baseHP);
        Damage = AnimaStatFormulas.CalcStat(Level, weight, baseAP);
        Defense = AnimaStatFormulas.CalcStat(Level, weight, baseDP);
        speed = AnimaStatFormulas.CalcStat(Level, weight, baseSP);
    }
}
