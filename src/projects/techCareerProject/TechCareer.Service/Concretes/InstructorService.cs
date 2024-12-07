using AutoMapper;
using Core.AOP.Aspects;
using Core.CrossCuttingConcerns.Exceptions.ExceptionTypes;
using Core.Persistence.Extensions;
using Core.Security.Entities;
using System.Linq.Expressions;
using TechCareer.DataAccess.Repositories.Abstracts;
using TechCareer.Models.Dtos.Instructors;
using TechCareer.Models.Entities;
using TechCareer.Service.Abstracts;
using TechCareer.Service.Constants;
using TechCareer.Service.Rules;

namespace TechCareer.Service.Concretes
{
    public class InstructorService : IInstructorService
    {
        private readonly IInstructorRepository _instructorRepository;
        private readonly IMapper _mapper;
        private readonly InstructorBusinessRules _businessRules;

        public InstructorService(IInstructorRepository instructorRepository, IMapper mapper, InstructorBusinessRules businessRules)
        {
            _instructorRepository = instructorRepository;
            _mapper = mapper;
            _businessRules = businessRules;
        }

        //[LoggerAspect]
        //[ClearCacheAspect("Instructors")]
        //[AuthorizeAspect("Admin")]
        public async Task<InstructorResponseDto> AddAsync(CreateInstructorRequestDto dto)
        {
            try
            {
                await _businessRules.InstructorNameMustBeUnique(dto.Name);

                var instructor = _mapper.Map<Instructor>(dto);
                instructor.Id = Guid.NewGuid();

                var addedInstructor = await _instructorRepository.AddAsync(instructor);
                InstructorResponseDto responseDto = _mapper.Map<InstructorResponseDto>(addedInstructor);
                return responseDto;
            }
            catch (Exception ex)
            {
                throw new BusinessException($"Error occurred while adding instructor: {ex.Message}", ex);
            }

        }

        //[LoggerAspect]
        //[ClearCacheAspect("Instructors")]
        //[AuthorizeAspect("Admin")]
        public async Task<string> DeleteAsync(Guid id, bool permanent = false)
        {
            try
            {
                var instructor = await _businessRules.InstructorMustExist(id);
                await _instructorRepository.DeleteAsync(instructor, permanent);
                return InstructorMessages.InstructorDeleted;
            }
            catch (Exception ex)
            {
                throw new BusinessException($"Error occurred while deleting instructor: {ex.Message}", ex);
            }
        }

        //[LoggerAspect]
        //[ClearCacheAspect("Instructors")]
        //[AuthorizeAspect("Admin")]
        public async Task<InstructorResponseDto> UpdateAsync(Guid id, UpdateInstructorRequestDto dto)
        {
            try
            {
                var instructor = await _businessRules.InstructorMustExist(id);

                _mapper.Map(dto, instructor);

                var updatedInstructor = await _instructorRepository.UpdateAsync(instructor);
                InstructorResponseDto responseDto = _mapper.Map<InstructorResponseDto>(updatedInstructor);
                return responseDto;
            }

            catch (Exception ex)
            {
                throw new BusinessException($"Error occurred while updating instructor: {ex.Message}", ex);
            }
        }

        //[CacheAspect(cacheKeyTemplate: "InstructorList", bypassCache: false, cacheGroupKey: "Instructors")]
        public async Task<List<InstructorResponseDto>> GetListAsync(
            Expression<Func<Instructor, bool>>? predicate = null,
            Func<IQueryable<Instructor>, IOrderedQueryable<Instructor>>? orderBy = null,
            bool include = false,
            bool withDeleted = false,
            bool enableTracking = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instructors = await _instructorRepository.GetListAsync(predicate, orderBy, include, withDeleted, enableTracking, cancellationToken);
                return _mapper.Map<List<InstructorResponseDto>>(instructors);
            }
            catch (Exception ex)
            {
                throw new BusinessException($"Error occurred while fetching instructor list: {ex.Message}", ex);
            }
        }

        public async Task<Paginate<InstructorResponseDto>> GetPaginateAsync(
            Expression<Func<Instructor, bool>>? predicate = null,
            Func<IQueryable<Instructor>, IOrderedQueryable<Instructor>>? orderBy = null,
            bool include = true,
            int index = 0,
            int size = 10,
            bool withDeleted = false,
            bool enableTracking = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var instructors = await _instructorRepository.GetPaginateAsync(
                predicate,
                orderBy,
                include,
                index,
                size,
                withDeleted,
                enableTracking,
                cancellationToken
            );

                var mappedInstructors = _mapper.Map<Paginate<InstructorResponseDto>>(instructors);
                return mappedInstructors;
            }
            catch (Exception ex)
            {
                throw new BusinessException($"Error occurred while fetching paginated instructors: {ex.Message}", ex);
            }
        }


        public async Task<InstructorResponseDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var instructor = await _businessRules.InstructorMustExist(id);
                return _mapper.Map<InstructorResponseDto>(instructor);
            }

            catch (Exception ex)
            {
                throw new BusinessException($"Error occurred while fetching instructor by ID: {ex.Message}", ex);
            }
        }

    }
}