using System.Collections;
using Hkmp.Api.Client.Networking;

namespace HKMP_HealthDisplay;

public class HKMP_HealthDisplay:Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
{
    private HealthDisplayClient clientAddon = new HealthDisplayClient();
    private HealthDisplayServer serverAddon = new HealthDisplayServer();
    private Menu menuRef;

    public static HKMP_HealthDisplay Instance;
    
    public GameObjectFollowingLayout gameObjectFollowingLayout;
    public LayoutRoot layout;
    public static GlobalSettings settings { get; set; } = new ();
    public void OnLoadGlobal(GlobalSettings s) => settings = s;
    public GlobalSettings OnSaveGlobal() => settings;

    public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();

    public override void Initialize()
    {
        Instance ??= this;
        ClientAddon.RegisterAddon(clientAddon);
        ServerAddon.RegisterAddon(serverAddon);


        layout = new LayoutRoot(true, "HKMP_HealthDisplay MaskUI")
        {
            VisibilityCondition = () => HeroController.instance != null && !HeroController.instance.cState.transitioning,
            RenderDebugLayoutBounds = false
        };
            
        gameObjectFollowingLayout = new GameObjectFollowingLayout(layout, "HKMP Players Follower");

        //ModHooks.HeroUpdateHook += BroadcastNewHealth;
        ModHooks.HeroUpdateHook += UpdateUI;
        ModHooks.BeforeSceneLoadHook += DeleteHealthBars;
        
    }
    private string DeleteHealthBars(string arg)
    {
        //fail safe if some health bar gets left on screen
        gameObjectFollowingLayout.Children.Clear();
        return arg;
    }

    /*private void BroadcastNewHealth()
    {
        if (clientAddon.clientApi is { NetClient.IsConnected: true })
        {
            clientAddon.SendUpdate(PlayerData.instance.health + PlayerData.instance.healthBlue,
                PlayerData.instance.MPCharge + PlayerData.instance.MPReserve);
        }
    }*/

    private void UpdateUI()
    {
        gameObjectFollowingLayout.InvalidateArrange();
    }


    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        menuRef ??= new Menu("HKMP_HealthDisplay", new Element[]
        {
            new HorizontalOption("Health Display Type",
                "Choose how health will be displayed. Note: Scene change is required to not cause overlaps", 
                Enum.GetNames(typeof(HealthDisplayType)),
                (i) =>
                {
                    settings._healthDisplayType = (HealthDisplayType)i;
                    foreach (var (_, component) in HealthDisplayClient.HealthBarComponentCache)
                    {
                        //destory all health bars and let the component deal with its consequences
                        if (component == null) continue;
                        component.HealthBarUI?.Destroy();
                        component.ClearAllTextUI();
                    }
                },
                () => (int)settings._healthDisplayType),
            new TextPanel(""),
            new TextPanel("This mod was made by Mulhima", fontSize: 50),
            new TextPanel("with help and support from:", fontSize: 50),
            new TextPanel("BadMagic (Health Bar UI)", fontSize: 50),
            new TextPanel("Extremelyd1 (HKMP API)", fontSize: 50),
            new TextPanel("Dandy (help with HKMP API)", fontSize: 50),
        });
        
        return menuRef.GetMenuScreen(modListMenu);
    }

    public bool ToggleButtonInsideMenu { get; }
}