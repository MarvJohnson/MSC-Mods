using HutongGames.PlayMaker;
using MSCLoader;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Menthus15Mods.Voicemail
{
    public class Voicemail : Mod
    {
        public override string ID => "Voicemail"; //Your mod ID (unique)
        public override string Name => "Voicemail"; //You mod name
        public override string Author => "Menthus15"; //Your Username
        public override string Version => "0.1.0"; //Version
        public override string Description => "Stores missed calls in a voicemail that can be played back at the player's convenience."; //Short description of your mod
        private const string EmbededAssetBundlePath = "Menthus15Mods.Voicemail.Assets.voicemail.unity3d";
        private const string PhoneLogicScenePath = "YARD/Building/LIVINGROOM/Telephone/Logic/PhoneLogic";
        private const string PhoneUseHandleScenePath = "YARD/Building/LIVINGROOM/Telephone/Logic/UseHandle";
        /// <summary>
        /// The maximum number of messages that can be stored in the voicemail.
        /// </summary>
        private SettingsSliderInt MaxMessages { get; set; }
        /// <summary>
        /// Determines whether the new messages indicator will be visible on the phone.
        /// </summary>
        private SettingsCheckBox ShowNewMessagesLightCheckBox { get; set; }
        /// <summary>
        /// An array of all the different types of calls the player can receive.
        /// </summary>
        private FsmBool[] PhoneCallTopics { get; set; }
        /// <summary>
        /// The component responsible for handling the phone's logic.
        /// </summary>
        private PlayMakerFSM PhoneLogicRingFsm { get; set; }
        /// <summary>
        /// The component responsible for handling the phone's ringing behaviour.
        /// </summary>
        private PlayMakerFSM PhoneRingFsm { get; set; }
        /// <summary>
        /// Actions belonging to the state responsible for handling the phone's ringing.
        /// </summary>
        private FsmStateAction[] PhoneRingActions { get; set; }
        /// <summary>
        /// The state responsible for the beeping behaviour that occurs after a caller has hung up.
        /// </summary>
        private FsmState PhoneBeepState { get; set; }
        /// <summary>
        /// The fsm boolean which indicates whether the player picked up the phone while the phone was ringing.
        /// </summary>
        private FsmBool PhoneRingAnswer { get; set; }
        /// <summary>
        /// The fsm string that selects which call to play back when the phone is answered.
        /// </summary>
        private FsmString PhoneRingTopic { get; set; }
        /// <summary>
        /// The fsm bool that indicates whether the player has paid their phone bill.
        /// </summary>
        private FsmBool PhoneBillPaid { get; set; }
        /// <summary>
        /// The audio source that plays the dial tone whenever the player picks up the phone with no active call.
        /// </summary>
        private AudioSource PhoneNoConnection { get; set; }
        /// <summary>
        /// The voicemail indicator, which shows whether there are any stored missed calls.
        /// </summary>
        private NewMessagesLight _NewMessagesLight { get; set; }
        /// <summary>
        /// A collection of any calls (i.e. topics) that have been missed.
        /// </summary>
        private Stack<FsmBool> Messages { get; } = new Stack<FsmBool>();

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
        }

        public override void ModSettings()
        {
            MaxMessages = Settings.AddSlider(this, "max_messages", "Max Messages", 0, 10, 4);
            ShowNewMessagesLightCheckBox = Settings.AddCheckBox(this, "show_new_messages_light", "Show New Messages Light", true, ToggleNewMessagesLight);
        }

        private void Mod_OnLoad()
        {
            SetupPhone();
            SetupAssetBundle();
        }

        private void SetupPhone()
        {
            SetupPhoneVoicemailUseHandle();
            SetupPhoneNoConnection();
            SetupPhoneRingVariables();
            SetupPhoneLogicVariables();
        }

        private void SetupAssetBundle()
        {
            var assetBundle = LoadAssets.LoadBundle(EmbededAssetBundlePath);
            var newMessageLightPrefab = assetBundle.LoadAsset<GameObject>("NewMessagesLight");
            _NewMessagesLight = ((GameObject)Object.Instantiate(newMessageLightPrefab, PhoneLogicRingFsm.transform.position, PhoneLogicRingFsm.transform.rotation)).GetComponent<NewMessagesLight>();
            _NewMessagesLight.transform.position += new Vector3(-0.035f, 0.03f, -0.08f);
            _NewMessagesLight.gameObject.SetActive(ShowNewMessagesLightCheckBox.GetValue());
            assetBundle.Unload(false);
        }

        private void SetupPhoneVoicemailUseHandle()
        {
            // Making a copy of the existing use handle for picking up the phone, to repurpose it for voicemails.
            var phoneUseHandle = GameObject.Find(PhoneUseHandleScenePath);
            var phoneVoicemailUseHandle = Object.Instantiate(phoneUseHandle, phoneUseHandle.transform.position, phoneUseHandle.transform.rotation) as GameObject;

            // Sets the name of the cloned object to something more meaningful, to make searching for it in the scene easier.
            phoneVoicemailUseHandle.name = "Menthus15Mods.Voicemail.useHandle";

            // Positions the use handle just above the button area on the phone.
            phoneVoicemailUseHandle.transform.localScale = new Vector3(1f, 0.5f, 1f);
            phoneVoicemailUseHandle.transform.localPosition -= Vector3.right * 0.1f - Vector3.forward * 0.04f;

            // Replaces the default use behaviour.
            var phoneVoicemailPickPhoneState = phoneVoicemailUseHandle.GetPlayMakerState("Flip");
            var transitionToWaitPlayer = new FsmTransition();
            transitionToWaitPlayer.FsmEvent = phoneVoicemailPickPhoneState.Fsm.GetEvent("FINISHED");
            transitionToWaitPlayer.ToState = "Wait player";
            phoneVoicemailPickPhoneState.Transitions = new FsmTransition[] { transitionToWaitPlayer };
            phoneVoicemailPickPhoneState.Actions = new FsmStateAction[0];
            phoneVoicemailUseHandle.FsmInject("Flip", StartPlayingMessages);
        }

        private void SetupPhoneNoConnection()
        {
            PhoneNoConnection = GameObject.Find("MasterAudio/Callers/phone_noconnection").GetComponent<AudioSource>();
        }

        private void SetupPhoneRingVariables()
        {
            var phoneRing = GameObject.Find("YARD/Building/LIVINGROOM/Telephone/Logic/Ring");
            PhoneRingFsm = phoneRing.GetPlayMaker("Ring");
            PhoneBeepState = phoneRing.GetPlayMakerState("Beep beep");
            // State 4 handles the phone ringing behavior.
            PhoneRingActions = PhoneRingFsm.GetState("State 4").Actions;
            PhoneRingAnswer = PhoneRingFsm.GetVariable<FsmBool>("Answer");
            PhoneRingTopic = PhoneRingFsm.GetVariable<FsmString>("Topic");
            PhoneBillPaid = PhoneRingFsm.GetVariable<FsmBool>("BillsPaid");
            phoneRing.FsmInject("Disable phone", OnMissedCall);
            phoneRing.FsmInject("Beep beep", OnMessageFinished);
        }

        private void SetupPhoneLogicVariables()
        {
            var phoneLogic = GameObject.Find(PhoneLogicScenePath);
            PhoneLogicRingFsm = phoneLogic.GetPlayMaker("Ring");
            PhoneCallTopics = PhoneLogicRingFsm.FsmVariables.BoolVariables
                .Where(topic => !topic.Name.ToLower().Contains("night"))
                .ToArray();
        }

        /// <summary>
        /// Changes the visibility of the new messages light based on the related checkbox setting.
        /// </summary>
        private void ToggleNewMessagesLight()
        {
            _NewMessagesLight.gameObject.SetActive(ShowNewMessagesLightCheckBox.GetValue());

            if (_NewMessagesLight.gameObject.activeSelf && Messages.Count > 0)
                _NewMessagesLight.EnableFlashing();
        }

        /// <summary>
        /// Begins the process of playing messages stored in the voicemail.
        /// </summary>
        private void StartPlayingMessages()
        {
            // If this gameObject is active, the phone is ringing and messages shouldn't play.
            if (PhoneRingFsm.gameObject.activeSelf)
                return;

            PhoneLogicRingFsm.gameObject.SetActive(false);
            _NewMessagesLight.Enable(true);

            DelayedCallback.CreateDelayedCallback(0.8f, PlayNextMessage);
        }

        /// <summary>
        /// Configures the phone and all of it's interelated components to play the next message, and begins the audio clip.
        /// </summary>
        private void PlayNextMessage()
        {
            if (Messages.Count > 0)
            {
                var currentMessage = Messages.Pop();
                var upperCurrentMessage = currentMessage.Name.ToUpper();
                var phoneLogicTopic = PhoneLogicRingFsm.GetVariable<FsmBool>(currentMessage.Name);
                phoneLogicTopic.Value = true;
                PhoneRingAnswer.Value = true;
                PhoneRingActions[0].Enabled = false;
                PhoneRingActions[2].Enabled = false;
                PhoneBeepState.Actions[1].Enabled = true;
                PhoneRingTopic.Value = upperCurrentMessage;
                PhoneRingFsm.SendEvent(upperCurrentMessage);
                PhoneLogicRingFsm.SendEvent(upperCurrentMessage);
                PhoneRingFsm.gameObject.SetActive(true);
                ModConsole.Log($"Attempting to listen to: {currentMessage.Name}");
            }
            else
                FinishPlayingMessages();
        }

        private void OnMessageFinished()
        {
            PhoneRingAnswer.Value = false;
            DelayedCallback.CreateDelayedCallback(0.8f, delegate
            {
                PhoneBeepState.Actions[1].Enabled = false;

                DelayedCallback.CreateDelayedCallback(0.7f, delegate
                {
                    PhoneRingFsm.gameObject.SetActive(false);
                    PlayNextMessage();
                });
            });
        }

        private void FinishPlayingMessages()
        {
            PhoneRingAnswer.Value = false;
            PhoneRingActions[0].Enabled = true;
            PhoneRingActions[2].Enabled = true;
            PhoneNoConnection.enabled = false;
            DelayedCallback.CreateDelayedCallback(0.8f, delegate {
                PhoneBeepState.Actions[1].Enabled = true;
                PhoneNoConnection.enabled = true;
                _NewMessagesLight.Disable(true);
                PhoneLogicRingFsm.gameObject.SetActive(true);
            });
        }

        /// <summary>
        /// The callback injected into the "Disabled phone" state, which conditionally stores whatever call was missed.
        /// </summary>
        private void OnMissedCall()
        {
            var currentPhoneCallTopic = GetCurrentPhoneCallTopic();
            if (CanStoreMissedCall(currentPhoneCallTopic))
            {
                _NewMessagesLight.EnableFlashing();
                PhoneLogicRingFsm.GetVariable<FsmBool>(currentPhoneCallTopic.Name).Value = false;
                Messages.Push(currentPhoneCallTopic);
                ModConsole.Log($"Added call topic: {currentPhoneCallTopic.Name}");
            }
        }

        /// <summary>
        /// Checks to see if it's possible to store a message in the voicemail.
        /// </summary>
        /// <param name="currentPhoneCallTopic">The current phone call topic.</param>
        /// <returns>True if more can be stored in the voicemail, and false otherwise.</returns>
        private bool CanStoreMissedCall(FsmBool currentPhoneCallTopic)
        {
            if (
                !PhoneBillPaid.Value ||
                PhoneRingAnswer.Value ||
                currentPhoneCallTopic == null ||
                CallTopicAlreadyStored(currentPhoneCallTopic.Name) ||
                Messages.Count >= MaxMessages.GetValue()
                )
                return false;

            return true;
        }

        /// <summary>
        /// Checks to see if a specific kind of phone call has been stored in the voicemail already.
        /// </summary>
        /// <param name="currentPhoneCallTopicName">The name of the current phone call topic.</param>
        /// <returns>True if the phone call topic already exists in the voicemail, and false otherwise.</returns>
        private bool CallTopicAlreadyStored(string currentPhoneCallTopicName)
        {
            return Messages.Any(message => message.Name == currentPhoneCallTopicName);
        }

        /// <summary>
        /// Finds whichever phone call topic is active (i.e. set to true).
        /// </summary>
        /// <returns>The phone call topic in an active state.</returns>
        private FsmBool GetCurrentPhoneCallTopic()
        {
            return PhoneCallTopics.SingleOrDefault(phoneCallType => phoneCallType.Value);
        }
    }
}
