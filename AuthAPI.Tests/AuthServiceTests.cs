// using AuthAPI.Application.CQRS.Commands.RefreshToken;
// using AuthAPI.Application.CQRS.Commands.User.CreateUser;
// using AuthAPI.Application.CQRS.Queries.RefreshToken;
// using AuthAPI.Application.CQRS.Queries.User;
// using AuthAPI.Application.Dto;
// using AuthAPI.Application.Interface;
// using AuthAPI.Application.Services.Authentication;
// using AuthAPI.DAL.Data;
// using AuthAPI.Domain.Models;
// using AuthAPI.Shared.Exceptions;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.EntityFrameworkCore.Infrastructure;
// using Microsoft.EntityFrameworkCore.Storage;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Xunit;
//
// namespace AuthAPI.Tests
// {
//     public class AuthServiceTests
//     {
//         private readonly Mock<AuthDbContext> _contextMock;
//         private readonly Mock<ITokenService> _tokenServiceMock;
//         private readonly Mock<IPasswordService> _passwordServiceMock;
//         private readonly Mock<IAuditService> _auditServiceMock;
//         private readonly Mock<ILoginActivityService> _loginActivityServiceMock;
//         private readonly Mock<GetUserByEmailHandler> _getUserByEmailHandlerMock;
//         private readonly Mock<AddUserHandler> _addUserHandlerMock;
//         private readonly Mock<GetRefreshTokenHandler> _getRefreshTokenHandlerMock;
//         private readonly Mock<AddRefreshTokenHandler> _addRefreshTokenHandlerMock;
//         private readonly Mock<DeleteRefreshToken> _deleteRefreshTokenHandlerMock;
//         private readonly Mock<ExistingUserHandler> _existingUserHandlerMock;
//         private readonly Mock<RevokeTokensHandler> _revokeTokensHandlerMock;
//         private readonly Mock<ILogger<AuthService>> _loggerMock;
//         private readonly Mock<IMessagePublisher> _messagePublisherMock;
//         private readonly AuthService _authService;
//
//         public AuthServiceTests()
//         {
//             // Создаем DbContextOptions для in-memory базы данных
//             var options = new DbContextOptionsBuilder<AuthDbContext>()
//                 .UseInMemoryDatabase(databaseName: "TestDatabase")
//                 .Options;
//
//             // Инициализация моков для всех зависимостей
//             _contextMock = new Mock<AuthDbContext>(options);
//             _loggerMock = new Mock<ILogger<AuthService>>();
//             _tokenServiceMock = new Mock<ITokenService>();
//             _passwordServiceMock = new Mock<IPasswordService>();
//             _auditServiceMock = new Mock<IAuditService>();
//             _loginActivityServiceMock = new Mock<ILoginActivityService>();
//             _getUserByEmailHandlerMock = new Mock<GetUserByEmailHandler>(_contextMock.Object);
//             _addUserHandlerMock = new Mock<AddUserHandler>(_contextMock.Object, _tokenServiceMock.Object, _loggerMock.Object);
//             _getRefreshTokenHandlerMock = new Mock<GetRefreshTokenHandler>();
//             _addRefreshTokenHandlerMock = new Mock<AddRefreshTokenHandler>();
//             _deleteRefreshTokenHandlerMock = new Mock<DeleteRefreshToken>();
//             _existingUserHandlerMock = new Mock<ExistingUserHandler>();
//             _revokeTokensHandlerMock = new Mock<RevokeTokensHandler>();
//             _messagePublisherMock = new Mock<IMessagePublisher>();
//
//             // Настройка DatabaseFacade для транзакций
//             var databaseFacadeMock = new Mock<DatabaseFacade>(_contextMock.Object);
//             var transactionMock = new Mock<IDbContextTransaction>();
//             databaseFacadeMock.Setup(d => d.BeginTransactionAsync(It.IsAny<CancellationToken>()))
//                               .ReturnsAsync(transactionMock.Object);
//             transactionMock.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
//             transactionMock.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
//             _contextMock.Setup(c => c.Database).Returns(databaseFacadeMock.Object);
//
//             // Настройка SaveChangesAsync
//             _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
//
//             // Инициализация AuthService со всеми зависимостями
//             _authService = new AuthService(
//                 _contextMock.Object,
//                 _tokenServiceMock.Object,
//                 _passwordServiceMock.Object,
//                 _auditServiceMock.Object,
//                 _loginActivityServiceMock.Object,
//                 _getUserByEmailHandlerMock.Object,
//                 _addUserHandlerMock.Object,
//                 _getRefreshTokenHandlerMock.Object,
//                 _addRefreshTokenHandlerMock.Object,
//                 _deleteRefreshTokenHandlerMock.Object,
//                 _existingUserHandlerMock.Object,
//                 _revokeTokensHandlerMock.Object,
//                 _loggerMock.Object,
//                 _messagePublisherMock.Object);
//         }
//
//         [Fact]
//         public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
//         {
//             // Arrange
//             var request = new LoginRequest { Email = "test@example.com", Password = "Password123!" };
//             var user = new User
//             {
//                 Id = Guid.NewGuid(),
//                 Email = request.Email,
//                 PasswordHash = "hashedPassword",
//                 LastLoginAt = null,
//                 FirstName = null,
//                 LastName = null,
//                 Username = null
//             };
//             var authResponse = new AuthResponse { Token = "jwt", RefreshToken = "refresh" };
//
//             _getUserByEmailHandlerMock.Setup(h => h.Handler(request, default)).ReturnsAsync(user);
//             _passwordServiceMock.Setup(p => p.VerifyPassword(request.Password, user.PasswordHash)).Returns(true);
//             _tokenServiceMock.Setup(t => t.GenerateAuthResponseAsync(user, default)).ReturnsAsync(authResponse);
//
//             // Act
//             var result = await _authService.LoginAsync(request, "127.0.0.1", "Mozilla/5.0", default);
//
//             // Assert
//             Assert.Equal(authResponse, result);
//             Assert.NotNull(user.LastLoginAt);
//             _auditServiceMock.Verify(a => a.LogLoginAsync(user.Id, "127.0.0.1", true, default), Times.Once);
//             _messagePublisherMock.Verify(m => m.PublishAsync("login-notifications", It.IsAny<LoginNotificationMessage>()), Times.Once);
//         }
//
//         [Fact]
//         public async Task LoginAsync_InvalidCredentials_ThrowsInvalidCredentialsException()
//         {
//             // Arrange
//             var request = new LoginRequest { Email = "test@example.com", Password = "WrongPassword" };
//             var user = new User
//             {
//                 Id = Guid.NewGuid(),
//                 Email = request.Email,
//                 PasswordHash = "hashedPassword",
//                 FirstName = null,
//                 LastName = null,
//                 Username = null
//             };
//
//             _getUserByEmailHandlerMock.Setup(h => h.Handler(request, default)).ReturnsAsync(user);
//             _passwordServiceMock.Setup(p => p.VerifyPassword(request.Password, user.PasswordHash)).Returns(false);
//
//             // Act & Assert
//             await Assert.ThrowsAsync<InvalidCredentialsException>(() => _authService.LoginAsync(request, "127.0.0.1", "Mozilla/5.0", default));
//             _auditServiceMock.Verify(a => a.LogLoginAsync(Guid.Empty, "127.0.0.1", false, default), Times.Once);
//         }
//
//         [Fact]
//         public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
//         {
//             // Arrange
//             var request = new CreateRequest
//             {
//                 Email = "new@example.com",
//                 Password = "Password123!",
//                 Username = null
//             };
//             var user = new User
//             {
//                 Id = Guid.NewGuid(),
//                 Email = request.Email,
//                 PasswordHash = null,
//                 FirstName = null,
//                 LastName = null,
//                 Username = null
//             };
//             var authResponse = new AuthResponse { Token = "jwt", RefreshToken = "refresh" };
//
//             _existingUserHandlerMock.Setup(h => h.Handler(request, default)).ReturnsAsync(false);
//             _addUserHandlerMock.Setup(h => h.Handle(request, default)).ReturnsAsync(user);
//             _tokenServiceMock.Setup(t => t.GenerateAuthResponseAsync(user, default)).ReturnsAsync(authResponse);
//
//             // Act
//             var result = await _authService.RegisterAsync(request, "127.0.0.1", "Mozilla/5.0", default);
//
//             // Assert
//             Assert.Equal(authResponse, result);
//             _auditServiceMock.Verify(a => a.LogRegistrationAsync(user.Id, "127.0.0.1", true, default), Times.Once);
//             _messagePublisherMock.Verify(m => m.PublishAsync("login-notifications", It.IsAny<LoginNotificationMessage>()), Times.Once);
//         }
//
//         [Fact]
//         public async Task RegisterAsync_ExistingUser_ThrowsInvalidOperationException()
//         {
//             // Arrange
//             var request = new CreateRequest
//             {
//                 Email = "existing@example.com",
//                 Password = "Password123!",
//                 Username = "Just"
//             };
//
//             _existingUserHandlerMock.Setup(h => h.Handler(request, CancellationToken.None)).ReturnsAsync(true);
//
//             // Act & Assert
//             await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request, "127.0.0.1", "Mozilla/5.0", default));
//         }
//
//         [Fact]
//         public async Task RefreshTokenAsync_ValidToken_ReturnsAuthResponse()
//         {
//             // Arrange
//             var refreshToken = "validRefreshToken";
//             var user = new User
//             {
//                 Id = Guid.NewGuid(),
//                 Email = "test@example.com",
//                 FirstName = "Test",
//                 LastName = "User",
//                 PasswordHash = null,
//                 Username = null
//             };
//             var existingToken = new RefreshToken 
//             { 
//                 Token = refreshToken, 
//                 User = user, 
//                 ExpiresAt = DateTime.UtcNow.AddDays(1), 
//                 IsRevoked = false 
//             };
//
//             // Предполагаем, что TokenResponse - это DTO с JwtToken и RefreshToken
//             var newTokens = new TokenResponse 
//             { 
//                 JwtToken = "newJwt", 
//                 RefreshToken = new RefreshToken
//                 {
//                     Token = null!
//                 }
//             };
//
//             _getRefreshTokenHandlerMock.Setup(h => h.Handler(refreshToken, default)).ReturnsAsync(existingToken);
//             _addRefreshTokenHandlerMock.Setup(h => h.Handler(user, default)).ReturnsAsync(newTokens);
//
//             // Act
//             var result = await _authService.RefreshTokenAsync(refreshToken, "127.0.0.1", default);
//
//             // Assert
//             Assert.Equal(newTokens.JwtToken, result.Token);
//             Assert.True(existingToken.IsRevoked);
//             Assert.Equal(DateTime.UtcNow.Date, existingToken.RevokedAt?.Date);
//             _deleteRefreshTokenHandlerMock.Verify(d => d.Handler(user, default), Times.Once);
//         }
//
//         [Fact]
//         public async Task RefreshTokenAsync_InvalidToken_ThrowsSecurityTokenException()
//         {
//             // Arrange
//             var refreshToken = "invalidRefreshToken";
//             _getRefreshTokenHandlerMock.Setup(h => h.Handler(refreshToken, default))!.ReturnsAsync((RefreshToken)null!);
//
//             // Act & Assert
//             await Assert.ThrowsAsync<SecurityTokenException>(() => _authService.RefreshTokenAsync(refreshToken, "127.0.0.1", default));
//             _auditServiceMock.Verify(a => a.LogRefreshTokenFailureAsync(refreshToken, "127.0.0.1", default), Times.Once);
//         }
//
//         [Fact]
//         public async Task LogoutAsync_ValidRequest_CompletesSuccessfully()
//         {
//             // Arrange
//             var request = new LogoutRequest { RefreshToken = "validRefreshToken" };
//             var token = new RefreshToken { Token = request.RefreshToken, UserId = Guid.NewGuid(), User = new User
//                 {
//                     Email = "test@example.com",
//                     PasswordHash = null,
//                     FirstName = null,
//                     LastName = null,
//                     Username = null
//                 }
//             };
//
//             _getRefreshTokenHandlerMock.Setup(h => h.Handler(request.RefreshToken, default)).ReturnsAsync(token);
//
//             // Act
//             await _authService.LogoutAsync(request, "127.0.0.1", "Mozilla/5.0", default);
//
//             // Assert
//             _revokeTokensHandlerMock.Verify(r => r.Handler(request, token, default), Times.Once);
//             _messagePublisherMock.Verify(m => m.PublishAsync("logout-notifications", It.IsAny<LogoutNotificationMessage>()), Times.Once);
//             _auditServiceMock.Verify(a => a.LogLogoutAsync(token.UserId, "127.0.0.1", false, default), Times.Once);
//         }
//     }
// }