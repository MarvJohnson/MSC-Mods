using MSCLoader;
using UnityEngine;

namespace Menthus15Mods.Voicemail
{
    public class Voicemail : Mod
    {
        public override string ID => "Voicemail"; //Your mod ID (unique)
        public override string Name => "Voicemail"; //You mod name
        public override string Author => "Menthus15"; //Your Username
        public override string Version => "0.1.0"; //Version
        public override string Description => ""; //Short description of your mod

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
        }

        public override void ModSettings()
        {
            // All settings should be created here. 
            // DO NOT put anything else here that settings or keybinds
        }

        private void Mod_OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            // Called once, when mod is loading after game is fully loaded
            var phone = GameObject.Find("YARD/Building/LIVINGROOM/Telephone/Logic/PhoneLogic");
            var phoneFsm = phone.GetPlayMaker("Ring");
            var phoneFsmStates = phoneFsm.FsmStates;

            foreach (var state in phoneFsmStates)
                phone.FsmInject(state.Name, () => ModConsole.Log($"{state.Name} was executed!"));
        }
    }
}
