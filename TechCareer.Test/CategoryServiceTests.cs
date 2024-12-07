

using AutoMapper;
using Moq;
using System.Linq.Expressions;
using TechCareer.DataAccess.Repositories.Abstracts;
using TechCareer.Models.Dtos.Category;
using TechCareer.Models.Entities;
using TechCareer.Service.Concretes;
using TechCareer.Service.Constants;
using TechCareer.Service.Rules;

namespace TechCareer.NunitTest
{
    [TestFixture]
    public class CategoryServiceTests
    {
        private Mock<ICategoryRepository> _categoryRepositoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<CategoryBusinessRules> _businessRulesMock;
        private CategoryService _categoryService;

        [SetUp]
        public void SetUp()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _mapperMock = new Mock<IMapper>();
            _businessRulesMock = new Mock<CategoryBusinessRules>();
            _categoryService = new CategoryService(
                _categoryRepositoryMock.Object,
                _mapperMock.Object,
                _businessRulesMock.Object
            );
        }

        [Test]
        public async Task AddAsync_ShouldAddCategorySuccessfully()
        {
            // Arrange
            var createDto = new CreateCategoryRequestDto { Name = "New Category" };
            var categoryEntity = new Category { Id = 1, Name = "New Category" };
            var responseDto = new CategoryResponseDto(1, "New Category");

            _businessRulesMock.Setup(b => b.CategoryNameMustBeUnique(createDto.Name)).Returns(Task.CompletedTask);
            _mapperMock.Setup(m => m.Map<Category>(createDto)).Returns(categoryEntity);
            _categoryRepositoryMock.Setup(r => r.AddAsync(categoryEntity)).ReturnsAsync(categoryEntity);
            _mapperMock.Setup(m => m.Map<CategoryResponseDto>(categoryEntity)).Returns(responseDto);

            // Act
            var result = await _categoryService.AddAsync(createDto);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(responseDto.Name, result.Name);
            _businessRulesMock.Verify(b => b.CategoryNameMustBeUnique(createDto.Name), Times.Once);
            _categoryRepositoryMock.Verify(r => r.AddAsync(categoryEntity), Times.Once);
            _mapperMock.Verify(m => m.Map<CategoryResponseDto>(categoryEntity), Times.Once);
        }

        [Test]
        public async Task DeleteAsync_ShouldDeleteCategorySuccessfully()
        {
            // Arrange
            var categoryId = 1;
            var categoryEntity = new Category { Id = categoryId, Name = "Test Category" };

            _businessRulesMock.Setup(b => b.CategoryMustExist(categoryId)).ReturnsAsync(categoryEntity);
            _categoryRepositoryMock.Setup(r => r.DeleteAsync(categoryEntity, false)).ReturnsAsync(categoryEntity);

            // Act
            var result = await _categoryService.DeleteAsync(categoryId, false);

            // Assert
            Assert.AreEqual(CategoryMessages.CategoryDeleted, result);
            _businessRulesMock.Verify(b => b.CategoryMustExist(categoryId), Times.Once);
            _categoryRepositoryMock.Verify(r => r.DeleteAsync(categoryEntity, false), Times.Once);
        }


        [Test]
        public async Task UpdateAsync_ShouldUpdateCategorySuccessfully()
        {
            // Arrange
            int categoryId = 1;
            var updateDto = new UpdateCategoryRequestDto(categoryId, "Updated Category");
            var existingCategory = new Category { Id = categoryId, Name = "Old Category" };
            var updatedCategory = new Category { Id = categoryId, Name = "Updated Category" };
            var responseDto = new CategoryResponseDto(categoryId, "Updated Category");

            _businessRulesMock
                .Setup(b => b.CategoryMustExist(categoryId))
                .ReturnsAsync(existingCategory);

            _mapperMock
                .Setup(m => m.Map(It.IsAny<UpdateCategoryRequestDto>(), It.IsAny<Category>()))
                .Callback<UpdateCategoryRequestDto, Category>((dto, entity) =>
                {
                    entity.Name = dto.Name; // Mapper'ýn yaptýðý iþlem simule ediliyor.
                });

            _categoryRepositoryMock
                .Setup(r => r.UpdateAsync(It.IsAny<Category>()))
                .ReturnsAsync((Category c) => c);

            _mapperMock
                .Setup(m => m.Map<CategoryResponseDto>(It.IsAny<Category>()))
                .Returns((Category c) => new CategoryResponseDto(c.Id, c.Name));

            // Act
            Console.WriteLine("Calling UpdateAsync on CategoryService...");
            var result = await _categoryService.UpdateAsync(categoryId, updateDto);

            // Assert
            Assert.NotNull(result, "Result should not be null.");
            Assert.AreEqual("Updated Category", result.Name, "Name mismatch.");
            _businessRulesMock.Verify(b => b.CategoryMustExist(categoryId), Times.Once);
            _mapperMock.Verify(m => m.Map(updateDto, existingCategory), Times.Once);
            _categoryRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Category>()), Times.Once);
            _mapperMock.Verify(m => m.Map<CategoryResponseDto>(It.IsAny<Category>()), Times.Once);
        }









        [Test]
        public async Task GetByIdAsync_ShouldReturnCategory()
        {
            // Arrange
            var categoryId = 1;
            var categoryEntity = new Category { Id = categoryId, Name = "Test Category" };
            var responseDto = new CategoryResponseDto(categoryId, "Test Category");

            _businessRulesMock.Setup(b => b.CategoryMustExist(categoryId)).ReturnsAsync(categoryEntity);
            _mapperMock.Setup(m => m.Map<CategoryResponseDto>(categoryEntity)).Returns(responseDto);

            // Act
            var result = await _categoryService.GetByIdAsync(categoryId);

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(responseDto.Name, result.Name);
            _businessRulesMock.Verify(b => b.CategoryMustExist(categoryId), Times.Once);
            _mapperMock.Verify(m => m.Map<CategoryResponseDto>(categoryEntity), Times.Once);
        }

        [Test]
        public async Task GetListAsync_ShouldReturnListOfCategories()
        {
            // Arrange
                var mockCategories = new List<Category>
        {
            new Category { Id = 1, Name = "Category 1" },
            new Category { Id = 2, Name = "Category 2" }
        };

            _categoryRepositoryMock.Setup(repo => repo.GetListAsync(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>())
            ).ReturnsAsync(mockCategories);

            _mapperMock.Setup(mapper => mapper.Map<List<CategoryResponseDto>>(mockCategories))
                       .Returns(new List<CategoryResponseDto>
                       {
                   new CategoryResponseDto(1, "Category 1"),
                   new CategoryResponseDto(2, "Category 2")
                       });

            // Act
            var result = await _categoryService.GetListAsync();

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Category 1", result[0].Name);
            Assert.AreEqual("Category 2", result[1].Name);

            // Verify
            _categoryRepositoryMock.Verify(repo => repo.GetListAsync(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<Func<IQueryable<Category>, IOrderedQueryable<Category>>>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
