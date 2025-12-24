#nullable enable
namespace UniT.UI
{
    using System;
    using System.Collections.Generic;
    using UniT.Extensions;
    #if UNIT_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public interface IUIManager
    {
        public IActivity? CurrentScreen { get; }

        public IActivity? PreviousScreen { get; }

        public IEnumerable<IActivity> CurrentPopups { get; }

        public IEnumerable<IActivity> CurrentOverlays { get; }

        public IEnumerable<IActivity> CurrentOverlayPopups { get; }

        public TActivity Register<TActivity>(TActivity activity) where TActivity : IActivity;

        public TActivity Get<TActivity>(IActivity prefab) where TActivity : IActivity;

        public TActivity Get<TActivity>(object key) where TActivity : IActivity;

        public ActivityType? GetType(IActivity activity);

        public void Show<TActivity>(TActivity activity, ActivityType type, bool force = false) where TActivity : IActivityWithoutParams;

        public void Show<TActivity, TParams>(TActivity activity, TParams @params, ActivityType type, bool force = true) where TActivity : IActivityWithParams<TParams> where TParams : notnull;

        public void Hide(IActivity activity, bool showPreviousScreen = true);

        public void Dispose(IActivity activity, bool showPreviousScreen = true);

        #region Implicit Key

        public TActivity Get<TActivity>() where TActivity : IActivity => this.Get<TActivity>(typeof(TActivity).GetKey());

        #endregion

        #region Async

        #if UNIT_UNITASK
        public UniTask<TActivity> GetAsync<TActivity>(object key, IProgress<float>? progress = null, CancellationToken cancellationToken = default) where TActivity : IActivity;

        public UniTask<TActivity> GetAsync<TActivity>(IProgress<float>? progress = null, CancellationToken cancellationToken = default) where TActivity : IActivity => this.GetAsync<TActivity>(typeof(TActivity).GetKey(), progress, cancellationToken);
        #else
        public IEnumerator GetAsync<TActivity>(object key, Action<TActivity> callback, IProgress<float>? progress = null) where TActivity : IActivity;

        public IEnumerator GetAsync<TActivity>(Action<TActivity> callback, IProgress<float>? progress = null) where TActivity : IActivity => this.GetAsync(typeof(TActivity).GetKey(), callback, progress);
        #endif

        #endregion
    }
}