using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUnit : BaseUnit<SkillUnitModel>
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _coolTime;
    [SerializeField] private GameObject _dim;

    private readonly float MIN_CLICK_DELAY = 0.3f;

    private float _clickTime = 0f;

    public override void Show()
    {
        ShowIcon().Forget();

        SetCoolTime(false);
    }

    private async UniTask ShowIcon()
    {
        if (_icon)
        {
            var request = Resources.LoadAsync<Sprite>(Model.ThumbnailPath);

            _icon.sprite = await request.ToUniTask() as Sprite;
        }
    }

    public void OnClick()
    {
        if (!CheckClickDelay())
            return;

        if (Model.OnEventUseSkill())
        {
            SetCoolTime(Model.IsCoolTime);
        }
    }

    void Update()
    {
        OnUpdateCoolTime();
    }

    private void SetCoolTime(bool isActive)
    {
        _dim.SetActive(isActive);

        if (isActive)
            _coolTime.SetText(Model.RemainCoolTime.ToString());
    }

    private bool CheckClickDelay()
    {
        if (Time.realtimeSinceStartup - _clickTime < MIN_CLICK_DELAY)
            return false;

        _clickTime = Time.realtimeSinceStartup;

        return true;
    }

    private void OnUpdateCoolTime()
    {
        bool beforeCoolTime = Model.IsCoolTime;

        Model.PassTime(Time.deltaTime);

        if (Model.IsCoolTime || beforeCoolTime != Model.IsCoolTime)
            SetCoolTime(Model.IsCoolTime);
    }
}
