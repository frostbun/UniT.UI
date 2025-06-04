#nullable enable
namespace UniT.UI
{
    public interface IActivity : IView
    {
    }

    public interface IActivityWithoutParams : IActivity, IViewWithoutParams
    {
    }

    public interface IActivityWithParams : IActivity, IViewWithParams
    {
    }
}