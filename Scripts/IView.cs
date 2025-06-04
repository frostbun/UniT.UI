#nullable enable
namespace UniT.UI
{
    using UniT.DI;
    using UnityEngine;

    public interface IView : IViewLifecycle
    {
        public IDependencyContainer Container { set; }

        public IUIManager Manager { get; set; }

        public IActivity Activity { get; set; }

        // ReSharper disable once InconsistentNaming
        public GameObject gameObject { get; }

        // ReSharper disable once InconsistentNaming
        public Transform transform { get; }
    }

    public interface IViewWithoutParams : IView
    {
    }

    public interface IViewWithParams : IView
    {
        public object Params { set; }
    }
}