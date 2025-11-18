using System;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class YearPartitionLimit() : PartitionLimit
{
    private DateTimeOffset? CurrentDate = null;

    // Fucking kill me my code is ASS
    private bool ForceSplitFlag = false;

    public override bool IsReached(long messagesWritten, long bytesWritten) => ForceSplitFlag;

    public override void Update(Message message, ExportContext context)
    {
        ForceSplitFlag = false;

        DateTimeOffset messageDate = context.NormalizeDate(message.Timestamp);
        CurrentDate ??= messageDate;

        if (messageDate.Year != CurrentDate?.Year)
        {
            CurrentDate = messageDate;
            ForceSplitFlag = true;
        }
    }
}
