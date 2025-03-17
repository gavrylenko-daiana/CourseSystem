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

public class UserGroupServiceTests
{
    [Fact]
    public async Task CreateUserGroups_ValidUserGroups_ReturnsTrue()
    {
        var userGroupsService = GetUserGroupService();
        
        var userGroups = new UserGroups
        {
            Progress = 0.0, 
            AppUserId = "user123",
            GroupId = 1
        }; 
            
        var result = await userGroupsService.CreateUserGroups(userGroups);
            
        Assert.True(result.IsSuccessful);
    }
    
    [Theory]
    [InlineData(null, "user123", 1)]
    [InlineData(0.0, null, 1)]
    public async Task CreateUserGroups_InvalidUserGroups_ReturnsFalse(double? progress, string appUserId, int groupId)
    {
        var userGroupsService = GetUserGroupService();
        
        var invalidUserGroups = new UserGroups
        {
            Progress = progress,
            AppUserId = appUserId,
            GroupId = groupId
        }; 
        
        var result = await userGroupsService.CreateUserGroups(invalidUserGroups);
        
        Assert.False(result.IsSuccessful);
    }

    [Fact]
    public async Task UpdateProgressInUserGroups_ExistentUserGroups_ReturnsTrue()
    {
        var userGroupsService = GetUserGroupService();
            
        var userGroups = new UserGroups
        {
            Progress = 0.0,
            AppUserId = "user123",
            GroupId = 1
        };
            
        await userGroupsService.CreateUserGroups(userGroups);
            
        var updatedProgress = 0.5;
        var result = await userGroupsService.UpdateProgressInUserGroups(userGroups, updatedProgress);

        Assert.True(result.IsSuccessful);
    }

    [Theory]
    [InlineData(0.0, "user123", 9999)]
    [InlineData(0.0, "12345", 1)]
    public async Task UpdateProgressInUserGroups_NonExistentUserGroups_ReturnsFalse(double? progress, string appUserId, int groupId)
    {
        var userGroupsService = GetUserGroupService();
            
        var nonExistentUserGroups = new UserGroups
        {
            Progress = progress,
            AppUserId = appUserId,
            GroupId = groupId
        }; 
            
        var result = await userGroupsService.UpdateProgressInUserGroups(nonExistentUserGroups, 0.5);

        Assert.False(result.IsSuccessful);
    }
    
    private IUserGroupService GetUserGroupService()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: "test_database")
            .Options;

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        
        using (var context = new ApplicationContext(options))
        {
            var unitOfWork = new UnitOfWork(context, loggerFactoryMock.Object);
            var loggerMock = new Mock<ILogger<UserGroupService>>();
            
            var userGroupsService = new UserGroupService(unitOfWork, loggerMock.Object);
            
            return userGroupsService;
        }
    }
}