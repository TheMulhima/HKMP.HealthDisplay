using JetBrains.Annotations;
using UnityEngine.Serialization;

namespace HKMP_HealthDisplay.MonoBehaviours;

public class HealthBarController : MonoBehaviour
{
    public GameObject Host;

    public GameObject empty = new();

    public int health;

    [CanBeNull] public HealthBarUI HealthBarUI;

    public void Start()
    {
        if (HKMP_HealthDisplay.settings.isUIShown)
        {
            CreateHealthBar();
        }
    }
    
    //to be called from outside
    public void UpdateText(int newhealth)
    {
        health = newhealth;
        UpdateDisplayText();
    }

    private void UpdateDisplayText()
    {
        if (HKMP_HealthDisplay.settings.isUIShown)
        {
            if (HealthBarUI == null)
            {
                CreateHealthBar();
            }
            
            HealthBarUI!.SetMasks(health);
        }
        else
        {
            HealthBarUI?.Destroy();
        }
    }

    private void CreateHealthBar()
    {
        if (Host == null)
        {
            HealthBarUI?.Destroy();
        }
        else
        {
            if (HealthBarUI == null)
            {
                HealthBarUI = new HealthBarUI(HKMP_HealthDisplay.Instance.layout, Host, "Health bar maybe");
                HKMP_HealthDisplay.Instance.gameObjectFollowingLayout.Children.Add(HealthBarUI);
            }
        }
    }
    public void OnDestroy()
    {
        //the other goes made here are children so they auto yeet themselves.
        //but this isnt so we yeet manually
        HealthBarUI?.Destroy();
    }
}