using NUnit.Framework;
using UnityEngine;

/// <summary>TurnOrderCarouselLayout(좌우 회전 캐러셀 배치 계산)의 회귀 테스트.</summary>
public class TurnOrderCarouselLayoutTests
{
    [Test]
    public void GetSlotAngleOffset_SlotAtCurrentIndex_IsZero()
    {
        Assert.AreEqual(0f, TurnOrderCarouselLayout.GetSlotAngleOffset(2, 2, 6, 15f), 0.0001f);
    }

    [Test]
    public void GetSlotAngleOffset_SlotAfterCurrent_IsPositive()
    {
        // 아직 차례가 안 온 유닛(index > currentIndex)은 오른쪽(+각도)
        Assert.AreEqual(30f, TurnOrderCarouselLayout.GetSlotAngleOffset(4, 2, 6, 15f), 0.0001f);
    }

    [Test]
    public void GetSlotAngleOffset_SlotBeforeCurrent_IsNegative()
    {
        // 이미 지나간 유닛(index < currentIndex)은 왼쪽(-각도)
        Assert.AreEqual(-30f, TurnOrderCarouselLayout.GetSlotAngleOffset(0, 2, 6, 15f), 0.0001f);
    }

    [Test]
    public void GetSlotAngleOffset_TotalCountZeroOrNegative_Throws()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => TurnOrderCarouselLayout.GetSlotAngleOffset(0, 0, 0, 15f));
    }

    [Test]
    public void GetSlotLocalPosition_AtFrontAngle_IsDirectlyAheadOnNegativeZ()
    {
        Vector3 pos = TurnOrderCarouselLayout.GetSlotLocalPosition(3, 3, 6, radius: 2f, angleStepDegrees: 15f);
        Assert.AreEqual(0f, pos.x, 0.0001f);
        Assert.AreEqual(-2f, pos.z, 0.0001f);
    }

    [Test]
    public void GetSlotLocalPosition_At90DegreeOffset_IsDirectlyToTheRight()
    {
        // index-currentIndex=1, angleStep=90 → 오프셋 90도 → sin(90)=1, cos(90)=0 → (radius, 0, 0)
        Vector3 pos = TurnOrderCarouselLayout.GetSlotLocalPosition(1, 0, 4, radius: 2f, angleStepDegrees: 90f);
        Assert.AreEqual(2f, pos.x, 0.0001f);
        Assert.AreEqual(0f, pos.z, 0.0001f);
    }

    [Test]
    public void GetCarouselYRotation_CurrentIndexZero_IsZero()
    {
        Assert.AreEqual(0f, TurnOrderCarouselLayout.GetCarouselYRotation(0, 15f), 0.0001f);
    }

    [Test]
    public void GetCarouselYRotation_AdvancingCurrentIndex_RotatesOppositeDirection()
    {
        // 현재 턴이 뒤로 갈수록(index 증가) 회전체는 반대 방향으로 돌아 그 슬롯을 정면으로 가져온다.
        Assert.AreEqual(-30f, TurnOrderCarouselLayout.GetCarouselYRotation(2, 15f), 0.0001f);
    }
}
