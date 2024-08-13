#nullable enable
namespace UniT.UI.View.Presenter
{
    using UniT.UI.Activity;
    using UniT.UI.Presenter;

    public abstract class BaseViewPresenter<TView> : Presenter<TView>, IViewPresenter where TView : IView, IHasPresenter
    {
        protected TView View => this.Owner;

        protected IUIManager Manager => this.Owner.Manager;

        protected IActivity Activity => this.Owner.Activity;

        public virtual void OnInitialize() { }

        public virtual void OnShow() { }

        public virtual void OnHide() { }
    }

    public abstract class ViewPresenter<TView> : BaseViewPresenter<TView> where TView : IViewWithoutParams, IHasPresenter
    {
    }

    public abstract class ViewPresenter<TView, TParams> : BaseViewPresenter<TView> where TView : IViewWithParams<TParams>, IHasPresenter
    {
        protected TParams Params => this.Owner.Params;
    }
}