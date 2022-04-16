using System;
using Framework.Attributes;

namespace Lifeboat.Roles;

[RegisterInIl2Cpp]
public sealed class CustomImportantTextTask : PlayerTask
{
    public CustomImportantTextTask(IntPtr ptr) : base(ptr)
    {
    }

    public override int TaskStep => 0;

    public override bool IsComplete => true;

    public override void Initialize()
    {
    }

    public void Awake()
    {
        TaskType = (TaskTypes) 99999;
    }

    public override bool ValidConsole(Console console)
    {
        return false;
    }

    public override void Complete()
    {
    }

    public override void AppendTaskText(Il2CppSystem.Text.StringBuilder sb)
    {
        sb.AppendLine(Text);
    }

    public string Text;
}