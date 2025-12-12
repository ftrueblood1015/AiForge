using AutoMapper;
using AiForge.Application.DTOs.Projects;
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

    public ProjectService(IProjectRepository projectRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var projects = await _projectRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<ProjectDto>>(projects);
    }

    public async Task<ProjectDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, cancellationToken);
        return project == null ? null : _mapper.Map<ProjectDto>(project);
    }

    public async Task<ProjectDto?> GetByKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetByKeyAsync(key, cancellationToken);
        return project == null ? null : _mapper.Map<ProjectDto>(project);
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

        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
