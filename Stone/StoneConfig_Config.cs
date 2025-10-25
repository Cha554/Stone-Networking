using gunlibary;
using Photon.Pun;
using Photon.Realtime;
using StupidTemplate.Classes;
using StupidTemplate.Notifications;
using StupidTemplate.Stone;
using StupidTemplate.Stone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using UnityEngine;
using static StupidTemplate.Stone.StoneBase;

namespace StupidTemplate.Stone
{
    internal class StoneConfig_Config
    {
        public static string HelperEvents = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/whitelist/HelperEvents").GetAwaiter().GetResult();
        public static string AdminEvents = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/whitelist/AdminEvents").GetAwaiter().GetResult();
        public static string HeadAdminEvents = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/whitelist/HeadAdminEvents").GetAwaiter().GetResult();
        public static string OwnerEvents = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/whitelist/OwnerEvents").GetAwaiter().GetResult();
        public static string userId = PhotonNetwork.LocalPlayer.UserId;
        public static void GunEvent(string Event)
        {
            GunTemplate.Gun(() =>
            {
                if ((IsOwner(userId) && StoneConfig_Config.OwnerEvents.Contains(Event)) ||
                    (IsHeadAdmin(userId) && StoneConfig_Config.HeadAdminEvents.Contains(Event)) ||
                    (IsAdmin(userId) && StoneConfig_Config.AdminEvents.Contains(Event)) ||
                    (IsHelper(userId) && StoneConfig_Config.HelperEvents.Contains(Event)))
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
                NotifiLib.SendNotification("STONE : You are not allowed to use this stone mod.");
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

