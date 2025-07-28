#nullable enable
namespace UniT.UI
{
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

    public abstract class Activity<TParams> : BaseActivity, IActivityWithParams
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        object IViewWithParams.Params { set => this.Params = value is null ? default! : (TParams)value; }

        protected TParams Params { get; private set; } = default!;

        public void Show(TParams @params, ActivityType type, bool force = true) => this.Manager.Show(this, @params!, type, force);
    }
}