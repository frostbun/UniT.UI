#nullable enable
namespace UniT.UI
{
    using System;
    using UniT.DI;
    using UniT.Extensions;
    using UnityEngine;

    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseView : BetterMonoBehavior, IView
    {
        IDependencyContainer IView.Container { set => this.Container = value; }

        IUIManager IView.Manager { get => this.Manager; set => this.Manager = value; }

        IActivity IView.Activity { get => this.Activity; set => this.Activity = value; }

        protected IDependencyContainer Container { get; private set; } = null!;

        public IUIManager Manager { get; private set; } = null!;

        public IActivity Activity { get; private set; } = null!;

        void IViewLifecycle.OnInitialize() => this.OnInitialize();

        void IViewLifecycle.OnShow() => this.OnShow();

        void IViewLifecycle.OnHide() => this.OnHide();

        void IViewLifecycle.OnDispose() => this.OnDispose();

        protected virtual void OnInitialize() { }

        protected virtual void OnShow() { }

        protected virtual void OnHide() { }

        protected virtual void OnDispose() { }
    }

    public abstract class View : BaseView, IViewWithoutParams
    {
    }

    public abstract class View<TParams> : BaseView, IViewWithParams
    {
        private TParams? @params;

        object IViewWithParams.Params
        {
            set => this.@params = value switch
            {
                null            => default,
                TParams @params => @params,
                _               => throw new InvalidCastException($"{this.GetType().Name} expected {typeof(TParams)}, got {value.GetType().Name}"),
            };
        }

        protected TParams Params => this.@params ?? throw new InvalidOperationException($"{this.name} not shown or already hidden");
    }
}