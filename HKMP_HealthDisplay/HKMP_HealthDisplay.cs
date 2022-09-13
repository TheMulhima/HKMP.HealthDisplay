using System.Linq;
using Hkmp.Api.Client;
using HkmpPouch;
using JetBrains.Annotations;

namespace HKMP_HealthDisplay;

[UsedImplicitly]
public class HKMP_HealthDisplay:Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
{
    internal static readonly Dictionary<IClientPlayer, HealthBarController> HealthBarComponentCache = new ();
    
    private static HkmpPipe SendPipe;
    private static HkmpPipe RequestPipe;

    //i hope no one names themselves like this 
    private const string Data_Seperator = "&*&*&()(())";

    public static HKMP_HealthDisplay Instance;
    
    public GameObjectFollowingLayout gameObjectFollowingLayout;
    public LayoutRoot layout;
    public static GlobalSettings settings { get; private set; } = new ();
    public void OnLoadGlobal(GlobalSettings s) => settings = s;
    public GlobalSettings OnSaveGlobal() => settings;

    public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();

    public override void Initialize()
    {
        Instance ??= this;
        
        layout = new LayoutRoot(true, "HKMP_HealthDisplay MaskUI")
        {
            VisibilityCondition = () => HeroController.instance != null && !HeroController.instance.cState.transitioning,
            RenderDebugLayoutBounds = false
        };
            
        gameObjectFollowingLayout = new GameObjectFollowingLayout(layout, "HKMP Players Follower");

        //create 2 pipes for sending and requesting health
        SendPipe = new HkmpPipe("HealthDisplaySendPipe", false);
        RequestPipe = new HkmpPipe("HealthDisplayRequestPipe", false);
        
        SendPipe.OnRecieve += OnSendPipeReceive;
        RequestPipe.OnRecieve += OnRequestReceive;

        //it fails in init idk
        On.HeroController.Start += AddPipeEvents;
        
        //request for missing data
        ModHooks.HeroUpdateHook += RequestForData;
        
        //update the UI to follow the players
        ModHooks.HeroUpdateHook += UpdateUI;
        
        //send update when health changes
        ModHooks.SetPlayerIntHook += SendUpdateWhenPDChange;
        
        //delete healthbars when going to new scene
        ModHooks.BeforeSceneLoadHook += DeleteHealthBars;

    }

    private void RequestForData()
    {
        if (Client.Instance == null) return;
        if (!Client.Instance.clientApi.NetClient.IsConnected) return;

        foreach (var player in Client.Instance.clientApi.ClientManager.Players)
        {
            if (player is { IsInLocalScene: true })
            {
                if (!HealthBarComponentCache.ContainsKey(player))
                {
                    Log($"requesting from player {player.Id}");
                    RequestUpdateFromPlayer(player);
                }
            }
        }
    }

    private void AddPipeEvents(On.HeroController.orig_Start orig, HeroController self)
    {
        Client.Instance.clientApi.ClientManager.PlayerEnterSceneEvent += RequestUpdateFromPlayer;
        Client.Instance.clientApi.ClientManager.PlayerConnectEvent += RequestUpdateFromPlayer;
        Client.Instance.clientApi.ClientManager.PlayerDisconnectEvent += RemovePlayerFromList;

        //unhook this. we only want it running once
        On.HeroController.Start -= AddPipeEvents;
        
        orig(self);
    }

    private void OnSendPipeReceive(object _, RecievedEventArgs R)
    {
        var data = R.packet.eventData.Split(new[] { Data_Seperator }, StringSplitOptions.None);
        
        var senderUsername = data[0];
        var senderHealth = data[1];
     
        if(Client.Instance == null) return;
        if (!Client.Instance.clientApi.NetClient.IsConnected) return;
        
        //we are using username in this case because local player only has access to other players id and not his own
        var player = Client.Instance.clientApi.ClientManager.Players.FirstOrDefault(player => player.Username == senderUsername);

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
            
            htop.Host = player.PlayerContainer;
            htop.UpdateText(int.Parse(senderHealth));
        }
    }
    
    private void OnRequestReceive(object _, RecievedEventArgs R)
    {
        if (Client.Instance == null) return;
        if (!Client.Instance.clientApi.NetClient.IsConnected) return;

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
        if (new [] {nameof(PlayerData.health), nameof(PlayerData.healthBlue)}.Contains(name))
        {
            SendUpdateToAll(name, orig);
        }

        return orig;
    }
    
    private static string GetEventData(string UpdatedPDName, int newPDValue)
    {
        //we are sometimes getting this data before the pd is set, we have the new value so we should use it
        var health = UpdatedPDName == nameof(PlayerData.health) ? newPDValue : PlayerData.instance.health;
        var healthBlue = UpdatedPDName == nameof(PlayerData.healthBlue) ? newPDValue : PlayerData.instance.healthBlue;
        
        return $"{Client.Instance.clientApi.ClientManager.Username}{Data_Seperator}{health + healthBlue}";
    }

    private static string previousData = string.Empty;
    private void SendUpdateToAll(bool bypass = false) => SendUpdateToAll(string.Empty, 0, bypass);
    private void SendUpdateToAll(string name, int orig, bool bypass = false)
    {
        if (Client.Instance == null) return;
        if (!Client.Instance.clientApi.NetClient.IsConnected) return;
        
        var data = GetEventData(name, orig);

        if (!bypass)
        {
            if (previousData == data)
            {
                return;
            }
        }

        previousData = data;

        SendPipe.SendToAll(0, "normal send", data, true, true);
    }
    
    private void RequestUpdateFromPlayer(IClientPlayer player)
    {
        RequestPipe.Send(0, player.Id, "request", "");
    }
    
    //fail safe if some health bar gets left on screen
    private string DeleteHealthBars(string arg)
    {
        for (int i = 0; i < gameObjectFollowingLayout.Children.Count; i++)
        {
            gameObjectFollowingLayout.Children[i].Destroy();
        }
        gameObjectFollowingLayout.Children.Clear();
        return arg;
    }

    private void UpdateUI()
    {
        gameObjectFollowingLayout.InvalidateArrange();
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
            htop.Host = player.PlayerContainer;
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