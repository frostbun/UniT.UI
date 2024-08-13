#nullable enable
namespace UniT.UI.Activity
{
    using UniT.UI.View;

    public interface IActivity : IView
    {
        public enum Status
        {
            Hidden,
            Showing,
            Disposed,
        }

        public Status CurrentStatus { get; set; }
    }

    public interface IActivityWithoutParams : IActivity, IViewWithoutParams
    {
    }

    public interface IActivityWithParams<TParams> : IActivity, IViewWithParams<TParams>
    {
    }
}