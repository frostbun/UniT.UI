#nullable enable
namespace UniT.UI.Activity.Presenter
{
    using UniT.UI.Presenter;
    using UniT.UI.View.Presenter;

    public abstract class BaseActivityPresenter<TActivity> : BaseViewPresenter<TActivity>, IActivityPresenter where TActivity : IActivity, IHasPresenter
    {
        protected new TActivity Activity => this.Owner;

        protected IActivity.Status CurrentStatus => this.Owner.CurrentStatus;

        protected void Hide(bool autoStack = true) => this.Manager.Hide(this.Owner, autoStack);

        protected void Dispose(bool autoStack = true) => this.Manager.Dispose(this.Owner, autoStack);
    }

    public abstract class ActivityPresenter<TActivity> : BaseActivityPresenter<TActivity> where TActivity : IActivityWithoutParams, IHasPresenter
    {
    }

    public abstract class ActivityPresenter<TActivity, TParams> : BaseActivityPresenter<TActivity> where TActivity : IActivityWithParams<TParams>, IHasPresenter
    {
        protected TParams Params => this.Owner.Params;
    }
}