using DG.Tweening;
using UnityEngine;

namespace DroneDispatcher.Drone
{
// Purely visual polish — makes the drone look alive.
// DOTween handles the hovering bob (smooth up/down float),
// and we manually rotate the rotor every frame.
// No game logic here at all, just eye candy.
public class DroneVisuals : MonoBehaviour
{
    [Header("Hover Bob — subtle up/down float")]
    [SerializeField] float bobAmplitude = 0.15f;  // how high above resting position
    [SerializeField] float bobDuration = 1.2f;    // time for one full bob cycle

    [Header("Optional spin — for placeholder propellers")]
    [SerializeField] Transform rotorTransform;     // drag the propeller child object here
    [SerializeField] float rotorSpeed = 720f;      // degrees per second

    Tweener _bobTween;
    float _startY;

    void Start()
    {
        _startY = transform.localPosition.y;

        // DOTween infinite yoyo = smooth up and down forever
        _bobTween = transform
            .DOLocalMoveY(_startY + bobAmplitude, bobDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void Update()
    {
        // Spin the rotor every frame (if assigned)
        if (rotorTransform != null)
            rotorTransform.Rotate(Vector3.up, rotorSpeed * Time.deltaTime, Space.Self);
    }

    void OnDestroy()
    {
        _bobTween?.Kill();  // clean up DOTween to avoid memory leaks
    }
}
}
