#nullable enable
namespace UniT.UI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniT.DI;
    using UniT.Extensions;
    using UniT.Logging;
    using UniT.ResourceManagement;
    using UniT.UI.Activity;
    using UniT.UI.Presenter;
    using UniT.UI.View;
    using UnityEngine.Scripting;
    using ILogger = UniT.Logging.ILogger;
    using Object = UnityEngine.Object;
    #if UNIT_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public sealed class UIManager : IUIManager
    {
        #region Constructor

        private readonly RootUICanvas         canvas;
        private readonly IDependencyContainer container;
        private readonly IAssetsManager       assetsManager;
        private readonly ILogger              logger;

        private readonly Dictionary<IActivity, ActivityEntry> activities       = new Dictionary<IActivity, ActivityEntry>();
        private readonly List<IScreen>                        screensStack     = new List<IScreen>();
        private readonly Dictionary<IActivity, IActivity>     prefabToActivity = new Dictionary<IActivity, IActivity>();
        private readonly Dictionary<IActivity, IActivity>     activityToPrefab = new Dictionary<IActivity, IActivity>();
        private readonly Dictionary<IActivity, string>        activityToKey    = new Dictionary<IActivity, string>();

        [Preserve]
        public UIManager(RootUICanvas canvas, IDependencyContainer container, IAssetsManager assetsManager, ILoggerManager loggerManager)
        {
            this.canvas        = canvas;
            this.container     = container;
            this.assetsManager = assetsManager;
            this.logger        = loggerManager.GetLogger(this);
            this.logger.Debug("Constructed");
        }

        #endregion

        #region Public

        void IUIManager.Initialize(IView view, IActivity parent) => this.Initialize(view, parent);

        TActivity IUIManager.RegisterActivity<TActivity>(TActivity activity) => this.RegisterActivity(activity);

        TActivity IUIManager.GetActivity<TActivity>(TActivity prefab) => this.GetActivity<TActivity>(prefab);

        TActivity IUIManager.GetActivity<TActivity>(string key)
        {
            var prefab = this.assetsManager.LoadComponent<IActivity>(key);
            return this.GetActivity<TActivity>(prefab, key);
        }

        #if UNIT_UNITASK
        async UniTask<TActivity> IUIManager.GetActivityAsync<TActivity>(string key, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            var prefab = await this.assetsManager.LoadComponentAsync<IActivity>(key, progress, cancellationToken);
            return this.GetActivity<TActivity>(prefab, key);
        }
        #else
        IEnumerator IUIManager.GetActivityAsync<TActivity>(string key, Action<TActivity> callback, IProgress<float>? progress)
        {
            var prefab = default(IActivity)!;
            yield return this.assetsManager.LoadComponentAsync<IActivity>(
                key,
                result => prefab = result,
                progress
            );
            callback(this.GetActivity<TActivity>(prefab, key));
        }
        #endif

        #region Query

        IScreen? IUIManager.CurrentScreen => this.screensStack.LastOrDefault(activity => this.activities[activity].Status is ActivityStatus.Showing);

        IScreen? IUIManager.PreviousScreen => this.screensStack.LastOrDefault(activity => this.activities[activity].Status is ActivityStatus.Hidden);

        IEnumerable<IPopup> IUIManager.CurrentPopups => this.activities.Keys.OfType<IPopup>().Where(activity => this.activities[activity].Status is ActivityStatus.Showing);

        IEnumerable<IOverlay> IUIManager.CurrentOverlays => this.activities.Keys.OfType<IOverlay>().Where(activity => this.activities[activity].Status is ActivityStatus.Showing);

        #endregion

        #region UI Flow

        ActivityStatus IUIManager.GetStatus(IActivity activity) => this.activities[activity].Status;

        void IUIManager.Show(IActivityWithoutParams activity, bool force)
        {
            if (!this.TryHide(activity, force)) return;
            this.Show(activity);
        }

        void IUIManager.Show<TParams>(IActivityWithParams<TParams> activity, TParams @params, bool force)
        {
            if (!this.TryHide(activity, force)) return;
            activity.Params = @params;
            this.Show(activity);
        }

        void IUIManager.Hide(IActivity activity, bool autoStack)
        {
            if (activity is IScreen screen) this.screensStack.Remove(screen);
            this.Hide(activity, autoStack);
        }

        void IUIManager.Dispose(IActivity activity, bool autoStack)
        {
            if (activity is IScreen screen) this.screensStack.Remove(screen);
            this.Dispose(activity, autoStack);
        }

        #endregion

        #endregion

        #region Private

        private void Initialize(IView view, IActivity parent)
        {
            view.Manager  = this;
            view.Activity = parent;
            if (view is IHasPresenter owner)
            {
                var presenter = (IPresenter)this.container.Instantiate(owner.PresenterType);
                presenter.Owner = owner;
                owner.Presenter = presenter;
            }
            this.logger.Debug($"{view.gameObject.name} initialized");
        }

        private TActivity RegisterActivity<TActivity>(TActivity activity) where TActivity : IActivity
        {
            this.activities.TryAdd(activity, () =>
            {
                var views = activity.gameObject.GetComponentsInChildren<IView>();
                views.ForEach(view => this.Initialize(view, activity));
                views.ForEach(view => view.OnInitialize());
                return new ActivityEntry(views);
            });
            return activity;
        }

        private TActivity GetActivity<TActivity>(IActivity prefab, string? key = null) where TActivity : IActivity
        {
            return (TActivity)this.prefabToActivity.GetOrAdd(prefab, () =>
            {
                var activity = Object.Instantiate(prefab.gameObject, this.canvas.Hiddens, false).GetComponent<IActivity>();
                this.activityToPrefab.Add(activity, prefab);
                if (key is { }) this.activityToKey.Add(activity, key);
                this.RegisterActivity(activity);
                return activity;
            });
        }

        private bool TryHide(IActivity activity, bool force)
        {
            if (!force && this.activities[activity].Status is ActivityStatus.Showing) return false;
            this.Hide(activity, false);
            return true;
        }

        private void SetStackTop(IScreen screen)
        {
            var index = this.screensStack.IndexOf(screen);
            if (index is -1)
            {
                this.screensStack.Add(screen);
            }
            else
            {
                this.screensStack.RemoveRange(index + 1, this.screensStack.Count - index - 1);
            }
            this.activities.Keys
                .Where(other => other is not IOverlay)
                .SafeForEach(other => this.Hide(other, false));
        }

        private void Show(IActivity activity)
        {
            if (activity is IScreen screen) this.SetStackTop(screen);
            activity.gameObject.transform.SetParent(
                activity switch
                {
                    IScreen  => this.canvas.Screens,
                    IPopup   => this.canvas.Popups,
                    IOverlay => this.canvas.Overlays,
                    _        => throw new NotSupportedException($"Showing {activity.GetType().Name} is not supported"),
                },
                false
            );
            activity.gameObject.transform.SetAsLastSibling();
            var entry = this.activities[activity];
            this.logger.Debug($"{activity.gameObject.name} status: {entry.Status = ActivityStatus.Showing}");
            entry.Views.ForEach(view => view.OnShow());
        }

        private void Hide(IActivity activity, bool autoStack)
        {
            var entry = this.activities[activity];
            if (entry.Status is ActivityStatus.Showing)
            {
                this.logger.Debug($"{activity.gameObject.name} status: {entry.Status = ActivityStatus.Hidden}");
                entry.Views.ForEach(view => view.OnHide());
                activity.gameObject.transform.SetParent(this.canvas.Hiddens, false);
            }
            if (autoStack && this.screensStack.LastOrDefault() is { } nextScreen && this.activities[nextScreen].Status is ActivityStatus.Hidden)
            {
                this.Show(nextScreen);
            }
        }

        private void Dispose(IActivity activity, bool autoStack)
        {
            this.Hide(activity, autoStack);
            this.activities.Remove(activity);
            if (this.activityToPrefab.Remove(activity, out var prefab))
            {
                this.prefabToActivity.Remove(prefab);
            }
            Object.Destroy(activity.gameObject);
            if (this.activityToKey.Remove(activity, out var key))
            {
                this.assetsManager.Unload(key);
            }
            this.logger.Debug($"{activity.gameObject.name} disposed");
        }

        #endregion

        private sealed class ActivityEntry
        {
            public IReadOnlyList<IView> Views  { get; }
            public ActivityStatus       Status { get; set; }

            public ActivityEntry(IReadOnlyList<IView> views)
            {
                this.Views  = views;
                this.Status = ActivityStatus.Hidden;
            }
        }
    }
}