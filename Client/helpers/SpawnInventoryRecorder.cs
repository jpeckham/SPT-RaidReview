using System;
using System.Collections.Generic;
using EFT;
using EFT.InventoryLogic;

namespace RAID_REVIEW
{
    internal static class SpawnInventoryRecorder
    {
        internal static void Capture(Player player)
        {
            if (player == null) return;
            using var measurement = RecordingPerformance.Measure(RecordingPerformance.Stage.SpawnInventory);
            try
            {
                var items = new List<TrackingInventoryItem>();
                foreach (var item in player.Inventory.GetPlayerItems(EPlayerItems.Equipment))
                {
                    if (item == null) continue;
                    items.Add(new TrackingInventoryItem
                    {
                        templateId = item.TemplateId.ToString(),
                        itemName = item.LocalizedShortName(),
                        price = RAID_REVIEW.GetHandbookPrice(item),
                        qty = item.StackObjectsCount,
                        slot = item.Parent?.Container?.ID ?? "unknown"
                    });
                }
                if (items.Count > 0)
                    Telemetry.SendSnapshot("PLAYER_INVENTORY", new TrackingPlayerInventory
                    {
                        sessionId = RAID_REVIEW.sessionId,
                        profileId = player.ProfileId,
                        time = RAID_REVIEW.stopwatch.ElapsedMilliseconds,
                        items = items
                    });
            }
            catch (Exception ex)
            {
                LoggerInstance.Log.LogWarning("Raid Review could not capture starting inventory: " + ex.Message);
            }
        }
    }
}
