using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 헥사곤 Axial 좌표계 (q, r).
/// 기존 2D Tilemap의 헥사곤 좌표 개념을 그대로 이어받아 3D 월드 좌표(XZ 평면)로 변환한다.
/// Flat-Top(평평한 윗변) 헥사곤 기준으로 좌표를 계산한다.
/// </summary>
[Serializable]
public struct HexCoord : IEquatable<HexCoord>
{
    public int q; // 축 좌표 1
    public int r; // 축 좌표 2

    public HexCoord(int q, int r)
    {
        this.q = q;
        this.r = r;
    }

    /// <summary>
    /// Flat-Top 헥사곤 기준, 6방향 이웃 오프셋.
    /// 시계방향: 우, 우상, 좌상, 좌, 좌하, 우하
    /// 구역(Zone) 간 배치 계산에도 재사용하므로 public으로 노출.
    /// </summary>
    public static readonly HexCoord[] Directions = new HexCoord[]
    {
        new HexCoord(+1,  0),
        new HexCoord(+1, -1),
        new HexCoord( 0, -1),
        new HexCoord(-1,  0),
        new HexCoord(-1, +1),
        new HexCoord( 0, +1),
    };

    public static HexCoord operator +(HexCoord a, HexCoord b) => new HexCoord(a.q + b.q, a.r + b.r);
    public static HexCoord operator -(HexCoord a, HexCoord b) => new HexCoord(a.q - b.q, a.r - b.r);
    public static HexCoord operator *(HexCoord a, int scalar) => new HexCoord(a.q * scalar, a.r * scalar);

    /// <summary>
    /// center를 기준으로 radius 범위 내 모든 헥사곤 좌표를 반환한다 (육각형 디스크 형태).
    /// 구역 생성 시 타일 좌표 전체를 뽑아낼 때 사용.
    /// </summary>
    public static List<HexCoord> GetRange(HexCoord center, int radius)
    {
        var result = new List<HexCoord>();
        for (int q = -radius; q <= radius; q++)
        {
            int r1 = Mathf.Max(-radius, -q - radius);
            int r2 = Mathf.Min(radius, -q + radius);
            for (int r = r1; r <= r2; r++)
            {
                result.Add(new HexCoord(center.q + q, center.r + r));
            }
        }
        return result;
    }

    /// <summary>
    /// 이 좌표를 기준으로 6방향 인접 좌표 목록을 반환한다.
    /// 기존 시스템의 "시작지점 기준 인접 타일 노출" 로직에서 사용.
    /// </summary>
    public List<HexCoord> GetNeighbors()
    {
        var result = new List<HexCoord>(6);
        foreach (var dir in Directions)
        {
            result.Add(new HexCoord(q + dir.q, r + dir.r));
        }
        return result;
    }

    /// <summary>
    /// Axial 좌표를 3D 월드 좌표(XZ 평면)로 변환.
    /// size: 헥사곤 한 변의 길이(반지름)
    /// y값은 항상 0으로 고정 — 지형 높이는 DungeonGridManager에서 별도로 얹는다.
    /// </summary>
    public Vector3 ToWorldPosition(float size)
    {
        float x = size * (1.5f * q);
        float z = size * (Mathf.Sqrt(3f) * 0.5f * q + Mathf.Sqrt(3f) * r);
        return new Vector3(x, 0f, z);
    }

    /// <summary>
    /// 두 헥사곤 좌표 사이의 헥사곤 거리(칸 수).
    /// 이동 가능 범위 체크 등에 사용.
    /// </summary>
    public int DistanceTo(HexCoord other)
    {
        int dq = other.q - q;
        int dr = other.r - r;
        return (Mathf.Abs(dq) + Mathf.Abs(dq + dr) + Mathf.Abs(dr)) / 2;
    }

    public bool Equals(HexCoord other) => q == other.q && r == other.r;
    public override bool Equals(object obj) => obj is HexCoord other && Equals(other);
    public override int GetHashCode() => (q, r).GetHashCode();
    public override string ToString() => $"Hex({q},{r})";

    public static bool operator ==(HexCoord a, HexCoord b) => a.Equals(b);
    public static bool operator !=(HexCoord a, HexCoord b) => !a.Equals(b);
}
