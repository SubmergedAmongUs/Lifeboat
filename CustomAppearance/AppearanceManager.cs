using System;
using System.Collections.Generic;
using System.Linq;
using Framework.Attributes;
using Framework.Extensions;
using Lifeboat.Extensions;
using Lifeboat.Roles;
using UnhollowerBaseLib.Attributes;
using UnityEngine;
using static Lifeboat.CustomAppearance.AppearanceModification;

namespace Lifeboat.CustomAppearance;

[RegisterInIl2Cpp]
public sealed class AppearanceManager : MonoBehaviour
{
    public AppearanceManager(IntPtr ptr) : base(ptr)
    {
    }
    
    public PlayerControl Owner;
    public RoleManager RoleManager;
    public AppearanceData Current;
    public List<AppearanceModification> Modifications = new();
        
    private void Awake()
    {
        Owner = GetComponent<PlayerControl>();
        RoleManager = Owner.GetRoleManager();
    }

    private void Start()
    {
        if (Owner.isDummy || Owner.notRealPlayer || Owner.Data == null)
        {
            Current = new AppearanceData();
        }
        else
        {
            Current = new AppearanceData
            {
                ColorId = Owner.Data.ColorId,
                HatId = Owner.Data.HatId,
                SkinId = Owner.Data.SkinId,
                PetId = Owner.Data.PetId,
                Alpha = 1f,
            };
        }
    }

    #region Set Stuff

    [HideFromIl2Cpp]
    public void SetName(string nameText)
    {
        Current ??= new AppearanceData();
        Current.Name = nameText;
        Owner.nameText.text = nameText;
    }
    
    [HideFromIl2Cpp]
    public void SetColor(int colorId)
    {
        Current ??= new AppearanceData();
        Current.ColorId = colorId;
        PlayerControl.SetPlayerMaterialColors(colorId, Owner.myRend);
        Owner.HatRenderer.SetColor(colorId);
        if (Owner.CurrentPet) PlayerControl.SetPlayerMaterialColors(colorId, Owner.CurrentPet.rend);
    }
    
    [HideFromIl2Cpp]
    public void SetHat(uint hatId, int colorId)
    {
        Current ??= new AppearanceData();
        Current.HatId = hatId;
        if (hatId == 4294967295U) return;
    
        Owner.HatRenderer.SetHat(hatId, colorId);
        Owner.nameText.transform.localPosition = new Vector3(0f, hatId == 0U ? 1.4f : 2.1f, -0.5f);
    }
        
    [HideFromIl2Cpp]
    public void SetSkin(uint skinId) 
    { 		
        Current ??= new AppearanceData();
        Current.SkinId = skinId;
        Owner.MyPhysics.SetSkin(skinId);
        Owner.MyPhysics.body.velocity = Vector2.zero;
        Owner.MyPhysics.HandleAnimation(false);
    }
    
    [HideFromIl2Cpp]
    public void SetPet(uint petId, int colorId)
    {
        Current ??= new AppearanceData();
        Current.PetId = petId;
        if (Owner.CurrentPet) Destroy(Owner.CurrentPet.gameObject);
        Owner.CurrentPet = Instantiate(HatManager.Instance.GetPetById(petId));
        Owner.CurrentPet.transform.position = transform.position;
        Owner.CurrentPet.Source = Owner;
        PlayerControl.SetPlayerMaterialColors(Current.ColorId, Owner.CurrentPet.rend);
    }

    [HideFromIl2Cpp]
    public void SetAlpha(float alpha)
    {
        Current ??= new AppearanceData();
        Current.Alpha = alpha;
            
        Color alphaColor = new(1, 1, 1, alpha);
        Owner.nameText.alpha = alpha;
        Owner.myRend.color = alphaColor;
        Owner.HatRenderer.FrontLayer.color = alphaColor;
        Owner.HatRenderer.BackLayer.color = alphaColor;
        Owner.MyPhysics.Skin.layer.color = alphaColor;
                
        if (Owner.CurrentPet)
        {
            if (Owner.CurrentPet.rend) Owner.CurrentPet.rend.color = alphaColor;
            if (Owner.CurrentPet.shadowRend) Owner.CurrentPet.shadowRend.color = alphaColor;
        }
    }

    #endregion

    private void LateUpdate()
    {
        if (!Owner || !PlayerControl.LocalPlayer || Owner.Data == null) return;

        string playerName = Owner.Data.PlayerName;
        if (ShipStatus.Instance && RoleManager.IsThere() && !MeetingHud.Instance.IsThere())
        {
            foreach (NameOverride nameModifier in RoleManager.NameOverrides)
            {
                playerName = nameModifier.Modifier?.Invoke(Owner, playerName, false);
            }
        }

        GameData.PlayerInfo ownerData = Owner.Data;
        AppearanceData currentPlayerData = new()
        {
            Name = playerName,
            ColorId = ownerData.ColorId,
            HatId = ownerData.HatId,
            SkinId = ownerData.SkinId,
            PetId = ownerData.PetId,
            Alpha = 1f
        };

        List<AppearanceModification> modifications = new(Modifications.OrderBy(m => m.Priority));
        Modifications.Clear();
            
        foreach (AppearanceModification modification in modifications)
        {
            if (modification.Timer < 0) continue;
                
            Modifications.Add(modification);
            modification.Timer -= Time.deltaTime;
                
            if ((modification.ModificationMask & Overrides.Name) == Overrides.Name) currentPlayerData.Name = modification.Data.Name;
            if ((modification.ModificationMask & Overrides.ColorId) == Overrides.ColorId) currentPlayerData.ColorId = modification.Data.ColorId;
            if ((modification.ModificationMask & Overrides.HatId) == Overrides.HatId) currentPlayerData.HatId = modification.Data.HatId;
            if ((modification.ModificationMask & Overrides.SkinId) == Overrides.SkinId) currentPlayerData.SkinId = modification.Data.SkinId;
            if ((modification.ModificationMask & Overrides.PetId) == Overrides.PetId) currentPlayerData.PetId = modification.Data.PetId;
            if ((modification.ModificationMask & Overrides.Alpha) == Overrides.Alpha) currentPlayerData.Alpha = modification.Data.Alpha;
        }
            
        if (Current.Name != currentPlayerData.Name) SetName(currentPlayerData.Name);
        if (Current.ColorId != currentPlayerData.ColorId) SetColor(currentPlayerData.ColorId);
        if (Current.HatId != currentPlayerData.HatId) SetHat(currentPlayerData.HatId, Current.ColorId);
        if (Current.SkinId != currentPlayerData.SkinId) SetSkin(currentPlayerData.SkinId);
        if (Current.PetId != currentPlayerData.PetId) SetPet(currentPlayerData.PetId, Current.ColorId);
        if (Math.Abs(Current.Alpha - currentPlayerData.Alpha) > 0.03f) SetAlpha(currentPlayerData.Alpha);
            
        Owner.SetHatAlpha(Current.Alpha * (ownerData.IsDead ? 0.5f : 1f));
    }
}