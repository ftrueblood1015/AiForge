using AutoMapper;
using AiForge.Application.DTOs.Projects;
using AiForge.Application.Interfaces;
using AiForge.Domain.Entities;
using AiForge.Domain.Interfaces;
using AiForge.Infrastructure.Repositories;

namespace AiForge.Application.Services;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProjectDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);
    Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<ProjectDto?> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IUserContext _userContext;
    private readonly IProjectMemberService _projectMemberService;

    public ProjectService(
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IUserContext userContext,
        IProjectMemberService projectMemberService)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _userContext = userContext;
        _projectMemberService = projectMemberService;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);

        // Service accounts and admins can see all projects
        if (_userContext.IsServiceAccount || _userContext.IsAdmin)
        {
            return _mapper.Map<IEnumerable<ProjectDto>>(projects);
        }

        // Filter by accessible projects for regular users
        if (_userContext.UserId.HasValue)
        {
            var accessibleIds = await _projectMemberService.GetAccessibleProjectIdsAsync(_userContext.UserId.Value, cancellationToken);
            var accessibleSet = accessibleIds.ToHashSet();
            projects = projects.Where(p => accessibleSet.Contains(p.Id));
        }
        else
        {
            projects = Enumerable.Empty<Project>();
        }

        return _mapper.Map<IEnumerable<ProjectDto>>(projects);
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Check access first (service accounts and admins bypass)
        if (!_userContext.IsServiceAccount && !_userContext.IsAdmin && _userContext.UserId.HasValue)
        {
            if (!await _projectMemberService.HasAccessAsync(id, _userContext.UserId.Value, cancellationToken))
                return null;
        }

        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        return project == null ? null : _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByKeyAsync(key, cancellationToken);
        if (project == null) return null;

        // Check access (service accounts and admins bypass)
        if (!_userContext.IsServiceAccount && !_userContext.IsAdmin && _userContext.UserId.HasValue)
        {
            if (!await _projectMemberService.HasAccessAsync(project.Id, _userContext.UserId.Value, cancellationToken))
                return null;
        }

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        // Validate key doesn't already exist
        if (await _projectRepository.ExistsByKeyAsync(request.Key, cancellationToken))
        {
            throw new InvalidOperationException($"Project with key '{request.Key}' already exists");
        }

        var project = _mapper.Map<Project>(request);
        project.Key = request.Key.ToUpperInvariant(); // Normalize key to uppercase
        project.CreatedByUserId = _userContext.UserId; // Set creator from authenticated user

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create ProjectMember with Owner role for the creator
        if (_userContext.UserId.HasValue)
        {
            await _projectMemberService.AddOwnerAsync(project.Id, _userContext.UserId.Value, cancellationToken);
        }

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto?> UpdateAsync(Guid id, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return null;

        if (request.Name != null)
            project.Name = request.Name;

        if (request.Description != null)
            project.Description = request.Description;

        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProjectDto>(project);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        if (project == null)
            return false;

        await _projectRepository.DeleteAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
