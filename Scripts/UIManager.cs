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
    using UnityEngine;
    using UnityEngine.EventSystems;
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
        private readonly EventSystem          eventSystem;
        private readonly IDependencyContainer container;
        private readonly IAssetsManager       assetsManager;
        private readonly ILogger              logger;

        private readonly Transform                        root              = new GameObject(nameof(UIManager)).DontDestroyOnLoad().transform;
        private readonly HashSet<IActivity>               showingActivities = new HashSet<IActivity>();
        private readonly Dictionary<object, IActivity>    keyToPrefab       = new Dictionary<object, IActivity>();
        private readonly Dictionary<IActivity, IActivity> prefabToActivity  = new Dictionary<IActivity, IActivity>();
        private readonly Dictionary<IActivity, object>    activityToKey     = new Dictionary<IActivity, object>();
        private readonly Dictionary<IActivity, IActivity> activityToPrefab  = new Dictionary<IActivity, IActivity>();
        private readonly Dictionary<IActivity, IView[]>   activityToViews   = new Dictionary<IActivity, IView[]>();

        [Preserve]
        public UIManager(RootUICanvas canvas, EventSystem eventSystem, IDependencyContainer container, IAssetsManager assetsManager, ILoggerManager loggerManager)
        {
            this.canvas                  = canvas;
            this.eventSystem             = eventSystem;
            this.container               = container;
            this.assetsManager           = assetsManager;
            this.logger                  = loggerManager.GetLogger(this);
            canvas.transform.parent      = this.root;
            eventSystem.transform.parent = this.root;
            this.logger.Debug("Constructed");
        }

        #endregion

        #region Public

        event Action<IActivity, IReadOnlyList<IView>> IUIManager.Initialized { add => this.initialized += value; remove => this.initialized -= value; }
        event Action<IActivity, IReadOnlyList<IView>> IUIManager.Shown       { add => this.shown += value;       remove => this.shown -= value; }
        event Action<IActivity, IReadOnlyList<IView>> IUIManager.Hidden      { add => this.hidden += value;      remove => this.hidden -= value; }
        event Action<IActivity, IReadOnlyList<IView>> IUIManager.Disposed    { add => this.disposed += value;    remove => this.disposed -= value; }

        IActivity? IUIManager.ShowingScreen => this.showingActivities.SingleOrDefault(activity => activity.Type is ActivityType.Screen);

        IEnumerable<IActivity> IUIManager.ShowingPopups => this.showingActivities.Where(activity => activity.Type is ActivityType.Popup);

        IEnumerable<IActivity> IUIManager.ShowingOverlays => this.showingActivities.Where(activity => activity.Type is ActivityType.Overlay);

        IEnumerable<IActivity> IUIManager.ShowingOverlayPopups => this.showingActivities.Where(activity => activity.Type is ActivityType.OverlayPopup);

        void IUIManager.LockInteraction()
        {
            if (++this.lockCount != 1) return;
            this.eventSystem.enabled = false;
            this.logger.Debug("Interaction locked");
        }

        void IUIManager.UnlockInteraction(bool force)
        {
            if (this.lockCount <= 0) return;
            if (force) this.lockCount = 1;
            if (--this.lockCount != 0) return;
            this.eventSystem.enabled = true;
            this.logger.Debug("Interaction unlocked");
        }

        TActivity IUIManager.Register<TActivity>(TActivity activity)
        {
            this.Initialize(activity);
            return activity;
        }

        TActivity IUIManager.Get<TActivity>(IActivity prefab) => this.Get<TActivity>(prefab);

        #if !UNITY_WEBGL
        TActivity IUIManager.Get<TActivity>(object key)
        {
            var prefab   = this.keyToPrefab.GetOrAdd(key, state => state.assetsManager.LoadComponent<IActivity>(state.key), (this.assetsManager, key));
            var activity = this.Get<TActivity>(prefab);
            this.activityToKey.TryAdd(activity, key);
            return activity;
        }
        #endif

        #if UNIT_UNITASK
        async UniTask<TActivity> IUIManager.GetAsync<TActivity>(object key, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            var prefab   = await this.keyToPrefab.GetOrAddAsync(key, state => state.assetsManager.LoadComponentAsync<IActivity>(state.key, state.progress, state.cancellationToken), (this.assetsManager, key, progress, cancellationToken));
            var activity = this.Get<TActivity>(prefab);
            this.activityToKey.TryAdd(activity, key);
            return activity;
        }
        #else
        IEnumerator IUIManager.GetAsync<TActivity>(object key, Action<TActivity> callback, IProgress<float>? progress)
        {
            var prefab = default(IActivity)!;
            yield return this.keyToPrefab.GetOrAddAsync(
                key,
                callback => this.assetsManager.LoadComponentAsync(key, callback, progress),
                result => prefab = result
            );
            var activity = this.Get<TActivity>(prefab);
            this.activityToKey.TryAdd(activity, key);
            callback(activity);
        }
        #endif

        void IUIManager.Show<TActivity>(TActivity activity, bool force) => this.Show(activity, null, force);

        void IUIManager.Show<TActivity, TParams>(TActivity activity, TParams @params, bool force) => this.Show(activity, @params, force);

        void IUIManager.Hide(IActivity activity) => this.Hide(activity);

        void IUIManager.Dispose(IActivity activity) => this.Dispose(activity);

        #endregion

        #region Private

        private Action<IActivity, IReadOnlyList<IView>>? initialized;
        private Action<IActivity, IReadOnlyList<IView>>? shown;
        private Action<IActivity, IReadOnlyList<IView>>? hidden;
        private Action<IActivity, IReadOnlyList<IView>>? disposed;

        private int lockCount;

        private void Initialize(IActivity activity)
        {
            var views = activity.gameObject.GetComponentsInChildren<IView>();
            this.activityToViews.Add(activity, views);
            foreach (var view in views.AsSpan())
            {
                view.Container = this.container;
                view.Manager   = this;
                view.Activity  = activity;
            }
            foreach (var view in views.AsSpan()) view.OnInitialize();
            this.initialized?.Invoke(activity, views);
            this.logger.Debug($"Initialized {activity.gameObject.name}");
        }

        private TActivity Get<TActivity>(IActivity prefab)
        {
            return (TActivity)this.prefabToActivity.GetOrAdd(prefab, state =>
            {
                var activity = Object.Instantiate(state.prefab.gameObject, state.@this.canvas.Hiddens).GetComponent<IActivity>();
                state.@this.Initialize(activity);
                state.@this.activityToPrefab.Add(activity, state.prefab);
                return activity;
            }, (@this: this, prefab));
        }

        private void Show(IActivity activity, object? @params, bool force)
        {
            if (!this.showingActivities.Add(activity) && !force) return;
            if (activity is IActivityWithParams activityWithParams && @params is { })
            {
                activityWithParams.Params = @params;
            }
            if (activity.Type is ActivityType.Screen)
            {
                this.showingActivities.Where(activity => activity.Type is not ActivityType.Overlay).SafeForEach(this.Hide);
            }
            activity.transform.SetParent(activity.Type switch
            {
                ActivityType.Screen       => this.canvas.Screens,
                ActivityType.Popup        => this.canvas.Popups,
                ActivityType.Overlay      => this.canvas.Overlays,
                ActivityType.OverlayPopup => this.canvas.OverlayPopups,
                _                         => throw new ArgumentOutOfRangeException(nameof(activity.Type), activity.Type, null),
            }, false);
            activity.transform.SetAsLastSibling();
            var views = this.activityToViews[activity];
            foreach (var view in views.AsSpan()) view.OnShow();
            this.shown?.Invoke(activity, views);
            this.logger.Debug($"Shown {activity.gameObject.name}");
        }

        private void Hide(IActivity activity)
        {
            if (!this.showingActivities.Remove(activity)) return;
            activity.transform.SetParent(this.canvas.Hiddens, false);
            var views = this.activityToViews[activity];
            foreach (var view in views.AsSpan()) view.OnHide();
            this.hidden?.Invoke(activity, views);
            this.logger.Debug($"Hidden {activity.gameObject.name}");
        }

        private void Dispose(IActivity activity)
        {
            this.Hide(activity);
            if (!this.activityToViews.Remove(activity, out var views)) return;
            if (this.activityToPrefab.Remove(activity, out var prefab))
            {
                this.prefabToActivity.Remove(prefab);
            }
            if (this.activityToKey.Remove(activity, out var name))
            {
                this.assetsManager.Unload(name);
                this.keyToPrefab.Remove(name);
            }
            var activityName = activity.gameObject.name;
            Object.Destroy(activity.gameObject);
            foreach (var view in views.AsSpan()) view.OnDispose();
            this.disposed?.Invoke(activity, views);
            this.logger.Debug($"Disposed {activityName}");
        }

        #endregion
    }
}