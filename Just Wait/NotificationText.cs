using MSCLoader;
using UnityEngine;
using UnityEngine.UI;

namespace Menthus15Mods.Just_Wait.UI
{
    public class NotificationText : MonoBehaviour
    {
        /// <summary>
        /// How long (in seconds) before the NotificationText's gameObject will be disabled.
        /// </summary>
        [field: SerializeField]
        private float DelayBeforeHidden { get; set; }
        /// <summary>
        /// The Text that's visible when the NotifcationText is enabled.
        /// </summary>
        [field: SerializeField]
        private Text TextUI { get; set; }

        /// <summary>
        /// Sets the TextUI's text to whatever message the player will see.
        /// </summary>
        /// <param name="notification">The message the player will see.</param>
        public void Notify(string notification)
        {
            TextUI.text = notification;
        }

        private void OnEnable()
        {
            Invoke("Disable", DelayBeforeHidden);
        }

        private void Disable()
        {
            gameObject.SetActive(false);
        }
    }
}