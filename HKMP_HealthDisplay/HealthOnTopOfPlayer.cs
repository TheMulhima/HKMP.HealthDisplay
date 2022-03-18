namespace HKMP_HealthDisplay;

public class HealthOnTopOfPlayer : MonoBehaviour
{
    public TextMeshPro HealthTmpro;
    public TextMeshPro SoulTmpro;
    public GameObject Host;

    public GameObject empty = new GameObject();

    public int health, soul;

    public HealthBar HealthBar;

    private void CreateMaskHolder()
    {
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskAndSoulText)
        {
            var healthGameObject = Instantiate(
                empty,
                Host.transform.position + new Vector3(-1f, 2f, 0),
                Quaternion.identity
            );
            healthGameObject.name = "Health Number";
            healthGameObject.transform.SetParent(Host.transform);
            healthGameObject.transform.localScale = new Vector3(0.25f, 0.25f, healthGameObject.transform.localScale.z);
            healthGameObject.AddComponent<KeepWorldScalePositive>();

            // Add a TextMeshPro component to it, so we can render text
            HealthTmpro = healthGameObject.AddComponent<TextMeshPro>();
            HealthTmpro.text = "";
            HealthTmpro.alignment = TextAlignmentOptions.Center;
            HealthTmpro.fontSize = 22;
            HealthTmpro.outlineWidth = 0.2f;
            HealthTmpro.outlineColor = Color.black;

            var maskObject = Instantiate(empty,
                healthGameObject.transform.position + new Vector3(0.8f, 0, 0), Quaternion.identity);
            maskObject.transform.localScale = new Vector3(0.15f, 0.15f, maskObject.transform.localScale.z);
            maskObject.name = name;
            maskObject.transform.SetParent(healthGameObject.transform);
            maskObject.AddComponent<KeepWorldScalePositive>();

            var spriteRenderer = maskObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetLoader.Mask;

            var soulGameObject = Instantiate(
                empty,
                Host.transform.position + new Vector3(0.5f, 2f, 0),
                Quaternion.identity
            );
            soulGameObject.name = "Soul Number";
            soulGameObject.transform.SetParent(Host.transform);
            soulGameObject.transform.localScale = new Vector3(0.25f, 0.25f, soulGameObject.transform.localScale.z);
            soulGameObject.AddComponent<KeepWorldScalePositive>();

            // Add a TextMeshPro component to it, so we can render text
            SoulTmpro = soulGameObject.AddComponent<TextMeshPro>();
            SoulTmpro.text = "";
            SoulTmpro.alignment = TextAlignmentOptions.Center;
            SoulTmpro.fontSize = 22;
            SoulTmpro.outlineWidth = 0.2f;
            SoulTmpro.outlineColor = Color.black;

            var vesselObject = Instantiate(empty,
                soulGameObject.transform.position + new Vector3(0.8f, 0, 0), Quaternion.identity);
            vesselObject.transform.localScale = new Vector3(0.3f, 0.3f, vesselObject.transform.localScale.z);
            vesselObject.name = name;
            vesselObject.transform.SetParent(healthGameObject.transform);
            vesselObject.AddComponent<KeepWorldScalePositive>();

            var spriteRenderersoul = vesselObject.AddComponent<SpriteRenderer>();
            spriteRenderersoul.sprite = AssetLoader.Vessel;


            healthGameObject.SetActive(true);
            maskObject.SetActive(true);
            soulGameObject.SetActive(true);
            vesselObject.SetActive(true);
        }
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskText)
        {
            var healthGameObject = Instantiate(
                empty,
                Host.transform.position + new Vector3(-0.5f, 2f, 0),
                Quaternion.identity
            );
            healthGameObject.name = "Health Number";
            healthGameObject.transform.SetParent(Host.transform);
            healthGameObject.transform.localScale = new Vector3(0.25f, 0.25f, healthGameObject.transform.localScale.z);
            healthGameObject.AddComponent<KeepWorldScalePositive>();

            // Add a TextMeshPro component to it, so we can render text
            HealthTmpro = healthGameObject.AddComponent<TextMeshPro>();
            HealthTmpro.text = "";
            HealthTmpro.alignment = TextAlignmentOptions.Center;
            HealthTmpro.fontSize = 22;
            HealthTmpro.outlineWidth = 0.2f;
            HealthTmpro.outlineColor = Color.black;

            var maskObject = Instantiate(empty,
                healthGameObject.transform.position + new Vector3(0.8f, 0, 0), Quaternion.identity);
            maskObject.transform.localScale = new Vector3(0.15f, 0.15f, maskObject.transform.localScale.z);
            maskObject.name = name;
            maskObject.transform.SetParent(healthGameObject.transform);
            maskObject.AddComponent<KeepWorldScalePositive>();

            var spriteRenderer = maskObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = AssetLoader.Mask;
            
            healthGameObject.SetActive(true);
            maskObject.SetActive(true);
        }
    }
    
    public void UpdateText(int newhealth, int newsoul)
    {
        health = newhealth;
        soul = newsoul;
        
        if (HKMP_HealthDisplay.settings._healthDisplayType == HealthDisplayType.MaskAndSoulText)
        {
            SoulTmpro.text = soul.ToString();
        }
        if (HKMP_HealthDisplay.settings._healthDisplayType is HealthDisplayType.MaskText or HealthDisplayType.MaskAndSoulText )
        {
            HealthTmpro.text = health.ToString();
        }
    }
    public void Start()
    {
        if (HKMP_HealthDisplay.settings._healthDisplayType != HealthDisplayType.MaskUI)
        {
            CreateMaskHolder();
        }
        else
        {
            HealthBar = new HealthBar(HKMP_HealthDisplay.Instance.Layout, Host, "Health bar maybe");
            HKMP_HealthDisplay.Instance.Gofl.Children.Add(HealthBar);
            HealthBar.Visibility = Visibility.Visible;
        }
    }

    public void Update()
    {
        if (HKMP_HealthDisplay.settings._healthDisplayType != HealthDisplayType.MaskUI) return;
        if (HealthBar != null)
        {
            HealthBar.SetMasks(health);
            HealthBar.InvalidateArrange();
        }
    }
}