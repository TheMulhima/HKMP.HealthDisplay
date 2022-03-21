namespace HKMP_HealthDisplay.Settings;
public enum HealthDisplayType
{
    MaskUI,
    MaskAndSoulText,
    MaskText,
}  
public class GlobalSettings
{
    public HealthDisplayType _healthDisplayType = HealthDisplayType.MaskUI;
}