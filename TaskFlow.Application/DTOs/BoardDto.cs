namespace TaskFlow.Application.DTOs;

/// <summary>
/// Data Transfer Object for Board — shape of what we return to the frontend.
/// DTOs decouple our domain entities from API responses. If Board entity changes,
/// the DTO can stay the same (backward compatible API).
/// </summary>
public record BoardDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    OwnerDto Owner,
    IEnumerable<ColumnDto> Columns);

public record OwnerDto(Guid Id, string FullName, string Email);

public record ColumnDto(
    Guid Id,
    string Name,
    int Position,
    IEnumerable<TaskItemDto> Tasks);

public record TaskItemDto(
    Guid Id,
    string Title,
    string? Description,
    string Priority,
    string Status,
    DateTime? DueDate,
    int Position,
    Guid? AssignedToId);
