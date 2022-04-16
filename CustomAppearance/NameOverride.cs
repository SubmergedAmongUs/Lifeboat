using System;

namespace Lifeboat.CustomAppearance;

public sealed class NameOverride : IDisposable
{
    public delegate string d_NameModifierDelegate(PlayerControl player, string currentName, bool inMeeting);
        
    public d_NameModifierDelegate Modifier;
    public int Priority = 0;

    public NameOverride(d_NameModifierDelegate modifier, int priority = 0)
    {
        Modifier = modifier;
        Priority = priority;
    }

    public void Dispose()
    {
        Modifier = (_, name, _) => name;
    }
}