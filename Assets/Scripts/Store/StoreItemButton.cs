using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemButton : MonoBehaviour {

    [Header("References")]
    private PlayerData playerData;
    private GridPlacementController gridPlacementController;
    private KingdomUIController kingdomUIController;
    private KingdomAudioManager audioManager;

    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private Image materialIcon;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    [Header("Object Data Settings")]
    private ObjectData objectData;

    [Header("Material Icons")]
    [SerializeField] private Sprite woodIcon;
    [SerializeField] private Sprite brickIcon;
    [SerializeField] private Sprite metalIcon;

    public void InitializeButton(ObjectData objectData) {

        this.objectData = objectData;

        playerData = FindObjectOfType<PlayerData>();
        gridPlacementController = FindObjectOfType<GridPlacementController>();
        kingdomUIController = FindObjectOfType<KingdomUIController>();
        audioManager = FindObjectOfType<KingdomAudioManager>();

        buyButton.onClick.AddListener(BuyItem);

        itemIcon.sprite = objectData.icon;
        itemNameText.text = objectData.name;

        switch (objectData.materialType) {

            case GameManager.MaterialType.Wood:

            materialIcon.sprite = woodIcon;
            break;

            case GameManager.MaterialType.Brick:

            materialIcon.sprite = brickIcon;
            priceText.transform.parent.GetComponent<HorizontalLayoutGroup>().spacing = 5f;
            break;

            case GameManager.MaterialType.Metal:

            materialIcon.sprite = metalIcon;
            priceText.transform.parent.GetComponent<HorizontalLayoutGroup>().spacing = 5f;
            break;

        }

        priceText.text = objectData.price + "";

    }

    private void BuyItem() {

        switch (objectData.materialType) {

            case GameManager.MaterialType.Wood:

            if (playerData.GetWoodCount() >= objectData.price) {

                playerData.RemoveWood(objectData.price);
                audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Buy);
                kingdomUIController.UpdateWoodCount();
                kingdomUIController.CloseStoreHUD();
                gridPlacementController.StartForcedPlacement(objectData.ID);
                break;

            }

            audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Error);
            break;

            case GameManager.MaterialType.Brick:

            if (playerData.GetBrickCount() >= objectData.price) {

                playerData.RemoveBrick(objectData.price);
                audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Buy);
                kingdomUIController.UpdateBrickCount();
                kingdomUIController.CloseStoreHUD();
                gridPlacementController.StartForcedPlacement(objectData.ID);
                break;

            }

            audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Error);
            break;

            case GameManager.MaterialType.Metal:

            if (playerData.GetMetalCount() >= objectData.price) {

                playerData.RemoveMetal(objectData.price);
                audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Buy);
                kingdomUIController.UpdateMetalCount();
                kingdomUIController.CloseStoreHUD();
                gridPlacementController.StartForcedPlacement(objectData.ID);
                break;

            }

            audioManager.PlaySound(KingdomAudioManager.KingdomSoundType.Error);
            break;

        }
    }
}
