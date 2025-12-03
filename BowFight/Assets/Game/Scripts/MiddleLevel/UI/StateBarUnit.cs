using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StateBarUnit : BaseUnit<StateBarUnitModel>
{
    [Header("HP")]
    [SerializeField] private Image _hpBar;
    [SerializeField] private TextMeshProUGUI _hpText;

    [Header("Buff")]
    [SerializeField] private BuffUnit _buffPrefab;
    [SerializeField] private Transform _buffParent;
    [SerializeField] float _buffUnitSize = 25f;
    [SerializeField] float _buffLayoutSpace = 10f;

    public override void Show()
    {

    }
}
