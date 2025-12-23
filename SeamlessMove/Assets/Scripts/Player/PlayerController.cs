using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private LayerMask _groundMask;
    [SerializeField] private Player _player;

    private bool CheckTouchDown() => Mouse.current != null &&
              (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame);

    private bool CheckEscDown() => Keyboard.current != null && Keyboard.current.escapeKey.IsPressed();

    private Vector2 GetTouchPosition() => Mouse.current?.position.ReadValue() ?? Vector2.zero;

    void Update()
    {
        if (!_player)
            return;

        if (CheckTouchDown())
        {
            Ray ray = Camera.main.ScreenPointToRay(GetTouchPosition());
            if (Physics.Raycast(ray, out RaycastHit hit, IntDefine.MAX_RAY_DISTANCE, _groundMask))
            {
                _player.SetMove(hit.point);
            }
        }
        if (CheckEscDown())
        {
            ApplicationQuit();
        }
    }

    public void ApplicationQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
