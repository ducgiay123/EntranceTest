using AuthenticationTest.Controller.src;
using AuthenticationTest.Service.src.Abstracts;
using AuthenticationTest.Service.src.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Xunit;
using System.Diagnostics;

namespace AuthenticationTestDotNet.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly Mock<ILogger<AuthController>> _loggerMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _loggerMock = new Mock<ILogger<AuthController>>();
            _controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
            SetupControllerContext();
        }

        private void SetupControllerContext()
        {
            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "user@example.com") };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            var httpContext = new DefaultHttpContext { User = user };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        #region SignUp Tests
        [Fact]
        public async Task SignUp_ValidRequest_ReturnsCreated()
        {
            // Arrange
            var request = new SignUpRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "SecurePass123"
            };
            var response = new SignUpResponseDto
            {
                Id = 0,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com"
            };
            _authServiceMock
            .Setup(s => s.SignUpAsync(It.Is<SignUpRequestDto>(r =>
                r.Email == request.Email &&
                r.FirstName == request.FirstName &&
                r.LastName == request.LastName &&
                r.Password == request.Password)))
            .ReturnsAsync(response);


            // Act
            var result = await _controller.SignUp(request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
            var returnValue = createdResult.Value; // Check actual type
            Assert.IsType<SignUpResponseDto>(returnValue); // Verify it's the correct type
            Assert.Equal("john.doe@example.com", ((SignUpResponseDto)returnValue).Email);
        }

        [Fact]
        public async Task SignUp_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.SignUp(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var errorMessage = badRequestResult.Value as string;
            Assert.Equal("Invalid request body.", errorMessage);
        }


        [Fact]
        public async Task SignUp_EmailTaken_ReturnsConflict()
        {
            // Arrange
            var request = new SignUpRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "SecurePass123"
            };
            _authServiceMock.Setup(s => s.SignUpAsync(request))
                .ThrowsAsync(new InvalidOperationException("Email is already registered."));

            // Act
            var result = await _controller.SignUp(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
            var errorMessage = conflictResult.Value; // Assume string error
            Assert.Equal("Email is already registered.", errorMessage);
        }

        [Fact]
        public async Task SignUp_InternalError_ReturnsInternalServerError()
        {
            // Arrange
            var request = new SignUpRequestDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Password = "SecurePass123"
            };
            _authServiceMock.Setup(s => s.SignUpAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.SignUp(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var errorMessage = statusCodeResult.Value as string; // Assume string error
            Assert.Equal("Internal server error.", errorMessage);
        }
        #endregion

        #region Login Tests
        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@example.com",
                Password = "SecurePass123"
            };
            var response = new LoginResponseDto
            {
                User = new UserDto { Email = "user@example.com", DisplayName = "User" },
                Token = "jwt-token",
                RefreshToken = "refresh-token"
            };
            _authServiceMock.Setup(s => s.LoginAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<LoginResponseDto>(okResult.Value);
            Assert.Equal("jwt-token", returnValue.Token);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@example.com",
                Password = "WrongPass"
            };
            _authServiceMock.Setup(s => s.LoginAsync(request))
                .ThrowsAsync(new UnauthorizedAccessException("Invalid email or password."));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            var errorMessage = unauthorizedResult.Value; // Assume string error
            Assert.Equal("Invalid email or password.", errorMessage);
        }

        [Fact]
        public async Task Login_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Login(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var errorMessage = badRequestResult.Value as string; // Assume string error
            Assert.Equal("Invalid request body.", errorMessage);
        }

        [Fact]
        public async Task Login_InternalError_ReturnsInternalServerError()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@example.com",
                Password = "SecurePass123"
            };
            _authServiceMock.Setup(s => s.LoginAsync(request))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var errorMessage = statusCodeResult.Value; // Assume string error
            Assert.Equal("Internal server error.", errorMessage);
        }
        #endregion

        #region Logout Tests
        [Fact]
        public async Task Logout_ValidToken_ReturnsOk()
        {
            // Arrange
            var request = new LogoutRequestDto { RefreshToken = "valid-refresh-token" };
            _authServiceMock.Setup(s => s.LogoutAsync("user@example.com", request.RefreshToken))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Logout(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
            var message = okResult.Value; // Assume string message
            Assert.Equal("Logged out successfully.", message);
        }

        [Fact]
        public async Task Logout_InvalidToken_ReturnsBadRequest()
        {
            // Arrange
            var request = new LogoutRequestDto { RefreshToken = "invalid-refresh-token" };
            _authServiceMock.Setup(s => s.LogoutAsync("user@example.com", request.RefreshToken))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Logout(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var errorMessage = badRequestResult.Value; // Assume string error
            Assert.Equal("Invalid or expired refresh token.", errorMessage);
        }

        [Fact]
        public async Task Logout_NoClaims_ReturnsUnauthorized()
        {
            // Arrange
            var request = new LogoutRequestDto { RefreshToken = "valid-refresh-token" };
            var controller = new AuthController(_authServiceMock.Object, _loggerMock.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() } // No claims
            };

            // Act
            var result = await controller.Logout(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
            var errorMessage = unauthorizedResult.Value; // Assume string error
            Assert.Equal("Invalid user authentication.", errorMessage);
        }

        [Fact]
        public async Task Logout_InternalError_ReturnsInternalServerError()
        {
            // Arrange
            var request = new LogoutRequestDto { RefreshToken = "error-refresh-token" };
            _authServiceMock.Setup(s => s.LogoutAsync("user@example.com", request.RefreshToken))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Logout(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var errorMessage = statusCodeResult.Value as string; // Assume string error
            Assert.Equal("Internal server error.", errorMessage);
        }
        #endregion

        #region Refresh Tests
        [Fact]
        public async Task Refresh_ValidRefreshToken_ReturnsOkWithNewTokens()
        {
            // Arrange
            var request = new RefreshTokenRequestDto { RefreshToken = "valid-refresh-token" };
            var response = new RefreshTokenResponseDto
            {
                Token = "new-jwt-token",
                RefreshToken = "new-refresh-token"
            };
            _authServiceMock.Setup(s => s.RefreshTokenAsync(request.RefreshToken))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Refresh(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<RefreshTokenResponseDto>(okResult.Value);
            Assert.Equal("new-jwt-token", returnValue.Token);
            Assert.Equal("new-refresh-token", returnValue.RefreshToken);
        }

        [Fact]
        public async Task Refresh_NullRequest_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Refresh(null);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            var errorMessage = badRequestResult.Value as string; // Assume string error
            Assert.Equal("Refresh token is required.", errorMessage);
        }

        [Fact]
        public async Task Refresh_InvalidRefreshToken_ReturnsNotFound()
        {
            // Arrange
            var request = new RefreshTokenRequestDto { RefreshToken = "invalid-refresh-token" };
            _authServiceMock.Setup(s => s.RefreshTokenAsync(request.RefreshToken))
                .ThrowsAsync(new KeyNotFoundException("Invalid or expired refresh token."));

            // Act
            var result = await _controller.Refresh(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFoundResult.StatusCode);
            var errorMessage = notFoundResult.Value as string; // Assume string error
            Assert.Equal("Invalid or expired refresh token.", errorMessage);
        }

        [Fact]
        public async Task Refresh_InternalError_ReturnsInternalServerError()
        {
            // Arrange
            var request = new RefreshTokenRequestDto { RefreshToken = "error-refresh-token" };
            _authServiceMock.Setup(s => s.RefreshTokenAsync(request.RefreshToken))
                .ThrowsAsync(new Exception("Internal error"));

            // Act
            var result = await _controller.Refresh(request);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            var errorMessage = statusCodeResult.Value as string; // Assume string error
            Assert.Equal("Internal server error.", errorMessage);
        }
        #endregion
    }
}