using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using API.Contracts.Responses.Common;
using API.Contracts.Requests.Auth;
using API.Common.Enums;

namespace API.IntegrationTests.Controllers
{
    public class AuthControllerTests : TestBase
    {
        [Fact]
        public async Task NullRequestBody_ReturnsBadRequest()
        {
            // Arrange
            var content = new StringContent("null", Encoding.UTF8, "application/json");
            
            // Act
            var response = await _client.PostAsync("/api/auth/register", content);

            // Assert: Verify 400 BadRequest status
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseJson = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"status\":400", responseJson);  // Verify numeric status
            Assert.Contains("Invalid request format", responseJson);
        }

        [Fact]
        public async Task InvalidEmailFormat_ReturnsValidationError()
        {
            // Arrange
            var invalidRequest = new RegisterRequest
            {
                Username = "testuser",
                FirstName = "Test",
                LastName = "User",
                Email = "invalid-email",
                BirthDate = new DateTime(1990, 1, 1),
                Gender = UserGender.Male,
                Password = "Password123!"
            };

            // Act
            var response = await PostAsync("/api/auth/register", invalidRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            
            var responseJson = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"status\":\"INVALID_EMAIL\"", responseJson);
            Assert.Contains("\"message\":\"Invalid email format\"", responseJson);
        }
    }

}
