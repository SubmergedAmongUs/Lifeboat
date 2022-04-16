using System.Collections.Generic;
using Lifeboat.Buttons;
using UnhollowerBaseLib;
using UnityEngine;

namespace Lifeboat.Roles.CrewmateRoles.EngineerRole.Buttons
{
    public sealed class VentButton : TargetedButton<PlayerControl>
    {
        public VentButton()
        {
	        KillButtonManager.renderer.sprite = HudManager.Instance.UseButton.otherButtons[ImageNames.VentButton].graphic.sprite;
            SetPosition(AspectPosition.EdgeAlignments.RightBottom);

            Cooldown = CurrentTime = 0;
        }

        public override void SetupButtonText()
        {
        }
        
        public override bool CanUseInVents => true;

        public override void SetOutline(PlayerControl component, bool on = false, Color color = default) { }

        public override PlayerControl GetClosest() => GetClosestVent() ? PlayerControl.LocalPlayer : null;

        public override void OnClick()
        {
        }

        public List<Vent> ItemsInRange = new(), NewItemsInRange = new();
        public Dictionary<Collider2D, Il2CppReferenceArray<IUsable>> Cache = new(ColliderComparer.Instance);

        public Vent GetClosestVent()
        {
	        PlayerControl localPlayer = PlayerControl.LocalPlayer;
	        GameData.PlayerInfo data = localPlayer.Data;
	        
            NewItemsInRange.Clear();
			Vent closestVent = null;
			float minDistance = float.MaxValue;
			
			foreach (Collider2D collider2D in Physics2D.OverlapCircleAll(localPlayer.GetTruePosition(), localPlayer.MaxReportDistance, Constants.Usables))
			{
				Il2CppReferenceArray<IUsable> array;
				if (Cache.ContainsKey(collider2D)) array = Cache[collider2D];
				else array = Cache[collider2D] = (IUsable[]) collider2D.GetComponents<IUsable>();
				
				if (array != null)
				{
					foreach (IUsable usable in array)
					{
						if (usable.TryCast<Vent>() is not { } vent) continue;

						float distance = vent.CanUse(data, out bool canUse, out bool couldUse);
						if (canUse || couldUse) NewItemsInRange.Add(vent);
						
						if (canUse && distance < minDistance)
						{
							minDistance = distance;
							closestVent = vent;
						}
					}
				}
			}
			
			for (int k = ItemsInRange.Count - 1; k > -1; k--)
			{
				Vent item = ItemsInRange[k];
				int index = NewItemsInRange.FindIndex(j => j == item);
				if (index == -1)
				{
					item.SetOutline(false, false);
					ItemsInRange.RemoveAt(k);
				}
				else
				{
					NewItemsInRange.RemoveAt(index);
					item.SetOutline(true, closestVent == item);
				}
			}
			
			foreach (Vent vent in NewItemsInRange)
			{
				vent.SetOutline(true, closestVent == vent);
				ItemsInRange.Add(vent);
			}

			return closestVent;
        }

        public sealed class ColliderComparer : IEqualityComparer<Collider2D>
        {
	        public static readonly ColliderComparer Instance = new();
	        
	        public bool Equals(Collider2D x, Collider2D y)
	        {
		        return x == y;
	        }

	        public int GetHashCode(Collider2D obj)
	        {
		        return obj.GetInstanceID();
	        }
        }
    }
}