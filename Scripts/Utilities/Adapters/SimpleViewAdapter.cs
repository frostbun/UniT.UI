#nullable enable
namespace UniT.UI.Utilities.Adapters
{
    using System.Collections.Generic;
    using UniT.Extensions;
    using UniT.UI.View;
    using UnityEngine;

    public abstract class SimpleViewAdapter<TParams, TView> : View where TView : IViewWithParams<TParams>
    {
        [SerializeField] private RectTransform content = null!;
        [SerializeField] private TView         prefab  = default!;

        private readonly Dictionary<IView, IReadOnlyCollection<IView>> views        = new Dictionary<IView, IReadOnlyCollection<IView>>();
        private readonly Queue<TView>                                  pooledViews  = new Queue<TView>();
        private readonly HashSet<TView>                                spawnedViews = new HashSet<TView>();

        public void Set(IEnumerable<TParams> allParams)
        {
            this.HideAll();
            allParams.ForEach(@params =>
            {
                var view = this.pooledViews.DequeueOrDefault(() =>
                {
                    var view       = Instantiate(this.prefab.gameObject, this.content).GetComponent<TView>();
                    var childViews = view.gameObject.GetComponentsInChildren<IView>();
                    this.views.Add(view, childViews);
                    childViews.ForEach(childView => this.Manager.Initialize(childView, this.Activity));
                    childViews.ForEach(childView => childView.OnInitialize());
                    return view;
                });
                view.gameObject.transform.SetAsLastSibling();
                view.gameObject.SetActive(true);
                this.spawnedViews.Add(view);
                view.Params = @params;
                this.views[view].ForEach(childView => childView.OnShow());
            });
        }

        private void HideAll()
        {
            this.spawnedViews.Clear(view =>
            {
                this.views[view].ForEach(childView => childView.OnHide());
                view.gameObject.SetActive(false);
                this.pooledViews.Enqueue(view);
            });
        }
    }
}