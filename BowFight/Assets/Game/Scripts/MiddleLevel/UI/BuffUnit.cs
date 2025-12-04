using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class BuffUnit : BaseUnit<BuffUnitModel>
{
    [SerializeField] private Image _icon;

    public override void Show()
    {
        ShowIcon().Forget();
    }

    private async UniTask ShowIcon()
    {
        var request = Resources.LoadAsync<Sprite>(Model.Thumbnail);

        _icon.sprite = await request.ToUniTask() as Sprite;
    }
}
