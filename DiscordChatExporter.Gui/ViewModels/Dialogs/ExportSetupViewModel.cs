﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;
using DiscordChatExporter.Core.Exporting;
using DiscordChatExporter.Core.Exporting.Filtering;
using DiscordChatExporter.Core.Exporting.Partitioning;
using DiscordChatExporter.Core.Utils.Extensions;
using DiscordChatExporter.Gui.Framework;
using DiscordChatExporter.Gui.Services;

namespace DiscordChatExporter.Gui.ViewModels.Dialogs;

public partial class ExportSetupViewModel(
    DialogManager dialogManager,
    SettingsService settingsService
) : DialogViewModelBase
{
    [ObservableProperty]
    public partial Guild? Guild { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSingleChannel))]
    public partial IReadOnlyList<Channel>? Channels { get; set; }

    [ObservableProperty]
    public partial string? OutputPath { get; set; }

    [ObservableProperty]
    public partial ExportFormat SelectedFormat { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAfterDateSet))]
    [NotifyPropertyChangedFor(nameof(After))]
    public partial DateTimeOffset? AfterDate { get; set; }

    [ObservableProperty]
    public partial TimeSpan? AfterTime { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsBeforeDateSet))]
    [NotifyPropertyChangedFor(nameof(Before))]
    public partial DateTimeOffset? BeforeDate { get; set; }

    [ObservableProperty]
    public partial TimeSpan? BeforeTime { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PartitionLimit))]
    public partial string? PartitionLimitValue { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MessageFilter))]
    public partial string? MessageFilterValue { get; set; }

    [ObservableProperty]
    public partial bool ShouldFormatMarkdown { get; set; }

    [ObservableProperty]
    public partial bool ShouldDownloadAssets { get; set; }

    [ObservableProperty]
    public partial bool ShouldReuseAssets { get; set; }

    [ObservableProperty]
    public partial string? AssetsDirPath { get; set; }

    [ObservableProperty]
    public partial bool IsAdvancedSectionDisplayed { get; set; }

    public bool IsSingleChannel => Channels?.Count == 1;

    public IReadOnlyList<ExportFormat> AvailableFormats { get; } = Enum.GetValues<ExportFormat>();

    public bool IsAfterDateSet => AfterDate is not null;

    public DateTimeOffset? After => AfterDate?.Add(AfterTime ?? TimeSpan.Zero);

    public bool IsBeforeDateSet => BeforeDate is not null;

    public DateTimeOffset? Before => BeforeDate?.Add(BeforeTime ?? TimeSpan.Zero);

    public PartitionLimit PartitionLimit =>
        !string.IsNullOrWhiteSpace(PartitionLimitValue)
            ? PartitionLimit.Parse(PartitionLimitValue)
            : PartitionLimit.Null;

    public MessageFilter MessageFilter =>
        !string.IsNullOrWhiteSpace(MessageFilterValue)
            ? MessageFilter.Parse(MessageFilterValue)
            : MessageFilter.Null;

    [RelayCommand]
    private void Initialize()
    {
        // Persist preferences
        SelectedFormat = settingsService.LastExportFormat;
        PartitionLimitValue = settingsService.LastPartitionLimitValue;
        MessageFilterValue = settingsService.LastMessageFilterValue;
        ShouldFormatMarkdown = settingsService.LastShouldFormatMarkdown;
        ShouldDownloadAssets = settingsService.LastShouldDownloadAssets;
        ShouldReuseAssets = settingsService.LastShouldReuseAssets;
        AssetsDirPath = settingsService.LastAssetsDirPath;

        // Show the "advanced options" section by default if any
        // of the advanced options are set to non-default values.
        IsAdvancedSectionDisplayed =
            After is not null
            || Before is not null
            || !string.IsNullOrWhiteSpace(PartitionLimitValue)
            || !string.IsNullOrWhiteSpace(MessageFilterValue)
            || ShouldDownloadAssets
            || ShouldReuseAssets
            || !string.IsNullOrWhiteSpace(AssetsDirPath);
    }

    [RelayCommand]
    private async Task ShowOutputPathPromptAsync()
    {
        if (IsSingleChannel)
        {
            var defaultFileName = ExportRequest.GetDefaultOutputFileName(
                Guild!,
                Channels!.Single(),
                SelectedFormat,
                After?.Pipe(Snowflake.FromDate),
                Before?.Pipe(Snowflake.FromDate)
            );

            var extension = SelectedFormat.GetFileExtension();

            var path = await dialogManager.PromptSaveFilePathAsync(
                [
                    new FilePickerFileType($"{extension.ToUpperInvariant()} file")
                    {
                        Patterns = [$"*.{extension}"],
                    },
                ],
                defaultFileName
            );

            if (!string.IsNullOrWhiteSpace(path))
                OutputPath = path;
        }
        else
        {
            var path = await dialogManager.PromptDirectoryPathAsync();
            if (!string.IsNullOrWhiteSpace(path))
                OutputPath = path;
        }
    }

    [RelayCommand]
    private async Task ShowAssetsDirPathPromptAsync()
    {
        var path = await dialogManager.PromptDirectoryPathAsync();
        if (!string.IsNullOrWhiteSpace(path))
            AssetsDirPath = path;
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        // Prompt the output path if it hasn't been set yet
        if (string.IsNullOrWhiteSpace(OutputPath))
        {
            await ShowOutputPathPromptAsync();

            // If the output path is still not set, cancel the export
            if (string.IsNullOrWhiteSpace(OutputPath))
                return;
        }

        // Persist preferences
        settingsService.LastExportFormat = SelectedFormat;
        settingsService.LastPartitionLimitValue = PartitionLimitValue;
        settingsService.LastMessageFilterValue = MessageFilterValue;
        settingsService.LastShouldFormatMarkdown = ShouldFormatMarkdown;
        settingsService.LastShouldDownloadAssets = ShouldDownloadAssets;
        settingsService.LastShouldReuseAssets = ShouldReuseAssets;
        settingsService.LastAssetsDirPath = AssetsDirPath;

        Close(true);
    }
}
