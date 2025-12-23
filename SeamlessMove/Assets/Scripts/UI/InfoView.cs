using System.Text;
using TMPro;
using UnityEngine;

public class InfoView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerInfo;
    [SerializeField] private TextMeshProUGUI _environmentInfo;

    void OnEnable()
    {
        ObserverManager.Instance.AddObserver<PrintPlayerInfoParam>(ShowPlayerInfo);
        ObserverManager.Instance.AddObserver<PrintEnvironmentInfoParam>(ShowEnvironmentInfo);
    }

    void OnDisable()
    {
        ObserverManager.Instance.RemoveObserver<PrintPlayerInfoParam>(ShowPlayerInfo);
        ObserverManager.Instance.RemoveObserver<PrintEnvironmentInfoParam>(ShowEnvironmentInfo);
    }

    private void ShowPlayerInfo(PrintPlayerInfoParam param)
    {
        _playerInfo.text = GetPlayerInfoText(in param);
    }

    private void ShowEnvironmentInfo(PrintEnvironmentInfoParam param)
    {
        _environmentInfo.text = GetEnvironmentInfoText(in param);
    }

    private string GetPlayerInfoText(in PrintPlayerInfoParam param)
    {
        return $"[Player Status]\n\nState:\n{param.MoveState}\n\nPosition:\n{param.CurrentPosition}\n\nDestination:\n{param.DestinationPosition}\n\nDistance Left:\n{param.LeftDistance:F1}m";
    }

    private string GetEnvironmentInfoText(in PrintEnvironmentInfoParam param)
    {
        return $"[Enviroinment]\n\nCurrent Area:\n{param.CurrentArea}\n\nTarget Area:\n{param.TargetArea}";
    }
}
