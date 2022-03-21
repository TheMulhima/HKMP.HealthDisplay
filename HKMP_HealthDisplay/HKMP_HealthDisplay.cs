using GlobalEnums;

namespace HKMP_HealthDisplay;

public class HKMP_HealthDisplay:Mod, IGlobalSettings<GlobalSettings>, ICustomMenuMod
{
    private HealthDisplayClient _clientAddon = new HealthDisplayClient();
    private HealthDisplayServer _serverAddon = new HealthDisplayServer();
    private Menu MenuRef;

    public static HKMP_HealthDisplay Instance;
    
    public GameObjectFollowingLayout Gofl;
    public LayoutRoot Layout;
    public static GlobalSettings settings { get; set; } = new ();
    public void OnLoadGlobal(GlobalSettings s) => settings = s;
    public GlobalSettings OnSaveGlobal() => settings;

    public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();

    public override void Initialize()
    {
        Instance ??= this;
        ClientAddon.RegisterAddon(_clientAddon);
        ServerAddon.RegisterAddon(_serverAddon);


        Layout = new LayoutRoot(true, "RandomStuff")
        {
            VisibilityCondition = () => HeroController.instance != null && !HeroController.instance.cState.transitioning,
            RenderDebugLayoutBounds = false
        };
            
        Gofl = new GameObjectFollowingLayout(Layout, "Some Layout");
        
        ModHooks.HeroUpdateHook += () =>
        {
            if (_clientAddon._clientApi is { NetClient.IsConnected: true })
            {
                _clientAddon.SendUpdate(PlayerData.instance.health + PlayerData.instance.healthBlue,
                    PlayerData.instance.MPCharge + PlayerData.instance.MPReserve);
                Gofl.InvalidateArrange();
            }
        };
    }

    public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        MenuRef ??= new Menu("HKMP_HealthDisplay", new Element[]
        {
            new HorizontalOption("Health Display Type",
                "Choose how health will be displayed. Note: Scene change is required to not cause overlaps", 
                Enum.GetNames(typeof(HealthDisplayType)),
                (i) =>
                {
                    settings._healthDisplayType = (HealthDisplayType)i;
                    foreach (var (player, component) in HealthDisplayClient.Cache)
                    {
                        //destory all health bars and let the component deal with its consequences
                        component?.HealthBar?.Destroy();
                        component?.ClearAllTextUI();
                    }
                },
                () => (int)settings._healthDisplayType),
            new TextPanel(""),
            new TextPanel("This mod was made by Mulhima", fontSize: 50),
            new TextPanel("with help and support from BadMagic", fontSize: 50),
        });
        
        return MenuRef.GetMenuScreen(modListMenu);
    }

    public bool ToggleButtonInsideMenu { get; }
}