using AutoMapper;
using Moq;
using System.Linq.Expressions;
using TechCareer.DataAccess.Repositories.Abstracts;
using TechCareer.Models.Dtos.VideoEducation;
using TechCareer.Models.Entities;
using TechCareer.Service.Concretes;
using TechCareer.Service.Constants;
using TechCareer.Service.Rules;
using NUnit.Framework;

namespace TechCareer.NunitTest
{
    [TestFixture]
    public class VideoEducationServiceTests
    {
        private Mock<IVideoEducationRepository> _videoEducationRepositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<VideoEducationBusinessRules> _businessRulesMock;
        private VideoEducationService _videoEducationService;

        [SetUp]
        public void SetUp()
        {
            _videoEducationRepositoryMock = new Mock<IVideoEducationRepository>();
            _mapperMock = new Mock<IMapper>();
            _businessRulesMock = new Mock<VideoEducationBusinessRules>();
            _videoEducationService = new VideoEducationService(
                _videoEducationRepositoryMock.Object,
                _mapperMock.Object,
                _businessRulesMock.Object
            );
        }

        [Test]
        public async Task AddAsync_ShouldAddVideoEducationSuccessfully()
        {
            // Arrange
            var createDto = new CreateVideoEducationRequestDto
            {
                Title = "New Video Education",
                Description = "Description",
                TotalHour = 5.5,
                IsCertified = true,
                Level = TechCareer.Models.Entities.Enum.Level.Beginner,
                ImageUrl = "image.jpg",
                InstructorId = Guid.NewGuid(),
                ProgrammingLanguage = "C#"
            };

            var videoEducationEntity = new VideoEducation
            {
                Id = 1,
                Title = createDto.Title,
                Description = createDto.Description,
                TotalHour = createDto.TotalHour,
                IsCertified = createDto.IsCertified,
                Level = createDto.Level,
                ImageUrl = createDto.ImageUrl,
                InstructorId = createDto.InstructorId,
                ProgrammingLanguage = createDto.ProgrammingLanguage
            };

            var responseDto = new VideoEducationResponseDto
            {
                Id = videoEducationEntity.Id,
                Title = videoEducationEntity.Title,
                Description = videoEducationEntity.Description,
                TotalHour = videoEducationEntity.TotalHour,
                Level = videoEducationEntity.Level,
                ImageUrl = videoEducationEntity.ImageUrl,
                InstructorId = videoEducationEntity.InstructorId,
                ProgrammingLanguage = videoEducationEntity.ProgrammingLanguage
            };

            _mapperMock
                .Setup(m => m.Map<VideoEducation>(createDto))
                .Returns(videoEducationEntity);

            _videoEducationRepositoryMock
                .Setup(r => r.AddAsync(videoEducationEntity))
                .ReturnsAsync(videoEducationEntity);

            _mapperMock
                .Setup(m => m.Map<VideoEducationResponseDto>(videoEducationEntity))
                .Returns(responseDto);

            // Act
            var result = await _videoEducationService.AddAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(responseDto.Title, result.Title);
            _videoEducationRepositoryMock.Verify(r => r.AddAsync(videoEducationEntity), Times.Once);
            _mapperMock.Verify(m => m.Map<VideoEducationResponseDto>(videoEducationEntity), Times.Once);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnVideoEducation()
        {
            // Arrange
            var videoEducationId = 1;
            var videoEducationEntity = new VideoEducation
            {
                Id = videoEducationId,
                Title = "Test Video Education",
                Description = "Test Description",
                TotalHour = 3.5,
                Level = TechCareer.Models.Entities.Enum.Level.Middle,
                ImageUrl = "image.jpg",
                InstructorId = Guid.NewGuid(),
                ProgrammingLanguage = "Python"
            };

            var responseDto = new VideoEducationResponseDto
            {
                Id = videoEducationEntity.Id,
                Title = videoEducationEntity.Title,
                Description = videoEducationEntity.Description,
                TotalHour = videoEducationEntity.TotalHour,
                Level = videoEducationEntity.Level,
                ImageUrl = videoEducationEntity.ImageUrl,
                InstructorId = videoEducationEntity.InstructorId,
                ProgrammingLanguage = videoEducationEntity.ProgrammingLanguage
            };

            _businessRulesMock
                .Setup(b => b.VideoEducationMustExist(videoEducationId))
                .ReturnsAsync(videoEducationEntity);

            _mapperMock
                .Setup(m => m.Map<VideoEducationResponseDto>(videoEducationEntity))
                .Returns(responseDto);

            // Act
            var result = await _videoEducationService.GetByIdAsync(videoEducationId);

            // Assert
            Assert.NotNull(result, "Result should not be null.");
            Assert.AreEqual(responseDto.Title, result.Title, "Title mismatch.");
            Assert.AreEqual(responseDto.Description, result.Description, "Description mismatch.");
            _businessRulesMock.Verify(b => b.VideoEducationMustExist(videoEducationId), Times.Once);
            _mapperMock.Verify(m => m.Map<VideoEducationResponseDto>(videoEducationEntity), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateVideoEducationSuccessfully()
        {
            // Arrange
            var videoEducationId = 1;
            var updateDto = new UpdateVideoEducationRequestDto
            {
                Id = videoEducationId,
                Title = "Updated Video Education",
                Description = "Updated Description",
                TotalHour = 6.0,
                InstructorId = Guid.NewGuid()
            };

            var existingVideoEducation = new VideoEducation
            {
                Id = videoEducationId,
                Title = "Old Video Education",
                Description = "Old Description",
                TotalHour = 5.0,
                InstructorId = updateDto.InstructorId
            };

            var updatedVideoEducation = new VideoEducation
            {
                Id = videoEducationId,
                Title = updateDto.Title,
                Description = updateDto.Description,
                TotalHour = updateDto.TotalHour,
                InstructorId = updateDto.InstructorId
            };

            var responseDto = new VideoEducationResponseDto
            {
                Id = videoEducationId,
                Title = updatedVideoEducation.Title,
                Description = updatedVideoEducation.Description,
                TotalHour = updatedVideoEducation.TotalHour,
                InstructorId = updatedVideoEducation.InstructorId,
                ImageUrl = "UpdatedImage.jpg",
                Level = TechCareer.Models.Entities.Enum.Level.Advaced,
                ProgrammingLanguage = "JavaScript"
            };

            _businessRulesMock
                .Setup(b => b.VideoEducationMustExist(videoEducationId))
                .ReturnsAsync(existingVideoEducation);

            _mapperMock
                .Setup(m => m.Map(It.IsAny<UpdateVideoEducationRequestDto>(), It.IsAny<VideoEducation>()))
                .Callback<UpdateVideoEducationRequestDto, VideoEducation>((dto, entity) =>
                {
                    entity.Title = dto.Title;
                    entity.Description = dto.Description;
                    entity.TotalHour = dto.TotalHour;
                });

            _videoEducationRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<VideoEducation>()))
                .ReturnsAsync(updatedVideoEducation);

            _mapperMock
                .Setup(m => m.Map<VideoEducationResponseDto>(updatedVideoEducation))
                .Returns(responseDto);

            // Act
            var result = await _videoEducationService.UpdateAsync(videoEducationId, updateDto);

            // Assert
            Assert.NotNull(result, "Result should not be null.");
            Assert.AreEqual("Updated Video Education", result.Title, "Title mismatch.");
            Assert.AreEqual(6.0, result.TotalHour, "TotalHour mismatch.");
            _businessRulesMock.Verify(b => b.VideoEducationMustExist(videoEducationId), Times.Once);
            _mapperMock.Verify(m => m.Map(updateDto, existingVideoEducation), Times.Once);
            _videoEducationRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<VideoEducation>()), Times.Once);
            _mapperMock.Verify(m => m.Map<VideoEducationResponseDto>(updatedVideoEducation), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteVideoEducationSuccessfully()
        {
            // Arrange
            var videoEducationId = 1;
            var videoEducationEntity = new VideoEducation
            {
                Id = videoEducationId,
                Title = "Test Video Education"
            };

            _businessRulesMock
                .Setup(b => b.VideoEducationMustExist(videoEducationId))
                .ReturnsAsync(videoEducationEntity);

            _videoEducationRepositoryMock
                .Setup(r => r.DeleteAsync(videoEducationEntity, false))
                .ReturnsAsync(videoEducationEntity);

            // Act
            var result = await _videoEducationService.DeleteAsync(videoEducationId, false);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("VideoEducation successfully deleted.", result); // Güncellenen mesaj
            _businessRulesMock.Verify(b => b.VideoEducationMustExist(videoEducationId), Times.Once);
            _videoEducationRepositoryMock.Verify(r => r.DeleteAsync(videoEducationEntity, false), Times.Once);
        }


        [Test]
        public async Task GetListAsync_ShouldReturnListOfVideoEducations()
        {
            // Arrange
            var mockVideoEducations = new List<VideoEducation>
    {
        new VideoEducation
        {
            Id = 1,
            Title = "Video Education 1",
            Description = "Description 1",
            TotalHour = 3.0,
            Level = TechCareer.Models.Entities.Enum.Level.Beginner,
            ImageUrl = "Image1.jpg",
            InstructorId = Guid.NewGuid(),
            ProgrammingLanguage = "C#"
        },
        new VideoEducation
        {
            Id = 2,
            Title = "Video Education 2",
            Description = "Description 2",
            TotalHour = 4.5,
            Level = TechCareer.Models.Entities.Enum.Level.Advaced,
            ImageUrl = "Image2.jpg",
            InstructorId = Guid.NewGuid(),
            ProgrammingLanguage = "Python"
        }
    };

            _videoEducationRepositoryMock.Setup(repo => repo.GetListAsync(
                It.IsAny<Expression<Func<VideoEducation, bool>>>(),
                It.IsAny<Func<IQueryable<VideoEducation>, IOrderedQueryable<VideoEducation>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockVideoEducations);

            _mapperMock.Setup(mapper => mapper.Map<List<VideoEducationResponseDto>>(mockVideoEducations))
                       .Returns(mockVideoEducations.Select(e => new VideoEducationResponseDto
                       {
                           Id = e.Id,
                           Title = e.Title,
                           Description = e.Description,
                           TotalHour = e.TotalHour,
                           Level = e.Level,
                           ImageUrl = e.ImageUrl,
                           InstructorId = e.InstructorId,
                           ProgrammingLanguage = e.ProgrammingLanguage
                       }).ToList());

            // Act
            var result = await _videoEducationService.GetListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Video Education 1", result[0].Title);
            Assert.AreEqual("Video Education 2", result[1].Title);

            // Verify
            _videoEducationRepositoryMock.Verify(repo => repo.GetListAsync(
                It.IsAny<Expression<Func<VideoEducation, bool>>>(),
                It.IsAny<Func<IQueryable<VideoEducation>, IOrderedQueryable<VideoEducation>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<List<VideoEducationResponseDto>>(mockVideoEducations), Times.Once);
        }

    }
}
