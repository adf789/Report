using System.Collections.Generic;
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

    private List<BuffUnit> _loadedBuffUnits;

    public override void Show()
    {
        ShowHP();

        ShowBuffs();
    }

    private void ShowHP()
    {
        _hpBar.fillAmount = Model.HpRate;
        _hpText.text = Model.CurrentHp.ToString("n0");
    }

    private void ShowBuffs()
    {
        if (_loadedBuffUnits == null)
            _loadedBuffUnits = new List<BuffUnit>();

        int index = 0;
        for (; index < Model.BuffCount; index++)
        {
            if (_loadedBuffUnits.Count <= index)
                _loadedBuffUnits.Add(Instantiate(_buffPrefab, _buffParent));

            _loadedBuffUnits[index].SetModel(Model.GetBuffUnitModel(index));
            _loadedBuffUnits[index].Show();

            _loadedBuffUnits[index].gameObject.SetActive(true);
        }

        for (; index < _loadedBuffUnits.Count; index++)
        {
            _loadedBuffUnits[index].gameObject.SetActive(false);
        }
    }
}
