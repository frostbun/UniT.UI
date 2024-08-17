#nullable enable
namespace UniT.UI.View.Presenter
{
    using UniT.UI.Presenter;

    public abstract class ViewPresenter<TView> : Presenter<TView>, IViewPresenter where TView : IView, IHasPresenter
    {
        protected TView View => this.Owner;

        protected IUIManager Manager => this.Owner.Manager;

        public virtual void OnInitialize() { }

        public virtual void OnShow() { }

        public virtual void OnHide() { }
    }
}