#nullable enable
namespace UniT.UI.View
{
    using UniT.Extensions;
    using UniT.UI.Activity;
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseView : BetterMonoBehavior, IView
    {
        IUIManager IView.Manager { get => this.Manager; set => this.Manager = value; }

        IActivity IView.Activity { get => this.Activity; set => this.Activity = value; }

        public IUIManager Manager { get; private set; } = null!;

        public IActivity Activity { get; private set; } = null!;

        void IView.OnInitialize() => this.OnInitialize();

        void IView.OnShow() => this.OnShow();

        void IView.OnHide() => this.OnHide();

        protected virtual void OnInitialize() { }

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }
    }

    public abstract class View : BaseView, IViewWithoutParams
    {
    }

    public abstract class View<TParams> : BaseView, IViewWithParams<TParams>
    {
        TParams IViewWithParams<TParams>.Params { get => this.Params; set => this.Params = value; }

        protected TParams Params { get; private set; } = default!;
    }
}