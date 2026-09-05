using System.Collections.Generic;
using NUnit.Framework;

/// <summary>
/// SkillDatabase.ParseJsonArray가 실제 이식된 Resources/Skills/SkillList.json(2D 원본과 동일 내용)을
/// 정확히 파싱하는지 확인하는 회귀 테스트.
/// </summary>
public class SkillDatabaseTests
{
    private const string SampleJson = @"[
      { ""name"": ""FelixBuff"", ""Type"": ""SingleBuff"", ""Weight"": 1.77 ,""Affect"":[""strengthup""], ""Turn"" : 3},
      { ""name"": ""AmareHeal"", ""Type"": ""SingleHeal"", ""Weight"": 0.8 },
      { ""name"": ""AmareShield"", ""Type"": ""SingleShield"", ""Weight"": 0.85 },
      { ""name"": ""AmareWideHeal"", ""Type"": ""MultiHeal"", ""Weight"": 0.66},
      { ""name"": ""HavetSkill"", ""Type"": ""SingleAttack"", ""Weight"": 1.5 },
      { ""name"": ""LacrimaWideSkill"", ""Type"": ""MultiAttack"", ""Weight"": 0.55 },
      { ""name"": ""PhobiaDebuff"", ""Type"": ""SingleDebuff"",""Affect"":[""defensedown""], ""Weight"": 1.88, ""Turn"" :  3 }
    ]";

    [Test]
    public void ParseJsonArray_ReturnsAllSevenSkills()
    {
        List<SkillData> skills = SkillDatabase.ParseJsonArray(SampleJson);
        Assert.AreEqual(7, skills.Count);
    }

    [Test]
    public void ParseJsonArray_FelixBuff_HasExpectedFields()
    {
        List<SkillData> skills = SkillDatabase.ParseJsonArray(SampleJson);
        SkillData felixBuff = SkillDatabase.Find(skills, "FelixBuff");

        Assert.IsNotNull(felixBuff);
        Assert.AreEqual("SingleBuff", felixBuff.Type);
        Assert.AreEqual(1.77f, felixBuff.Weight, 0.0001f);
        Assert.AreEqual(3, felixBuff.Turn);
        CollectionAssert.AreEqual(new[] { "strengthup" }, felixBuff.Affect);
    }

    [Test]
    public void ParseJsonArray_PhobiaDebuff_HasExpectedFields()
    {
        List<SkillData> skills = SkillDatabase.ParseJsonArray(SampleJson);
        SkillData phobiaDebuff = SkillDatabase.Find(skills, "PhobiaDebuff");

        Assert.IsNotNull(phobiaDebuff);
        Assert.AreEqual(1.88f, phobiaDebuff.Weight, 0.0001f);
        Assert.AreEqual(3, phobiaDebuff.Turn);
        CollectionAssert.AreEqual(new[] { "defensedown" }, phobiaDebuff.Affect);
    }

    [Test]
    public void ParseJsonArray_SkillWithoutAffectOrTurn_LeavesThemDefault()
    {
        List<SkillData> skills = SkillDatabase.ParseJsonArray(SampleJson);
        SkillData amareHeal = SkillDatabase.Find(skills, "AmareHeal");

        Assert.IsNotNull(amareHeal);
        Assert.AreEqual(0.8f, amareHeal.Weight, 0.0001f);
        Assert.AreEqual(0, amareHeal.Turn);
        // JsonUtility는 누락된 List<string> 필드를 null이 아닌 빈 리스트로 채운다(원본 없는 필드는 미사용이므로 무해).
        Assert.IsTrue(amareHeal.Affect == null || amareHeal.Affect.Count == 0);
    }

    [Test]
    public void Find_UnknownName_ReturnsNull()
    {
        List<SkillData> skills = SkillDatabase.ParseJsonArray(SampleJson);
        Assert.IsNull(SkillDatabase.Find(skills, "NoSuchSkill"));
    }
}
