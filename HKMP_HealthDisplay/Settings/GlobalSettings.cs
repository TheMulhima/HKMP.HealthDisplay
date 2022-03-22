namespace HKMP_HealthDisplay.Settings;
public enum HealthDisplayType
{
    MaskUI,
    MaskAndSoulText,
    MaskText,
    None,
}  
public class GlobalSettings
{
    public HealthDisplayType _healthDisplayType = HealthDisplayType.MaskUI;
}