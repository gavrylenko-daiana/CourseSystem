using System.Linq.Expressions;
using BLL.Interfaces;
using BLL.Services;
using Core.Enums;
using Core.Models;
using DAL;
using DAL.Interfaces;
using DAL.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace UnitTests;

public class CourseServiceTests
{
    [Fact]
    public async Task CreateCourse_NullCourse_ReturnsFalse()
    {
        var mockFile = new Mock<IFormFile>();
        var mockUser = new Mock<AppUser>();

        var courseService = GetCourseService();
        
        Course invalidCourse = null;

        var result = await courseService.CreateCourse(invalidCourse, mockUser.Object, mockFile.Object);
        
        Assert.False(result.IsSuccessful);
        Assert.Equal($"course not found", result.Message);
    }
    
    [Fact]
    public async Task CreateCourse_NullUser_ReturnsFalse()
    {
        var mockFile = new Mock<IFormFile>();
        var mockCourse = new Mock<Course>();

        var courseService = GetCourseService();
        
        AppUser invalidUser = null;

        var result = await courseService.CreateCourse(mockCourse.Object, invalidUser, mockFile.Object);
        
        Assert.False(result.IsSuccessful);
        Assert.Equal($"currentUser not found", result.Message);
    }

    private ICourseService GetCourseService(UserManager<AppUser> userManager = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(databaseName: "test_database")
            .Options;

        var loggerFactoryMock = new Mock<ILoggerFactory>();
        
        using (var context = new ApplicationContext(options))
        {
            var unitOfWork = new UnitOfWork(context, loggerFactoryMock.Object);
            var userCourseServiceMock = new Mock<IUserCourseService>();
            var educationMaterialServiceMock = new Mock<IEducationMaterialService>();
            var groupServiceMock = new Mock<IGroupService>();
            var dropboxServiceMock = new Mock<IDropboxService>();
            
            if (userManager == null)
            {
                userManager = new Mock<UserManager<AppUser>>(
                    Mock.Of<IUserStore<AppUser>>(), Mock.Of<IOptions<IdentityOptions>>(),
                    Mock.Of<IPasswordHasher<AppUser>>(), new IUserValidator<AppUser>[0],
                    new IPasswordValidator<AppUser>[0], Mock.Of<ILookupNormalizer>(),
                    Mock.Of<IdentityErrorDescriber>(), Mock.Of<IServiceProvider>(), Mock.Of<ILogger<UserManager<AppUser>>>()).Object;
            }

            var courseService = new CourseService(unitOfWork, userCourseServiceMock.Object, educationMaterialServiceMock.Object,
                groupServiceMock.Object, dropboxServiceMock.Object, userManager);
            
            return courseService;
        }
    }
}