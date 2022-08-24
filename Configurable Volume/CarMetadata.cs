using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Menthus15Mods.Configurable_Sound_Volume
{
    public class CarMetadata
    {
        private static CarMetadata[] _allCars;

        private static readonly string[] TargetedSoundControllerFields;

        public static Dictionary<string, FieldInfo> SoundControllerFields { get; }

        public static CarMetadata[] AllCars
        {
            get
            {
                if (_allCars == null || _allCars[0].Car == null)
                {
                    _allCars = (from car in Resources.FindObjectsOfTypeAll(SoundControllerType)
                                select new CarMetadata(car)).ToArray();
                }
                return _allCars;
            }
        }

        public Dictionary<string, float> OriginalSoundControllerFieldValues { get; } = new Dictionary<string, float>();


        private Object Car { get; }

        private static System.Type SoundControllerType => Object.FindObjectsOfType(typeof(Component)).First((Object comp) => comp.GetType().Name == "SoundController").GetType();

        static CarMetadata()
        {
            TargetedSoundControllerFields = new string[10] { "brakeNoiseVolume", "engineNoThrottleVolume", "engineThrottleVolume", "scrapeNoiseVolume", "shiftTriggerVolume", "skidVolume", "startEngineVolume", "transmissionVolume", "transmissionVolumeReverse", "windVolume" };
            SoundControllerFields = (from field in SoundControllerType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                                     where TargetedSoundControllerFields.Contains(field.Name)
                                     select field).ToDictionary((field) => field.Name);
        }

        public CarMetadata(Object car)
        {
            Car = car;
            InitializeOriginalSoundControllerFieldValues();
        }

        public void SetCarVolume(float scalarValue)
        {
            foreach (string fieldName in SoundControllerFields.Keys)
            {
                SetCarField(fieldName, scalarValue);
            }
            if (Car && Car.name.Contains("JONNEZ"))
            {
                AudioSource starterAudio = ((Component)Car).transform.FindChild("Starter").GetComponent<AudioSource>();
                starterAudio.volume = scalarValue;
            }
        }

        private void SetCarField(string fieldName, float scalarValue)
        {
            float originalValue = OriginalSoundControllerFieldValues[fieldName];
            float newValue = originalValue * scalarValue;
            FieldInfo settingField = SoundControllerFields[fieldName];
            settingField.SetValue(Car, newValue);
        }

        private void InitializeOriginalSoundControllerFieldValues()
        {
            string[] targetedSoundControllerFields = TargetedSoundControllerFields;
            foreach (string fieldName in targetedSoundControllerFields)
            {
                FieldInfo fieldInfo = SoundControllerFields[fieldName];
                float originalValue = (float)fieldInfo.GetValue(Car);
                OriginalSoundControllerFieldValues.Add(fieldName, originalValue);
            }
        }
    }
}