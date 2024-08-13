﻿#nullable enable
namespace UniT.UI.Utilities.Buttons
{
    using UniT.UI.View;
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Button))]
    public sealed class DisposeButton : View
    {
        protected override void OnInitialize()
        {
            this.GetComponent<Button>().onClick.AddListener(() => this.Manager.Dispose(this.Activity));
        }
    }
}