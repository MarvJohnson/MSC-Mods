using HutongGames.PlayMaker;
using MSCLoader;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Menthus15Mods.Just_Wait.UI
{
    public class WaitPanel : MonoBehaviour
    {
        /// <summary>
        /// The text that shows how many hours will pass and what the new time will be.
        /// </summary>
        [field: SerializeField, Header("Wait Time Text")]
        private Text WaitTimeText { get; set; }
        /// <summary>
        /// The slider responsible for setting how many hours will pass.
        /// </summary>
        [field: SerializeField, Header("Wait Time Slider")]
        private Slider WaitTimeSlider { get; set; }
        /// <summary>
        /// The value of WaitTimeSlider, formatted to match MSC's timing scheme.
        /// </summary>
        private int WaitTime { get; set; }
        /// <summary>
        /// The method called when the player clicks the 'confirm' button.
        /// </summary>
        private Action<int> ConfirmCallback { get; set; }
        /// <summary>
        /// The method called whenever the WaitPanel is disabled.
        /// </summary>
        private Action CloseCallback { get; set; }
        /// <summary>
        /// A delegate that returns the sun time from a variable belonging to the JustWait class.
        /// </summary>
        private Func<int> GetSunTime { get; set; }

        /// <summary>
        /// Caches the WaitTime, sets the text shown to the player, and locks the WaitTimeSlider's value to a multiple of 2.
        /// </summary>
        /// <param name="waitTime"></param>
        public void UpdateWaitTime(float waitTime)
        {
            WaitTime = GetMilitaryTimeAsMultipleOfTwo(waitTime);
            WaitTimeSlider.value = WaitTime;
            var calculatedWaitTime = GetMSCTime();
            var dayPeriod = GetMilitaryDayPeriod(calculatedWaitTime);
            var regularTime = GetRegularTimeFromMilitaryTime(calculatedWaitTime);
            var formattedRegularTime = GetFormattedRegularTime(regularTime, dayPeriod);
            SetWaitTimeText(formattedRegularTime);
        }

        /// <summary>
        /// Caches callbacks and a fsm variable from the JustWait class.
        /// </summary>
        /// <param name="confirmCallback">The method called when the player clicks the 'confirm' button.</param>
        /// <param name="closeCallback">The method called whenever the WaitPanel is disabled.</param>
        /// <param name="sunTime">A delegate that returns the sun time from a variable belonging to the JustWait class.</param>
        public void Setup(Action<int> confirmCallback, Action closeCallback, Func<int> sunTime)
        {
            GetSunTime = sunTime;
            ConfirmCallback = confirmCallback;
            CloseCallback = closeCallback;
        }

        /// <summary>
        /// Invokes the ConfirmCallback and disables the WaitPanel.
        /// </summary>
        public void ConfirmWait()
        {
            ConfirmCallback(WaitTime);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Takes a military time input and returns it as a ceiled multiple of 2. For example, an input of 11 would return 12 and an input of 3 would return 4.
        /// </summary>
        /// <param name="militaryTime">A number between 0 and 24.</param>
        /// <returns>A multiple of 2 between 2 and 24 (inclusive).</returns>
        private int GetMilitaryTimeAsMultipleOfTwo(float militaryTime)
        {
            return Mathf.CeilToInt(Mathf.Clamp(militaryTime, 2f, 24f) / 2f) * 2;
        }

        /// <summary>
        /// Gets the in-game time, plus however many hours the player would like to wait, as a multiple of 2.
        /// </summary>
        /// <returns>The sum of the current time in-game and the number of hours the player would like to wait, as a multiple of 2.</returns>
        private int GetMSCTime()
        {
            return Mathf.CeilToInt(Mathf.Clamp((GetSunTime() + WaitTime) % 25f, 2f, 24f) / 2f) * 2;
        }

        /// <summary>
        /// Takes a number between 0 and 24 and returns a number between 1 and 12 (inclusive).
        /// </summary>
        /// <param name="militaryTime">A whole number between 0 and 24.</param>
        /// <returns>A number between 1 and 12.</returns>
        private int GetRegularTimeFromMilitaryTime(int militaryTime)
        {
            return militaryTime > 12 ? militaryTime - 12 : militaryTime;
        }

        /// <summary>
        /// Returns AM or PM based on the time given.
        /// </summary>
        /// <param name="militaryTime">A whole number between 0 and 24.</param>
        /// <returns>The string 'AM' or 'PM'.</returns>
        private string GetMilitaryDayPeriod(int militaryTime)
        {
            return militaryTime % 24 < 12f ? "AM" : "PM";
        }

        /// <summary>
        /// Sets the text for the WaitTimeText UI element.
        /// </summary>
        /// <param name="formattedRegularTime">A string in the format of time:dayPeriod.</param>
        private void SetWaitTimeText(string formattedRegularTime)
        {
            var newText = $"{WaitTime} hours ({formattedRegularTime})";
            WaitTimeText.text = newText;
        }

        /// <summary>
        /// Creates a string in the format of time:dayPeriod.
        /// </summary>
        /// <param name="regularTime">A whole number between 1 and 12.</param>
        /// <param name="dayPeriod">The string 'AM' or 'PM'.</param>
        /// <returns>A string in the format of time:dayPeriod.</returns>
        private string GetFormattedRegularTime(int regularTime, string dayPeriod)
        {
            return $"{regularTime}{dayPeriod}";
        }

        private void OnEnable()
        {
            WaitTimeSlider.value = 2;
            WaitTimeSlider.onValueChanged.Invoke(2);
        }

        private void OnDisable()
        {
            CloseCallback();
        }
    }
}