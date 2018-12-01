using UnityEngine;
using System.Collections;

/// <summary>
/// 用于加载资源
/// </summary>
public class Loader
{
    public static Object LoadObject(string aPathObjectName)
    {
        return Resources.Load(aPathObjectName);
    }

    public static GameObject LoadGameObject(string aPathGameObjectName)
    {
        Object tObject;
        tObject = Resources.Load("Prefabs/" + aPathGameObjectName, typeof(GameObject));

        if (tObject != null)
        {
            GameObject tGameObject = Object.Instantiate(tObject) as GameObject;
            tGameObject.name = aPathGameObjectName.Split('/')[1];
            return tGameObject;
        }
        else return null;
    }

    public static Material LoadMaterial(string aPathMaterialName)
    {
        return Resources.Load("Materials/" + aPathMaterialName, typeof(Material)) as Material;
    }

    public static Texture LoadTexture(string aPathTextureName)
    {
        return Resources.Load("Textures/" + aPathTextureName, typeof(Texture)) as Texture;
    }

    public static TextAsset LoadTextFile(string aPathTextFileName)
    {
        return Resources.Load(aPathTextFileName, typeof(TextAsset)) as TextAsset;
    }

    public static AudioClip LoadAudio(string anAudioSourceName)
    {
        return Resources.Load("Audio/" + anAudioSourceName, typeof(AudioClip)) as AudioClip;
    }

    public static PhysicMaterial LoadPhysicMaterial(string aPathPhysicMaterialName)
    {
        return Resources.Load("PhysicMaterials/" + aPathPhysicMaterialName, typeof(PhysicMaterial)) as PhysicMaterial;
    }
}
