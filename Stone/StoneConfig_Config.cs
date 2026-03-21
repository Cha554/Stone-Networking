using Photon.Pun;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using static StupidTemplate.Stone.StoneBase;
using gunlibary;
using Spectral_Menu.Notifications;
using Spectral_Menu.Classes;

namespace StupidTemplate.Stone
{
    internal class StoneConfig_Config
    {
        public static string HelperEvents   = string.Empty;
        public static string AdminEvents    = string.Empty;
        public static string HeadAdminEvents = string.Empty;
        public static string OwnerEvents    = string.Empty;

        private static readonly HttpClient _http = new HttpClient { Timeout = System.TimeSpan.FromSeconds(5) };

        public static async Task LoadWhitelistAsync()
        {
            var tasks = new[]
            {
                SafeGetAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/whitelist/HelperEvents"),
                SafeGetAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/whitelist/AdminEvents"),
                SafeGetAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/whitelist/HeadAdminEvents"),
                SafeGetAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/whitelist/OwnerEvents")
            };

            string[] results = await Task.WhenAll(tasks);
            HelperEvents    = results[0];
            AdminEvents     = results[1];
            HeadAdminEvents = results[2];
            OwnerEvents     = results[3];

            Debug.Log("Stone: Whitelist events loaded.");
        }

        private static async Task<string> SafeGetAsync(string url)
        {
            try { return await _http.GetStringAsync(url); }
            catch { return string.Empty; }
        }

        public static void GunEvent(string Event)
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;
            GunTemplate.Gun(() =>
            {
                if ((IsOwner(userId)     && OwnerEvents.Contains(Event))    ||
                    (IsHeadAdmin(userId) && HeadAdminEvents.Contains(Event)) ||
                    (IsAdmin(userId)     && AdminEvents.Contains(Event))     ||
                    (IsHelper(userId)    && HelperEvents.Contains(Event)))
                {
                    StoneBase.SendEvent(Event, RigManager.GetPlayerFromVRRig(GunTemplate.LockedRig));
                }
                else
                {
                    NotifiLib.SendNotification("STONE : You are not allowed to use this stone mod.");
                }
            }, true);
        }

        public static void EventAll(string Event)
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;
            if ((IsOwner(userId) && OwnerEvents.Contains(Event))         ||
                (IsHeadAdmin(userId) && HeadAdminEvents.Contains(Event)) ||
                (IsAdmin(userId) && AdminEvents.Contains(Event)))
            {
                StoneBase.SendEvent(Event);
            }
        }

        public static void PrimaryButtonEventAll(string Event)
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton ||
                ControllerInputPoller.instance.leftControllerPrimaryButton)
            {
                StoneBase.SendEvent(Event);
            }
        }

        public static void EventPlayer(string Event, Photon.Realtime.Player plr)
        {
            StoneBase.SendEvent(Event);
        }

        public static void Grab()
        {
            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == GorillaTagger.Instance.offlineVRRig) continue;

                if (Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.headMesh.transform.position) < 0.9f
                    && ControllerInputPoller.instance.rightGrab)
                {
                    StoneBase.SendEvent("GrabR", Spectral_Menu.Classes.RigManager.GetPlayerFromVRRig(rig));
                }

                if (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.headMesh.transform.position) < 0.9f
                    && ControllerInputPoller.instance.leftGrab)
                {
                    StoneBase.SendEvent("GrabL", Spectral_Menu.Classes.RigManager.GetPlayerFromVRRig(rig));
                }
            }
        }
    }
}
