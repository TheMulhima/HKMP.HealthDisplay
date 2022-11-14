using HutongGames.PlayMaker.Actions;
using JetBrains.Annotations;
using UnityEngine.Serialization;

namespace HKMP_HealthDisplay.MonoBehaviours;

public class HealthBarController : MonoBehaviour
{
    public GameObject MaskHolder;

    public int healthMain;
    public int healthMax;
    public int healthBlue;

    //to be called from outside
    public void UpdateText(int newHealthMain, int newHealthMax, int newHealthBlue)
    {
        healthMain = newHealthMain;
        healthMax = newHealthMax;
        healthBlue = newHealthBlue;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (HKMP_HealthDisplay.settings.isUIShown)
        {
            if (MaskHolder == null)
            {
                CreateHealthBarHolder();
            }
            
            int totalMasks = (healthMax + healthBlue);
            int maxShownMasks = 15; //heart + 4 lifeblood
            int maskToShow = Math.Min(totalMasks, maxShownMasks);

            float maskWidth = AssetLoader.Mask.bounds.size.x;
            float maskSpacing = maskWidth / 2f;

            float start = MaskHolder.transform.position.x - (maskToShow * maskSpacing) / 2f + maskSpacing;

            for (int i = 0; i < maxShownMasks; i++)
            {
                SpriteRenderer spriteRenderer;
                var maskGo = MaskHolder.Find($"Mask {i}");
                if (maskGo == null)
                {
                    maskGo = new GameObject($"Mask {i}", typeof(KeepWorldScalePositive));
                    spriteRenderer = maskGo.GetAddComponent<SpriteRenderer>();
                    maskGo.transform.SetParent(MaskHolder.transform);
                }
                else
                {
                    spriteRenderer = maskGo.GetAddComponent<SpriteRenderer>();
                }

                maskGo.transform.position = new Vector3(start + i * maskSpacing, MaskHolder.transform.position.y, -2 - (totalMasks - i)/(float)totalMasks);

                if (i < healthMain)
                {
                    spriteRenderer.sprite = AssetLoader.Mask;
                    maskGo.SetActive(true);
                }
                else if (i < healthMax)
                {
                    spriteRenderer.sprite = AssetLoader.MaskEmpty;
                    maskGo.SetActive(true);
                }
                else if (i < totalMasks)
                {
                    spriteRenderer.sprite = AssetLoader.MaskBlue;
                    maskGo.SetActive(true);
                }
                else if (i < maxShownMasks)
                {
                    maskGo.SetActive(false);
                }
            }

            if (totalMasks > maxShownMasks)
            {
                SpriteRenderer spriteRenderer;
                var maskGo = MaskHolder.Find($"End Mask");
                if (maskGo == null)
                {
                    maskGo = new GameObject($"End Mask", typeof(KeepWorldScalePositive));
                    spriteRenderer = maskGo.GetAddComponent<SpriteRenderer>();
                    maskGo.transform.SetParent(MaskHolder.transform);
                }
                else
                {
                    spriteRenderer = maskGo.GetAddComponent<SpriteRenderer>();
                }

                maskGo.transform.position = new Vector3(start + maxShownMasks * maskSpacing, MaskHolder.transform.position.y, -2 - (totalMasks - maxShownMasks)/(float)totalMasks);

                spriteRenderer.sprite = AssetLoader.Plus;
                maskGo.SetActive(true);
            }
            else
            {
                var maskGo = MaskHolder.Find($"End Mask");
                if (maskGo != null)
                {
                    maskGo.SetActive(false);
                }
            }
        }
        else
        {
            if (MaskHolder != null)
            {
                Destroy(MaskHolder);
            }
        }
    }

    private void CreateHealthBarHolder()
    {
        var oldGo = gameObject.Find("MaskHolder");
        if (oldGo != null) Destroy(oldGo);

        MaskHolder = new GameObject("MaskHolder", typeof(KeepWorldScalePositive))
        {
            transform =
            {
                position = gameObject.transform.position + Vector3.up * 2f
            }
        };
        MaskHolder.transform.SetParent(gameObject.transform);
        MaskHolder.SetActive(true);
    }
}