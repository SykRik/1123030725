using UnityEngine;
using UniRx;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Joystick movementJoystick = null;

    private readonly float threshold = 0.01f;

    private void Start()
    {
        if (movementJoystick != null)
        {
            Observable.EveryUpdate()
                .Select(_ => movementJoystick.Direction)
                .Subscribe(direction => InputManager.Instance?.UpdateJoystick(direction))
                .AddTo(this);
        }
    }
}