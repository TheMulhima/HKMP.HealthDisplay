using System.Linq;
using Hkmp.Api.Client;
using HkmpPouch;
using JetBrains.Annotations;

namespace HKMP_HealthDisplay;

[UsedImplicitly]
public class HKMP_HealthDisplay:Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
{
    internal static readonly Dictionary<IClientPlayer, HealthBarController> HealthBarComponentCache = new ();
    
    private static HkmpPipe HkmpPipe;

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

        //ModHooks.HeroUpdateHook += BroadcastNewHealth;
        ModHooks.HeroUpdateHook += UpdateUI;
        ModHooks.BeforeSceneLoadHook += DeleteHealthBars;

        HkmpPipe = new HkmpPipe("HealthDisplay", false);

        HkmpPipe.OnRecieve += (_, R) =>
        {
            var player =  Client.Instance.clientApi.ClientManager.GetPlayer(R.packet.fromPlayer);

            if (player != null)
            {
                Log($"Recieved Update from {player.Id} {player.Username} {player.IsInLocalScene}");
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
                    htop.UpdateText( Int32.Parse(data[0]), Int32.Parse(data[1]));
                }
                else
                {
                    htop = AddPlayerToCache(player);
                    htop.Host = player.PlayerContainer;
                    string[] data = R.packet.eventData.Split('&');
                    htop.UpdateText( Int32.Parse(data[0]), Int32.Parse(data[1]));
                }
            }
        };
        
        Client.Instance.clientApi.ClientManager.PlayerDisconnectEvent += (player) =>
        {
            if (HealthBarComponentCache.TryGetValue(player, out var healthbar))
            {
                UnityEngine.Object.Destroy(healthbar);
                HealthBarComponentCache.Remove(player);
            }
        };

        ModHooks.SetPlayerIntHook += (name, orig) =>
        {
            if (name == nameof(PlayerData.health))
            {
                SendUpdateToAll();
            }

            return orig;
        };

    }

    private static string GetEventData() =>
        $"{PlayerData.instance.health + PlayerData.instance.healthBlue}&{PlayerData.instance.MPCharge + PlayerData.instance.MPReserve}";

    private static ushort GetID() => Client.Instance.clientApi.ClientManager.Players
        .First(player => player.Username == Client.Instance.clientApi.ClientManager.Username).Id;

    private void SendUpdateToAll()
    {
        Log("Sending update to all players in same scene");
        Log($"{GetID()} {GetEventData()}");
        HkmpPipe.SendToAll(GetID(), "normal send", GetEventData(), true, true);
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