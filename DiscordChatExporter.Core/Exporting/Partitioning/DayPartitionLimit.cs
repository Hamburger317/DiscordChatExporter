using System;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class DayPartitionLimit() : PartitionLimit
{
    private DateTimeOffset? CurrentDay = null;

    // Fucking kill me my code is ASS
    private bool ForceSplitFlag = false;

    public override bool IsReached(long messagesWritten, long bytesWritten) => ForceSplitFlag;

    public override void Update(Message message, ExportContext context)
    {
        ForceSplitFlag = false;

        DateTimeOffset messageDate = context.NormalizeDate(message.Timestamp);
        CurrentDay ??= messageDate;

        if (messageDate.Date != CurrentDay?.Date)
        {
            CurrentDay = messageDate;
            ForceSplitFlag = true;
        }
    }
}
