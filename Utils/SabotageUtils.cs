using System.Linq;
using Submerged.Enums;
using Submerged.Map.MonoBehaviours;
using Submerged.Systems.CustomSystems.Oxygen.Patches;

namespace Lifeboat.Utils;

public static class SabotageUtils
{
    public static bool AnyActive()
    {
        PlayerTask_TaskIsEmergency_Patch.DisableO2MaskCheck = true;
        bool result = PlayerControl.LocalPlayer.myTasks.ToArray().Any(PlayerTask.TaskIsEmergency);
        PlayerTask_TaskIsEmergency_Patch.DisableO2MaskCheck = false;

        return result;
    }

    public static void FixAllSabotages()
    {
        // Skeld - Lights, Comms, Reactor, O2
        // Mira - Lights, Comms, Reactor, O2
        // Polus - Lights, Comms, Reactor
        // Airship - Lights, Comms, Heli
        // Submerged - Lights, Comms, Ballast, O2

        FixLights();

        SubmarineStatus submerged = SubmarineStatus.Instance;
        if (submerged)
        {
            FixComms();
            FixReactor(SystemTypes.Reactor);
            FixSubmergedOxygenForAll();
            SubmarineStatus.Instance.LightFlickerActive = false;
            return;
        }

        AirshipStatus airship = ShipStatus.Instance.TryCast<AirshipStatus>();
        if (airship)
        {
            FixComms();
            FixAirshipReactor();
            return;
        }

        PolusShipStatus polus = ShipStatus.Instance.TryCast<PolusShipStatus>();
        if (polus)
        {
            FixComms();
            FixReactor(SystemTypes.Laboratory);
            return;
        }

        MiraShipStatus mira = ShipStatus.Instance.TryCast<MiraShipStatus>();
        if (mira)
        {
            FixMiraComms();
            FixReactor(SystemTypes.Reactor);
            FixOxygen();
            return;
        }

        SkeldShipStatus skeld = ShipStatus.Instance.TryCast<SkeldShipStatus>();
        if (skeld)
        {
            FixComms();
            FixReactor(SystemTypes.Reactor);
            FixOxygen();
        }
    }

    public static void FixLights()
    {
        SwitchSystem switchSystem = ShipStatus.Instance.Systems[SystemTypes.Electrical].TryCast<SwitchSystem>();
        for (int i = 0; i < 5; i++)
        {
            int mask = 1 << i;
            bool shouldFlick = (switchSystem.ActualSwitches & mask) != (switchSystem.ExpectedSwitches & mask);
            if (shouldFlick) ShipStatus.Instance.RpcRepairSystem(SystemTypes.Electrical, i);
        }
    }

    public static void FixReactor(SystemTypes system)
    {
        ShipStatus.Instance.RpcRepairSystem(system, 16);
    }

    public static void FixOxygen()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.LifeSupp, 16);
    }

    public static void FixComms()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 0);
    }

    public static void FixMiraComms()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 0);
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Comms, 16 | 1);
    }

    public static void FixAirshipReactor()
    {
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16 | 0);
        ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, 16 | 1);
    }

    public static void FixSubmergedOxygenForAll()
    {
        ShipStatus.Instance.RpcRepairSystem(CustomSystemTypes.UpperCentral, 16);
    }
}