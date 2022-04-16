using System.Collections.Generic;

namespace Lifeboat.StaticDataModifiers;

public abstract class StaticDataModifier
{
    public static readonly List<StaticDataModifier> StaticDataModifiers = new();
    protected StaticDataModifier() => StaticDataModifiers.Add(this);
    
    public abstract void Patch();
    public abstract void Unpatch();
}