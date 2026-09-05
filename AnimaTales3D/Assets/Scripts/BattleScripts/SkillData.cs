using System.Collections.Generic;

/// <summary>
/// 스킬 1개 정의 — 2D 원본(BattleScript/SkillData.cs)을 그대로 이식.
/// 필드명(특히 소문자 name)은 JsonUtility가 JSON 키와 대소문자까지 정확히 일치해야 채워주므로
/// Resources/Skills/SkillList.json의 키 표기를 그대로 따른다 — 임의로 PascalCase로 바꾸지 않는다.
/// </summary>
[System.Serializable]
public class SkillData
{
    public string name;
    public string Type;
    public float Weight;
    public List<string> Affect;
    public int Turn;
}
