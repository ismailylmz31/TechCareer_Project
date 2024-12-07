using AutoMapper;
using Moq;
using System.Linq.Expressions;
using TechCareer.DataAccess.Repositories.Abstracts;
using TechCareer.Models.Dtos.Events;
using TechCareer.Models.Entities;
using TechCareer.Service.Concretes;
using TechCareer.Service.Constants;
using TechCareer.Service.Rules;

[TestFixture]
public class EventServiceTests
{
    private Mock<IEventRepository> _eventRepositoryMock;
    private Mock<IMapper> _mapperMock;
    private Mock<EventBusinessRules> _businessRulesMock;
    private EventService _eventService;

    [SetUp]
    public void SetUp()
    {
        _eventRepositoryMock = new Mock<IEventRepository>();
        _mapperMock = new Mock<IMapper>();
        _businessRulesMock = new Mock<EventBusinessRules>();
        _eventService = new EventService(
            _eventRepositoryMock.Object,
            _mapperMock.Object,
            _businessRulesMock.Object
        );
    }

    [Test]
    public async Task AddAsync_ShouldAddEventSuccessfully()
    {
        // Arrange
        var createDto = new CreateEventRequestDto(
            "New Event",
            "Description",
            "image.jpg",
            "Participation Text",
            1
        );

        var eventEntity = new Event
        {
            Id = Guid.NewGuid(),
            Title = "New Event",
            Description = "Description",
            ImageUrl = "image.jpg",
            ParticipationText = "Participation Text"
        };

        var responseDto = new EventResponseDto
        {
            Id = eventEntity.Id,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            ImageUrl = eventEntity.ImageUrl,
            ParticipationText = eventEntity.ParticipationText,
            CategoryName = "Category"
        };

        _businessRulesMock
            .Setup(b => b.EventTitleMustBeUnique(createDto.Title))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<Event>(createDto))
            .Returns(eventEntity);

        _eventRepositoryMock
            .Setup(r => r.AddAsync(eventEntity))
            .ReturnsAsync(eventEntity);

        _mapperMock
            .Setup(m => m.Map<EventResponseDto>(eventEntity))
            .Returns(responseDto);

        // Act
        var result = await _eventService.AddAsync(createDto);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(responseDto.Title, result.Title);
        _businessRulesMock.Verify(b => b.EventTitleMustBeUnique(createDto.Title), Times.Once);
        _eventRepositoryMock.Verify(r => r.AddAsync(eventEntity), Times.Once);
        _mapperMock.Verify(m => m.Map<EventResponseDto>(eventEntity), Times.Once);
    }

    [Test]
    public async Task GetByIdAsync_ShouldReturnEvent()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new Event
        {
            Id = eventId,
            Title = "Test Event",
            Description = "Test Description",
            ImageUrl = "TestImage.jpg",
            ParticipationText = "Test Participation Text"
        };

        var responseDto = new EventResponseDto
        {
            Id = eventId,
            Title = eventEntity.Title,
            Description = eventEntity.Description,
            ImageUrl = eventEntity.ImageUrl,
            ParticipationText = eventEntity.ParticipationText,
            CategoryName = "Test Category"
        };

        _businessRulesMock
            .Setup(b => b.EventMustExist(eventId))
            .ReturnsAsync(eventEntity);

        _mapperMock
            .Setup(m => m.Map<EventResponseDto>(eventEntity))
            .Returns(responseDto);

        // Act
        var result = await _eventService.GetByIdAsync(eventId);

        // Assert
        Assert.NotNull(result, "Result should not be null.");
        Assert.AreEqual(responseDto.Title, result.Title, "Title mismatch.");
        Assert.AreEqual(responseDto.Description, result.Description, "Description mismatch.");
        Assert.AreEqual(responseDto.ImageUrl, result.ImageUrl, "Image URL mismatch.");
        Assert.AreEqual(responseDto.ParticipationText, result.ParticipationText, "Participation Text mismatch.");
        Assert.AreEqual(responseDto.CategoryName, result.CategoryName, "Category Name mismatch.");

        _businessRulesMock.Verify(b => b.EventMustExist(eventId), Times.Once);
        _mapperMock.Verify(m => m.Map<EventResponseDto>(eventEntity), Times.Once);
    }




    [Test]
    public async Task DeleteAsync_ShouldDeleteEventSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new Event { Id = eventId, Title = "Event Title" };

        _businessRulesMock
            .Setup(b => b.EventMustExist(eventId))
            .ReturnsAsync(eventEntity);

        _eventRepositoryMock
            .Setup(r => r.DeleteAsync(eventEntity, false))
            .ReturnsAsync(eventEntity);

        // Act
        var result = await _eventService.DeleteAsync(eventId, false);

        // Assert
        Assert.NotNull(result);
        Assert.AreEqual(EventMessages.EventDeleted, result);
        _businessRulesMock.Verify(b => b.EventMustExist(eventId), Times.Once);
        _eventRepositoryMock.Verify(r => r.DeleteAsync(eventEntity, false), Times.Once);
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateEventSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var updateDto = new UpdateEventRequestDto(
            eventId,
            "Updated Event",
            "Updated Description",
            "UpdatedImage.jpg",
            "Updated Participation Text"
        );

        var existingEvent = new Event
        {
            Id = eventId,
            Title = "Old Event",
            Description = "Old Description",
            ImageUrl = "OldImage.jpg",
            ParticipationText = "Old Participation Text"
        };

        var updatedEvent = new Event
        {
            Id = eventId,
            Title = "Updated Event",
            Description = "Updated Description",
            ImageUrl = "UpdatedImage.jpg",
            ParticipationText = "Updated Participation Text"
        };

        var responseDto = new EventResponseDto
        {
            Id = eventId,
            Title = "Updated Event",
            Description = "Updated Description",
            ImageUrl = "UpdatedImage.jpg",
            ParticipationText = "Updated Participation Text",
            CategoryName = "Updated Category"
        };

        _businessRulesMock
            .Setup(b => b.EventMustExist(eventId))
            .ReturnsAsync(existingEvent);

        _mapperMock
            .Setup(m => m.Map(It.IsAny<UpdateEventRequestDto>(), It.IsAny<Event>()))
            .Callback<UpdateEventRequestDto, Event>((dto, entity) =>
            {
                entity.Title = dto.Title;
                entity.Description = dto.Description;
                entity.ImageUrl = dto.ImageUrl;
                entity.ParticipationText = dto.ParticipationText;
            });

        _eventRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Event>()))
            .ReturnsAsync((Event e) => e);

        _mapperMock
            .Setup(m => m.Map<EventResponseDto>(It.IsAny<Event>()))
            .Returns((Event e) => new EventResponseDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                ImageUrl = e.ImageUrl,
                ParticipationText = e.ParticipationText,
                CategoryName = "Updated Category"
            });

        // Act
        Console.WriteLine("Calling UpdateAsync on EventService...");
        var result = await _eventService.UpdateAsync(eventId, updateDto);

        // Assert
        Assert.NotNull(result, "Result should not be null.");
        Assert.AreEqual("Updated Event", result.Title, "Title mismatch.");
        Assert.AreEqual("Updated Description", result.Description, "Description mismatch.");
        _businessRulesMock.Verify(b => b.EventMustExist(eventId), Times.Once);
        _mapperMock.Verify(m => m.Map(updateDto, existingEvent), Times.Once);
        _eventRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Event>()), Times.Once);
        _mapperMock.Verify(m => m.Map<EventResponseDto>(It.IsAny<Event>()), Times.Once);
    }


    [Test]
    public async Task GetListAsync_ShouldReturnListOfEvents()
    {
        // Arrange
        var mockEvents = new List<Event>
    {
        new Event { Id = Guid.NewGuid(), Title = "Event 1", Description = "Description 1", ImageUrl = "Image1.jpg", ParticipationText = "Participation 1" },
        new Event { Id = Guid.NewGuid(), Title = "Event 2", Description = "Description 2", ImageUrl = "Image2.jpg", ParticipationText = "Participation 2" }
    };

        _eventRepositoryMock.Setup(repo => repo.GetListAsync(
            It.IsAny<Expression<Func<Event, bool>>>(),
            It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>())
        ).ReturnsAsync(mockEvents);

        _mapperMock.Setup(mapper => mapper.Map<List<EventResponseDto>>(mockEvents))
                   .Returns(new List<EventResponseDto>
                   {
                   new EventResponseDto
                   {
                       Id = mockEvents[0].Id,
                       Title = mockEvents[0].Title,
                       Description = mockEvents[0].Description,
                       ImageUrl = mockEvents[0].ImageUrl,
                       ParticipationText = mockEvents[0].ParticipationText,
                       CategoryName = "Category 1"
                   },
                   new EventResponseDto
                   {
                       Id = mockEvents[1].Id,
                       Title = mockEvents[1].Title,
                       Description = mockEvents[1].Description,
                       ImageUrl = mockEvents[1].ImageUrl,
                       ParticipationText = mockEvents[1].ParticipationText,
                       CategoryName = "Category 2"
                   }
                   });

        // Act
        var result = await _eventService.GetListAsync();

        // Assert
        Assert.NotNull(result, "Result should not be null.");
        Assert.AreEqual(2, result.Count, "Count mismatch.");
        Assert.AreEqual("Event 1", result[0].Title, "First event title mismatch.");
        Assert.AreEqual("Event 2", result[1].Title, "Second event title mismatch.");

        // Verify
        _eventRepositoryMock.Verify(repo => repo.GetListAsync(
            It.IsAny<Expression<Func<Event, bool>>>(),
            It.IsAny<Func<IQueryable<Event>, IOrderedQueryable<Event>>>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<bool>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<List<EventResponseDto>>(mockEvents), Times.Once);
    }

}
