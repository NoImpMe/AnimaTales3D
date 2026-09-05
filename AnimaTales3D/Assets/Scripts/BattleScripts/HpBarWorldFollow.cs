using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 유닛 위에 world-to-screen 방식으로 떠 있는 HP 바 — CONVERSION_SPEC.md 5절에서 정한 방식대로
/// Camera.WorldToScreenPoint 기반으로 구현(카메라가 orthographic이든 perspective든 동일하게 동작).
/// </summary>
public class HpBarWorldFollow : MonoBehaviour
{
    [SerializeField] private RectTransform selfRect;
    [SerializeField] private Image fillImage;

    private Transform target;
    private AnimaUnit boundUnit;
    private float yOffset;
    private Camera trackingCamera;

    public void Bind(Transform targetTransform, AnimaUnit unit, float worldYOffset, Camera camera)
    {
        target = targetTransform;
        boundUnit = unit;
        yOffset = worldYOffset;
        trackingCamera = camera;
    }

    private void LateUpdate()
    {
        if (target == null || boundUnit == null || trackingCamera == null) return;

        Vector3 worldPos = target.position + Vector3.up * yOffset;
        selfRect.position = trackingCamera.WorldToScreenPoint(worldPos);

        float ratio = boundUnit.Maxstamina > 0f ? boundUnit.Stamina / boundUnit.Maxstamina : 0f;
        fillImage.fillAmount = Mathf.Clamp01(ratio);
    }
}
