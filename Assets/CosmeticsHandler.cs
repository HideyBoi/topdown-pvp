using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CosmeticsHandler : MonoBehaviour
{
    public SkinnedMeshRenderer player;
    public SkinnedMeshRenderer hat;

    public int currSkinId;
    public int currHatId;

    public void LoadCosmetics()
    {
        currHatId = PlayerPrefs.GetInt("DES_HAT");
        currSkinId = PlayerPrefs.GetInt("DES_SKIN");

        player.material = CosmeticsManager.i.skins[currSkinId];

        hat.sharedMesh = CosmeticsManager.i.hats[currHatId].mesh;
        hat.materials = CosmeticsManager.i.hats[currHatId].materials;
    }

    public void SetCosmetics(int hatId, int skinId)
    {
        player.material = CosmeticsManager.i.skins[skinId];

        hat.sharedMesh = CosmeticsManager.i.hats[hatId].mesh;
        hat.materials = CosmeticsManager.i.hats[hatId].materials;
    }
}
