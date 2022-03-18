namespace HKMP_HealthDisplay.Settings;
public enum HealthDisplayType
{
    MaskAndSoulText,
    MaskText,
    MaskUI
}  
public class GlobalSettings
{
    public HealthDisplayType _healthDisplayType = HealthDisplayType.MaskAndSoulText;
}