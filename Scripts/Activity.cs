#nullable enable
namespace UniT.UI
{
    using System;
    using UnityEngine;

    [DisallowMultipleComponent]
    public abstract class BaseActivity : BaseView, IActivity
    {
        public abstract ActivityType Type { get; }

        public void Hide() => this.Manager.Hide(this);

        public void Dispose() => this.Manager.Dispose(this);
    }

    public abstract class Activity : BaseActivity, IActivityWithoutParams
    {
        public void Show(bool force = false) => this.Manager.Show(this, force);
    }

    public abstract class Activity<TParams> : BaseActivity, IActivityWithParams<TParams> where TParams : notnull
    {
        TParams? IViewWithParams<TParams>.Params { set => this.@params = value; }

        private TParams? @params;

        protected TParams Params => this.@params ?? throw new InvalidOperationException($"{this.name} not shown or already hidden");

        public void Show(TParams @params, bool force = true) => this.Manager.Show(this, @params, force);
    }
}