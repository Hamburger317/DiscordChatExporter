using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class FileSizePartitionLimit(long limit) : PartitionLimit
{
    public override bool IsReached(long messagesWritten, long bytesWritten) =>
        bytesWritten >= limit;

    public override void Update(Message message, ExportContext context) { }
}
