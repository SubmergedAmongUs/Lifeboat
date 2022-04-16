using System;

namespace Lifeboat.CustomAppearance;

public sealed class AppearanceModification
{
    [Flags]
    public enum Overrides
    {
        Name = 1,
        ColorId = 2,
        HatId = 4,
        SkinId = 8,
        PetId = 16,
        Alpha = 32,
    }

    public AppearanceData Data { get; set; }
    public Overrides ModificationMask { get; set; }
    public int Priority { get; set; }
    public float Timer { get; set; }
}