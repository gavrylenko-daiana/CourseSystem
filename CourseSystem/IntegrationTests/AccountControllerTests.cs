using System.Net;

namespace IntegrationTests;

[Collection("ControllerCollection")]
public class AccountControllerTests
{
    private readonly ControllerFixture _fixture;

    public AccountControllerTests(ControllerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_ValidUser_302Redirect()
    {
        using var client = _fixture.CreateClient();

        var response = await client.PostAsync($"Account/Login?emailaddress={_fixture.Admin.User.Email!}&password={_fixture.Admin.Password}", null);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/User", response.Headers.Location!.OriginalString);
    }
    
    [Fact]
    public async Task Login_InvalidUser_200Ok()
    {
        using var client = _fixture.CreateClient();

        var multipartContent = new MultipartFormDataContent();

        multipartContent.Add(new StringContent("emailaddress"), "invalidEmail@gmail.com");
        multipartContent.Add(new StringContent("password"), "invalidPassword");

        var response = await client.PostAsync("Account/Login", multipartContent);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ResetPassword_InvalidNewPassword_200Ok()
    {
        using var client = _fixture.CreateClient();
        
        var response = await client.PostAsync($"Account/ResetPassword?email={_fixture.Admin.User.Email!}&newpassword={_fixture.Admin.Password}&confirmnewpassword={_fixture.Admin.Password + "1"}", null);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task ResetPassword_ValidChangePassword_302Redirect()
    {
        using var client = _fixture.CreateClient();
        
        var response = await client.PostAsync($"Account/ResetPassword?email={_fixture.Admin.User.Email!}&newpassword={_fixture.Admin.Password}&confirmnewpassword={_fixture.Admin.Password}", null);
        
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Account/Login", response.Headers.Location!.OriginalString);
    }
    
    [Theory]
    [InlineData("password111")]
    [InlineData("onlylowercase")]
    [InlineData("123456")]
    [InlineData("!@#%^&*")]
    [InlineData(" ")]
    public async Task Register_IncorrectPassword_200Ok(string password)
    {
        using var client = _fixture.CreateClient();

        var response = await client.PostAsync($"Account/Register?email={"newmail@gmail.com"}&password={password}&confirmpassword={password}&firstname={_fixture.Admin.User.FirstName}" +
                                              $"&lastname={_fixture.Admin.User.LastName}", null);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}