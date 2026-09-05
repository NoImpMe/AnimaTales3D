using System.Collections;
using UnityEngine;

/// <summary>
/// 우클릭 드래그로 카메라를 지면(Y=0 기준 평면)에서 팬(pan) 이동시킨다.
/// 좌클릭은 그대로 DungeonGridManager의 타일 클릭(OnTileClicked)에서 사용하므로 충돌 없음.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraDragController : MonoBehaviour
{
    public static CameraDragController Instance { get; private set; }
    [Header("Drag Settings")]
    [Tooltip("0 = 좌클릭, 1 = 우클릭, 2 = 휠클릭")]
    [SerializeField] private int dragMouseButton = 1;
    [Tooltip("드래그 기준이 되는 가상의 지면 높이 (보통 타일이 놓인 Y값)")]
    [SerializeField] private float planeHeight = 0f;

    [Header("Bounds")]
    [SerializeField] private bool useBounds = true;
    [Tooltip("지정하면 이 오브젝트 하위 Renderer들을 스캔해서 자동으로 이동 범위를 계산 (예: DungeonGridManager)")]
    [SerializeField] private Transform tilesRoot;
    [SerializeField] private float boundsPadding = 2f;
    [Tooltip("tilesRoot가 비어있을 때 사용할 수동 범위")]
    [SerializeField] private Vector2 manualMinXZ;
    [SerializeField] private Vector2 manualMaxXZ = new Vector2(50f, 50f);

    private Camera cam;
    private Plane dragPlane;
    private Vector3 dragOrigin;
    private bool isDragging;
    private Vector2 minXZ;
    private Vector2 maxXZ;
    private Coroutine followRoutine;

private void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
        dragPlane = new Plane(Vector3.up, new Vector3(0f, planeHeight, 0f));
    }

private void Start()
    {
        RecalculateBounds();
    }

/// <summary>
    /// 카메라 이동 가능 범위를 다시 계산한다. 새 구역(Zone)이 생성되어 타일이 늘어날 때마다
    /// DungeonGridManager 쪽에서 호출해준다.
    /// </summary>
    public void RecalculateBounds()
    {
        if (!useBounds) return;

        if (tilesRoot != null)
        {
            CalculateBoundsFromChildren();
        }
        else
        {
            minXZ = manualMinXZ;
            maxXZ = manualMaxXZ;
        }
    }


    /// <summary>
    /// Player가 타일 간 이동할 때 같은 방향·거리·시간만큼 카메라도 따라 움직이게 한다
    /// (쿼터뷰 느낌으로 카메라가 Player를 중심으로 따라오는 연출). PlayerToken.MoveTo에서 호출.
    /// </summary>
    public void FollowMove(Vector3 delta, float duration)
    {
        if (followRoutine != null) StopCoroutine(followRoutine);
        followRoutine = StartCoroutine(FollowMoveRoutine(delta, duration));
    }

    private IEnumerator FollowMoveRoutine(Vector3 delta, float duration)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + delta;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = ClampToBounds(Vector3.Lerp(startPos, targetPos, t));
            yield return null;
        }

        transform.position = ClampToBounds(targetPos);
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (useBounds)
        {
            position.x = Mathf.Clamp(position.x, minXZ.x, maxXZ.x);
            position.z = Mathf.Clamp(position.z, minXZ.y, maxXZ.y);
        }
        return position;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(dragMouseButton))
        {
            if (TryGetGroundPoint(Input.mousePosition, out Vector3 hitPoint))
            {
                dragOrigin = hitPoint;
                isDragging = true;
            }
        }
        else if (Input.GetMouseButtonUp(dragMouseButton))
        {
            isDragging = false;
        }

        if (isDragging && Input.GetMouseButton(dragMouseButton))
        {
            if (TryGetGroundPoint(Input.mousePosition, out Vector3 currentPoint))
            {
                Vector3 delta = dragOrigin - currentPoint;
                transform.position = ClampToBounds(transform.position + delta);

                // 카메라를 이미 이동시켰으므로, 다음 프레임 delta 계산 기준점을 다시 잡아준다.
                TryGetGroundPoint(Input.mousePosition, out dragOrigin);
            }
        }
    }

    private bool TryGetGroundPoint(Vector3 screenPosition, out Vector3 point)
    {
        Ray ray = cam.ScreenPointToRay(screenPosition);
        if (dragPlane.Raycast(ray, out float distance))
        {
            point = ray.GetPoint(distance);
            return true;
        }

        point = Vector3.zero;
        return false;
    }

    private void CalculateBoundsFromChildren()
    {
        Renderer[] renderers = tilesRoot.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            minXZ = manualMinXZ;
            maxXZ = manualMaxXZ;
            return;
        }

        Bounds bounds = renderers[0].bounds;
        foreach (var r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        minXZ = new Vector2(bounds.min.x - boundsPadding, bounds.min.z - boundsPadding);
        maxXZ = new Vector2(bounds.max.x + boundsPadding, bounds.max.z + boundsPadding);
    }
}
