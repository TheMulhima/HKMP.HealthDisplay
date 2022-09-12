namespace HKMP_HealthDisplay.Settings;

public static class ModMenu
{
    private static Menu menuRef;
    public static MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
    {
        menuRef ??= new Menu("HKMP_HealthDisplay", new Element[]
        {
            new HorizontalOption("Health Display Type",
                "Choose how health will be displayed. Note: Scene change is required to not cause overlaps", 
                Enum.GetNames(typeof(HealthDisplayType)),
                (i) =>
                {
                    HKMP_HealthDisplay.settings._healthDisplayType = (HealthDisplayType)i;
                    foreach (var (_, component) in HKMP_HealthDisplay.HealthBarComponentCache)
                    {
                        //destory all health bars and let the component deal with its consequences
                        if (component == null) continue;
                        component.HealthBarUI?.Destroy();
                        component.ClearAllTextUI();
                    }
                },
                () => (int)HKMP_HealthDisplay.settings._healthDisplayType),
            new TextPanel(""),
            new TextPanel("This mod was made by Mulhima", fontSize: 50),
            new TextPanel("with help and support from:", fontSize: 50),
            new TextPanel("BadMagic (Health Bar UI)", fontSize: 50),
            new TextPanel("Extremelyd1 (HKMP API)", fontSize: 50),
            new TextPanel("Dandy (help with HKMP API)", fontSize: 50),
        });
        
        return menuRef.GetMenuScreen(modListMenu);
    }
}