﻿using System.Linq;
using Hkmp.Api.Client;
using HkmpPouch;
using JetBrains.Annotations;
using HKMirror;
using HKMirror.Hooks.OnHooks;
using HkmpPouch;
using HkmpPouch.Networking;
using HkmpPouch.Packets;

namespace HKMP_HealthDisplay;

[UsedImplicitly]
public class HKMP_HealthDisplay:Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
{
    internal static readonly Dictionary<IClientPlayer, HealthBarController> HealthBarComponentCache = new ();
    
    internal static PipeClient SendPipe;
    private static PipeClient RequestPipe;

    //i hope no one names themselves like this 
    private const string Data_Seperator = "&*&*&()(())";

    public static HKMP_HealthDisplay Instance;
    public static GlobalSettings settings { get; private set; } = new ();
    public void OnLoadGlobal(GlobalSettings s) => settings = s;
    public GlobalSettings OnSaveGlobal() => settings;

    public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();

    public override void Initialize()
    {
        Instance ??= this;

        //create 2 pipes for sending and requesting health
        SendPipe = new PipeClient("HealthDisplaySendPipe");
        RequestPipe = new PipeClient("HealthDisplayRequestPipe");
        
        SendPipe.OnRecieve += OnSendPipeReceive;
        RequestPipe.OnRecieve += OnRequestReceive;

        //it fails in init idk
        OnHeroController.BeforeOrig.Start += AddPipeEvents;

        //request for missing data
        ModHooks.HeroUpdateHook += RequestForData;

        #if DEBUG
        
        ModHooks.HeroUpdateHook += () =>
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                var c = HeroController.instance.gameObject.GetAddComponent<HealthBarController>();
                c.UpdateText(PlayerDataAccess.health, PlayerDataAccess.maxHealth, PlayerDataAccess.healthBlue);
            }
        };
        
        #endif
        
        //send update when health changes
        ModHooks.SetPlayerIntHook += SendUpdateWhenPDChange;

    }

    private void RequestForData()
    {
        if (SendPipe?.ClientApi == null) return;
        if (!SendPipe.ClientApi.NetClient.IsConnected) return;

        foreach (var player in SendPipe.ClientApi.ClientManager.Players)
        {
            if (player is { IsInLocalScene: true })
            {
                if (!HealthBarComponentCache.ContainsKey(player))
                {
                    timer += Time.deltaTime;
                    if (timer > 0.3f)
                    {
                        timer = 0f;
                        RequestUpdateFromPlayer(player);
                    }
                }
                else
                {
                    //remove the timer value because we got the data requested
                    timer = 0f;
                }
            }
        }
    }

    private float timer = 0f;

    private void AddPipeEvents(OnHeroController.Delegates.Params_Start args)
    {
        SendPipe.ClientApi.ClientManager.PlayerEnterSceneEvent += RequestUpdateFromPlayer;
        SendPipe.ClientApi.ClientManager.PlayerConnectEvent += RequestUpdateFromPlayer;
        SendPipe.ClientApi.ClientManager.PlayerDisconnectEvent += RemovePlayerFromList;

        //unhook this. we only want it running once
        OnHeroController.BeforeOrig.Start -= AddPipeEvents;
    }

    private void OnSendPipeReceive(object sender, ReceivedEventArgs R)
    {
        var data = R.Data.EventData.Split(new[] { Data_Seperator }, StringSplitOptions.None);
        
        var senderUsername = data[0];
        var senderHealthMain = data[1];
        var senderHealthMax = data[2];
        var senderHealthBlue = data[3];
     
        if (SendPipe?.ClientApi == null) return;
        if (!SendPipe.ClientApi.NetClient.IsConnected) return;
        
        //we are using username in this case because local player only has access to other players id and not his own
        var player = SendPipe.ClientApi.ClientManager.Players.FirstOrDefault(player => player.Username == senderUsername);

        if (player is { IsInLocalScene: true })
        {
            //check if a healthbar is already there
            if (HealthBarComponentCache.TryGetValue(player, out var htop) && htop == null)
            {
                htop = getAddHealthOnTopOfPlayer(player);
            }
            else
            {
                htop = AddPlayerToCache(player);
            }
            
            htop.UpdateText(int.Parse(senderHealthMain), int.Parse(senderHealthMax), int.Parse(senderHealthBlue));
        }
    }
    
    private void OnRequestReceive(object _, ReceivedEventArgs R)
    {
        if (SendPipe?.ClientApi == null) return;
        if (!SendPipe.ClientApi.NetClient.IsConnected) return;

        SendUpdateToAll(bypass:true);
    }

    private static void RemovePlayerFromList(IClientPlayer player)
    {
        if (HealthBarComponentCache.TryGetValue(player, out var healthBar))
        {
            UnityEngine.Object.Destroy(healthBar);
            HealthBarComponentCache.Remove(player);
        }
    }

    private int SendUpdateWhenPDChange(string name, int orig)
    {
        if (new [] {nameof(PlayerDataAccess.health), nameof(PlayerDataAccess.healthBlue), nameof(PlayerDataAccess.maxHealth)}.Contains(name))
        {
            SendUpdateToAll(name, orig);
        }

        return orig;
    }
    
    private static string GetEventData(string UpdatedPDName, int newPDValue)
    {
        //we are sometimes getting this data before the pd is set, we have the new value so we should use it
        var health = UpdatedPDName == nameof(PlayerDataAccess.health) ? newPDValue : PlayerDataAccess.health;
        var healthBlue = UpdatedPDName == nameof(PlayerDataAccess.healthBlue) ? newPDValue : PlayerDataAccess.healthBlue;
        var healthMax = UpdatedPDName == nameof(PlayerDataAccess.maxHealth) ? newPDValue : PlayerDataAccess.maxHealth;
        
        return $"{SendPipe.ClientApi.ClientManager.Username}{Data_Seperator}{health}{Data_Seperator}{healthMax}{Data_Seperator}{healthBlue}";
    }

    private static string previousData = string.Empty;
    private void SendUpdateToAll(bool bypass = false) => SendUpdateToAll(string.Empty, 0, bypass);
    private void SendUpdateToAll(string name, int orig, bool bypass = false)
    {
        if (SendPipe?.ClientApi == null) return;
        if (!SendPipe.ClientApi.NetClient.IsConnected) return;
        
        var data = GetEventData(name, orig);

        if (!bypass)
        {
            if (previousData == data)
            {
                return;
            }
        }

        previousData = data;

        SendPipe.Broadcast("normal send", data, true, true);
    }
    
    public void RequestUpdateFromPlayer(IClientPlayer player)
    {
        RequestPipe.SendToPlayer(player.Id, "request", "");
    }

    private HealthBarController AddPlayerToCache(IClientPlayer player)
    {
        HealthBarController htop = getAddHealthOnTopOfPlayer(player);
        
        HealthBarComponentCache[player] = htop;
        return htop;
    }

    private HealthBarController getAddHealthOnTopOfPlayer(IClientPlayer player)
    {
        HealthBarController htop;
        if (player.PlayerContainer.GetComponent<HealthBarController>() == null)
        {
            htop = player.PlayerContainer.AddComponent<HealthBarController>();
        }
        else
        {
            htop = player.PlayerContainer.GetComponent<HealthBarController>();
        }

        return htop;
    }

    public bool ToggleButtonInsideMenu => false;

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.GetMenuScreen(modListMenu, toggleDelegates);
}