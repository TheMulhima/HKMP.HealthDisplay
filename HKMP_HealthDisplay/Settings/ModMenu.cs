﻿namespace HKMP_HealthDisplay.Settings;

public static class ModMenu
{
    private static Menu menuRef;
    public static MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        menuRef ??= new Menu("HKMP_HealthDisplay", new Element[]
        {
            new HorizontalOption("Is Health Bar Displayed",
                "Should the HKMP health bar be visible", 
                new []{"True", "False"},
                (i) =>
                {
                    HKMP_HealthDisplay.settings.isUIShown = i == 0;
                    if (!HKMP_HealthDisplay.settings.isUIShown)
                    {
                        foreach (var (_, component) in HKMP_HealthDisplay.HealthBarComponentCache)
                        {
                            //destory all health bars and let the component deal with its consequences
                            if (component == null) continue;
                            component.HealthBarUI?.Destroy();
                        }
                    }
                },
                () => HKMP_HealthDisplay.settings.isUIShown ? 0 : 1),
            new TextPanel(""),
            new TextPanel("This mod was made by Mulhima", fontSize: 50),
            new TextPanel("with help and support from:", fontSize: 50),
            new TextPanel("BadMagic, Extremelyd1, Dandy and Dwarfwoot", fontSize: 50),
        });
        
        return menuRef.GetMenuScreen(modListMenu);
    }
}