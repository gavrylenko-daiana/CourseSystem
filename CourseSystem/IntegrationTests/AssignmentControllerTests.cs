using System.Net;
using Core.Enums;
using Core.Models;

namespace IntegrationTests
{
    [Collection("ControllerCollection")]
    public class AssignmentControllerTests
    {
        private readonly ControllerFixture _fixture;

        public AssignmentControllerTests(ControllerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Craete_ValidAssignment_302Redirect()
        {
            //Arrange
            using var client = _fixture.CreateClient();
            var course = await _fixture.CreateCourse("Test", _fixture.Admin.User);
            var group = await _fixture.CreateGroup(course, "TestGroup", DateTime.Now, DateTime.Now.AddDays(2), GroupAccess.InProgress, _fixture.Admin.User);

            //Act
            await client.PostAsync($"Account/Login?emailaddress={_fixture.Admin.User.Email!}&password={_fixture.Admin.Password}", null);

            var response = await client.PostAsync($"Assignment/Create?name=test&enddate={group.EndDate}&groupid={group.Id}&startdate={DateTime.Today.AddDays(1)}", null);

            //Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal($"/Assignment?groupId={group.Id}", response.Headers.Location!.OriginalString);
        }

        [Fact]
        public async Task Create_InValidAssignmentTime_CreateView()
        {
            //Arrange
            using var client = _fixture.CreateClient();
            var course = await _fixture.CreateCourse("Test", _fixture.Admin.User);
            var group = await _fixture.CreateGroup(course, "TestGroup", DateTime.Now, DateTime.Now.AddDays(2), GroupAccess.InProgress, _fixture.Admin.User);

            //Act
            await client.PostAsync($"Account/Login?emailaddress={_fixture.Admin.User.Email!}&password={_fixture.Admin.Password}", null);

            var response = await client.PostAsync($"Assignment/Create?name=test&enddate={group.EndDate}&groupid={group.Id}&startdate={DateTime.Today.AddDays(5)}", null);

            //Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DeleteAssignment_ValidAssignmentId_302Redirect()
        {
            //Arrange
            using var client = _fixture.CreateClient();
            var course = await _fixture.CreateCourse("Test", _fixture.Admin.User);
            var group = await _fixture.CreateGroup(course, "TestGroup", DateTime.Now, DateTime.Now.AddDays(2), GroupAccess.InProgress, _fixture.Admin.User);
            var assignemnt = await _fixture.CreateAssignment(group, "Test", DateTime.Today, DateTime.Now.AddDays(2), AssignmentAccess.InProgress);

            //Act
            await client.PostAsync($"Account/Login?emailaddress={_fixture.Admin.User.Email!}&password={_fixture.Admin.Password}", null);

            var response = await client.PostAsync($"Assignment/DeleteAssignment?id={assignemnt.Id}", null);

            //Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal("/Group", response.Headers.Location!.OriginalString);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public async Task DeleteAssignment_InValidAssignmentId_302Redirect(int assignemntId)
        {
            //Arrange
            using var client = _fixture.CreateClient();

            //Act
            await client.PostAsync($"Account/Login?emailaddress={_fixture.Admin.User.Email!}&password={_fixture.Admin.Password}", null);

            var response = await client.PostAsync($"Assignment/DeleteAssignment?id={assignemntId}", null);

            //Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal($"/Assignment/Delete?assignmentId={assignemntId}", response.Headers.Location!.OriginalString);
        }

    }
}
