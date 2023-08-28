using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Net.Http.Json;
using UI.ViewModels;

namespace IntegrationTests
{
    [Collection("ControllerCollection")]
    public class AccountControllerTests
    {
        private readonly ControllerFixture _fixture;

        public AccountControllerTests(ControllerFixture fixture)
        {
            _fixture = fixture;                
        }

        [Fact]
        public async Task Get_Course_ValidUser_200OK()
        {
            using var client = _fixture.CreateClient();
            //var jsonContent = JsonContent.Create(new LoginViewModel
            //{
            //    EmailAddress = _fixture.Admin.User.Email,
            //    Password = _fixture.Admin.Password
            //});
            var multipartContent = new MultipartFormDataContent();
            multipartContent.Add(new StringContent("emailaddress"), _fixture.Admin.User.Email);
            multipartContent.Add(new StringContent("password"), _fixture.Admin.Password);
            //await client.PostAsync($"Account/Login?emailaddress={_fixture.Admin.User.Email}&password={_fixture.Admin.Password}", null);
            await client.PostAsync("Account/Login", multipartContent);

            var response = await client.GetAsync("Course/Index");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Get_Course_InvalidUser_302Redirect()
        {
            using var client = _fixture.CreateClient();

            await client.PostAsync("Account/Login?emailaddress=alexeysafonov01@gmail.com&password=!!Dragon777", null);

            var response = await client.GetAsync("Course/Index");
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
    }
}
