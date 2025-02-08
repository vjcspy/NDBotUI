using System;

namespace NDBotUI.Modules.Shared.EventManager
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class EffectAttribute : Attribute
    {
        // Khai báo Types với giá trị mặc định là mảng rỗng nếu không truyền tham số
        public string[] Types { get; }

        public EffectAttribute(params string[] types)
        {
            Types = types ?? [];
        }
    }
}