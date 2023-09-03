using BLL.Interfaces;
using Core.Enums;
using Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace IntegrationTests;

[CollectionDefinition("ControllerCollection")]
    public class ControllerFixtureCollection : ICollectionFixture<ControllerFixture>
    {

    }

    public class ControllerFixture : IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly List<HttpClient> _clients = new();
        
        public (AppUser User, string Password) Admin { get; private set; }

        public ControllerFixture()
        {
            _factory = new CustomWebApplicationFactory();
        }

        public HttpClient CreateClient()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            
            _clients.Add(client);
            
            return client;
        }

        public async Task<AppUser> CreateUser(string email, string password, AppUserRoles role = AppUserRoles.Student)
        {
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            
            var user = new AppUser
            {
                Email = email,
                Role = role,
                FirstName = "testname",
                LastName = "testlastname",
                UserName = "testname_testlastname"
            };
            
            await userManager.CreateAsync(user, password);
            var createdUser = await userManager.FindByEmailAsync(email);
            
            return createdUser!;
        }
        
        public async Task<Course> CreateCourse(string name, AppUser currentUser)
        {
            using var scope = _factory.Services.CreateScope();
            var courseService = scope.ServiceProvider.GetRequiredService<ICourseService>();
            
            var course = new Course
            {
                Name = name
            };

            await courseService.CreateCourse(course, currentUser);

            return course;
        }
        
        public void Dispose()
        {
            _factory?.Dispose();
            
            foreach (var client in _clients)
            {
                client?.Dispose();
            }
        }

        public async Task InitializeAsync()
        {
            var adminPassword = "!!Dragon77";
            
            Admin = (await CreateUser("q@gmail.com", adminPassword, AppUserRoles.Admin), adminPassword);
        }

        public Task DisposeAsync()
        {
            Dispose();
            return Task.CompletedTask;
        }
    }