using BepInEx;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using Newtonsoft.Json;
using Oculus.Platform;
using Photon.Pun;
using Spectral_Menu.Classes;
using Spectral_Menu.Menu;
using Spectral_Menu.Notifications;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static Spectral_Menu.Menu.modules;
using static Spectral_Menu.Settings;
using static UnityEngine.ParticleSystem;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Random = UnityEngine.Random;
using ReceiverGroup = Photon.Realtime.ReceiverGroup;
using Debug = UnityEngine.Debug;
using Spectral_Menu.Mods;

namespace StupidTemplate.Stone
{
    internal class StoneBase : MonoBehaviour
    {
        #region Start
        private static string playername;
        public static double currentStoneVersion = 2.3;
        public static string PLayerID = PhotonNetwork.LocalPlayer.UserId;

        public static string userid      = string.Empty;
        public static string Tortise     = string.Empty;
        public static string Cha         = string.Empty;
        public static string webhookUrl  = string.Empty;
        public static string ADuserid    = string.Empty;
        public static string HeadADuserid = string.Empty;
        public static string HELPERuserid = string.Empty;
        public static string NOVAuserid  = string.Empty;

        public static double latestStoneVersion = 0;
        public static bool dataReady = false;

        private static readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };

        private async Task Awake()
        {
            await Task.Delay(5000);
            await LoadAllDataAsync();

            GorillaTagger.Instance.StartCoroutine(CreateButton());

            Debug.Log("Stone Networking - Developed by Cha554 and Cheemspookie");

            await Task.Delay(10000);
            SendWeb("**" + PhotonNetwork.LocalPlayer.NickName,
                "has loaded into the game with Spectral ** Stone Version:" + currentStoneVersion +
                " User ID:" + PhotonNetwork.LocalPlayer.UserId);
        }

        private static async Task LoadAllDataAsync()
        {
            var whitelistTask = StoneConfig_Config.LoadWhitelistAsync();

            try
            {
                var tasks = new[]
                {
                    SafeGetAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/userid"),
                    SafeGetAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/Cha"),
                    SafeGetAsync("WEBHOOK FOR YOUR MENU TRACKER"),
                    SafeGetAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/ADUserID's"),
                    SafeGetAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/HeadADuserid"),
                    SafeGetAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/MistHelper"),
                    SafeGetAsync("https://raw.githubusercontent.com/Cha554/mist-ext/refs/heads/main/NOVA"),
                    SafeGetAsync("https://raw.githubusercontent.com/Cha554/Stone-Networking/refs/heads/main/Stone/StoneVersion")
                };

                string[] results = await Task.WhenAll(tasks);

                userid        = results[0];
                Tortise       = results[1];
                Cha           = results[2];
                webhookUrl    = results[3];
                ADuserid      = results[4];
                HeadADuserid  = results[5];
                HELPERuserid  = results[6];
                NOVAuserid    = results[7];

                if (double.TryParse(results[8].Trim(), out double ver))
                    latestStoneVersion = ver;

                dataReady = true;
                Debug.Log($"Stone: Data loaded. Latest version: {latestStoneVersion}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Stone: Failed to load data - {ex.Message}");
                dataReady = true;
            }

            await whitelistTask;
        }

        private static async Task<string> SafeGetAsync(string url)
        {
            try { return await _http.GetStringAsync(url); }
            catch { return string.Empty; }
        }

        
        
        public static IEnumerator CreateButton()
        {
            yield return new WaitForSeconds(9f);
            StoneButtonCreation();
        }

        public static void StoneButtonCreation()
        {
            if (userid.Contains(PhotonNetwork.LocalPlayer.UserId))
            {
                playername = PhotonNetwork.NickName;
                var list = buttons[0].ToList();
                list.Add(new ButtonInfo
                {
                    ButtonText = "<color=#9FA0A1>[</color><color=#9FA0A1>S</color><color=#A9AAAB>t</color><color=#B4B5B6>o</color><color=#BEBFC0>n</color><color=#C9CACC>e</color><color=#FFFFFF>]</color>",
                    Method = () => StoneBase.BaseAccess(),
                    IsToggleable = false,
                    ToolTip = ""
                });
                buttons[0] = list.ToArray();
                NotifiLib.SendNotification($"<color=#9FA0A1>[</color><color=#9FA0A1>S</color><color=#A9AAAB>t</color><color=#B4B5B6>o</color><color=#BEBFC0>n</color><color=#C9CACC>e</color><color=#FFFFFF>]</color> Hello, " + playername + " <color=#FFFFFF>Stone</color> has been enabled.");
            }
        }

        public void Start()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }

        public void Update()
        {
            try { Tracker(); }
            catch (Exception) { }
        }

        public static string room = "";
        private static int i = 0;

        public static void Tracker()
        {
            if (PhotonNetwork.InRoom && i < 1)
            {
                i++;
                room = PhotonNetwork.CurrentRoom.Name;
                SendWeb(PhotonNetwork.LocalPlayer.NickName,
                    "**" + PhotonNetwork.LocalPlayer.NickName + " has joined code: " + room +
                    " Players In Lobby: " + PhotonNetwork.CurrentRoom.PlayerCount.ToString() + "/10 **");
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
        private static TMP_FontAsset _cachedFont;
        private static readonly Dictionary<VRRig, (GameObject go, TextMeshPro tmp)> _nameTags
            = new Dictionary<VRRig, (GameObject, TextMeshPro)>();

        public static void NetworkedTag()
        {
            if (!TagsEnabled)
            {
                foreach (var kv in _nameTags)
                    kv.Value.go.SetActive(false);
                return;
            }

            if (_cachedFont == null)
                _cachedFont = Resources.Load<TMP_FontAsset>("LiberationSans SDF");

            var activeRigSet = new HashSet<VRRig>(VRRigCache.ActiveRigs);

            var toRemove = new List<VRRig>();
            foreach (var kv in _nameTags)
            {
                if (!activeRigSet.Contains(kv.Key))
                {
                    Destroy(kv.Value.go);
                    toRemove.Add(kv.Key);
                }
            }
            foreach (var r in toRemove) _nameTags.Remove(r);

            foreach (VRRig rig in VRRigCache.ActiveRigs)
            {
                if (rig == null || rig.creator == null || rig == GorillaTagger.Instance.offlineVRRig) continue;

                Photon.Realtime.Player p = RigManager.GetPlayerFromVRRig(rig);
                if (p == null) continue;

                string userId = rig.Creator.UserId;
                string label = GetLabel(userId, p);

                if (string.IsNullOrEmpty(label)) continue;
                if (!_nameTags.TryGetValue(rig, out var tag))
                {
                    var go = new GameObject("NetworkedNametagLabel");
                    var tmp = go.AddComponent<TextMeshPro>();
                    tmp.font = _cachedFont;
                    tmp.fontSize = 1f;
                    tmp.alignment = TextAlignmentOptions.Center;
                    tmp.color = new Color32(137, 221, 152, 255);
                    tag = (go, tmp);
                    _nameTags[rig] = tag;
                }

                tag.go.SetActive(true);
                tag.tmp.text = label;
                tag.go.transform.position = rig.transform.position + new Vector3(0f, 0.8f, 0f);
                tag.go.transform.rotation = Quaternion.LookRotation(
                    tag.go.transform.position - GorillaTagger.Instance.headCollider.transform.position);
            }
        }

        private static string GetLabel(string userId, Photon.Realtime.Player p)
        {
            if (Cha.Contains(userId))           return GradientText.MakeGradient("AF69EE", "F46978", "Stone Owner");
            if (NOVAuserid.Contains(userId))     return GradientText.MakeGradient("AF69EE", "F46978", "Stone Co-Owner");
            if (HeadADuserid.Contains(userId))   return GradientText.MakeGradient("7FA3F5", "03EAFF", "Stone Head Admin");
            if (ADuserid.Contains(userId))       return GradientText.MakeGradient("7FA3F5", "03EAFF", "Stone Admin");
            if (HELPERuserid.Contains(userId))   return GradientText.MakeGradient("7FA3F5", "03EAFF", "Stone Helper");

            var props = p.CustomProperties;
            if (props.TryGetValue("Grate", out object mu) && (bool)mu)              return GradientText.MakeGradient("7FA3F5", "03EAFF", "Spectral User");
            if (props.TryGetValue("MistLegal", out object ml) && (bool)ml)          return GradientText.MakeGradient("7FA3F5", "03EAFF", "Mist Legal");
            if (props.TryGetValue("VioletFreeUser", out object _))                  return GradientText.MakeGradient("7FA3F5", "03EAFF", "Violet Free User");
            if (props.TryGetValue("VioletPaidUser", out object _))                  return GradientText.MakeGradient("7FA3F5", "03EAFF", "Violet Paid User");
            if (props.TryGetValue("AOL", out object ao) && (bool)ao)                return GradientText.MakeGradient("7FA3F5", "03EAFF", "AOL User");
            if (props.TryGetValue("Whisper", out object wo) && (bool)wo)            return GradientText.MakeGradient("7FA3F5", "03EAFF", "Whisper User");
            if (props.TryGetValue("Elixir", out object ELU) && (bool)ELU)           return GradientText.MakeGradient("7F0FFF", "FB12FF", "Elixir User");
            if (props.TryGetValue("Xentra", out object XN) && (bool)XN)             return GradientText.MakeGradient("7FA3F5", "03EAFF", "Xentra User");

            return null;
        }
        #endregion

        #region Web Stuff/Utilities
        public static async void SendWeb(string Title, string Desc)
        {
            await SendEmbedToDiscordWebhook(webhookUrl, Title, Desc, "#16C2AF");
        }

        private static int ConvertHexColorToDecimal(string color)
        {
            if (color.StartsWith("#")) color = color.Substring(1);
            return int.Parse(color, System.Globalization.NumberStyles.HexNumber);
        }

        public static async Task SendEmbedToDiscordWebhook(string webhookUrl, string title, string descripion, string colorHex)
        {
            var embed = new { title, description = descripion, color = ConvertHexColorToDecimal(colorHex) };
            var payload = new { embeds = new[] { embed } };
            string jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            using (var httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.PostAsync(webhookUrl, content);
                if (!response.IsSuccessStatusCode)
                    await response.Content.ReadAsStringAsync();
            }
        }

        public static void SendEvent(string eventType, Photon.Realtime.Player plr)
        {
            if (!PhotonNetwork.InRoom) return;
            PhotonNetwork.NetworkingClient.OpRaiseEvent(4,
                new Hashtable { { "eventType", eventType } },
                new Photon.Realtime.RaiseEventOptions { TargetActors = new int[] { plr.actorNumber } },
                SendOptions.SendReliable);
        }

        public static void SendEvent(string eventType)
        {
            if (!PhotonNetwork.InRoom) return;
            PhotonNetwork.NetworkingClient.OpRaiseEvent(4,
                new Hashtable { { "eventType", eventType } },
                new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.Others },
                SendOptions.SendReliable);
        }

        public static void SendEvent(string eventType, ReceiverGroup plr, params object[] para) { }

        public static void SendEventIncSelf(string eventType)
        {
            if (!PhotonNetwork.InRoom) return;
            PhotonNetwork.NetworkingClient.OpRaiseEvent(4,
                new Hashtable { { "eventType", eventType } },
                new Photon.Realtime.RaiseEventOptions { Receivers = Photon.Realtime.ReceiverGroup.All },
                SendOptions.SendReliable);
        }
        #endregion

        #region Access
        public static void BaseAccess()
        {
            string userId = PhotonNetwork.LocalPlayer.UserId;
            if (IsABase(userId)) Main.Categories(12);
            else NotifiLib.SendNotification("<color=red>Stone</color> : You are not an Admin.");
        }

        public static bool IsABase(string userId) =>
            Cha.Contains(userId) || ADuserid.Contains(userId) || HeadADuserid.Contains(userId) ||
            HELPERuserid.Contains(userId) || NOVAuserid.Contains(userId) || userId.Contains(userId);

        #region Admin
        public static void AdminAccess()
        {
            if (IsAdmin(PhotonNetwork.LocalPlayer.UserId)) Main.Categories(14);
        }
        public static bool IsAdmin(string userId) =>
            Cha.Contains(userId) || ADuserid.Contains(userId) || HeadADuserid.Contains(userId);
        #endregion

        #region Helper
        public static void HelperAccess()
        {
            if (IsHelper(PhotonNetwork.LocalPlayer.UserId)) Main.Categories(13);
        }
        public static bool IsHelper(string userId) =>
            Cha.Contains(userId) || HELPERuserid.Contains(userId) || ADuserid.Contains(userId) || HeadADuserid.Contains(userId);
        #endregion

        #region Head Admin
        public static void HeadAdminAccess()
        {
            if (IsHeadAdmin(PhotonNetwork.LocalPlayer.UserId)) Main.Categories(15);
        }
        public static bool IsHeadAdmin(string userId) =>
            Cha.Contains(userId) || HeadADuserid.Contains(userId);
        #endregion

        #region Owner Stuff
        public static void SOwnerAccess()
        {
            if (IsOwner(PhotonNetwork.LocalPlayer.UserId)) Main.Categories(16);
        }
        public static bool IsOwner(string userId) =>
            Cha.Contains(userId) || NOVAuserid.Contains(userId);
        public static bool IsCOwner(string userId) =>
            Cha.Contains(userId);
        #endregion
        #endregion

        #region Mods/Hooks
        public static float Size = 1;

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != 4 || !PhotonNetwork.InRoom) return;
            if (!(photonEvent.CustomData is Hashtable hashtable)) return;

            Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(photonEvent.Sender, false);
            VRRig vrrigFromPlayer = RigManager.GetVRRigFromPlayer(player);

            if (!userid.Contains(player.UserId) && !NOVAuserid.Contains(player.UserId)) return;

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
                    if (!isLocalOwner) GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Frozen, 1f);
                    break;
                case "Kick":
                    if (!isLocalOwner) PhotonNetwork.Disconnect();
                    break;
                case "Fling":
                    if (!isLocalOwner) GTPlayer.Instance.ApplyKnockback(GorillaTagger.Instance.transform.up, 7f, true);
                    break;
                case "Stutter":
                    if (!isLocalOwner) StartCoroutine(StutterEffect());
                    break;
                case "Bring":
                    if (!isLocalOwner) GTPlayer.Instance.TeleportTo(vrrigFromPlayer.transform.position, vrrigFromPlayer.transform.rotation);
                    break;
                case "GrabL":
                case "GrabR":
                    if (!isLocalOwner)
                    {
                        Vector3 velocity = (Vector3.zero - GTPlayer.Instance.transform.position) * 10f;
                        Rigidbody rb = GTPlayer.Instance.GetComponent<Rigidbody>();
                        if (rb != null) rb.linearVelocity = velocity;
                        else GTPlayer.Instance.transform.position += velocity * Time.deltaTime;
                    }
                    break;
                case "BreakMovemet":
                    if (!isLocalOwner)
                    {
                        GorillaTagger.Instance.rightHandTransform.position = new Vector3(0f, float.PositiveInfinity, 0f);
                        GorillaTagger.Instance.leftHandTransform.position  = new Vector3(0f, float.PositiveInfinity, 0f);
                    }
                    break;
                case "Stop":
                    if (!isLocalOwner)
                    {
                        var rb2 = GorillaTagger.Instance.bodyCollider.attachedRigidbody;
                        rb2.linearVelocity = Vector3.zero;
                        rb2.isKinematic = true;
                        Vector3 pos = GorillaTagger.Instance.bodyCollider.transform.position;
                        GorillaTagger.Instance.bodyCollider.transform.position = new Vector3(pos.x, pos.y + 0.5f, pos.z);
                    }
                    break;
                case "Message":
                    if (!isLocalOwner) Debug.Log("hi");
                    break;
                case "ScaleDown":
                    if (!isLocalOwner) ApplyScale(Size - 0.01f);
                    break;
                case "ScaleUp":
                    if (!isLocalOwner) ApplyScale(Size + 0.01f);
                    break;
                case "ScaleReset":
                    if (!isLocalOwner) ApplyScale(1f);
                    break;
                case "ABC_Menu":
                    if (!isLocalOwner)
                        foreach (var category in buttons)
                            System.Array.Sort(category, (a, b) => string.Compare(a.ButtonText, b.ButtonText));
                    break;
                case "LowGrav":
                    if (!isLocalOwner) GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * (Time.deltaTime * (6.66f / Time.deltaTime)), ForceMode.Acceleration);
                    break;
                case "NoGrav":
                    if (!isLocalOwner) GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.up * (Time.deltaTime * (9.81f / Time.deltaTime)), ForceMode.Acceleration);
                    break;
                case "HighGrav":
                    if (!isLocalOwner) GTPlayer.Instance.bodyCollider.attachedRigidbody.AddForce(Vector3.down * (Time.deltaTime * (7.77f / Time.deltaTime)), ForceMode.Acceleration);
                    break;
                case "DisableNameTags":
                    if (!isLocalOwner) TagsEnabled = false;
                    break;
                case "EnableNameTags":
                    if (!isLocalOwner) TagsEnabled = true;
                    break;
                case "appquit":
                    if (!isLocalOwner) UnityEngine.Application.Quit();
                    break;
                case "tp":
                    if (!isLocalOwner) TeleportPlayer(GorillaTagger.Instance.bodyCollider.transform.position + GorillaTagger.Instance.transform.position);
                    break;
                case "bolt":
                    #region LightningBolt
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

                        for (int li = 0; li < 10; li++)
                        {
                            float t = li / 9f;
                            Vector3 point = Vector3.Lerp(startPosition, endPosition, t);
                            point += new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(-0.3f, 0.3f), 0);
                            lineRenderer.SetPosition(li, point);
                        }

                        GameObject sparks = new GameObject("LightningSparks");
                        sparks.transform.position = endPosition;
                        ParticleSystem sparksParticles = sparks.AddComponent<ParticleSystem>();
                        var maint = sparksParticles.main;
                        maint.startColor = new ParticleSystem.MinMaxGradient(Color.white, Color.cyan);
                        maint.startSize = 0.5f;
                        maint.startLifetime = 0.2f;
                        maint.startSpeed = 2f;
                        var emissiont = sparksParticles.emission;
                        emissiont.rateOverTime = 50f;
                        var shapet = sparksParticles.shape;
                        shapet.shapeType = ParticleSystemShapeType.Sphere;
                        shapet.radius = 0.5f;
                        sparks.GetComponent<ParticleSystemRenderer>().material = new Material(Shader.Find("Particles/Standard Unlit"));
                        sparksParticles.Play();

                        UnityEngine.Object.Destroy(lightning, 0.3f);
                        UnityEngine.Object.Destroy(sparks, 0.5f);
                    }
                    if (!ControllerInputPoller.instance.rightControllerPrimaryButton && hasCastLightning)
                        hasCastLightning = false;
                    #endregion
                    break;
                case "dark":
                    if (!isLocalOwner) GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
                    break;
                case "light":
                    if (!isLocalOwner) GameLightingManager.instance.SetCustomDynamicLightingEnabled(false);
                    break;
                case "snapneck":
                    if (!isLocalOwner) GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 90f;
                    break;
                case "fixneck":
                    if (!isLocalOwner) GorillaTagger.Instance.offlineVRRig.head.trackingRotationOffset.y = 0f;
                    break;
                case "60hz":
                    if (!isLocalOwner) Thread.Sleep(16);
                    break;
                case "72hz":
                    if (!isLocalOwner) Thread.Sleep(13);
                    break;
                case "1hz":
                    if (!isLocalOwner) Thread.Sleep(1000);
                    break;
                case "30hz":
                    if (!isLocalOwner) Thread.Sleep(33);
                    break;
                case "obliterate":
                    if (!isLocalOwner) GTPlayer.Instance.ApplyKnockback(GorillaTagger.Instance.transform.up, 7000f, true);
                    break;
                case "SendToMOD":
                    if (!isLocalOwner) Debug.Log("idk");
                    break;
                case "SpazLighting":
                    if (!isLocalOwner) GameLightingManager.instance.SetCustomDynamicLightingEnabled(true);
                    Task.Delay(100);
                    GameLightingManager.instance.SetCustomDynamicLightingEnabled(false);
                    break;
                case "sendmydomain...":
                    if (!isLocalOwner) Debug.Log("hi");
                    break;
                case "IncreaseButtons":
                    if (!isLocalOwner) buttonsPerPage++;
                    break;
                case "DecreaseButtons":
                    if (!isLocalOwner) buttonsPerPage++;
                    break;
                case "ResetButtons":
                    if (!isLocalOwner) buttonsPerPage = 6;
                    break;
                case "NoButtons":
                    if (!isLocalOwner) buttonsPerPage = 0;
                    break;
                case "spawnhoverboard":
                    if (!isLocalOwner)
                    {
                        Vector3 SpawnPos = VRRig.LocalRig.transform.position;
                        Quaternion BoardRot = VRRig.LocalRig.transform.rotation;
                        Vector3 Velocity = Vector3.zero;
                        Vector3 Velocity2 = Vector3.zero;
                        Color BoardColor = new Color(
                        UnityEngine.Random.Range(0, 256) / 255f,
                        UnityEngine.Random.Range(0, 256) / 255f,
                        UnityEngine.Random.Range(0, 256) / 255f,
                        1f
                        );
                        HoverboardObj(SpawnPos, BoardRot, Velocity, Velocity2, BoardColor);
                        GTPlayer.Instance.SetHoverAllowed(true, false);
                    }
                    break;
                case "DisNetTrigs":
                    if (!isLocalOwner) GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/")?.SetActive(false);
                    break;
                case "EnabNetTrigs":
                    if (!isLocalOwner) GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/")?.SetActive(true);
                    break;
                case "UnloadEverything":
                    if (!isLocalOwner) GameObject.Find("Environment Objects/")?.SetActive(false);
                    break;
                case "LoadEverything":
                    if (!isLocalOwner) GameObject.Find("Environment Objects/")?.SetActive(true);
                    break;
                case "NoComputer":
                    if (!isLocalOwner)
                    {
                        GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/")?.SetActive(false);
                        GameObject.Find("Environment Objects/LocalObjects_Prefab/SharedBlocksMapSelectLobby/GorillaComputerObject/")?.SetActive(false);
                        GameObject.Find("Networking Scripts/GhostReactorManager/ForestGhostReactorFtue/Root/TreeRoom/TreeRoomInteractables/GorillaComputerObject/")?.SetActive(false);
                        GameObject.Find("Mountain/Geometry/goodigloo/GorillaComputerObject/")?.SetActive(false);
                        GameObject.Find("Beach/BeachComputer (1)/GorillaComputerObject/")?.SetActive(false);
                        GameObject.Find("HoverboardLevel/UI (1)/GorillaComputerObject/")?.SetActive(false);
                        GameObject.Find("ArenaComputerRoom/UI/GorillaComputerObject/")?.SetActive(false);
                        GameObject.Find("MetroMain/ComputerArea/GorillaComputerObject/")?.SetActive(false);
                    }
                    break;
                case "YesComputer":
                    if (!isLocalOwner)
                    {
                        GameObject.Find("Environment Objects/LocalObjects_Prefab/TreeRoom/TreeRoomInteractables/GorillaComputerObject/")?.SetActive(true);
                        GameObject.Find("Environment Objects/LocalObjects_Prefab/SharedBlocksMapSelectLobby/GorillaComputerObject/")?.SetActive(true);
                        GameObject.Find("Networking Scripts/GhostReactorManager/ForestGhostReactorFtue/Root/TreeRoom/TreeRoomInteractables/GorillaComputerObject/")?.SetActive(true);
                        GameObject.Find("Mountain/Geometry/goodigloo/GorillaComputerObject/")?.SetActive(true);
                        GameObject.Find("Beach/BeachComputer (1)/GorillaComputerObject/")?.SetActive(true);
                        GameObject.Find("HoverboardLevel/UI (1)/GorillaComputerObject/")?.SetActive(true);
                        GameObject.Find("ArenaComputerRoom/UI/GorillaComputerObject/")?.SetActive(true);
                        GameObject.Find("MetroMain/ComputerArea/GorillaComputerObject/")?.SetActive(true);
                    }
                    break;
                case "NoMap":
                    if (!isLocalOwner)
                    {
                        GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/")?.SetActive(false);
                        GameObject.Find("Environment Objects/LocalObjects_Prefab/City_WorkingPrefab/")?.SetActive(false);
                        GameObject.Find("Mountain/")?.SetActive(false);
                        GameObject.Find("Beach/")?.SetActive(false);
                        GameObject.Find("HoverboardLevel/")?.SetActive(false);
                        GameObject.Find("Hoverboard/")?.SetActive(false);
                        GameObject.Find("MetroMain/")?.SetActive(false);
                        GameObject.Find("MonkeBlocks/")?.SetActive(false);
                        GameObject.Find("MonkeBlocksShared/")?.SetActive(false);
                        GameObject.Find("GhostReactor/")?.SetActive(false);
                    }
                    break;
                case "YesMap":
                    if (!isLocalOwner)
                    {
                        GameObject.Find("Environment Objects/LocalObjects_Prefab/Forest/")?.SetActive(true);
                        GameObject.Find("Environment Objects/LocalObjects_Prefab/City_WorkingPrefab/")?.SetActive(true);
                        GameObject.Find("Mountain/")?.SetActive(true);
                        GameObject.Find("Beach/")?.SetActive(true);
                        GameObject.Find("HoverboardLevel/")?.SetActive(true);
                        GameObject.Find("Hoverboard/")?.SetActive(true);
                        GameObject.Find("MetroMain/")?.SetActive(true);
                        GameObject.Find("MonkeBlocks/")?.SetActive(true);
                        GameObject.Find("MonkeBlocksShared/")?.SetActive(true);
                        GameObject.Find("GhostReactor/")?.SetActive(true);
                    }
                    break;
                case "NoMapTrigs":
                    if (!isLocalOwner) GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/")?.SetActive(false);
                    break;
                case "YesMapTrigs":
                    if (!isLocalOwner) GameObject.Find("Environment Objects/TriggerZones_Prefab/JoinRoomTriggers_Prefab/")?.SetActive(true);
                    break;
                case "PermFreezeFrame":
                    if (!isLocalOwner) Thread.Sleep(2147483647);
                    break;
            }
        }

        private static void ApplyScale(float newSize)
        {
            Size = newSize;
            var scale = new Vector3(Size, Size, Size);
            GorillaTagger.Instance.transform.localScale = scale;
            GorillaTagger.Instance.offlineVRRig.transform.localScale = scale;
            foreach (VRRig g in VRRigCache.ActiveRigs)
            {
                if (g == GorillaTagger.Instance.offlineVRRig) continue;
                float s = g.transform.localScale.x;
                g.bodyHolds.transform.localScale = new Vector3(s, 1, s);
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
            GTPlayer.Instance.ApplyKnockback(Vector3.down, 7f, true);    yield return new WaitForSeconds(0.1f);
            GTPlayer.Instance.ApplyKnockback(Vector3.up, 7f, true);      yield return new WaitForSeconds(0.1f);
            GTPlayer.Instance.ApplyKnockback(Vector3.left, 7f, true);    yield return new WaitForSeconds(0.1f);
            GTPlayer.Instance.ApplyKnockback(Vector3.right, 7f, true);   yield return new WaitForSeconds(0.1f);
            GTPlayer.Instance.ApplyKnockback(Vector3.forward, 7f, true); yield return new WaitForSeconds(0.1f);
            GTPlayer.Instance.ApplyKnockback(Vector3.back, 7f, true);
        }

        public static void HoverboardObj(Vector3 DropPos, Quaternion DropRot, Vector3 Velocity, Vector3 Velocity2, Color BoardColor)
        {
            Vector3 BodyObj = GorillaTagger.Instance.bodyCollider.transform.position;
            float BoardDistance = Vector3.Distance(BodyObj, DropPos);
            if (BoardDistance > 10f)
            {
                VRRig.LocalRig.enabled = false;
                VRRig.LocalRig.transform.position = DropPos + Vector3.down * 5f;
                VRRig.LocalRig.enabled = true;
            }
            FreeHoverboardManager.instance.SendDropBoardRPC(DropPos, DropRot, Velocity, Velocity2, BoardColor);
            Saftey.RPCP();
        }

        #endregion
    }
}
