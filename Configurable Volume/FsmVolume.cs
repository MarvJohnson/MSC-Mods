using HutongGames.PlayMaker;
using UnityEngine;

namespace Menthus15Mods.Configurable_Sound_Volume
{
    public class FsmVolume
    {
        private FsmFloat _FsmFloat { get; set; }

        private AudioSource _AudioSource { get; set; }

        public FsmVolume(FsmFloat fsmFloat, GameObject gameObject)
        {
            _FsmFloat = fsmFloat;
            _AudioSource = gameObject.GetComponent<AudioSource>();
        }

        public FsmFloat GetFsmFloat()
        {
            return _FsmFloat;
        }

        public AudioSource GetAudioSource()
        {
            return _AudioSource;
        }
    }
}