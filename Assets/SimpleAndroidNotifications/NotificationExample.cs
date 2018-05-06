using System;
using UnityEngine;

namespace Assets.SimpleAndroidNotifications
{
    public class NotificationExample : MonoBehaviour
    {
        public void OnGUI ()
        {
            if (GUILayout.Button("Simple 5 sec", GUILayout.Height(Screen.height * 0.2f), GUILayout.Width(Screen.width)))
            {
                NotificationManager.Send(TimeSpan.FromSeconds(5), "Simple notification", "Customize icon and color", new Color(1, 0.3f, 0.15f));
            }

            if (GUILayout.Button("Normal 5 sec", GUILayout.Height(Screen.height * 0.2f), GUILayout.Width(Screen.width)))
            {
                NotificationManager.SendWithAppIcon(TimeSpan.FromSeconds(5), "Notification", "Notification with app icon", new Color(0, 0.6f, 1), NotificationIcon.Message);
            }

            if (GUILayout.Button("Custom 5 sec", GUILayout.Height(Screen.height * 0.2f), GUILayout.Width(Screen.width)))
            {
                var notificationParams = new NotificationParams
                {
                    Id = UnityEngine.Random.Range(0, int.MaxValue),
                    Delay = TimeSpan.FromSeconds(5),
                    Title = "Custom notification",
                    Message = "Message",
                    Ticker = "Ticker",
                    Sound = true,
                    Vibrate = true,
                    Light = true,
                    SmallIcon = NotificationIcon.Heart,
                    SmallIconColor = new Color(0, 0.5f, 0),
                    LargeIcon = "app_icon"
                };

                NotificationManager.SendCustom(notificationParams);
            }

            if (GUILayout.Button("Cancel all", GUILayout.Height(Screen.height * 0.2f), GUILayout.Width(Screen.width)))
            {
                NotificationManager.CancelAll();
            }
        }
    }
}