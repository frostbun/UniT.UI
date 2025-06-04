#nullable enable
namespace UniT.UI.Utilities.Adapters
{
    using System.Collections.Generic;
    using UniT.Extensions;
    using UnityEngine;

    public abstract class SimpleViewAdapter<TParams, TView> : View where TView : IViewWithParams
    {
        [SerializeField] private RectTransform content = null!;
        [SerializeField] private TView         prefab  = default!;

        private readonly Dictionary<IView, IReadOnlyList<IView>> views        = new Dictionary<IView, IReadOnlyList<IView>>();
        private readonly Queue<TView>                            pooledViews  = new Queue<TView>();
        private readonly HashSet<TView>                          spawnedViews = new HashSet<TView>();

        public void Set(IEnumerable<TParams> allParams)
        {
            this.OnHide();
            allParams.ForEach(@params =>
            {
                var view = this.pooledViews.DequeueOrDefault(() =>
                {
                    var view       = Instantiate(this.prefab.gameObject, this.content).GetComponent<TView>();
                    var childViews = view.gameObject.GetComponentsInChildren<IView>();
                    this.views.Add(view, childViews);
                    childViews.ForEach(childView =>
                    {
                        childView.Container = this.Container;
                        childView.Manager   = this.Manager;
                        childView.Activity  = this.Activity;
                    });
                    childViews.ForEach(childView => childView.OnInitialize());
                    return view;
                });
                view.transform.SetAsLastSibling();
                view.gameObject.SetActive(true);
                this.spawnedViews.Add(view);
                view.Params = @params!;
                this.views[view].ForEach(childView => childView.OnShow());
            });
        }

        protected override void OnHide()
        {
            this.spawnedViews.Clear(view =>
            {
                this.views[view].ForEach(childView => childView.OnHide());
                view.gameObject.SetActive(false);
                this.pooledViews.Enqueue(view);
            });
        }

        protected override void OnDispose()
        {
            this.pooledViews.Clear(view =>
            {
                this.views[view].ForEach(childView => childView.OnDispose());
                Destroy(view.gameObject);
            });
        }
    }
}