using System.Text.RegularExpressions;

namespace RaidReview.PostRaid;

internal static class RecordingIdentity
{
    internal static void Apply(Dictionary<string, object?> raid, string? username, IEnumerable<Dictionary<string, object?>> players)
    {
        // Normalize response objects only, including historical/imported recordings.
        // Keep the original database evidence intact.
        var playerRows = players.ToArray();
        foreach (var player in playerRows)
        {
            var role = player.GetValueOrDefault("type")?.ToString();
            var side = player.GetValueOrDefault("team")?.ToString();
            player["team"] = role switch
            {
                "PMC|USEC" => "Usec",
                "PMC|BEAR" => "Bear",
                _ => side switch { "1" => "Usec", "2" => "Bear", "4" => "Savage", _ => side }
            };
            if ((role == "PMC|USEC" || role == "PMC|BEAR") && player.GetValueOrDefault("mod_SAIN_brain")?.ToString() == "SCAV")
                player["mod_SAIN_brain"] = "PMC";
        }
        if (!string.IsNullOrEmpty(raid.GetValueOrDefault("importedAt")?.ToString()))
        {
            raid["profileName"] = raid.GetValueOrDefault("importedProfileName")?.ToString() ?? "";
            raid["isHeadless"] = Convert.ToInt64(raid.GetValueOrDefault("importedIsHeadless") ?? 0) != 0;
            return;
        }
        var headless = Regex.IsMatch(username ?? "", "^Headless_[0-9a-f]{24}$", RegexOptions.IgnoreCase);
        var humans = playerRows.Where(p => p.GetValueOrDefault("type")?.ToString() == "HUMAN").ToArray();
        var names = humans.Select(p => p.GetValueOrDefault("name")?.ToString())
            .Where(name => !string.IsNullOrWhiteSpace(name)).Distinct().ToArray();
        raid["isHeadless"] = headless;
        raid["profileName"] = headless
            ? names.Length > 0 ? string.Join(", ", names) + " (headless)" : "Headless"
            : username ?? "";

        // START has no local player on a headless host and defaults to PMC.
        // Use recorded humans, including late joiners, for presentation. AI factions
        // and ambiguous/missing human metadata must not change the recorded type.
        if (headless && humans.Length > 0)
        {
            var types = humans.Select(p => RaidType(p.GetValueOrDefault("team")?.ToString())).Distinct().ToArray();
            if (types.Length == 1 && types[0] != null) raid["type"] = types[0];
        }
    }

    private static string? RaidType(string? side) => side switch
    {
        "Savage" or "4" => "SCAV",
        "Usec" or "Bear" or "1" or "2" => "PMC",
        _ => null
    };
}
