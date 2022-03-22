using JetBrains.Annotations;
using UnityEngine.Serialization;
using Logger = Modding.Logger;

namespace HKMP_HealthDisplay.MonoBehaviours;

public class HealthBarController : MonoBehaviour
{ 
    [CanBeNull] public TextMeshPro HealthText;
    [CanBeNull] public TextMeshPro SoulText;
    public GameObject Host;

    public GameObject empty = new();

    public int health, soul;

    [CanBeNull] public HealthBarUI HealthBarUI;

    public void Start()
    {
        if (HKMP_HealthDisplay.settings._healthDisplayType != HealthDisplayType.MaskUI)
        {
            CreateMaskAndSoulDisplay();
        }
        else
        {
            CreateHealthBar();   
        }
    }
    
    //to be called from outside
    public void UpdateText(int newhealth, int newsoul)
    {
        health = newhealth;
        soul = newsoul;

        UpdateDisplayText();
    }
    
    private void CreateMaskAndSoulDisplay()
    {
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskAndSoulText)
        {
            ClearAllTextUI();
            HealthBarUI?.Destroy();
            
            HealthText = CreateTextUI("health text", AssetLoader.Mask, -1f);
            SoulText = CreateTextUI("soul text", AssetLoader.Vessel, 0.5f);
        }
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskText)
        {
            ClearAllTextUI();
            HealthBarUI?.Destroy();
            
            HealthText = CreateTextUI("health text", AssetLoader.Mask, -0.5f);
        }
    }

    [CanBeNull]
    private TextMeshPro CreateTextUI(string goname, Sprite sprite, float xpos)
    {
        if (Host == null) return null;
        var textGameObject = Instantiate(
            new GameObject(),
            Host.transform.position + new Vector3(xpos, 2f, 0f),
            Quaternion.identity
        );
        textGameObject.name = goname;
        textGameObject.transform.SetParent(Host.transform);
        textGameObject.transform.localScale = new Vector3(0.25f, 0.25f, textGameObject.transform.localScale.z);
        textGameObject.AddComponent<KeepWorldScalePositive>();

        // Add a TextMeshPro component to it, so we can render text
        var Tmpro = textGameObject.AddComponent<TextMeshPro>();
        Tmpro.text = "";
        Tmpro.alignment = TextAlignmentOptions.Center;
        Tmpro.fontSize = 22;
        Tmpro.outlineWidth = 0.2f;
        Tmpro.outlineColor = Color.black;

        var imageObject = Instantiate(empty,
            Tmpro.transform.position + new Vector3(0.8f, 0, 0), Quaternion.identity);
        imageObject.name = goname;
        imageObject.transform.SetParent(Tmpro.transform);
        imageObject.AddComponent<KeepWorldScalePositive>();

        var spriteRenderer = imageObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        
        textGameObject.SetActive(true);
        imageObject.SetActive(true);

        return Tmpro;
    }

    private void UpdateDisplayText()
    {
        switch (HKMP_HealthDisplay.settings._healthDisplayType)
        {
            case HealthDisplayType.MaskAndSoulText:
            {
                if (SoulText == null || HealthText == null)
                {
                    ClearAllTextUI();
                    CreateMaskAndSoulDisplay();
                }

                if (SoulText != null) SoulText.text = soul.ToString();
                if (HealthText != null) HealthText.text = health.ToString();
                break;
            }
            case HealthDisplayType.MaskText:
            {
                if (HealthText == null)
                {
                    ClearAllTextUI();
                    CreateMaskAndSoulDisplay();
                }

                if (HealthText != null) HealthText.text = health.ToString();
                break;
            }
            case HealthDisplayType.MaskUI:
            {
                if (HealthBarUI == null)
                {
                    CreateHealthBar();
                }
                else
                {
                    HealthBarUI?.SetMasks(health);
                }
                break;
            }
        }
    }

    private void CreateHealthBar()
    {
        if (Host == null)
        {
            HealthBarUI = null;
        }
        else
        {
            HealthBarUI = new HealthBarUI(HKMP_HealthDisplay.Instance.layout, Host, "Health bar maybe");
            HKMP_HealthDisplay.Instance.gameObjectFollowingLayout.Children.Add(HealthBarUI);
        }
    }

    public void ClearAllTextUI()
    {
        if (HealthText != null)
        {
            var go = HealthText.gameObject;
            Destroy(go.GetComponentInChildren<SpriteRenderer>());
            Destroy(go.GetComponent<TextMeshPro>());
            Destroy(go);
        }
        if (SoulText != null)
        {
            var go = SoulText.gameObject;
            Destroy(go.GetComponentInChildren<SpriteRenderer>());
            Destroy(go.GetComponent<TextMeshPro>());
            Destroy(go);
        }
    }
    
    
    public void OnDestroy()
    {
        //the other goes made here are children so they auto yeet themselves.
        //but this isnt so we yeet manually
        HealthBarUI?.Destroy();
    }
}