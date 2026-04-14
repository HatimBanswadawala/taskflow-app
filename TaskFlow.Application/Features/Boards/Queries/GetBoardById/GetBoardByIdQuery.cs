using MediatR;
using TaskFlow.Application.DTOs;

namespace TaskFlow.Application.Features.Boards.Queries.GetBoardById;

public record GetBoardByIdQuery(Guid Id) : IRequest<BoardDto?>;
