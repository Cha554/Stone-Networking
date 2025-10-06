using System;
using System.Collections.Generic;
using System.Text;
using Mist.Mods.Stone;
using gunlibary;
using UnityEngine;
using Photon.Realtime;
using StupidTemplate.Classes;
using Console;

namespace Mist.Mods.Stone
{
    internal class StoneConfig_Config
    {
        public static void GunEvent(string Event)
        {
            GunTemplate.Gun(() =>
            {
                StoneBase.SendEvent(Event, RigManager.GetPlayerFromVRRig(GunTemplate.LockedRig));
            }, true);
        }

        public static void EventAll(string Event)
        {
            StoneBase.SendEvent(Event);
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
