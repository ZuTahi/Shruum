using UnityEngine;

[System.Serializable]
public class ArtifactChoice
{
    public bool isPermanent;
    public PermanentItemType permanentType;
    public ArtifactSO temporaryArtifact;

    public string GetName()
    {
        return isPermanent ? permanentType.ToString() : temporaryArtifact.artifactName;
    }

    public Sprite GetIcon(Sprite flower, Sprite leaf, Sprite water, Sprite fruit, Sprite root, Sprite key)
    {
        if (isPermanent)
        {
            switch (permanentType)
            {
                case PermanentItemType.Flower: return flower;
                case PermanentItemType.Leaf:   return leaf;
                case PermanentItemType.Water:  return water;
                case PermanentItemType.Fruit:  return fruit;
                case PermanentItemType.Root:   return root;
                case PermanentItemType.WeaponKey: return key;
            }
        }
        return temporaryArtifact != null ? temporaryArtifact.artifactIcon : null;
    }
}
