﻿#nullable enable
namespace UniT.UI.View
{
    using UniT.UI.Activity;
    using UnityEngine;
    #if UNIT_UNITASK
    using System.Threading;
    #else
    using System.Collections;
    using System.Collections.Generic;
    #endif

    public interface IView
    {
        public IUIManager Manager { get; set; }

        public IActivity Activity { get; set; }

        public void OnInitialize();

        public void OnShow();

        public void OnHide();

        #if UNIT_UNITASK
        public CancellationToken GetCancellationTokenOnDisable();
        #else
        public void StartCoroutine(IEnumerator coroutine);

        public void StopCoroutine(IEnumerator coroutine);

        public IEnumerator GatherCoroutines(params IEnumerator[] coroutines);

        public IEnumerator GatherCoroutines(IEnumerable<IEnumerator> coroutines);
        #endif

        // ReSharper disable once InconsistentNaming
        public GameObject gameObject { get; }
    }

    public interface IViewWithoutParams : IView
    {
    }

    public interface IViewWithParams<TParams> : IView
    {
        public TParams Params { get; set; }
    }
}