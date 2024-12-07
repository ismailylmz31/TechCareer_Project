using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions.ExceptionTypes;
using Core.Persistence.Extensions;
using System.Linq.Expressions;
using TechCareer.DataAccess.Repositories.Abstracts;
using TechCareer.Models.Dtos.VideoEducation;
using TechCareer.Models.Entities;
using TechCareer.Service.Abstracts;
using TechCareer.Service.Constants;
using TechCareer.Service.Rules;

namespace TechCareer.Service.Concretes;
public sealed class VideoEducationService : IVideoEducationService
{
    private readonly IVideoEducationRepository _videoEducationRepository;
    private readonly IMapper _mapper;
    private readonly VideoEducationBusinessRules _businessRules;


    public VideoEducationService(
        IVideoEducationRepository videoEducationRepository,
        IMapper mapper,
        VideoEducationBusinessRules businessRules)
    {
        _videoEducationRepository = videoEducationRepository;
        _mapper = mapper;
        _businessRules = businessRules;
    }



    //[LoggerAspect]
    //[ClearCacheAspect("VideoEducations")]
    //[AuthorizeAspect("Admin")]
    public async Task<VideoEducationResponseDto> AddAsync(CreateVideoEducationRequestDto dto)
    {
        try
        {
            await _businessRules.VideoEducationTitleMustBeUnique(dto.Title);
            var videoEducationEntity = _mapper.Map<VideoEducation>(dto);

            var addedVideoEducation = await _videoEducationRepository.AddAsync(videoEducationEntity);

            return _mapper.Map<VideoEducationResponseDto>(addedVideoEducation);
        }

        catch (Exception ex)
        {
            throw new BusinessException($"Error occurred while adding video: {ex.Message}", ex);
        }

    }

    //[LoggerAspect]
    //[ClearCacheAspect("VideoEducations")]
    //[AuthorizeAspect("Admin")]
    public async Task<string> DeleteAsync(int id, bool permanent = false)
    {
        try
        {
            var videoEducationEntity = await _businessRules.VideoEducationMustExist(id);
            await _videoEducationRepository.DeleteAsync(videoEducationEntity, permanent);
            return VideoEducationMessage.VideoEducationDeleted;
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error occurred while deleting video: {ex.Message}", ex);
        }
    }

    //[LoggerAspect]
    //[ClearCacheAspect("VideoEducations")]
    //[AuthorizeAspect("Admin")]
    public async Task<VideoEducationResponseDto> UpdateAsync(int id, UpdateVideoEducationRequestDto dto)
    {
        try
        {
            var videoEducationEntity = await _businessRules.VideoEducationMustExist(id);

            _mapper.Map(dto, videoEducationEntity);

            var updatedVideoEducation = await _videoEducationRepository.UpdateAsync(videoEducationEntity);
            return _mapper.Map<VideoEducationResponseDto>(updatedVideoEducation);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error occurred while updating video: {ex.Message}", ex);
        }
    }

    //[CacheAspect(cacheKeyTemplate: "VideoEducationList", bypassCache: false, cacheGroupKey: "VideoEducations")]
    public async Task<List<VideoEducationResponseDto>> GetListAsync(
        Expression<Func<VideoEducation, bool>>? predicate = null,
        Func<IQueryable<VideoEducation>, IOrderedQueryable<VideoEducation>>? orderBy = null,
        bool include = false,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var videoEducations = await _videoEducationRepository.GetListAsync(predicate, orderBy, include, withDeleted, enableTracking, cancellationToken);
            return _mapper.Map<List<VideoEducationResponseDto>>(videoEducations);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error occurred while fetching video list: {ex.Message}", ex);
        }
    }

    public async Task<Paginate<VideoEducationResponseDto>> GetPaginateAsync(
        Expression<Func<VideoEducation, bool>>? predicate = null,
        Func<IQueryable<VideoEducation>, IOrderedQueryable<VideoEducation>>? orderBy = null,
        bool include = true,
        int index = 0,
        int size = 10,
        bool withDeleted = false,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var videoEducations = await _videoEducationRepository.GetPaginateAsync(
            predicate,
            orderBy,
            include,
            index,
            size,
            withDeleted,
            enableTracking,
            cancellationToken
        );

            return new Paginate<VideoEducationResponseDto>
            {
                Items = _mapper.Map<IList<VideoEducationResponseDto>>(videoEducations.Items),
                Index = videoEducations.Index,
                Size = videoEducations.Size,
                Count = videoEducations.Count,
                Pages = videoEducations.Pages
            };
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error occurred while fetching paginated videos: {ex.Message}", ex);
        }
    }

    public async Task<VideoEducationResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var videoEducationEntity = await _businessRules.VideoEducationMustExist(id);
            return _mapper.Map<VideoEducationResponseDto>(videoEducationEntity);
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Error occurred while fetching video by ID: {ex.Message}", ex);
        }
    }
}