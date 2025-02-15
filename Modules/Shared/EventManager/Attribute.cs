using System;

namespace NDBotUI.Modules.Shared.EventManager;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class EffectAttribute : Attribute
{
    public EffectAttribute(params string[] types)
    {
        Types = types ?? [];
    }

    // Khai báo Types với giá trị mặc định là mảng rỗng nếu không truyền tham số
    public string[] Types { get; }
}