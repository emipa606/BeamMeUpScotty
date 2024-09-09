using UnityEngine;
using Verse;

namespace LTF_Teleport;

[StaticConstructorOnStartup]
public class MyGizmo
{
    private static readonly string GizmoPath = "UI/Commands/";

    private static readonly string DebugPath = $"{GizmoPath}Debug/";

    private static readonly string HaxPath = $"{GizmoPath}Hax/";

    private static readonly string QualityPath = $"{GizmoPath}Quality/";

    private static readonly string SpotPath = $"{GizmoPath}TpSpot/";

    private static readonly string BenchPath = $"{GizmoPath}TpBench/";

    private static readonly string IssuePath = $"{GizmoPath}TpBench/Issue/";

    public static string tpBenchManualCastIssuePath = $"{BenchPath}ManualCastIssue/";

    public static readonly Texture2D DebugOnGz = ContentFinder<Texture2D>.Get($"{DebugPath}DebugOn");

    public static readonly Texture2D DebugOffGz = ContentFinder<Texture2D>.Get($"{DebugPath}DebugOff");

    public static readonly Texture2D DebugLogGz = ContentFinder<Texture2D>.Get($"{DebugPath}DebugLog");

    public static readonly Texture2D HaxAddGz = ContentFinder<Texture2D>.Get($"{HaxPath}HaxAdd");

    public static readonly Texture2D HaxSubGz = ContentFinder<Texture2D>.Get($"{HaxPath}HaxSub");

    public static readonly Texture2D HaxFullGz = ContentFinder<Texture2D>.Get($"{HaxPath}HaxFull");

    public static readonly Texture2D HaxEmptyGz = ContentFinder<Texture2D>.Get($"{HaxPath}HaxEmpty");

    public static Texture2D HaxWorseGz = ContentFinder<Texture2D>.Get($"{HaxPath}HaxWorse");

    public static Texture2D HaxBetterGz = ContentFinder<Texture2D>.Get($"{HaxPath}HaxBetter");

    public static readonly Texture2D QualityBadGz = ContentFinder<Texture2D>.Get($"{QualityPath}Bad");

    public static readonly Texture2D QualityGoodGz = ContentFinder<Texture2D>.Get($"{QualityPath}Good");

    public static readonly Texture2D QualityNormalGz = ContentFinder<Texture2D>.Get($"{QualityPath}Normal");

    public static readonly Texture2D AutoOnGz = ContentFinder<Texture2D>.Get($"{SpotPath}AutoOn");

    public static readonly Texture2D AutoOffGz = ContentFinder<Texture2D>.Get($"{SpotPath}AutoOff");

    public static readonly Texture2D LinkedGz = ContentFinder<Texture2D>.Get($"{SpotPath}LinkOn");

    public static readonly Texture2D OrphanGz = ContentFinder<Texture2D>.Get($"{SpotPath}LinkOff");

    public static readonly Texture2D WayNoGz = ContentFinder<Texture2D>.Get($"{SpotPath}WayNo");

    public static readonly Texture2D WayInGz = ContentFinder<Texture2D>.Get($"{SpotPath}WayIn");

    public static readonly Texture2D WayOutGz = ContentFinder<Texture2D>.Get($"{SpotPath}WayOut");

    public static readonly Texture2D WaySwapGz = ContentFinder<Texture2D>.Get($"{SpotPath}WaySwap");

    public static readonly Texture2D TpLogGz = ContentFinder<Texture2D>.Get($"{BenchPath}RegistryLog");

    public static readonly Texture2D EmptyRegistryGz = ContentFinder<Texture2D>.Get($"{BenchPath}RegistryEmpty");

    public static readonly Texture2D FullRegistryGz = ContentFinder<Texture2D>.Get($"{BenchPath}RegistryFull");

    public static readonly Texture2D NextTpGz = ContentFinder<Texture2D>.Get($"{BenchPath}NextTp");

    public static readonly Texture2D IssueCooldownGz = ContentFinder<Texture2D>.Get($"{IssuePath}Cooldown");

    public static readonly Texture2D IssueCooldownTwinGz = ContentFinder<Texture2D>.Get($"{IssuePath}CooldownTwin");

    public static readonly Texture2D IssueEmptyGz = ContentFinder<Texture2D>.Get($"{IssuePath}Empty");

    public static readonly Texture2D IssueEmptyTwinGz = ContentFinder<Texture2D>.Get($"{IssuePath}EmptyTwin");

    public static readonly Texture2D IssueNoFacilityGz = ContentFinder<Texture2D>.Get($"{IssuePath}NoFacility");

    public static readonly Texture2D IssueNoFacilityTwinGz = ContentFinder<Texture2D>.Get($"{IssuePath}NoFacilityTwin");

    public static readonly Texture2D IssueOrphanGz = ContentFinder<Texture2D>.Get($"{IssuePath}Orphan");

    public static readonly Texture2D IssueOrphanTwinGz = ContentFinder<Texture2D>.Get($"{IssuePath}OrphanTwin");

    public static readonly Texture2D IssueOverweightGz = ContentFinder<Texture2D>.Get($"{IssuePath}Overweight");

    public static readonly Texture2D IssueOverweightTwinGz = ContentFinder<Texture2D>.Get($"{IssuePath}OverweightTwin");

    public static readonly Texture2D IssuePowerGz = ContentFinder<Texture2D>.Get($"{IssuePath}Unpowered");

    public static readonly Texture2D IssuePowerTwinGz = ContentFinder<Texture2D>.Get($"{IssuePath}UnpoweredTwin");

    public static readonly Texture2D IssueNoPoweredFacilityGz =
        ContentFinder<Texture2D>.Get($"{IssuePath}UnpoweredFacility");

    public static readonly Texture2D IssueNoPoweredFacilityTwinGz = ContentFinder<Texture2D>.Get(
        $"{IssuePath}UnpoweredFacilityTwin");

    public static readonly Texture2D[] WayGizmo = [WayNoGz, WayOutGz, WayInGz, WaySwapGz];

    public static Texture2D EmptyStatus2Gizmo(bool empty, bool full)
    {
        var result = TpLogGz;
        if (empty)
        {
            result = EmptyRegistryGz;
        }

        if (full)
        {
            result = FullRegistryGz;
        }

        return result;
    }
}