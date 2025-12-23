using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 그리드 기반 맵 배치 정보
/// </summary>
[Serializable]
public struct MapGridData
{
    public string GroundName;
    public Vector2Int Grid;   // 실제 월드 좌표

    public string SceneName => $"Scenes/{GroundName}";

    /// <summary>
    /// 맵 범위를 반환
    /// </summary>
    public Bounds GetBounds()
    {
        var mapCenter = new Vector3(
            Grid.x * IntDefine.MAP_UNIT_SIZE + IntDefine.MAP_UNIT_HALF_SIZE,
            0,
            Grid.y * IntDefine.MAP_UNIT_SIZE + IntDefine.MAP_UNIT_HALF_SIZE);

        var mapSize = new Vector3(
            IntDefine.MAP_UNIT_SIZE,
            1,
            IntDefine.MAP_UNIT_SIZE);

        return new Bounds(mapCenter, mapSize);
    }
}
