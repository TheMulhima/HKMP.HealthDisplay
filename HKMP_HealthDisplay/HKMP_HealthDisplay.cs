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

    public static HKMP_HealthDisplay Instance;
    
    public GameObjectFollowingLayout gameObjectFollowingLayout;
    public LayoutRoot layout;
    public static GlobalSettings settings { get; private set; } = new ();
    public void OnLoadGlobal(GlobalSettings s) => settings = s;
    public GlobalSettings OnSaveGlobal() => settings;

    public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();
    
    private static readonly List<string> InterestedPDInts = new()
    {
        nameof(PlayerData.health),
        nameof(PlayerData.healthBlue),
        nameof(PlayerData.MPCharge),
        nameof(PlayerData.MPReserve)
    };

    public override void Initialize()
    {
        Instance ??= this;
        
        layout = new LayoutRoot(true, "HKMP_HealthDisplay MaskUI")
        {
            VisibilityCondition = () => HeroController.instance != null && !HeroController.instance.cState.transitioning,
            RenderDebugLayoutBounds = false
        };
            
        gameObjectFollowingLayout = new GameObjectFollowingLayout(layout, "HKMP Players Follower");

        SendPipe = new HkmpPipe("HealthDisplaySendPipe", false);
        RequestPipe = new HkmpPipe("HealthDisplayRequestPipe", false);
        
        SendPipe.OnRecieve += OnSendPipeReceive;
        RequestPipe.OnRecieve += OnRequestReceive;

        On.HeroController.Start += AddPipeEvents;
        
        ModHooks.HeroUpdateHook += UpdateUI;
        ModHooks.BeforeSceneLoadHook += DeleteHealthBars;
        ModHooks.SetPlayerIntHook += SendUpdateWhenPDChange;
    }

    private void AddPipeEvents(On.HeroController.orig_Start orig, HeroController self)
    {
        Client.Instance.clientApi.ClientManager.PlayerEnterSceneEvent += RequestUpdateFromPlayer;
        Client.Instance.clientApi.ClientManager.PlayerDisconnectEvent += RemovePlayerFromList;

        On.HeroController.Start -= AddPipeEvents;
        
        orig(self);
    }

    private void OnSendPipeReceive(object _, RecievedEventArgs R)
    {
        var player = Client.Instance.clientApi.ClientManager.GetPlayer(R.packet.fromPlayer);

        if (player != null)
        {
            Log($"Received Update from {player.Id} {player.Username} {player.IsInLocalScene}");
        }
        else
        {
            Log($"{R.packet.fromPlayer} id not found");
        }

        if (player is { IsInLocalScene: true })
        {
            if (HealthBarComponentCache.TryGetValue(player, out var htop))
            {
                if (htop == null)
                {
                    htop = getAddHealthOnTopOfPlayer(player);
                }

                htop.Host = player.PlayerContainer;
                string[] data = R.packet.eventData.Split('&');
                htop.UpdateText(Int32.Parse(data[0]), Int32.Parse(data[1]));
            }
            else
            {
                htop = AddPlayerToCache(player);
                htop.Host = player.PlayerContainer;
                string[] data = R.packet.eventData.Split('&');
                htop.UpdateText(Int32.Parse(data[0]), Int32.Parse(data[1]));
            }
        }
    }
    
    private void OnRequestReceive(object _, RecievedEventArgs R)
    {
        if (Client.Instance == null) return;
        
        var player = Client.Instance.clientApi.ClientManager.GetPlayer(R.packet.fromPlayer);

        if (player != null)
        {
            Log($"Received Request from {player.Id} {player.Username} {player.IsInLocalScene}");
        }
        else
        {
            Log($"{R.packet.fromPlayer} id not found");
        }

        SendUpdateToAll();
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
        if (InterestedPDInts.Contains(name))
        {
            SendUpdateToAll();
        }

        return orig;
    }


    private static string GetHealthAndSoulData() =>
        $"{PlayerData.instance.health + PlayerData.instance.healthBlue}&{PlayerData.instance.MPCharge + PlayerData.instance.MPReserve}";

    private static ushort? GetCurrentPlayerID()
    {
        var player = Client.Instance.clientApi.ClientManager.Players
            .FirstOrDefault(player => player.Username == Client.Instance.clientApi.ClientManager.Username);

        if (player == null) return null;
        else return player.Id;
    }

    private void SendUpdateToAll()
    {
        if (Client.Instance == null) return;
        
        Log("Sending update to all players in same scene");

        var id = GetCurrentPlayerID();
        
        if (id == null) return;

        Log($"{id.Value} {GetHealthAndSoulData()}");
        SendPipe.SendToAll(id.Value, "normal send", GetHealthAndSoulData(), true, true);
    }

    private void RequestUpdateFromPlayer(IClientPlayer player)
    {
        var id = GetCurrentPlayerID();
        
        if (id == null) return;
        
        RequestPipe.Send(id.Value, player.Id, "request", "");
    }
    
    //fail safe if some health bar gets left on screen
    private string DeleteHealthBars(string arg)
    {
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

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        return ModMenu.GetMenuScreen(modListMenu, toggleDelegates);
    }
}