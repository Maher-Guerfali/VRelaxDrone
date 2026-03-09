using DG.Tweening;
using UnityEngine;

namespace DroneDispatcher.Drone
{
// Attach to the root drone GameObject (same level as DroneController).
// If you have a 3D model child, it handles its own mesh. This just adds
// a gentle hover bob via DOTween. The Animator handles idle/fly states.
public class DroneVisuals : MonoBehaviour
{
    [Header("Hover Bob — subtle up/down float")]
    [SerializeField] float bobAmplitude = 0.15f;
    [SerializeField] float bobDuration = 1.2f;

    [Header("Optional spin — for placeholder propellers")]
    [SerializeField] Transform rotorTransform;
    [SerializeField] float rotorSpeed = 720f;

    Tweener _bobTween;
    float _startY;

    void Start()
    {
        _startY = transform.localPosition.y;

        _bobTween = transform
            .DOLocalMoveY(_startY + bobAmplitude, bobDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void Update()
    {
        if (rotorTransform != null)
            rotorTransform.Rotate(Vector3.up, rotorSpeed * Time.deltaTime, Space.Self);
    }

    void OnDestroy()
    {
        _bobTween?.Kill();
    }
}
}
