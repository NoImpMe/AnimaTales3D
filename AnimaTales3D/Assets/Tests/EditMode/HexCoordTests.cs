using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// HexCoord(순수 axial 좌표계)의 EditMode 테스트.
/// MonoBehaviour/씬에 의존하지 않는 순수 로직이라 EditMode에서 바로 검증 가능하다.
/// </summary>
public class HexCoordTests
{
    [Test]
    public void Directions_Has6UniqueUnitVectors()
    {
        var dirs = HexCoord.Directions;
        Assert.AreEqual(6, dirs.Length);
        var set = new HashSet<HexCoord>(dirs);
        Assert.AreEqual(6, set.Count, "6방향이 서로 달라야 한다");
    }

    [Test]
    public void GetNeighbors_ReturnsAllSixDirectionOffsets()
    {
        var center = new HexCoord(2, -1);
        var neighbors = center.GetNeighbors();
        Assert.AreEqual(6, neighbors.Count);
        foreach (var dir in HexCoord.Directions)
        {
            Assert.Contains(center + dir, neighbors);
        }
    }

    [Test]
    public void GetNeighbors_AllAreDistance1FromCenter()
    {
        var center = new HexCoord(0, 0);
        foreach (var n in center.GetNeighbors())
        {
            Assert.AreEqual(1, center.DistanceTo(n));
        }
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void GetRange_CountMatchesHexagonalDiskFormula(int radius)
    {
        // 반지름 r인 육각형 디스크의 타일 수 공식: 3r^2 + 3r + 1
        var result = HexCoord.GetRange(new HexCoord(0, 0), radius);
        int expected = 3 * radius * radius + 3 * radius + 1;
        Assert.AreEqual(expected, result.Count);
    }

    [Test]
    public void GetRange_AllCoordsAreWithinRadius()
    {
        var center = new HexCoord(5, -3);
        int radius = 2;
        var result = HexCoord.GetRange(center, radius);
        foreach (var c in result)
        {
            Assert.LessOrEqual(center.DistanceTo(c), radius);
        }
    }

    [Test]
    public void DistanceTo_SameCoord_IsZero()
    {
        var a = new HexCoord(3, 4);
        Assert.AreEqual(0, a.DistanceTo(a));
    }

    [Test]
    public void DistanceTo_IsSymmetric()
    {
        var a = new HexCoord(1, -2);
        var b = new HexCoord(-3, 5);
        Assert.AreEqual(a.DistanceTo(b), b.DistanceTo(a));
    }

    [Test]
    public void ToWorldPosition_CenterIsOrigin()
    {
        var pos = new HexCoord(0, 0).ToWorldPosition(1.2f);
        Assert.AreEqual(Vector3.zero, pos);
    }

    [Test]
    public void ToWorldPosition_YIsAlwaysZero()
    {
        var pos = new HexCoord(4, -2).ToWorldPosition(1.5f);
        Assert.AreEqual(0f, pos.y);
    }

    [Test]
    public void ToWorldPosition_DifferentCoords_ProduceDifferentPositions()
    {
        float size = 1.2f;
        var a = new HexCoord(0, 0).ToWorldPosition(size);
        var b = new HexCoord(1, 0).ToWorldPosition(size);
        Assert.AreNotEqual(a, b);
    }

    [Test]
    public void OperatorPlusMinus_AreInverse()
    {
        var a = new HexCoord(3, -1);
        var b = new HexCoord(-2, 4);
        Assert.AreEqual(a, (a + b) - b);
    }

    [Test]
    public void Equality_SameQR_AreEqual()
    {
        Assert.AreEqual(new HexCoord(1, 2), new HexCoord(1, 2));
        Assert.IsTrue(new HexCoord(1, 2) == new HexCoord(1, 2));
        Assert.IsFalse(new HexCoord(1, 2) != new HexCoord(1, 2));
    }
}
