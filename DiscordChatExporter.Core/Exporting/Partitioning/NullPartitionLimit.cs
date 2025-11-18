using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting.Partitioning;

internal class NullPartitionLimit : PartitionLimit
{
    public override bool IsReached(long messagesWritten, long bytesWritten) => false;

    public override void Update(Message message, ExportContext context) { }
}
