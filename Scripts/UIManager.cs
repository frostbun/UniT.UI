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
    using Unity.Collections.LowLevel.Unsafe;
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

        private readonly List<IActivity>                      screensStack     = new List<IActivity>();
        private readonly Dictionary<object, IActivity>        keyToPrefab      = new Dictionary<object, IActivity>();
        private readonly Dictionary<IActivity, IActivity>     prefabToActivity = new Dictionary<IActivity, IActivity>();
        private readonly Dictionary<IActivity, object>        activityToKey    = new Dictionary<IActivity, object>();
        private readonly Dictionary<IActivity, IActivity>     activityToPrefab = new Dictionary<IActivity, IActivity>();
        private readonly Dictionary<IActivity, ActivityEntry> activityToEntry  = new Dictionary<IActivity, ActivityEntry>();

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

        IActivity? IUIManager.CurrentScreen => this.screensStack.LastOrDefault(prefab => this.activityToEntry[prefab].Type is ActivityType.Screen);

        IActivity? IUIManager.PreviousScreen => this.screensStack.LastOrDefault(prefab => this.activityToEntry[prefab].Type is not ActivityType.Screen);

        IEnumerable<IActivity> IUIManager.CurrentPopups => this.activityToEntry.WhereValue(entry => entry.Type is ActivityType.Popup).SelectKeys();

        IEnumerable<IActivity> IUIManager.CurrentOverlays => this.activityToEntry.WhereValue(entry => entry.Type is ActivityType.Overlay).SelectKeys();

        IEnumerable<IActivity> IUIManager.CurrentOverlayPopups => this.activityToEntry.WhereValue(entry => entry.Type is ActivityType.OverlayPopup).SelectKeys();

        TActivity IUIManager.Register<TActivity>(TActivity activity)
        {
            this.Initialize(activity);
            return activity;
        }

        TActivity IUIManager.Get<TActivity>(IActivity prefab) => this.Get<TActivity>(prefab);

        TActivity IUIManager.Get<TActivity>(object key)
        {
            var prefab   = this.keyToPrefab.GetOrAdd(key, () => this.assetsManager.LoadComponent<IActivity>(key));
            var activity = this.Get<TActivity>(prefab);
            this.activityToKey.TryAdd(activity, key);
            return activity;
        }

        #if UNIT_UNITASK
        async UniTask<TActivity> IUIManager.GetAsync<TActivity>(object key, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            var prefab   = await this.keyToPrefab.GetOrAddAsync(key, () => this.assetsManager.LoadComponentAsync<IActivity>(key, progress, cancellationToken));
            var activity = this.Get<TActivity>(prefab);
            this.activityToKey.TryAdd(activity, key);
            return activity;
        }
        #else
        IEnumerator IUIManager.GetAsync<TActivity>(object key, Action<TActivity> callback, IProgress<float>? progress)
        {
            var prefab = default(IActivity)!;
            yield return this.nameToPrefab.GetOrAddAsync(
                name,
                callback => this.assetsManager.LoadComponentAsync(name, callback, progress),
                result => prefab = result
            );
            var activity = this.Get<TActivity>(prefab);
            this.activityToKey.TryAdd(activity, key);
            callback(activity);
        }
        #endif

        ActivityType? IUIManager.GetType(IActivity activity) => this.activityToEntry[activity].Type;

        void IUIManager.Show<TActivity>(TActivity activity, ActivityType type, bool force) => this.Show(activity, null, type, force);

        void IUIManager.Show<TActivity, TParams>(TActivity activity, TParams @params, ActivityType type, bool force) => this.Show(activity, @params, type, force);

        void IUIManager.Hide(IActivity activity, bool showPreviousScreen) => this.Hide(activity, showPreviousScreen, removeFromStack: true);

        void IUIManager.Dispose(IActivity activity, bool showPreviousScreen) => this.Dispose(activity, showPreviousScreen);

        #endregion

        #region Private

        private void Initialize(IActivity activity)
        {
            var views = activity.gameObject.GetComponentsInChildren<IView>();
            this.activityToEntry.Add(activity, new ActivityEntry(views));
            views.ForEach(view =>
            {
                view.Container = this.container;
                view.Manager   = this;
                view.Activity  = activity;
            });
            views.ForEach(view => view.OnInitialize());
            this.logger.Debug($"Initialize {activity.gameObject.name}");
        }

        private TActivity Get<TActivity>(IActivity prefab)
        {
            return (TActivity)this.prefabToActivity.GetOrAdd(prefab, () =>
            {
                var activity = Object.Instantiate(prefab.gameObject, this.canvas.Hiddens).GetComponent<IActivity>();
                this.Initialize(activity);
                this.activityToPrefab.Add(activity, prefab);
                return activity;
            });
        }

        private void Show(IActivity activity, object? @params, ActivityType type, bool force)
        {
            var entry = this.activityToEntry[activity];
            if (!force && entry.Type == type) return;
            if (activity is IActivityWithParams activityWithParams)
            {
                activityWithParams.Params = @params;
            }
            if (type is ActivityType.Screen)
            {
                var index = this.screensStack.IndexOf(activity);
                if (index is -1)
                {
                    this.screensStack.Add(activity);
                }
                else
                {
                    this.screensStack.RemoveRange(index + 1, this.screensStack.Count - index - 1);
                }
                this.activityToEntry.WhereValue(entry => entry.Type is not ActivityType.Overlay)
                    .SelectKeys()
                    .ForEach(prefab => this.Hide(prefab, showPreviousScreen: false, removeFromStack: false));
            }
            else
            {
                this.screensStack.Remove(activity);
            }
            activity.transform.SetParent(type switch
            {
                ActivityType.Screen       => this.canvas.Screens,
                ActivityType.Popup        => this.canvas.Popups,
                ActivityType.Overlay      => this.canvas.Overlays,
                ActivityType.OverlayPopup => this.canvas.OverlayPopups,
                _                         => throw new ArgumentOutOfRangeException(nameof(type), type, null),
            }, false);
            activity.transform.SetAsLastSibling();
            entry.Type = type;
            entry.Views.ForEach(view => view.OnShow());
            this.logger.Debug($"Show {entry.Type} {activity.gameObject.name}");
        }

        private void Hide(IActivity activity, bool showPreviousScreen, bool removeFromStack)
        {
            var entry = this.activityToEntry[activity];
            if (entry.Type is { })
            {
                activity.transform.SetParent(this.canvas.Hiddens, false);
                entry.Type = null;
                entry.Views.ForEach(view => view.OnHide());
                this.logger.Debug($"Hide {activity.gameObject.name}");
            }
            if (removeFromStack)
            {
                this.screensStack.Remove(activity);
            }
            if (showPreviousScreen && this.screensStack.LastOrDefault() is { } nextScreen && this.activityToEntry[nextScreen].Type is null)
            {
                this.Show(nextScreen, null, ActivityType.Screen, false);
            }
        }

        private void Dispose(IActivity activity, bool showPreviousScreen)
        {
            this.Hide(activity, showPreviousScreen, removeFromStack: true);
            var activityName = activity.gameObject.name;
            Object.Destroy(activity.gameObject);
            this.activityToEntry.Remove(activity, out var entry);
            entry.Views.ForEach(view => view.OnDispose());
            if (this.activityToPrefab.Remove(activity, out var prefab))
            {
                this.prefabToActivity.Remove(prefab);
            }
            if (this.activityToKey.Remove(activity, out var name))
            {
                this.assetsManager.Unload(name);
                this.keyToPrefab.Remove(name);
            }
            this.logger.Debug($"Dispose {activityName}");
        }

        #endregion

        private sealed class ActivityEntry
        {
            public IReadOnlyList<IView> Views { get; }
            public ActivityType?        Type  { get; set; }

            public ActivityEntry(IReadOnlyList<IView> views)
            {
                this.Views = views;
            }
        }
    }
}