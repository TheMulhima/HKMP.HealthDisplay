using Logger = Modding.Logger;

namespace HKMP_HealthDisplay;

public class HealthOnTopOfPlayer : MonoBehaviour
{
    public TextMeshPro HealthTmpro;
    public TextMeshPro SoulTmpro;
    public GameObject Host;

    public GameObject empty = new();

    public int health, soul;

    public HealthBar HealthBar;

    #region TextDisplay
    private void CreateMaskAndSoulDisplay()
    {
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskAndSoulText)
        {
            ClearAllTextUI();
            HealthTmpro = CreateTextUI("health text", AssetLoader.Mask, -1f);
            SoulTmpro = CreateTextUI("soul text", AssetLoader.Vessel, 0.5f);
        }
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskText)
        {
            ClearAllTextUI();
            HealthTmpro = CreateTextUI("health text", AssetLoader.Mask, -0.5f);
        }
    }

    private TextMeshPro CreateTextUI(string goname, Sprite sprite, float xpos)
    {
        var textGameObject = Instantiate(
            empty,
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
        spriteRenderer.sprite = AssetLoader.Mask;
        
        textGameObject.SetActive(true);
        imageObject.SetActive(true);

        return Tmpro;
    }

    private void UpdateDisplayText()
    {
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskAndSoulText)
        {
            if (SoulTmpro == null || HealthTmpro == null)
            {
                CreateMaskAndSoulDisplay();
            }
            SoulTmpro.text = soul.ToString();
            HealthTmpro.text = health.ToString();
        }
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskText)
        {
            if (HealthTmpro == null) CreateMaskAndSoulDisplay();
            HealthTmpro.text = health.ToString();
        }
    }
    
    
    #endregion
    
    public void UpdateText(int newhealth, int newsoul)
    {
        health = newhealth;
        soul = newsoul;

        UpdateDisplayText();
    }
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
    public void Update()
    {
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskUI)
        {
            HandleHealthBar();
        }
    }
    
    private void CreateHealthBar()
    {
        HealthBar = new HealthBar(HKMP_HealthDisplay.Instance.Layout, Host, "Health bar maybe");
        HKMP_HealthDisplay.Instance.Gofl.Children.Add(HealthBar);
    }


    private void HandleHealthBar()
    {
        if (HealthBar != null)
        {
            HealthBar?.SetMasks(health);
        }
        else
        {
            if (Host != null)
            {
                CreateHealthBar();
            }
        }
    }

    public void OnDestroy()
    {
        HealthBar?.Destroy();
    }

    public void ClearAllTextUI()
    {
        if (HealthTmpro != null)
        {
            var go = HealthTmpro.gameObject;
            Destroy(go.GetComponentInChildren<SpriteRenderer>());
            Destroy(go.GetComponent<TextMeshPro>());
            Destroy(go);
        }
        if (SoulTmpro != null)
        {
            var go = SoulTmpro.gameObject;
            Destroy(go.GetComponentInChildren<SpriteRenderer>());
            Destroy(go.GetComponent<TextMeshPro>());
            Destroy(go);
        }
    }
}