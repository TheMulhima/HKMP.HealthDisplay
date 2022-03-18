namespace HKMP_HealthDisplay;

public class HKMP_HealthDisplay:Mod, IGlobalSettings<GlobalSettings>
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
        
        
        Layout = new LayoutRoot(true, "RandomStuff");
        Gofl = new GameObjectFollowingLayout(Layout, "Some Layout");
        
        ModHooks.HeroUpdateHook += () =>
        {
            if (_clientAddon._clientApi is { NetClient.IsConnected: true })
            {
                _clientAddon.SendUpdate(PlayerData.instance.health + PlayerData.instance.healthBlue,
                    PlayerData.instance.MPCharge + PlayerData.instance.MPReserve);
            }
        };
    }
}