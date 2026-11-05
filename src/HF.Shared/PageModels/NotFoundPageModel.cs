using System;
using CommunityToolkit.Mvvm.ComponentModel;
namespace HF.Shared.PageModels;

public partial class NotFoundPageModel : PageModelBase
{
    [ObservableProperty]
    public partial string Legend { get; set; } = "404 - Page Not Found";
    [ObservableProperty]
    public partial string Description { get; set; } = "Sorry, the content you are looking for does not exist.";

    public NotFoundPageModel()
    {
        Title = "Not Found";
        IsBusy = false;
    }
}
