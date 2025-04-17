using UnityEngine;
using DG.Tweening;

public class CameraFocusController : MonoBehaviour
{
    public static CameraFocusController Instance { get; private set; }

    private Vector3 originalPos;
    private float originalSize;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        originalPos = transform.position;
        originalSize = Camera.main.orthographicSize;
    }

    public void FocusOnTeam(Transform teamPanel, float zoomSize = 3f, float duration = 0.5f)
    {
        Vector3 targetPos = new Vector3(teamPanel.position.x, teamPanel.position.y, originalPos.z);
        Camera.main.transform.DOMove(targetPos, duration).SetEase(Ease.OutExpo);
        Camera.main.DOOrthoSize(zoomSize, duration).SetEase(Ease.OutExpo);
    }

    public void ResetFocus(float duration = 0.5f)
    {
        Camera.main.transform.DOMove(originalPos, duration).SetEase(Ease.InOutSine);
        Camera.main.DOOrthoSize(originalSize, duration).SetEase(Ease.InOutSine);
    }
}
