﻿using Hkmp.Api.Client;
using Hkmp.Api.Server;
using HKMP_HealthDisplay.Settings;
using Satchel.BetterMenus;

namespace HKMP_HealthDisplay;

public class HKMP_HealthDisplay:Mod, IGlobalSettings<GlobalSettings>
{
    private HealthDisplayClient _clientAddon = new HealthDisplayClient();
    private HealthDisplayServer _serverAddon = new HealthDisplayServer();
    private Menu MenuRef;

    public static HKMP_HealthDisplay Instance;
    
    public static GlobalSettings settings { get; set; } = new ();
    public void OnLoadGlobal(GlobalSettings s) => settings = s;
    public GlobalSettings OnSaveGlobal() => settings;
    
    public override void Initialize()
    {
        Instance ??= this;
        ClientAddon.RegisterAddon(_clientAddon);
        ServerAddon.RegisterAddon(_serverAddon);
        
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