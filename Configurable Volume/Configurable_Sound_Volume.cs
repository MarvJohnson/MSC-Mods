using System.Linq;
using System.Reflection;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace Menthus15Mods.Configurable_Sound_Volume
{
    public class Configurable_Sound_Volume : Mod
    {
        public override string ID => "Configurable_Sound_Volume";

        public override string Name => "Configurable Sound Volume";

        public override string Author => "Menthus15";

        public override string Version => "0.1.0";

        public override string Description => "Enables the configuring of audio volumes in the game.";

        public override bool LoadInMenu => true;

        private float MaxSliderVolumes { get; } = 1f;


        private int SliderDecimalPlaces { get; } = 2;


        private float GlobalSliderVolumeScalar { get; } = 50f;


        private SettingsSlider MasterVolume { get; set; }

        private SettingsSlider MusicVolume { get; set; }

        private SettingsSlider AmbientVolume { get; set; }

        private SettingsSlider UIVolume { get; set; }

        private SettingsSlider CarVolume { get; set; }

        private SettingsSlider PlayerVolume { get; set; }

        private SettingsSlider PeopleVolume { get; set; }

        private SettingsSlider MiscVolume { get; set; }

        private static string[] CarSounds { get; } = new string[11]
        {
        "Starting", "Van", "Motor", "Truck", "Muscle", "Ruscko", "CarBuilding", "CarFoley", "Valmet", "NPH",
        "Crashes"
        };


        private static string[] PlayerSounds { get; } = new string[2] { "PlayerMisc", "Walking" };


        private static string[] PeopleSounds { get; } = new string[28]
        {
        "Swearing", "Drunk", "Fuck", "Hangover", "Shit", "Callers", "Burb", "DrunkLifter", "Rindell", "Teimo",
        "Fleetari", "JokkeWife", "Uncle", "Shitman", "Berryman", "Fighter", "Portsari", "Janitor", "CopHighway", "Cophome",
        "Singer", "Drunks", "Yes", "StatusD", "Suski", "Mummo", "Latanen", "Tohvakka"
        };


        private static string[] MiscSounds { get; } = new string[11]
        {
        "HouseFoley", "WoodChop", "Slotmachine", "Theatre", "386", "Death", "Pig", "GUI", "Store", "Bottles",
        "BottlesEmpty"
        };


        private bool InitializedMainMenuUI { get; set; }

        private AudioSource MusicVolumeComponent { get; set; }

        private GameObject MasterAudio { get; set; }

        public Configurable_Sound_Volume() : base()
        {
            InitializeMainMenuMusicVolume();
        }

        public override void ModSetup()
        {
            SetupFunction((Setup)8, InitializeMainMenuUIVolumes);
            SetupFunction((Setup)4, Mod_OnPostLoad);
        }

        public override void ModSettings()
        {
            Settings.AddHeader((Mod)(object)this, "Global");
            MasterVolume = Settings.AddSlider((Mod)(object)this, "master_volume", "Master Volume", 0f, MaxSliderVolumes, 0.07f, UpdateVolumes, SliderDecimalPlaces);
            Settings.AddText((Mod)(object)this, "Scales the volume for every sound in the game.");
            MusicVolume = Settings.AddSlider((Mod)(object)this, "music_volume", "Music Volume", 0f, MaxSliderVolumes, 0.02f, UpdateVolumes, SliderDecimalPlaces);
            Settings.AddText((Mod)(object)this, "Scales the volume for all music in the game (e.g. car radios, the main menu song, etc)");
            AmbientVolume = Settings.AddSlider((Mod)(object)this, "ambient_volume", "Ambient Volume", 0f, MaxSliderVolumes, 1f, UpdateVolumes, SliderDecimalPlaces);
            Settings.AddText((Mod)(object)this, "Scales the volume for miscellaneous sounds in the game (e.g. birds, trains, etc)");
            UIVolume = Settings.AddSlider((Mod)(object)this, "ui_volume", "UI Volume", 0f, MaxSliderVolumes, 0.7f, UpdateMainMenuUIVolumes, SliderDecimalPlaces);
            Settings.AddText((Mod)(object)this, "Scales the volume for any UI elements in the game (e.g. buttons, toggles, etc)");
            Settings.AddHeader((Mod)(object)this, "Sound Categories");
            CarVolume = Settings.AddSlider((Mod)(object)this, "car_volume", "Car Volume", 0f, MaxSliderVolumes, 0.06f, UpdateAudioCategories, SliderDecimalPlaces);
            Settings.AddText((Mod)(object)this, "Scales the volume for all cars in the game (e.g. crashing sounds, engines, braking, car building, etc)");
            PlayerVolume = Settings.AddSlider((Mod)(object)this, "player_volume", "Player Volume", 0f, MaxSliderVolumes, 1f, UpdateAudioCategories, SliderDecimalPlaces);
            Settings.AddText((Mod)(object)this, "Scales the volume for all player specific sounds (e.g. walking, chopping wood, etc)");
            PeopleVolume = Settings.AddSlider((Mod)(object)this, "people_volume", "People Volume", 0f, MaxSliderVolumes, 0.25f, UpdateAudioCategories, SliderDecimalPlaces);
            Settings.AddText((Mod)(object)this, "Scales the volume for all sounds made by people (e.g. talking, punching, cursing, etc)");
            MiscVolume = Settings.AddSlider((Mod)(object)this, "misc_volume", "Misc Volume", 0f, MaxSliderVolumes, 0.3f, UpdateAudioCategories, SliderDecimalPlaces);
            Settings.AddText((Mod)(object)this, "Scales the volume for any sound that doesn't fall under one of the other categories (e.g. light switches, slot machines, bottles, etc)");
        }

        private void Mod_OnPostLoad()
        {
            MasterAudio = GameObject.Find("MasterAudio");
            UpdateVolumes();
        }

        private void InitializeMainMenuMusicVolume()
        {
            MusicVolumeComponent = GameObject.Find("Music").GetComponent<AudioSource>();
            MusicVolumeComponent.volume = 0f;
        }

        private void InitializeMainMenuUIVolumes()
        {
            if (!InitializedMainMenuUI)
            {
                InitializedMainMenuUI = true;
                UpdateMainMenuUIVolumes();
            }
        }

        private void UpdateMainMenuUIVolumes()
        {
            MusicVolumeComponent.volume = MusicVolume.GetValue() * MasterVolume.GetValue();
            GameObject licence = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault((obj) => obj.name == "Licence");
            licence.SetActive(true);
            licence.SetActive(false);
            int result;
            FsmFloat[] volumes = (from action in (from state in (from fsm in Resources.FindObjectsOfTypeAll<PlayMakerFSM>()
                                                                 where (fsm.name.ToLower().Contains("button") || fsm.name.ToLower().Contains("color") || int.TryParse(fsm.name, out result)) && fsm.FsmStates != null && fsm.FsmStates.Length != 0
                                                                 select fsm).SelectMany((PlayMakerFSM fsm) => fsm.FsmStates)
                                                  where state.ActionsLoaded && state.Actions != null && state.Actions.Length != 0
                                                  select state).SelectMany((FsmState state) => state.Actions)
                                  where ((object)action).GetType().GetField("volume", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null
                                  select ((object)action).GetType().GetField("volume", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(action)).Cast<FsmFloat>().ToArray();
            FsmFloat[] array = volumes;
            foreach (FsmFloat volume in array)
            {
                volume.Value = UIVolume.GetValue() * MasterVolume.GetValue();
            }
        }

        private void UpdateAudioCategories()
        {
            //IL_001e: Unknown result type (might be due to invalid IL or missing references)
            //IL_0024: Expected O, but got Unknown
            foreach (Transform item in MasterAudio.transform)
            {
                Transform audioCategory = item;
                if (CarSounds.Contains(audioCategory.name))
                {
                    UpdateAudioChildren(CarVolume.GetValue(), audioCategory);
                    UpdateCarVolumes();
                }
                else if (PlayerSounds.Contains(audioCategory.name))
                {
                    UpdateAudioChildren(PlayerVolume.GetValue(), audioCategory);
                }
                else if (PeopleSounds.Contains(audioCategory.name))
                {
                    UpdateAudioChildren(PeopleVolume.GetValue(), audioCategory);
                }
                else if (MiscSounds.Contains(audioCategory.name))
                {
                    UpdateAudioChildren(MiscVolume.GetValue(), audioCategory);
                }
            }
        }

        private void UpdateAudioChildren(float newVolume, Transform audioCategory)
        {
            float finalVolume = GetCalculatedSliderValue(newVolume);
            Component masterAudioGroup = audioCategory.GetComponent("MasterAudioGroup");
            masterAudioGroup.GetType().GetField("groupMasterVolume").SetValue(masterAudioGroup, finalVolume);
        }

        private void UpdateVolumes()
        {
            UpdateGlobalVolumes();
            UpdateAudioCategories();
        }

        private void UpdateCarVolumes()
        {
            float carVolumeScalar = GetCalculatedSliderValue(CarVolume.GetValue());
            CarMetadata[] allCars = CarMetadata.AllCars;
            foreach (CarMetadata vehicle in allCars)
            {
                vehicle.SetCarVolume(carVolumeScalar);
            }
        }

        private void UpdateGlobalVolumes()
        {
            FsmFloat gameVolume = PlayMakerGlobals.Instance.Variables.GetFsmFloat("GameVolume");
            FsmFloat ambience = PlayMakerGlobals.Instance.Variables.GetFsmFloat("SoundAmbienceVolume");
            gameVolume.Value = MasterVolume.GetValue();
            ambience.Value = GetCalculatedSliderValue(AmbientVolume.GetValue());
            if (MusicVolumeComponent)
            {
                MusicVolumeComponent.volume = GetCalculatedSliderValue(MusicVolume.GetValue());
            }
        }

        private float GetCalculatedSliderValue(float baseValue)
        {
            return baseValue * MasterVolume.GetValue() * GlobalSliderVolumeScalar;
        }
    }
}