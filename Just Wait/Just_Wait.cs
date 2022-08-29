using HutongGames.PlayMaker;
using Menthus15Mods.Just_Wait.UI;
using MSCLoader;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Menthus15Mods.Just_Wait
{
    public class Just_Wait : Mod
    {
        #region Fields
        public override string ID => "Just_Wait"; //Your mod ID (unique)
        public override string Name => "Just Wait"; //You mod name
        public override string Author => "Menthus15"; //Your Username
        public override string Version => "1.5.0"; //Version
        public override string Description => "Allows the player to wait for a specified period of time."; //Short description of your mod
        /// <summary>
        /// The path to the embeded unity3d resource(s), which is used when loading assets.
        /// </summary>
        private const string EmbededAssetBundlePath = "Menthus15Mods.Just_Wait.Assets.justwait.unity3d";
        /// <summary>
        /// How long (in real-time seconds) it takes for the blindness from the player being burned by the Satsume's radiator to wear off.
        /// </summary>
        private const float PlayerBurnBlindnessEaseTime = 600f;
        /// <summary>
        /// How long (in real-time seconds) it takes for the blindness from the player being stung by a wasp to wear off.
        /// </summary>
        private const float PlayerWaspStingBlindnessEaseTime = 600f;
        /// <summary>
        /// How quickly the player's hungover state drops.
        /// </summary>
        private const float PlayerHangoverEaseRate = 0.05f;
        /// <summary>
        /// How quickly the player's hunger increases while hungover.
        /// </summary>
        private const float PlayerHangoverHungerIncreaseRate = 0.007f;
        /// <summary>
        /// How quickly the player's stress increases while hungover.
        /// </summary>
        private const float PlayerHangoverStressIncreaseRate = 0.0014f;
        private Keybind AdvanceTimeKeybind { get; set; }
        private SettingsCheckBox AllowWaitInJailCheckBox { get; set; }
        private SettingsCheckBox AllowWaitInCarCheckBox { get; set; }
        private SettingsCheckBox AllowWaitWhileDyingCheckBox { get; set; }
        private SettingsCheckBox AllowWaitWhileMovingCheckBox { get; set; }
        private SettingsCheckBox UpdatePlayerAttributesCheckBox { get; set; }
        private SettingsCheckBox UpdateFleetariTimeCheckBox { get; set; }
        private SettingsCheckBox UpdateFishTrapTimeCheckBox { get; set; }
        private SettingsCheckBox UpdateKiljuBrewTimeCheckBox { get; set; }
        private SettingsCheckBox UpdatePlayerBillsCheckBox { get; set; }
        private SettingsCheckBox UpdatePlayerPhoneCheckBox { get; set; }
        /// <summary>
        /// The component used to drive the player's movement.
        /// </summary>
        private CharacterController PlayerCharacterController { get; set; }
        /// <summary>
        /// A toggle for enabling and disabling player movement.
        /// </summary>
        private FsmBool PlayerStop { get; set; }
        /// <summary>
        /// The component responsible for how quickly the player's status bars fill.
        /// </summary>
        private FsmFloat PlayerSimulationRate { get; set; }
        /// <summary>
        /// A cache for the player's fatigue fsm variable.
        /// </summary>
        private FsmFloat PlayerFatigue { get; set; }
        /// <summary>
        /// A cache for the player's fatigue rate fsm variable.
        /// </summary>
        private FsmFloat PlayerFatigueRate { get; set; }
        /// <summary>
        /// A cache for the player's stress fsm variable.
        /// </summary>
        private FsmFloat PlayerStress { get; set; }
        /// <summary>
        /// A cache for the player's thirst fsm variable.
        /// </summary>
        private FsmFloat PlayerThirst { get; set; }
        /// <summary>
        /// A cache for the player's urine fsm variable.
        /// </summary>
        private FsmFloat PlayerUrine { get; set; }
        /// <summary>
        /// A cache for the player's hunger fsm variable.
        /// </summary>
        private FsmFloat PlayerHunger { get; set; }
        /// <summary>
        /// A cache for the player's stress rate fsm variable.
        /// </summary>
        private FsmFloat PlayerStressRate { get; set; }
        /// <summary>
        /// A cache for the player's sleeping toggle.
        /// </summary>
        private FsmBool PlayerSleeps { get; set; }
        /// <summary>
        /// A cache for the player's car control toggle. This indicates whether the player is driving.
        /// </summary>
        private FsmBool PlayerInCar { get; set; }
        /// <summary>
        /// A cache for the component which is responsible for controlling the player blindness easing, which stems from the player being burned by the Satsume's radiator.
        /// </summary>
        private FsmStateAction PlayerBurnBlindnessEasing { get; set; }
        /// <summary>
        /// A cache for the float which determines how much longer the player has to wait before regaining their vision, after being burned by the Satsume's radiator.
        /// </summary>
        private FieldInfo PlayerBurnBlindnessEasingRunningTime { get; set; }
        /// <summary>
        /// A cache for the component which is responsible for controlling the player blindness easing, which stems from the player being stung by a wasp.
        /// </summary>
        private FsmStateAction PlayerWaspStingBlindnessEasing { get; set; }
        /// <summary>
        /// A cache for the float which determines how much longer the player has to wait before regaining their vision, after being stung by a wasp.
        /// </summary>
        private FieldInfo PlayerWaspStingBlindnessEasingRunningTime { get; set; }
        /// <summary>
        /// A cache for the float which helps calculate how quickly the player's drunkenness will fade.
        /// </summary>
        private FsmFloat PlayerMetabolism { get; set; }
        /// <summary>
        /// A cache for the float which represents how drunk the player currently is.
        /// </summary>
        private FsmFloat PlayerDrunkAdjusted { get; set; }
        /// <summary>
        /// A cache for the float which represents how long before the player is no longer hungover.
        /// </summary>
        private FsmFloat PlayerHangoverTimeLeft { get; set; }
        /// <summary>
        /// The component that represents what time of day it is (in military time).
        /// </summary>
        private FsmInt SunTime { get; set; }
        /// <summary>
        /// A cache for the sun's playmaker fsm.
        /// </summary>
        private Fsm SunFSM { get; set; }
        /// <summary>
        /// The component that represents what day of the week it is (with 1 being Monday and 7 being Sunday).
        /// </summary>
        private FsmInt GlobalDay { get; set; }
        /// <summary>
        /// A global FSM boolean that manages the in-game mouse cursor state.
        /// </summary>
        private FsmBool PlayerInMenu { get; set; }
        /// <summary>
        /// The options menu (opens when you press escape in-game). Prevents raycasts from the mouse from interacting with the world while the WaitPanel is enabled.
        /// </summary>
        private GameObject OptionsMenu { get; set; }
        /// <summary>
        /// A cache for the sofa's playmaker fsm.
        /// </summary>
        private PlayMakerFSM SofaPMFSM { get; set; }
        /// <summary>
        /// A cache for the sofa's fsm.
        /// </summary>
        private Fsm SofaFSM { get; set; }
        /// <summary>
        /// A cache for the sofa's sleep time fsm variable.
        /// </summary>
        private FsmInt SofaSleepTime { get; set; }
        /// <summary>
        /// A cache for the sofa's time of day fsm variable.
        /// </summary>
        private FsmInt SofaTimeOfDay { get; set; }
        /// <summary>
        /// A cache for the sofa's rate fsm variable.
        /// </summary>
        private FsmFloat SofaRate { get; set; }
        /// <summary>
        /// A cache for the Just Wait canvas UI.
        /// </summary>
        private GameObject Canvas { get; set; }
        /// <summary>
        /// A cache for the wait panel UI.
        /// </summary>
        private WaitPanel WaitPanelComp { get; set; }
        /// <summary>
        /// A cache for the notification text UI.
        /// </summary>
        private NotificationText NotificationText { get; set; }
        /// <summary>
        /// A cache for the Jail component, which is later used to check for whether the player is jailed.
        /// </summary>
        private GameObject Jail { get; set; }
        /// <summary>
        /// A cache for Fleetari's order time.
        /// </summary>
        private FsmFloat FleetariOrderTime { get; set; }
        /// <summary>
        /// A cache for Fleetari's ferndale wait time.
        /// </summary>
        private FsmFloat FleetariFerndaleWait { get; set; }
        /// <summary>
        /// A cache for the Kilju Bucket's brew time.
        /// </summary>
        private FsmFloat KiljuBrewTime { get; set; }
        /// <summary>
        /// A cache for the Fish Trap's timer.
        /// </summary>
        private FsmFloat FishTrapTime { get; set; }
        /// <summary>
        /// A cache for the house's main electricity switch, which is used to determine if the kwh meter should advance when progressing time.
        /// </summary>
        private FsmBool HouseElectricityMainSwitch { get; set; }
        /// <summary>
        /// A cache for the house's kwh meter, which is evaluated when determining how much the player must pay.
        /// </summary>
        private FsmFloat HouseElectricityKWH { get; set; }
        /// <summary>
        /// A cache for the electricity bill wait countdown, which is evaluated when determining when to serve another bill to the player.
        /// </summary>
        private FsmFloat ElectricityBillWait { get; set; }
        /// <summary>
        /// A cache for the electricity bill cutoff countdown, which is evaluated when determining whether to cut off the power to the player's home.
        /// </summary>
        private FsmFloat ElectricityBillWaitCutoff { get; set; }
        /// <summary>
        /// A cache for the elctricity consumption speed, which is used to update the kwh meter.
        /// </summary>
        private FsmFloat ElectricityConsumptionSpeed { get; set; }
        /// <summary>
        /// A cache for the house's phone bill wait countdown, which changes how long before the next bill will arrive.
        /// </summary>
        private FsmFloat PhoneBillWait { get; set; }
        /// <summary>
        /// A cache for the house's phone bill cutoff countdown, which is evaluated when determining whether to cut off the player's home phone.
        /// </summary>
        private FsmFloat PhoneBillWaitCutoff { get; set; }
        /// <summary>
        /// A cache for the phone's delay before it checks if a call should come through (assuming one of the call topics is enabled and waiting).
        /// </summary>
        private FsmFloat PhoneRingTime { get; set; }
        /// <summary>
        /// A flag for whether time is being sped up or not.
        /// </summary>
        private bool AdvancingTime { get; set; }
        /// <summary>
        /// A flag for whether time has already been advanced.
        /// </summary>
        private bool AlreadyAdvancedTime { get; set; }
        /// <summary>
        /// When the advancing of time will stop.
        /// </summary>
        private int WaitTimestamp { get; set; }
        /// <summary>
        /// The Time.time when time advancing will be at full speed.
        /// </summary>
        private float TimeAdvanceEaseTimestamp { get; set; }
        /// <summary>
        /// How long it takes (in seconds) for time advancing to reach full speed.
        /// </summary>
        private float TimeAdvanceEaseTime { get; } = 4f;
        #endregion

        #region Setup
        public override void ModSetup()
        {
            SetupFunction(Setup.PostLoad, Mod_PostLoad);
            SetupFunction(Setup.Update, Mod_OnUpdate);
        }

        public override void ModSettings()
        {
            AdvanceTimeKeybind = Keybind.Add(this, "advance_time", "Advance Time", KeyCode.T, KeyCode.LeftShift);
            Settings.AddHeader(this, "Main Toggles");
            UpdateFleetariTimeCheckBox = Settings.AddCheckBox(this, "update_fleetari_time", "Update Fleetari Time", true);
            Settings.AddText(this, "Toggles whether the time for Fleetari service orders will pass when you advance time.");
            UpdateFishTrapTimeCheckBox = Settings.AddCheckBox(this, "update_fish_trap_time", "Update Fish Trap Time", true);
            Settings.AddText(this, "Toggles whether the time to randomly catch a fish will progress when you advance time.");
            UpdateKiljuBrewTimeCheckBox = Settings.AddCheckBox(this, "update_kilju_brew_time", "Update Kilju Brew Time", true);
            Settings.AddText(this, "Toggles whether Kilju brewing will progress when you advance time.");
            UpdatePlayerBillsCheckBox = Settings.AddCheckBox(this, "update_player_bills", "Update Player Bills", true);
            Settings.AddText(this, "Toggles whether resources that effect the cost of bills, and how long before new bills are served, progress when you advance time.");
            UpdatePlayerPhoneCheckBox = Settings.AddCheckBox(this, "update_player_phone", "Update Player Phone", true);
            Settings.AddText(this, "Toggles whether the player's phone will progress (i.e. is more likely to ring, or will stop ringing) when you advance time.");

            Settings.AddHeader(this, "Cheat Toggles");
            UpdatePlayerAttributesCheckBox = Settings.AddCheckBox(this, "update_player_attributes", "Update Player Attributes", true);
            Settings.AddText(this, "Toggles whether the player's attributes (e.g. hunger, thirst, dirtiness, etc) will progress when you advance time.");
            AllowWaitInJailCheckBox = Settings.AddCheckBox(this, "allow_wait_in_jail", "Allow Wait In Jail", false);
            Settings.AddText(this, "Toggles the ability to wait while in jail.");
            AllowWaitInCarCheckBox = Settings.AddCheckBox(this, "allow_wait_in_car", "Allow Wait In Car", false);
            Settings.AddText(this, "Toggles the ability to wait while operating a vehicle (i.e. whether you can wait while in a car).");
            AllowWaitWhileDyingCheckBox = Settings.AddCheckBox(this, "allow_wait_while_dying", "Allow Wait While Dying");
            Settings.AddText(this, "Toggles the ability to wait while you're dying from hunger, thirst, urine, etc.");
            AllowWaitWhileMovingCheckBox = Settings.AddCheckBox(this, "allow_wait_while_moving", "Allow Wait While Moving");
            Settings.AddText(this, "Toggles the ability to wait while in motion (falling, walking, etc).");

        }

        private void Mod_PostLoad()
        {
            SetupMouseCursorObjects();
            SetupTimeAdvancement();
            SetupAssetBundle();
        }

        /// <summary>
        /// Caches the objects necessary for toggling the in-game mouse cursor and handling in-menu raycasts.
        /// </summary>
        private void SetupMouseCursorObjects()
        {
            PlayerInMenu = PlayMakerGlobals.Instance.Variables
                .FindFsmBool("PlayerInMenu");
            OptionsMenu = GameObject.Find("Systems").transform
                .GetChild(7).gameObject;
        }

        /// <summary>
        /// Initializes a range of variables meant for caching.
        /// </summary>
        private void SetupTimeAdvancement()
        {
            GlobalDay = PlayMakerGlobals.Instance.Variables
                .FindFsmInt("GlobalDay");
            SetupJailVariables();
            SetupSofaVariables();
            SetupPlayerVariables();
            SetupPlayerSimulationVariables();
            SetupSunVariables();
            SetupFleetariOrderVariables();
            SetupKiljuVariables();
            SetupFishTrapVariables();
            SetupBillsVariables();
            SetupPhoneVariables();
        }

        /// <summary>
        /// Caches the components pertaining to bills the player has to pay.
        /// </summary>
        private void SetupBillsVariables()
        {
            HouseElectricityKWH = PlayMakerGlobals.Instance.Variables
                .FindFsmFloat("HouseElectricityKWH");
            HouseElectricityMainSwitch = GameObject.Find("YARD/Building/Dynamics/FuseTable/Fusetable/MainSwitch")
                .GetPlayMaker("Use")
                .GetVariable<FsmBool>("Switch");
            ElectricityBillWait = GameObject.Find("Systems/ElectricityBills")
                .GetPlayMaker("Data")
                .GetVariable<FsmFloat>("WaitBill");
            ElectricityBillWaitCutoff = GameObject.Find("Systems/ElectricityBills")
                .GetPlayMaker("Data")
                .GetVariable<FsmFloat>("WaitCutoff");
            ElectricityConsumptionSpeed = GameObject.Find("YARD/Building/Dynamics/FuseTable/Gauge/KWH-meter")
                .GetPlayMaker("Data")
                .GetVariable<FsmFloat>("Speed");
            PhoneBillWait = GameObject.Find("Systems/PhoneBills")
                .GetPlayMaker("Data")
                .GetVariable<FsmFloat>("WaitBill");
            PhoneBillWaitCutoff = GameObject.Find("Systems/PhoneBills")
                .GetPlayMaker("Data")
                .GetVariable<FsmFloat>("WaitCutoff");
        }

        /// <summary>
        /// Caches the JAIL gameObject for reuse later.
        /// </summary>
        private void SetupJailVariables()
        {
            Jail = Resources.FindObjectsOfTypeAll<GameObject>()
                .Single(obj => obj.name == "JAIL");
        }

        /// <summary>
        /// Caches sofa fsm variables for reuse later.
        /// </summary>
        private void SetupSofaVariables()
        {
            var sofa = Resources.FindObjectsOfTypeAll<Rigidbody>()
                .Single(rigi => rigi.name == "sofa(itemx)").transform
                .Find("Sleep/SleepTrigger");
            SofaPMFSM = sofa.GetComponent<PlayMakerFSM>();
            SofaFSM = SofaPMFSM.Fsm;
            SofaTimeOfDay = SofaPMFSM.GetVariable<FsmInt>("TimeOfDay");
            SofaSleepTime = SofaPMFSM.GetVariable<FsmInt>("SleepTime");
            SofaRate = SofaPMFSM.GetVariable<FsmFloat>("Rate");
        }

        /// <summary>
        /// Caches several, regularly used, player fsm variables.
        /// </summary>
        private void SetupPlayerVariables()
        {
            PlayerCharacterController = PlayMakerGlobals.Instance.Variables
                .FindFsmGameObject("SavePlayer").Value
                .GetComponent<CharacterController>();

            PlayerStop = PlayMakerGlobals.Instance.Variables
                .FindFsmBool("PlayerStop");

            PlayerFatigue = PlayMakerGlobals.Instance.Variables
                .FindFsmFloat("PlayerFatigue");

            PlayerFatigueRate = PlayMakerGlobals.Instance.Variables
                .FindFsmFloat("PlayerFatigueRate");

            PlayerStress = PlayMakerGlobals.Instance.Variables
                .FindFsmFloat("PlayerStress");

            PlayerStressRate = PlayMakerGlobals.Instance.Variables
                .FindFsmFloat("PlayerStressRate");

            PlayerHunger = PlayMakerGlobals.Instance.Variables
                .FindFsmFloat("PlayerHunger");

            PlayerThirst = PlayMakerGlobals.Instance.Variables
                .FindFsmFloat("PlayerThirst");

            PlayerUrine = PlayMakerGlobals.Instance.Variables
                .FindFsmFloat("PlayerUrine");

            PlayerSleeps = PlayMakerGlobals.Instance.Variables
                .FindFsmBool("PlayerSleeps");

            PlayerInCar = PlayMakerGlobals.Instance.Variables
                .FindFsmGameObject("SavePlayer").Value
                .GetComponents<PlayMakerFSM>()
                .Single(pmfsm => pmfsm.FsmName == "Crouch")
                .GetVariable<FsmBool>("PlayerInCar");

            var playerFPSCamera = GameObject.Find("PLAYER/Pivot/AnimPivot/Camera/FPSCamera/FPSCamera");

            var playerBlindnessPMFSM = playerFPSCamera
                .GetPlayMaker("Blindness");

            PlayerBurnBlindnessEasing = playerBlindnessPMFSM
                .GetState("Blindness").Actions[3];

            PlayerBurnBlindnessEasingRunningTime = PlayerBurnBlindnessEasing
                .GetType()
                .GetField("runningTime", BindingFlags.NonPublic | BindingFlags.Instance);

            PlayerWaspStingBlindnessEasing = playerBlindnessPMFSM
                .GetState("Ease allergy").Actions[2];

            PlayerWaspStingBlindnessEasingRunningTime = PlayerWaspStingBlindnessEasing
                .GetType()
                .GetField("runningTime", BindingFlags.NonPublic | BindingFlags.Instance);

            PlayerMetabolism = playerFPSCamera
                .GetPlayMaker("Drunk Mode")
                .GetVariable<FsmFloat>("Metabolism");

            PlayerDrunkAdjusted = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerDrunkAdjusted");

            PlayerHangoverTimeLeft = playerFPSCamera
                .GetPlayMaker("Hangover")
                .GetVariable<FsmFloat>("TimeLeft");
        }

        /// <summary>
        /// Caches the SimulationRate belonging to the 'Player' object's 'SimulationRate' PlayMakerFSM, as well as the value it has at the time.
        /// </summary>
        private void SetupPlayerSimulationVariables()
        {
            var playerDatabase = GameObject.Find("PlayerDatabase");
            var playerSimulation = playerDatabase.GetComponents<PlayMakerFSM>()
                .Single(pmfsm => pmfsm.FsmName == "Simulation");
            PlayerSimulationRate = playerSimulation.FsmVariables
                .FindFsmFloat("SimulationRate");
        }

        /// <summary>
        /// Caches the timescale and it's value, as well as the time, both belonging to the 'SUN' object's 'Color' PlayMakerFSM.
        /// </summary>
        private void SetupSunVariables()
        {
            var sunPMFSM = Object.FindObjectsOfType<PlayMakerFSM>()
                            .Single(pmfsm => pmfsm.FsmName == "Color" && pmfsm.name == "SUN");
            SunFSM = sunPMFSM.Fsm;
            SunTime = sunPMFSM.FsmVariables
                .FindFsmInt("Time");
            // Handles sending the necessary Sun event when the time gets reset to 0 when waking up at 12AM.
            // Before, the time would be set to 0 and there were no conditions checking for that time; therefore, no event
            // would be raised to update several time-based variables.
            SunFSM.GameObject.
                FsmInject("State 3", delegate
            {
                if (SunTime.Value == 0)
                {
                    SunFSM.GameObject
                    .GetPlayMaker("Color")
                    .SendEvent("24");
                }
            });
        }

        /// <summary>
        /// Loads the AssetBundle used by 'Just Wait', as well as the objects contained within the bundle.
        /// </summary>
        private void SetupAssetBundle()
        {
            var assetBundle = LoadAssets.LoadBundle(EmbededAssetBundlePath);
            SetupCanvas(assetBundle);
            SetupWaitPanel();
            SetupNotificationText();
            assetBundle.Unload(false);
        }

        /// <summary>
        /// Instantiates and caches the canvas asset.
        /// </summary>
        /// <param name="assetBundle">The asset bundle from which to pull the canvas asset.</param>
        private void SetupCanvas(AssetBundle assetBundle)
        {
            var canvasPrefab = assetBundle.LoadAsset<GameObject>("Canvas");
            Canvas = Object.Instantiate(canvasPrefab);
        }

        /// <summary>
        /// Caches and initializes the wait panel UI.
        /// </summary>
        private void SetupWaitPanel()
        {
            WaitPanelComp = Canvas.transform.Find("WaitPanel").GetComponent<WaitPanel>();
            WaitPanelComp.Setup(WaitPanelConfirmCallback, WaitPanelDisabledCallback, () => SunTime.Value);
        }

        /// <summary>
        /// Caches the notification text UI.
        /// </summary>
        private void SetupNotificationText()
        {
            NotificationText = Canvas.transform.Find("NotificationText").GetComponent<NotificationText>();
        }

        /// <summary>
        /// Caches the Kilju Bucket's "Time" variable.
        /// </summary>
        private void SetupKiljuVariables()
        {
            var kiljuBucket = GameObject.Find("ITEMS/bucket(itemx)");
            var kiljuBucketFSM = kiljuBucket.GetPlayMaker("Use");
            KiljuBrewTime = kiljuBucketFSM.GetVariable<FsmFloat>("Time");
        }

        /// <summary>
        /// Caches the Fleetari "Order" PlayMakerFSM.
        /// </summary>
        private void SetupFleetariOrderVariables()
        {
            var fleetariOrder = GameObject.Find("REPAIRSHOP/Order");
            var fleetariOrderFSM = fleetariOrder.GetPlayMaker("Data");
            FleetariOrderTime = fleetariOrderFSM.GetVariable<FsmFloat>("_OrderTime");
            FleetariFerndaleWait = fleetariOrderFSM.GetVariable<FsmFloat>("_FerndaleWait");
        }

        /// <summary>
        /// Caches the Fish Trap's "Wait" variable.
        /// </summary>
        private void SetupFishTrapVariables()
        {
            var fishTrap = GameObject.Find("fish trap(itemx)/Logic");
            var fishTrapFSM = fishTrap.GetPlayMaker("Logic");
            FishTrapTime = fishTrapFSM.GetVariable<FsmFloat>("Wait");
        }

        /// <summary>
        /// Caches the phone's logic PlayMakerFSM, which is, in part, responsible for determining when the phone should check for an incoming call and begin ringing if there is.
        /// </summary>
        private void SetupPhoneVariables()
        {
            PhoneRingTime = GameObject.Find("YARD/Building/LIVINGROOM/Telephone/Logic/PhoneLogic")
                .GetPlayMaker("Ring")
                .GetVariable<FsmFloat>("Time");
        }
        #endregion

        private void Mod_OnUpdate()
        {
            if (AdvanceTimeKeybind.GetKeybindDown() && CanWait())
                EnableWaitPanel();

            if (WaitPanelComp.gameObject.activeSelf && !OptionsMenu.activeSelf)
                WaitPanelComp.gameObject.SetActive(false);

            WaitToAdvanceTime();
        }

        /// <summary>
        /// Returns a bool signifying whether the player can wait. If the player cannot wait, a notification is raised with a reason as to why.
        /// </summary>
        /// <returns>True if the player can wait and false otherwise.</returns>
        private bool CanWait()
        {
            var playerNotification = "";

            if (ModLoader.GetCurrentScene() != CurrentScene.Game)
                playerNotification = "Must be in GAME scene!";
            else if (
                !AllowWaitInJailCheckBox.GetValue() &&
                Jail.activeSelf
                )
                playerNotification = "Cannot wait in jail!";
            else if (PlayerSleeps.Value)
                playerNotification = "Cannot wait while sleeping!";
            else if (
                !AllowWaitInCarCheckBox.GetValue() &&
                PlayerInCar.Value
                )
                playerNotification = "Cannot wait while operating a vehicle!";
            else if (
                !AllowWaitWhileMovingCheckBox.GetValue() &&
                IsPlayerMoving()
                )
                playerNotification = "Cannot wait while moving!";
            else if (
                !AllowWaitWhileDyingCheckBox.GetValue() &&
                IsPlayerDyingFromAttribute()
                )
                playerNotification = "Cannot wait while at risk of dying!";
            
            if (playerNotification.Length > 0)
            {
                NotifyPlayer(playerNotification);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether the player is moving.
        /// </summary>
        /// <returns>True if the player is moving and false otherwise.</returns>
        private bool IsPlayerMoving()
        {
            return PlayerCharacterController.velocity.sqrMagnitude > 0f;
        }

        /// <summary>
        /// Checks for whether the player is dying from any lethal attributes.
        /// </summary>
        /// <returns>True if the player is dying and false otherwise.</returns>
        private bool IsPlayerDyingFromAttribute()
        {
            if (
                PlayerHunger.Value >= 100 ||
                PlayerThirst.Value >= 100 ||
                PlayerStress.Value >= 100 ||
                PlayerUrine.Value >= 100
                )
                return true;

            return false;
        }

        /// <summary>
        /// Makes the notification text UI visible with a notification message.
        /// </summary>
        /// <param name="notification">The text used to notify the player.</param>
        private void NotifyPlayer(string notification)
        {
            NotificationText.gameObject.SetActive(true);
            NotificationText.Notify(notification);
        }

        /// <summary>
        /// Restricts player control, shows the player's in-game mouse cursor, and enables the WaitPanel.
        /// </summary>
        private void EnableWaitPanel()
        {
            if (AdvancingTime || AlreadyAdvancedTime)
                return;

            SetPlayerControllable(false);
            SetMouseCursor(true);
            NotificationText.gameObject.SetActive(false);
            WaitPanelComp.gameObject.SetActive(true);
        }

        /// <summary>
        /// Strips or returns movement control to the player.
        /// </summary>
        /// <param name="controllable">The flag that determines controllability.</param>
        private void SetPlayerControllable(bool controllable)
        {
            PlayerStop.Value = !controllable;
        }

        /// <summary>
        /// Determines whether the player's in-game mouse cursor is visible.
        /// </summary>
        /// <param name="state">The new 'visibility' state for the mouse cursor.</param>
        private void SetMouseCursor(bool state)
        {
            PlayerInMenu.Value = state;
            OptionsMenu.SetActive(state);
        }

        /// <summary>
        /// A callback for when the WaitPanel's GameObject is disabled. Hides the player's in-game mouse cursor and conditionally enables movement.
        /// </summary>
        private void WaitPanelDisabledCallback()
        {
            OptionsMenu.SetActive(false);

            if (!AdvancingTime)
            {
                SetMouseCursor(false);
                SetPlayerControllable(true);
            }
        }

        /// <summary>
        /// Sets up the variables necessary for indicating time advancement, when the timescale will be at full speed, when to top advancing time, and closes the player's eyes.
        /// </summary>
        /// <param name="waitTimestamp"></param>
        private void WaitPanelConfirmCallback(int waitTimestamp)
        {
            AdvancingTime = true;
            WaitTimestamp = waitTimestamp;
            TimeAdvanceEaseTimestamp = Time.time + TimeAdvanceEaseTime;
            ExecuteFSMState(SofaFSM, "Sleep");
        }

        /// <summary>
        /// Controls when time is advanced, based on a graceperiod.
        /// </summary>
        private void WaitToAdvanceTime()
        {
            if (Time.time < TimeAdvanceEaseTimestamp)
                return;

            if (AdvancingTime)
                AdvanceTime();
            else if (AlreadyAdvancedTime)
                FinishAdvancingTime();
        }

        /// <summary>
        /// Advances time and increases player attributes proportional to hours passed.
        /// </summary>
        private void AdvanceTime()
        {
            var waitTimestampAsRealtimeSeconds = WaitTimestamp / 12f * 60f * 60f;
            SetPlayerAttributes();
            UpdateFleetariTime(waitTimestampAsRealtimeSeconds);
            UpdateKiljuBrewTime();
            UpdateFishTrapTimer(waitTimestampAsRealtimeSeconds);
            UpdatePlayerBills(waitTimestampAsRealtimeSeconds);
            UpdatePhoneRing(waitTimestampAsRealtimeSeconds);
            MimicSofaTransitions();
            StartFinishTimeAdvancement();
        }

        /// <summary>
        /// Begins the waiting period for returning control to the player and opening their eyes.
        /// </summary>
        private void StartFinishTimeAdvancement()
        {
            AlreadyAdvancedTime = true;
            // TODO: Create another variable for handling the finishing graceperiod. Reusing WaitTimestamp in this way will likely lead to future confusion.
            WaitTimestamp = Mathf.RoundToInt(Time.time);
            TimeAdvanceEaseTimestamp = Time.time + TimeAdvanceEaseTime;
            AdvancingTime = false;
        }

        /// <summary>
        /// Updates Fleetari's order timer based on the amount of time passed, proportional to real time.
        /// </summary>
        private void UpdateFleetariTime(float waitTimestampAsRealtimeSeconds)
        {
            if (!UpdateFleetariTimeCheckBox.GetValue())
                return;

            FleetariFerndaleWait.Value = Mathf.Clamp(FleetariFerndaleWait.Value - waitTimestampAsRealtimeSeconds, 0f, float.PositiveInfinity);
            FleetariOrderTime.Value = Mathf.Clamp(FleetariOrderTime.Value - waitTimestampAsRealtimeSeconds, 0f, float.PositiveInfinity);
        }

        /// <summary>
        /// Updates the Kilju Bucket's brew time based on the amount of time passed, proportional to real time.
        /// </summary>
        private void UpdateKiljuBrewTime()
        {
            if (!UpdateKiljuBrewTimeCheckBox.GetValue())
                return;

            var waitTimestampAsRealtimeMinutes = WaitTimestamp / 12f * 60f;
            KiljuBrewTime.Value = Mathf.Clamp(KiljuBrewTime.Value + waitTimestampAsRealtimeMinutes, 0f, float.PositiveInfinity);
        }

        /// <summary>
        /// Updates the Fish Trap's timer based on the amount of time passed, proportional to real time.
        /// </summary>
        private void UpdateFishTrapTimer(float waitTimestampAsRealtimeSeconds)
        {
            if (!UpdateFishTrapTimeCheckBox.GetValue())
                return;

            FishTrapTime.Value = Mathf.Clamp(FishTrapTime.Value - waitTimestampAsRealtimeSeconds, 0f, float.PositiveInfinity);
        }

        /// <summary>
        /// Updates the resources which effect how much the player will have to pay for bills, as well as how long before the next bill will be served to them.
        /// </summary>
        private void UpdatePlayerBills(float waitTimestampAsRealtimeSeconds)
        {
            if (!UpdatePlayerBillsCheckBox.GetValue())
                return;

            if (HouseElectricityMainSwitch.Value)
                HouseElectricityKWH.Value += waitTimestampAsRealtimeSeconds * (ElectricityConsumptionSpeed.Value * 0.0001f);

            ElectricityBillWait.Value += waitTimestampAsRealtimeSeconds;
            ElectricityBillWaitCutoff.Value += waitTimestampAsRealtimeSeconds;
            PhoneBillWait.Value += waitTimestampAsRealtimeSeconds;
            PhoneBillWaitCutoff.Value += waitTimestampAsRealtimeSeconds;
        }

        /// <summary>
        /// Updates the time before the phone will check for an incoming call and ring if there is one.
        /// </summary>
        private void UpdatePhoneRing(float waitTimestampAsRealtimeSeconds)
        {
            if (!UpdatePlayerPhoneCheckBox.GetValue())
                return;

            PhoneRingTime.Value -= waitTimestampAsRealtimeSeconds;
        }

        /// <summary>
        /// Sets player and sofa fsm values to what they would be after waiting for 'WaitTimestamp' number of hours.
        /// </summary>
        private void SetPlayerAttributes()
        {
            if (!UpdatePlayerAttributesCheckBox.GetValue())
                return;

            var waitTimestampMinutes = WaitTimestamp * 60f;
            var waitTimestampSeconds = waitTimestampMinutes * 60f;
            var waitTimestampRealtimeSeconds = waitTimestampSeconds / 12f;
            var playerIsDrunk = PlayerDrunkAdjusted.Value > 0f;
            var playerIsHungover = PlayerHangoverTimeLeft.Value > 0f;
            var addedPlayerStressFromHangover = playerIsHungover ? waitTimestampSeconds * PlayerHangoverHungerIncreaseRate : 0f;
            var addedPlayerStressFromTime = waitTimestampMinutes / PlayerSimulationRate.Value * PlayerStressRate.Value;
            var totalAddedPlayerStress = addedPlayerStressFromTime + addedPlayerStressFromHangover;
            var addedPlayerHungerFromHangover = playerIsHungover ? waitTimestampSeconds * PlayerHangoverStressIncreaseRate : 0f;

            PlayerFatigue.Value += waitTimestampMinutes / PlayerSimulationRate.Value * PlayerFatigueRate.Value;
            PlayerStress.Value += totalAddedPlayerStress;
            PlayerHunger.Value += addedPlayerHungerFromHangover;
            SetPlayerEasing(waitTimestampMinutes, PlayerBurnBlindnessEasing, PlayerBurnBlindnessEasingRunningTime, PlayerBurnBlindnessEaseTime);
            SetPlayerEasing(waitTimestampMinutes, PlayerWaspStingBlindnessEasing, PlayerWaspStingBlindnessEasingRunningTime, PlayerWaspStingBlindnessEaseTime);

            if (playerIsDrunk)
                PlayerDrunkAdjusted.Value = Mathf.Clamp(PlayerDrunkAdjusted.Value - waitTimestampRealtimeSeconds * PlayerMetabolism.Value, 0f, float.PositiveInfinity);

            if (playerIsHungover)
                PlayerHangoverTimeLeft.Value = Mathf.Clamp(PlayerHangoverTimeLeft.Value - waitTimestampRealtimeSeconds * PlayerHangoverEaseRate, 0f, float.PositiveInfinity);

            // These two assignments allow the existing fsm state to handle updating all the other attributes.
            SofaSleepTime.Value = WaitTimestamp;
            SofaRate.Value = waitTimestampMinutes / PlayerSimulationRate.Value;
        }

        private void SetPlayerEasing(float waitTimestampMinutes, object playerEasing, FieldInfo playerEasingRunningTime, float easeTime)
        {
            var currentRunningTime = (float)playerEasingRunningTime.GetValue(playerEasing);
            var increaseInRunningTime = waitTimestampMinutes * 60f / 12f;
            // Must clamp the value to the "wait time" assigned to the easing function. If it goes above that, unexpected behavior can occur.
            var newRunningTime = Mathf.Clamp(currentRunningTime + increaseInRunningTime, 0f, easeTime);
            playerEasingRunningTime.SetValue(playerEasing, newRunningTime);
        }

        /// <summary>
        /// Attempts to execute all of the same actions and transitions that occur in some of the sofa's playmaker fsm.
        /// </summary>
        private void MimicSofaTransitions()
        {
            SetConflictingActionEnabled(false);

            if (UpdatePlayerAttributesCheckBox.GetValue())
                ExecuteFSMState(SofaFSM, "Calc rates");

            var shouldWakeUp = MimicCheckTimeOfDay();

            if (shouldWakeUp)
            {
                ExecuteFSMState(SofaFSM, "Wake up");
            }

            // The state called after the player wakes up
            ExecuteFSMState(SunFSM, "State 3");

            SetConflictingActionEnabled(true);
        }

        /// <summary>
        /// Changes the enabled state of several conflicting state actions belonging to the sofa fsm.
        /// </summary>
        /// <param name="enabled">The new state action enabled toggle.</param>
        private void SetConflictingActionEnabled(bool enabled)
        {
            // Sets rate based on fatigue.
            SofaPMFSM.GetState("Calc rates").Actions[0].Enabled = enabled;
            // Adjusts drunkenness
            SofaPMFSM.GetState("Calc rates").Actions[2].Enabled = enabled;
            // Sets fatigue to 0
            SofaPMFSM.GetState("Calc rates").Actions[6].Enabled = enabled;
            // Clamps TimeOfDay between 2 and 24
            SofaPMFSM.GetState("Advance day").Actions[1].Enabled = enabled;
            // Sets the Sun's Time to 24
            SunFSM.GetState("00-02").Actions[10].Enabled = enabled;
        }

        /// <summary>
        /// Attempts to execute all of the same actions belonging to the 'Check time of day' state on the sofa fsm.
        /// </summary>
        /// <returns>True if the day should advance and false otherwise.</returns>
        private bool MimicCheckTimeOfDay()
        {
            ExecuteFSMState(SofaFSM, "Check time of day");

            if (SofaTimeOfDay.Value >= 24)
            {
                ExecuteFSMState(SofaFSM, "Advance day");

                if (GlobalDay.Value > 7)
                    GlobalDay.Value = 1;
            }
            else
            {
                ExecuteFSMState(SofaFSM, "Wake up");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Stops, sets the start state for, and starts the sofa fsm in order to run the actions for a given state.
        /// </summary>
        /// <param name="state">The name of the state to run.</param>
        private void ExecuteFSMState(Fsm fsm, string state)
        {
            fsm.Stop();
            fsm.StartState = state;
            fsm.Start();
        }

        /// <summary>
        /// Returns movement control to the player.
        /// </summary>
        private void FinishAdvancingTime()
        {
            SetPlayerControllable(true);
            SetMouseCursor(false);
            AlreadyAdvancedTime = false;
        }
    }
}
