using AutoMapper;
using Moq;
using System.Linq.Expressions;
using TechCareer.DataAccess.Repositories.Abstracts;
using TechCareer.Models.Dtos.Instructors;
using TechCareer.Models.Entities;
using TechCareer.Service.Concretes;
using TechCareer.Service.Rules;
using NUnit.Framework;

namespace TechCareer.NunitTest
{
    [TestFixture]
    public class InstructorServiceTests
    {
        private Mock<IInstructorRepository> _instructorRepositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<InstructorBusinessRules> _businessRulesMock;
        private InstructorService _instructorService;

        [SetUp]
        public void SetUp()
        {
            _instructorRepositoryMock = new Mock<IInstructorRepository>();
            _mapperMock = new Mock<IMapper>();
            _businessRulesMock = new Mock<InstructorBusinessRules>();
            _instructorService = new InstructorService(
                _instructorRepositoryMock.Object,
                _mapperMock.Object,
                _businessRulesMock.Object
            );
        }

        [Test]
        public async Task AddAsync_ShouldAddInstructorSuccessfully()
        {
            // Arrange
            var createDto = new CreateInstructorRequestDto("John Doe", "Expert in Software Development");
            var instructorEntity = new Instructor { Id = Guid.NewGuid(), Name = createDto.Name, About = createDto.About };
            var responseDto = new InstructorResponseDto { Id = instructorEntity.Id, Name = instructorEntity.Name, About = instructorEntity.About };

            _mapperMock
                .Setup(m => m.Map<Instructor>(createDto))
                .Returns(instructorEntity);

            _instructorRepositoryMock
                .Setup(r => r.AddAsync(instructorEntity))
                .ReturnsAsync(instructorEntity);

            _mapperMock
                .Setup(m => m.Map<InstructorResponseDto>(instructorEntity))
                .Returns(responseDto);

            // Act
            var result = await _instructorService.AddAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(responseDto.Name, result.Name);
            _instructorRepositoryMock.Verify(r => r.AddAsync(instructorEntity), Times.Once);
            _mapperMock.Verify(m => m.Map<InstructorResponseDto>(instructorEntity), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteInstructorSuccessfully()
        {
            // Arrange
            var instructorId = Guid.NewGuid();
            var instructorEntity = new Instructor { Id = instructorId, Name = "John Doe" };

            _businessRulesMock
                .Setup(b => b.InstructorMustExist(instructorId))
                .ReturnsAsync(instructorEntity);

            _instructorRepositoryMock
                .Setup(r => r.DeleteAsync(instructorEntity, false))
                .ReturnsAsync(instructorEntity);

            // Act
            var result = await _instructorService.DeleteAsync(instructorId, false);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Instructor successfully deleted.", result);
            _businessRulesMock.Verify(b => b.InstructorMustExist(instructorId), Times.Once);
            _instructorRepositoryMock.Verify(r => r.DeleteAsync(instructorEntity, false), Times.Once);
        }

        [Test]
        public async Task UpdateAsync_ShouldUpdateInstructorSuccessfully()
        {
            // Arrange
            var instructorId = Guid.NewGuid();
            var updateDto = new UpdateInstructorRequestDto(instructorId, "Updated Name", "Updated About");
            var existingInstructor = new Instructor { Id = instructorId, Name = "Old Name", About = "Old About" };
            var updatedInstructor = new Instructor { Id = instructorId, Name = updateDto.Name, About = updateDto.About };
            var responseDto = new InstructorResponseDto { Id = instructorId, Name = updatedInstructor.Name, About = updatedInstructor.About };

            _businessRulesMock
                .Setup(b => b.InstructorMustExist(instructorId))
                .ReturnsAsync(existingInstructor);

            _mapperMock
                .Setup(m => m.Map(updateDto, existingInstructor))
                .Callback<UpdateInstructorRequestDto, Instructor>((dto, entity) =>
                {
                    entity.Name = dto.Name;
                    entity.About = dto.About;
                });

            _instructorRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Instructor>()))
                .ReturnsAsync(updatedInstructor);

            _mapperMock
                .Setup(m => m.Map<InstructorResponseDto>(updatedInstructor))
                .Returns(responseDto);

            // Act
            var result = await _instructorService.UpdateAsync(instructorId, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Updated Name", result.Name);
            Assert.AreEqual("Updated About", result.About);
            _businessRulesMock.Verify(b => b.InstructorMustExist(instructorId), Times.Once);
            _mapperMock.Verify(m => m.Map(updateDto, existingInstructor), Times.Once);
            _instructorRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Instructor>()), Times.Once);
            _mapperMock.Verify(m => m.Map<InstructorResponseDto>(updatedInstructor), Times.Once);
        }

        [Test]
        public async Task GetByIdAsync_ShouldReturnInstructor()
        {
            // Arrange
            var instructorId = Guid.NewGuid();
            var instructorEntity = new Instructor { Id = instructorId, Name = "John Doe", About = "Expert in C#" };
            var responseDto = new InstructorResponseDto { Id = instructorId, Name = instructorEntity.Name, About = instructorEntity.About };

            _businessRulesMock
                .Setup(b => b.InstructorMustExist(instructorId))
                .ReturnsAsync(instructorEntity);

            _mapperMock
                .Setup(m => m.Map<InstructorResponseDto>(instructorEntity))
                .Returns(responseDto);

            // Act
            var result = await _instructorService.GetByIdAsync(instructorId);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(responseDto.Name, result.Name);
            _businessRulesMock.Verify(b => b.InstructorMustExist(instructorId), Times.Once);
            _mapperMock.Verify(m => m.Map<InstructorResponseDto>(instructorEntity), Times.Once);
        }

        [Test]
        public async Task GetListAsync_ShouldReturnListOfInstructors()
        {
            // Arrange
            var mockInstructors = new List<Instructor>
            {
                new Instructor { Id = Guid.NewGuid(), Name = "Instructor 1", About = "About Instructor 1" },
                new Instructor { Id = Guid.NewGuid(), Name = "Instructor 2", About = "About Instructor 2" }
            };

            _instructorRepositoryMock.Setup(repo => repo.GetListAsync(
                It.IsAny<Expression<Func<Instructor, bool>>>(),
                It.IsAny<Func<IQueryable<Instructor>, IOrderedQueryable<Instructor>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(mockInstructors);

            _mapperMock.Setup(mapper => mapper.Map<List<InstructorResponseDto>>(mockInstructors))
                       .Returns(new List<InstructorResponseDto>
                       {
                           new InstructorResponseDto { Id = mockInstructors[0].Id, Name = "Instructor 1", About = "About Instructor 1" },
                           new InstructorResponseDto { Id = mockInstructors[1].Id, Name = "Instructor 2", About = "About Instructor 2" }
                       });

            // Act
            var result = await _instructorService.GetListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Instructor 1", result[0].Name);
            Assert.AreEqual("Instructor 2", result[1].Name);

            // Verify
            _instructorRepositoryMock.Verify(repo => repo.GetListAsync(
                It.IsAny<Expression<Func<Instructor, bool>>>(),
                It.IsAny<Func<IQueryable<Instructor>, IOrderedQueryable<Instructor>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
