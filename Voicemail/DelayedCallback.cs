using MSCLoader;
using System;
using UnityEngine;

namespace Menthus15Mods.Voicemail
{
    public class DelayedCallback : MonoBehaviour
    {
        public Action Callback { get; set; }

        public static void CreateDelayedCallback(float delay, Action callback)
        {
            var delayedCallbackObject = new GameObject("Menthus15Mods.Voicemail.DelayedCallback", typeof(DelayedCallback));
            var delayedCallback = delayedCallbackObject.GetComponent<DelayedCallback>();
            delayedCallback.StartCountdown(delay, callback);
        }

        private void StartCountdown(float delay, Action callback)
        {
            Callback = callback;
            Invoke(nameof(InvokeCallback), delay);
        }

        private void InvokeCallback()
        {
            Callback();
            Destroy(gameObject);
        }
    }
}