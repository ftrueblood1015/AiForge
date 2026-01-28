using AutoMapper;
using AiForge.Application.DTOs.Comments;
using AiForge.Application.DTOs.Handoffs;
using AiForge.Application.DTOs.Projects;
using AiForge.Application.DTOs.Tickets;
using AiForge.Domain.Entities;

namespace AiForge.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Project mappings
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.TicketCount, opt => opt.MapFrom(src => src.Tickets.Count));

        CreateMap<CreateProjectRequest, Project>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.NextTicketNumber, opt => opt.MapFrom(_ => 1));

        // Ticket mappings
        CreateMap<Ticket, TicketDto>()
            .ForMember(dest => dest.ProjectKey, opt => opt.MapFrom(src => src.Project.Key))
            .ForMember(dest => dest.SubTicketCount, opt => opt.MapFrom(src => src.SubTickets.Count));

        CreateMap<Ticket, SubTicketSummaryDto>();

        CreateMap<Ticket, TicketDetailDto>()
            .ForMember(dest => dest.ProjectKey, opt => opt.MapFrom(src => src.Project.Key))
            .ForMember(dest => dest.SubTickets, opt => opt.MapFrom(src => src.SubTickets))
            .ForMember(dest => dest.SubTicketCount, opt => opt.MapFrom(src => src.SubTickets.Count))
            .ForMember(dest => dest.CompletedSubTicketCount, opt => opt.MapFrom(src =>
                src.SubTickets.Count(st => st.Status == Domain.Enums.TicketStatus.Done)))
            .ForMember(dest => dest.SubTicketProgress, opt => opt.MapFrom(src =>
                src.SubTickets.Count == 0 ? 0m :
                (decimal)src.SubTickets.Count(st => st.Status == Domain.Enums.TicketStatus.Done) / src.SubTickets.Count * 100))
            .ForMember(dest => dest.ParentTicket, opt => opt.MapFrom(src => src.ParentTicket))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments.Count));

        CreateMap<CreateTicketRequest, Ticket>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => Domain.Enums.TicketStatus.ToDo))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // Comment mappings
        CreateMap<Comment, CommentDto>();

        CreateMap<CreateCommentRequest, Comment>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // TicketHistory mappings
        CreateMap<TicketHistory, TicketHistoryDto>();

        // FileSnapshot mappings
        CreateMap<FileSnapshot, FileSnapshotDto>();
    }
}
