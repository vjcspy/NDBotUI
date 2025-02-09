using CommunityToolkit.Mvvm.ComponentModel;

namespace NDBotUI.Store;

public partial class GlobalState : ObservableObject
{
    public static GlobalState Instance { get; } = new();

    [ObservableProperty] private string appName = "NDBot";
}