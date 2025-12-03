
using UnityEngine;

public class SkillTableData : BaseTableData
{
    public int CoolTime => _coolTime;
    public bool IsDirect => _isDirect;
    public bool IsJump => _isJump;
    public string ThumbnailPath => _thumbnailPath;

    [SerializeField] private string _thumbnailPath;
    [SerializeField] private int _coolTime;
    [SerializeField] private bool _isDirect;
    [SerializeField] private bool _isJump;
}
