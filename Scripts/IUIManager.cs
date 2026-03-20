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
        public event Action<IActivity, IReadOnlyList<IView>> Initialized;

        public event Action<IActivity, IReadOnlyList<IView>> Shown;

        public event Action<IActivity, IReadOnlyList<IView>> Hidden;

        public event Action<IActivity, IReadOnlyList<IView>> Disposed;

        public IActivity? ShowingScreen { get; }

        public IEnumerable<IActivity> ShowingPopups { get; }

        public IEnumerable<IActivity> ShowingOverlays { get; }

        public IEnumerable<IActivity> ShowingOverlayPopups { get; }

        public void LockInteraction();

        public void UnlockInteraction(bool force = false);

        public TActivity Register<TActivity>(TActivity activity) where TActivity : IActivity;

        public TActivity Get<TActivity>(IActivity prefab) where TActivity : IActivity;

        #if !UNITY_WEBGL
        public TActivity Get<TActivity>(object key) where TActivity : IActivity;
        #endif

        public void Show<TActivity>(TActivity activity, bool force = false) where TActivity : IActivityWithoutParams;

        public void Show<TActivity, TParams>(TActivity activity, TParams @params, bool force = true) where TActivity : IActivityWithParams<TParams> where TParams : notnull;

        public void Hide(IActivity activity);

        public void Dispose(IActivity activity);

        #region Implicit Key

        #if !UNITY_WEBGL
        public TActivity Get<TActivity>() where TActivity : IActivity => this.Get<TActivity>(typeof(TActivity).GetKey());
        #endif

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