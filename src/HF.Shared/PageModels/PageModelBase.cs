using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace HF.Shared.PageModels;

public partial class PageModelBase : ObservableObject
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;
    [ObservableProperty]
    public partial bool IsBusy { get; set; } = false;
}
