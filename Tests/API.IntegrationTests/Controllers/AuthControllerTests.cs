using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using API.Contracts.Responses.Common;
namespace API.IntegrationTests.Controllers
{
    public class AuthControllerTests : TestBase
    {
        [Fact]
        public async Task NullRequestBody_ReturnsBadRequest()
        {
            // Arrange: send a JSON null literal
            var content = new StringContent("null", Encoding.UTF8, "application/json");
            
            // Act
            var response = await _client.PostAsync("/api/auth/register", content);

            // Assert: Verify 400 BadRequest status
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var responseJson = await response.Content.ReadAsStringAsync();
            Assert.Contains("\"status\":400", responseJson);  // Verify numeric status
            Assert.Contains("Invalid request format", responseJson);
        }
    }

}
