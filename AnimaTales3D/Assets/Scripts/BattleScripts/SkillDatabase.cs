using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkillList.json(모든 스킬의 weight 배율을 포함한 정의)을 읽어들이는 로더.
/// 2D 원본은 BattleManager.cs에서 Newtonsoft(JsonConvert)로 역직렬화했지만, 이 프로젝트에는
/// Newtonsoft 패키지가 없어(패키지 추가는 사람 승인 필요) 대신 Unity 내장 JsonUtility를 쓴다.
/// JsonUtility는 최상위가 배열인 JSON을 바로 못 읽어서, "{"skills": ...}"로 감싸는 표준 우회를 쓴다.
/// </summary>
public static class SkillDatabase
{
    [System.Serializable]
    private class ListWrapper
    {
        public List<SkillData> skills;
    }

    /// <summary>JSON 배열 문자열(SkillList.json의 원본 형식)을 파싱해 SkillData 목록으로 반환한다.</summary>
    public static List<SkillData> ParseJsonArray(string json)
    {
        string wrapped = "{\"skills\":" + json + "}";
        var parsed = JsonUtility.FromJson<ListWrapper>(wrapped);
        return parsed?.skills ?? new List<SkillData>();
    }

    /// <summary>이름으로 스킬 하나를 찾는다. 없으면 null.</summary>
    public static SkillData Find(List<SkillData> skills, string name)
    {
        foreach (var skill in skills)
        {
            if (skill.name == name) return skill;
        }
        return null;
    }
}
