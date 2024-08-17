#nullable enable
namespace UniT.UI.Activity.Presenter
{
    using UniT.UI.Presenter;
    using UniT.UI.View.Presenter;

    public abstract class ActivityPresenter<TActivity> : ViewPresenter<TActivity>, IActivityPresenter where TActivity : IActivity, IHasPresenter
    {
    }
}