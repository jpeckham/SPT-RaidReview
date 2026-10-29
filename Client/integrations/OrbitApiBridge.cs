using System;
using System.Reflection;
namespace RAID_REVIEW
{
    // Bind only when the optional ORBIT plugin is present; no Mono game references at build time.
    internal static class OrbitApiBridge
    {
        private static Type Api => Type.GetType("Orbit.Api.OrbitTelemetry, ORBIT", true);
        internal static Type MainObjectiveType => Type.GetType("Orbit.Api.OrbitMainObjective, ORBIT", true);
        private static dynamic Call(string name, params object[] args)
            => (Api.GetMethod(name, BindingFlags.Public | BindingFlags.Static) ?? throw new MissingMethodException(name)).Invoke(null, args);
        private static dynamic Property(string name)
            => (Api.GetProperty(name, BindingFlags.Public | BindingFlags.Static) ?? throw new MissingMemberException(name)).GetValue(null);
        internal static bool IsAvailable => Property("IsAvailable");
        internal static int MainObjectivesRevision => Property("MainObjectivesRevision");
        internal static bool IsBotDormant(string id) => Call("IsBotDormant", id);
        internal static dynamic DrainGhostFights() => Call("DrainGhostFights");
        internal static dynamic DrainGhostWakes() => Call("DrainGhostWakes");
        internal static dynamic DrainGhostHearing() => Call("DrainGhostHearing");
        internal static dynamic GetBotObjective(string id) => Call("GetBotObjective", id);
        internal static dynamic GetFieldSnapshot() => Call("GetFieldSnapshot");
        internal static dynamic GetMainObjectivesSnapshot() => Call("GetMainObjectivesSnapshot");
    }
}
