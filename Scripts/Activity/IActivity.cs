#nullable enable
namespace UniT.UI.Activity
{
    using UniT.UI.View;

    public interface IActivity : IView
    {
    }

    public interface IActivityWithoutParams : IActivity, IViewWithoutParams
    {
    }

    public interface IActivityWithParams<in TParams> : IActivity, IViewWithParams<TParams>
    {
    }
}