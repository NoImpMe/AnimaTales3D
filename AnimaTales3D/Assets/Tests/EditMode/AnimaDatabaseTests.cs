using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// AnimaDatabase.ParseJsonArray가 실제 이식된 Resources/Anima/AnimaList.json의 형태를 정확히
/// 파싱하는지 확인하는 회귀 테스트. 아래 샘플은 실제 파일의 일부 항목을 그대로 옮긴 것이다
/// (일반 유닛/보스/스탯이 전부 0인 특수 유닛/스킬 없는 유닛 케이스를 고루 포함).
/// </summary>
public class AnimaDatabaseTests
{
    private const string SampleJson = @"[
      {""name"":""felix0"",""HP"":99941.1,""Weight"":0.7,""AP"":18.02,""DP"":9.01,""SP"":99911.54,""DropRate"":0.0,""DropGold"":0,""Objectfile"":""felix0"",""Type"":""Felix"",""Attack"":""FelixAttack"",""Skill"":[""FelixBuff""],""IsBoss"":false},
      {""name"":""irascor6"",""HP"":21.9,""Weight"":1.275,""AP"":12.4,""DP"":7.52,""SP"":6.5,""DropRate"":20.0,""DropGold"":600,""Objectfile"":""irascor6"",""Type"":""Irascor"",""Attack"":""IrascorAttack"",""Skill"":[],""IsBoss"":true},
      {""name"":""tombstone0"",""HP"":30.0,""Weight"":1.2,""AP"":0.0,""DP"":6.49,""SP"":0.0,""DropRate"":0.0,""DropGold"":0,""Objectfile"":""tombstone0"",""Type"":""tombstone"",""Attack"":""no"",""Skill"":[""no""],""IsBoss"":false},
      {""name"":""inanis0"",""HP"":0.0,""Weight"":0.0,""AP"":0.0,""DP"":0.0,""SP"":0.0,""DropRate"":0.0,""DropGold"":0,""Objectfile"":""inanis0"",""Type"":""Inanis"",""Attack"":""InanisNormalAttack"",""Skill"":[""InanisBuff""],""IsBoss"":false}
    ]";

    [Test]
    public void ParseJsonArray_ReturnsAllFourSampleEntries()
    {
        List<AnimaTemplate> templates = AnimaDatabase.ParseJsonArray(SampleJson);
        Assert.AreEqual(4, templates.Count);
    }

    [Test]
    public void ParseJsonArray_Felix0_HasExpectedFields()
    {
        var templates = AnimaDatabase.ParseJsonArray(SampleJson);
        AnimaTemplate felix0 = AnimaDatabase.Find(templates, "felix0");

        Assert.IsNotNull(felix0);
        Assert.AreEqual("Felix", felix0.Type);
        Assert.AreEqual(0.7f, felix0.Weight, 0.0001f);
        Assert.AreEqual("FelixAttack", felix0.Attack);
        Assert.IsFalse(felix0.IsBoss);
        CollectionAssert.AreEqual(new[] { "FelixBuff" }, felix0.Skill);
    }

    [Test]
    public void ParseJsonArray_BossEntry_IsBossIsTrue()
    {
        var templates = AnimaDatabase.ParseJsonArray(SampleJson);
        AnimaTemplate irascor6 = AnimaDatabase.Find(templates, "irascor6");

        Assert.IsNotNull(irascor6);
        Assert.IsTrue(irascor6.IsBoss);
        Assert.AreEqual(600, irascor6.DropGold);
    }

    [Test]
    public void ParseJsonArray_EntryWithNoSkill_HasEmptySkillList()
    {
        var templates = AnimaDatabase.ParseJsonArray(SampleJson);
        AnimaTemplate irascor6 = AnimaDatabase.Find(templates, "irascor6");

        Assert.IsNotNull(irascor6.Skill);
        Assert.AreEqual(0, irascor6.Skill.Count);
    }

    [Test]
    public void ParseJsonArray_ZeroStatEntry_ParsesZerosCorrectly()
    {
        var templates = AnimaDatabase.ParseJsonArray(SampleJson);
        AnimaTemplate inanis0 = AnimaDatabase.Find(templates, "inanis0");

        Assert.IsNotNull(inanis0);
        Assert.AreEqual(0f, inanis0.HP, 0.0001f);
        Assert.AreEqual(0f, inanis0.Weight, 0.0001f);
    }

    [Test]
    public void Find_UnknownName_ReturnsNull()
    {
        var templates = AnimaDatabase.ParseJsonArray(SampleJson);
        Assert.IsNull(AnimaDatabase.Find(templates, "nonexistent"));
    }
}
