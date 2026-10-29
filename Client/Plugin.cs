using BepInEx;
using BepInEx.Unity.IL2CPP;
namespace RAID_REVIEW
{
    [BepInPlugin("ekky.raidreview", "Raid Review", "2.0.1")]
    [BepInDependency("com.sptushonka.core", "5.0.0")]
    [BepInDependency("me.sol.sain", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.danw.questingbots", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.janky.phobos", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.chazut.orbit", BepInDependency.DependencyFlags.SoftDependency)]
    public class RaidReviewPlugin : BasePlugin
    {
        internal static RaidReviewPlugin Instance;
        public override void Load()
        {
            Instance = this;
            AddComponent<RAID_REVIEW>();
        }
    }
}
