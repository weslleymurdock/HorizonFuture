using System;

namespace HF.Shared.PageModels;

public partial class HomePageModel : PageModelBase
{
    public HomePageModel()
    {
        Title = "Home";
        IsBusy = false;
    }
}
