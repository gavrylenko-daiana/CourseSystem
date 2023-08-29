using BLL.Interfaces;
using BLL.Services;
using Castle.DynamicProxy;
using Core.Models;
using DAL;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace UnitTests;

public class GroupServiceTests
{
    [Fact]
    public async Task CreateGroup_NullGroup_ReturnsFailureResult()
    {
        var mockUser = new Mock<AppUser>();
    
        var groupService = GetGroupService();
    
        var result = await groupService.CreateGroup(null, mockUser.Object);

        Assert.False(result.IsSuccessful);
        Assert.Equal("group not found", result.Message);
    }
    
    [Fact]
    public async Task CreateGroup_NullUser_ReturnsFailureResult()
    {
        var mockGroup = new Mock<Group>();

        var groupService = GetGroupService();

        var result = await groupService.CreateGroup(mockGroup.Object, null);

        Assert.False(result.IsSuccessful);
        Assert.Equal("currentUser not found", result.Message);
    }
    
    [Fact]
    public async Task CreateGroup_InvalidDateRange_ReturnsFailureResult()
    {
        var group = new Group()
        {
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow
        };

        var mockUser = new Mock<AppUser>();

        var groupService = GetGroupService();

        var result = await groupService.CreateGroup(group, mockUser.Object);

        Assert.False(result.IsSuccessful);
        Assert.Equal("Start date must be less than end date", result.Message);
    }
    
    [Fact]
    public async Task CalculateStudentProgressInGroup_NoCompletedAssignments_ReturnsZeroProgress()
    {
        var mockGroup = new Mock<Group>();
        mockGroup.Setup(group => group.Assignments).Returns(new List<Assignment>());

        var mockUser = new Mock<AppUser>();
        mockUser.Setup(user => user.Id).Returns("testUserId");

        var groupService = GetGroupService();

        var result = await groupService.CalculateStudentProgressInGroup(mockGroup.Object, mockUser.Object);
        
        Assert.Equal("0.0", result);
    }
    
    [Fact]
    public async Task CalculateStudentProgressInGroup_SomeCompletedAssignments_ReturnsCorrectProgress()
    {
        var userAssignment1 = new UserAssignments
        {
            AppUserId = "testUserId",
            IsChecked = true,
        };

        var userAssignment2 = new UserAssignments
        {
            AppUserId = "testUserId",
            IsChecked = false,
        };

        var mockAssignment1 = new Mock<Assignment>();
        mockAssignment1.Setup(a => a.UserAssignments).Returns(new List<UserAssignments> { userAssignment1 });

        var mockAssignment2 = new Mock<Assignment>();
        mockAssignment2.Setup(a => a.UserAssignments).Returns(new List<UserAssignments> { userAssignment2 });

        var mockGroup = new Mock<Group>();
        mockGroup.Setup(group => group.Assignments).Returns(new List<Assignment> { mockAssignment1.Object, mockAssignment2.Object });

        var mockUser = new Mock<AppUser>();
        mockUser.Setup(user => user.Id).Returns("testUserId");

        var groupService = GetGroupService();
    
        var result = await groupService.CalculateStudentProgressInGroup(mockGroup.Object, mockUser.Object);
    
        Assert.Equal("50", result);
    }

    private IGroupService GetGroupService()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: "test_database")
            .Options;
        
        var loggerFactoryMock = new Mock<ILoggerFactory>();

        using (var context = new ApplicationContext(options))
        {
            var unitOfWork = new UnitOfWork(context, loggerFactoryMock.Object);
            var userGroupServiceMock = new Mock<IUserGroupService>();
            var educationMaterialServiceMock = new Mock<IEducationMaterialService>();
            var userServiceMock = new Mock<IUserService>();
            var loggerMock = new Mock<ILogger<GroupService>>();

            var groupService = new GroupService(unitOfWork, userGroupServiceMock.Object, educationMaterialServiceMock.Object, userServiceMock.Object, loggerMock.Object);

            return groupService;
        }
    }
}