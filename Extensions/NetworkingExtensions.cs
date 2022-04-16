using Hazel;
using UnityEngine;

namespace Lifeboat.Extensions;

public static class NetworkingExtensions
{
    public static void Write(this MessageWriter writer, Color32 color)
    {
        writer.Write(color.r);
        writer.Write(color.g);
        writer.Write(color.b);
        writer.Write(color.a);
    }

    public static Color32 ReadColor32(this MessageReader reader) => new(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
}