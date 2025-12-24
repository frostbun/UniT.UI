#nullable enable
namespace UniT.UI
{
    using System;
    using UnityEngine;

    [DisallowMultipleComponent]
    public abstract class BaseActivity : BaseView, IActivity
    {
        public void Hide() => this.Manager.Hide(this);

        public void Dispose() => this.Manager.Dispose(this);
    }

    public abstract class Activity : BaseActivity, IActivityWithoutParams
    {
        public void Show(ActivityType type, bool force = false) => this.Manager.Show(this, type, force);
    }

    public abstract class Activity<TParams> : BaseActivity, IActivityWithParams<TParams> where TParams : notnull
    {
        TParams? IViewWithParams<TParams>.Params { set => this.@params = value; }

        private TParams? @params;

        protected TParams Params => this.@params ?? throw new InvalidOperationException($"{this.name} not shown or already hidden");

        public void Show(TParams @params, ActivityType type, bool force = true) => this.Manager.Show(this, @params, type, force);
    }
}