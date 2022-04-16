using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.Unstripping;
using Framework.Utilities;
using UnityEngine;

namespace Lifeboat.Utils;

public static class CustomHats
{
    public static void CreateHats()
    {
        if (HatManager.Instance.AllHats.ToArray().Any(a => a.StoreName == "Poggers")) return;
        
        Assembly asm = typeof(CustomHats).Assembly;
        IEnumerable<string> hatNames = asm.GetManifestResourceNames().Where(n => n.Contains("Resources.Hats"));
        int id = 0;
        foreach (string name in hatNames)
        {
            byte[] bytes = asm.GetManifestResourceStream(name).ReadAll();
                
            Texture2D tex = new(2, 2, TextureFormat.ARGB32, false);
            ImageUnstrip.LoadImage(tex, bytes, false);
                
            Sprite sprite = ImageUnstrip.CreateSprite(tex, 100);

            HatBehaviour hatBehaviour = ScriptableObject.CreateInstance<HatBehaviour>();
            hatBehaviour.ChipOffset = new Vector2(-0.1f, 0.35f);
            hatBehaviour.StoreName = "Poggers";
            hatBehaviour.MainImage = sprite;
            hatBehaviour.ProductId = name;
            hatBehaviour.Order = 99 + id;
            id++;
            hatBehaviour.InFront = true;
            hatBehaviour.NoBounce = true;

            hatBehaviour.MainImage = sprite;
            HatManager.Instance.AllHats.Add(hatBehaviour);
        }
    }
}