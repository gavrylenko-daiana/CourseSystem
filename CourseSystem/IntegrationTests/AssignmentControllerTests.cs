using System.Net;

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
            //create course
            //create group -> assignment
        }

    }
}
