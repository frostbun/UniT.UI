#nullable enable
namespace UniT.UI.Activity
{
    using UniT.UI.View;
    using UnityEngine;

    [DisallowMultipleComponent]
    public abstract class BaseActivity : BaseView, IActivity
    {
        public ActivityStatus Status => this.Manager.GetStatus(this);

        public void Hide(bool autoStack = true) => this.Manager.Hide(this, autoStack);

        public void Dispose(bool autoStack = true) => this.Manager.Dispose(this, autoStack);
    }

    public abstract class Activity : BaseActivity, IActivityWithoutParams
    {
        public void Show(bool force = false) => this.Manager.Show(this, force);
    }

    public abstract class Activity<TParams> : BaseActivity, IActivityWithParams<TParams>
    {
        TParams IViewWithParams<TParams>.Params { set => this.Params = value; }

        public TParams Params { get; private set; } = default!;

        public void Show(TParams @params, bool force = true) => this.Manager.Show(this, @params, force);
    }
}