using System.Linq;
using Framework.Extensions;
using UnityEngine;

namespace Lifeboat.Extensions;

public static class DeadBodyExtensions
{
    public static void DestroyBody(this DeadBody body)
    {
        if (!body) return;
        foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>().Where(b => b.ParentId == body.ParentId))
        {
            deadBody.gameObject.Destroy();
        }
    }
        
    public static void DestroyBody(byte playerId)
    {
        foreach (DeadBody deadBody in GameObject.FindObjectsOfType<DeadBody>().Where(b => b.ParentId == playerId))
        {
            deadBody.gameObject.Destroy();
        }
    }
}