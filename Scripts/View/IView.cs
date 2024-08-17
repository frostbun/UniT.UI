#nullable enable
namespace UniT.UI.View
{
    using UniT.UI.Activity;
    using UnityEngine;

    public interface IView
    {
        public IUIManager Manager { get; set; }

        public IActivity Activity { get; set; }

        public void OnInitialize();

        public void OnShow();

        public void OnHide();

        // ReSharper disable once InconsistentNaming
        public GameObject gameObject { get; }
    }

    public interface IViewWithoutParams : IView
    {
    }

    public interface IViewWithParams<in TParams> : IView
    {
        public TParams Params { set; }
    }
}