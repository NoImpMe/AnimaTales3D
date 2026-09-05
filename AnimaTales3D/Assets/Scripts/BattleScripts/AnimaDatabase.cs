using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resources/Anima/AnimaList.json(43종 아니마 기본 스탯)을 읽어들이는 로더.
/// 이 JSON은 2D 원본 BGDatabase의 "Anima" 테이블을 내보낸 엑셀(BGDatabaseExcelEditor로 가져오기/
/// 내보내기하던 원본 편집용 파일)에서 추출했다 — 라이브 BGRepo 바이너리(bansheegz_database.bytes)는
/// 커스텀 직렬화 포맷이라 Unity 밖에서 직접 파싱할 수 없어, 같은 프로젝트에 있던 엑셀 원본을 대신 썼다.
/// <see cref="SkillDatabase"/>와 동일한 JsonUtility 배열 파싱 우회(래퍼 객체) 패턴을 사용한다.
/// </summary>
public static class AnimaDatabase
{
    [System.Serializable]
    private class ListWrapper
    {
        public List<AnimaTemplate> anima;
    }

    /// <summary>JSON 배열 문자열(AnimaList.json의 원본 형식)을 파싱해 AnimaTemplate 목록으로 반환한다.</summary>
    public static List<AnimaTemplate> ParseJsonArray(string json)
    {
        string wrapped = "{\"anima\":" + json + "}";
        var parsed = JsonUtility.FromJson<ListWrapper>(wrapped);
        return parsed?.anima ?? new List<AnimaTemplate>();
    }

    /// <summary>이름으로 템플릿 하나를 찾는다. 없으면 null.</summary>
    public static AnimaTemplate Find(List<AnimaTemplate> templates, string name)
    {
        foreach (var template in templates)
        {
            if (template.name == name) return template;
        }
        return null;
    }
}
