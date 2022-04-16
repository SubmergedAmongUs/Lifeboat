using System.Collections;
using System.Collections.Generic;

namespace Lifeboat.CustomAppearance;

public sealed class NameOverrideManager : IEnumerable<NameOverride>
{
    public List<NameOverride> Overrides = new();

    public void Add(NameOverride nameOverride)
    {
        Overrides.Add(nameOverride);
        Overrides.Sort((o1, o2) => o2.Priority.CompareTo(o1.Priority));
    }

    public IEnumerator<NameOverride> GetEnumerator()
    {
        return Overrides.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}