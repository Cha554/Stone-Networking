using gunlibary;
using Photon.Pun;
using Photon.Realtime;
using StupidTemplate.Classes;
using StupidTemplate.Stone;
using StupidTemplate.Stone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static StupidTemplate.Stone.StoneBase;

namespace StupidTemplate.Stone
{
    internal class StoneConfig_Config
    {
        public static string[] AdminEvents = { "Vibrate", "Slow", "Fling", "Stutter", "Bring", "BreakMovemet", "Message", "SendToMOD", "LowGrav", "NoGrav", "HighGrav", "ScaleDown", "ScaleUp", "ScaleReset", "dark", "light", "60hz", "72hz", "-1hz", "0hz", "999hz", "snapneck", "fixneck" };
        public static string[] HeadAdminEvents = { "Vibrate", "appquit", "Slow", "Kick", "Fling", "Stutter", "Bring", "BreakMovemet", "Stop", "Message", "SendToMOD", "LowGrav", "NoGrav", "HighGrav", "DisableNameTags", "EnableNameTags", "ScaleDown", "ScaleUp", "ScaleReset", "dark", "light", "60hz", "72hz", "-1hz", "0hz", "999hz", "tp", "snapneck", "fixneck", "obliterate", "sendmydomain...", "ABC_Menu" };
        public static string[] OwnerEvents = { "Vibrate", "Slow", "Kick", "Fling", "Stutter", "Bring", "BreakMovemet", "Stop", "Message", "SendToMOD", "SpazLighting", "LowGrav", "NoGrav", "HighGrav", "DisableNameTags", "EnableNameTags", "ScaleDown", "ScaleUp", "ScaleReset", "appquit", "dark", "light", "60hz", "72hz", "-1hz", "0hz", "999hz", "tp", "snapneck", "fixneck", "Ban4H", "an", "obliterate", "sendmydomain...", "ABC_Menu" };
        public static string userId = PhotonNetwork.LocalPlayer.UserId;
        public static void GunEvent(string Event)
        {
            GunTemplate.Gun(() =>
            {
                if ((IsOwner(userId) && StoneConfig_Config.OwnerEvents.Contains(Event)) ||
                    (IsHeadAdmin(userId) && StoneConfig_Config.HeadAdminEvents.Contains(Event)) ||
                    (IsAdmin(userId) && StoneConfig_Config.AdminEvents.Contains(Event)))
                {
                    StoneBase.SendEvent(Event, RigManager.GetPlayerFromVRRig(GunTemplate.LockedRig));
                }
                else
                {
                    NotificationLib.SendNotification("STONE : You are not allowed to use this stone mod.");
                }
            }, true);
        }


        public static void EventAll(string Event)
        {
            if (IsOwner(userId) && StoneConfig_Config.OwnerEvents.Contains(Event))
            {
                StoneBase.SendEvent(Event);
            }
            else if (IsHeadAdmin(userId) && StoneConfig_Config.HeadAdminEvents.Contains(Event))
            {
                StoneBase.SendEvent(Event);
            }
            else if (IsAdmin(userId) && StoneConfig_Config.AdminEvents.Contains(Event))
            {
                StoneBase.SendEvent(Event);
            }
            else
            {
                NotificationLib.SendNotification("STONE : You are not allowed to use this stone mod.");
            }
        }


        public static void PrimaryButtonEventAll(string Event)
        {
            if (ControllerInputPoller.instance.rightControllerPrimaryButton || ControllerInputPoller.instance.leftControllerPrimaryButton)
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
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (rig != GorillaTagger.Instance.offlineVRRig)
                {
                    if (Vector3.Distance(GorillaTagger.Instance.rightHandTransform.position, rig.headMesh.transform.position) < 0.9f
                        && ControllerInputPoller.instance.rightGrab)
                    {
                        StoneBase.SendEvent("GrabR", RigManager.GetPlayerFromVRRig(rig));
                    }

                    if (Vector3.Distance(GorillaTagger.Instance.leftHandTransform.position, rig.headMesh.transform.position) < 0.9f
                        && ControllerInputPoller.instance.leftGrab)
                    {
                        StoneBase.SendEvent("GrabL", RigManager.GetPlayerFromVRRig(rig));
                    }
                }
            }
        }
    }
}
