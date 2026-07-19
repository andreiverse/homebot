namespace HomeBot.Display;

public record WidgetSnapshotDto(string WidgetId, string WidgetName, string? WidgetDescription, Card Card);

public record ActionRequestDto(string ActionId, string? Label, string? Parameter);

public record ActionResponseDto(bool Success, string? Message, Card? UpdatedCard);
