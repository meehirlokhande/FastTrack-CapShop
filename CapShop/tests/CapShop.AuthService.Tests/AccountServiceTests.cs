using CapShop.AuthService.Dtos;
using CapShop.AuthService.Models;
using CapShop.AuthService.Repositories;
using CapShop.AuthService.Services;
using CapShop.Tests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CapShop.AuthService.Tests;

[TestFixture]
public class AccountServiceTests
{
    private Mock<IUserRepository> _users = null!;
    private Mock<IJwtTokenService> _jwt = null!;
    private Mock<ITwoFactorService> _twoFactor = null!;
    private Mock<IWebHostEnvironment> _env = null!;
    private AccountService _sut = null!;
    private string _tempRoot = null!;

    [SetUp]
    public void SetUp()
    {
        _users = new Mock<IUserRepository>();
        _jwt = new Mock<IJwtTokenService>();
        _twoFactor = new Mock<ITwoFactorService>();
        _env = new Mock<IWebHostEnvironment>();
        _tempRoot = Path.Combine(Path.GetTempPath(), "capshop-auth-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
        _env.Setup(e => e.ContentRootPath).Returns(_tempRoot);
        _env.Setup(e => e.WebRootPath).Returns(Path.Combine(_tempRoot, "wwwroot"));

        _sut = new AccountService(
            _users.Object,
            _jwt.Object,
            _twoFactor.Object,
            _env.Object,
            NullLogger<AccountService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        try
        {
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, recursive: true);
        }
        catch (IOException)
        {
            // Temp dir may still be locked by AV or another process.
        }
    }

    [Test]
    public async Task SignupAsync_EmptyEmail_ThrowsArgumentException()
    {
        var req = new SignUpRequest { Email = " ", Password = "secret1", FullName = "A", PhoneNumber = "1" };
        var ex = await AsyncAssert.CatchAsync(() => _sut.SignupAsync(req));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
        Assert.That(ex!.Message, Does.Contain("required"));
    }

    [Test]
    public async Task SignupAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        _users.Setup(u => u.ExistsByEmailAsync("a@b.com")).ReturnsAsync(true);
        var req = new SignUpRequest
        {
            Email = "A@B.COM",
            Password = "secret1",
            FullName = "User",
            PhoneNumber = "123"
        };
        var ex = await AsyncAssert.CatchAsync(() => _sut.SignupAsync(req));
        Assert.That(ex, Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task SignupAsync_Valid_PersistsUser()
    {
        _users.Setup(u => u.ExistsByEmailAsync("new@x.com")).ReturnsAsync(false);
        var req = new SignUpRequest
        {
            Email = "NEW@X.COM",
            Password = "secret1",
            FullName = "  Neo  ",
            PhoneNumber = " 99 "
        };

        var msg = await _sut.SignupAsync(req);

        Assert.That(msg, Is.EqualTo("Signup successful."));
        _users.Verify(u => u.AddAsync(It.Is<User>(x =>
            x.Email == "new@x.com"
            && x.FullName == "Neo"
            && x.PhoneNumber == "99"
            && x.Role == UserRole.Customer
            && BCrypt.Net.BCrypt.Verify("secret1", x.PasswordHash))), Times.Once);
    }

    [Test]
    public async Task LoginAsync_UserNotFound_ThrowsUnauthorized()
    {
        _users.Setup(u => u.FindByEmailAsync("a@b.com")).ReturnsAsync((User?)null);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "x" }));
        Assert.That(ex, Is.TypeOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorized()
    {
        var user = new User { Email = "a@b.com", IsActive = false, PasswordHash = BCrypt.Net.BCrypt.HashPassword("x") };
        _users.Setup(u => u.FindByEmailAsync("a@b.com")).ReturnsAsync(user);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "x" }));
        Assert.That(ex, Is.TypeOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        var user = new User { Email = "a@b.com", IsActive = true, PasswordHash = BCrypt.Net.BCrypt.HashPassword("right") };
        _users.Setup(u => u.FindByEmailAsync("a@b.com")).ReturnsAsync(user);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "wrong" }));
        Assert.That(ex, Is.TypeOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task LoginAsync_NoTwoFactor_ReturnsToken()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            Role = UserRole.Customer,
            IsActive = true,
            TwoFactorEnabled = false,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ok")
        };
        _users.Setup(u => u.FindByEmailAsync("a@b.com")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("access-token");

        var res = await _sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "ok" });

        Assert.That(res.Token, Is.EqualTo("access-token"));
        Assert.That(res.Role, Is.EqualTo("Customer"));
        Assert.That(res.RequiresTwoFactor, Is.False);
    }

    [Test]
    public async Task LoginAsync_TwoFactorEmail_SendsOtpAndReturnsTemp()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "a@b.com",
            Role = UserRole.Customer,
            IsActive = true,
            TwoFactorEnabled = true,
            TwoFactorMethod = TwoFactorMethod.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("ok")
        };
        _users.Setup(u => u.FindByEmailAsync("a@b.com")).ReturnsAsync(user);
        _jwt.Setup(j => j.GenerateTempToken(user.Id, TwoFactorMethod.Email)).Returns("temp-jwt");

        var res = await _sut.LoginAsync(new LoginRequest { Email = "a@b.com", Password = "ok" });

        Assert.That(res.RequiresTwoFactor, Is.True);
        Assert.That(res.TempToken, Is.EqualTo("temp-jwt"));
        _twoFactor.Verify(t => t.SendOtpAsync(user.Id, TwoFactorMethod.Email), Times.Once);
    }

    [Test]
    public async Task VerifyTwoFactorAsync_InvalidTempToken_Throws()
    {
        _jwt.Setup(j => j.ValidateTempToken("bad")).Returns((TempTokenPayload?)null);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.VerifyTwoFactorAsync(new VerifyTwoFactorRequest { TempToken = "bad", Code = "123456" }));
        Assert.That(ex, Is.TypeOf<UnauthorizedAccessException>());
    }

    [Test]
    public async Task VerifyTwoFactorAsync_ValidOtp_ReturnsAccessToken()
    {
        var uid = Guid.NewGuid();
        _jwt.Setup(j => j.ValidateTempToken("t")).Returns(new TempTokenPayload(uid, TwoFactorMethod.Email));
        var user = new User { Id = uid, Email = "x@y.com", Role = UserRole.Customer, IsActive = true };
        _users.Setup(u => u.FindByIdAsync(uid)).ReturnsAsync(user);
        _twoFactor.Setup(t => t.VerifyOtpCodeAsync(uid, "111111")).ReturnsAsync(true);
        _jwt.Setup(j => j.GenerateToken(user)).Returns("final-token");

        var res = await _sut.VerifyTwoFactorAsync(new VerifyTwoFactorRequest { TempToken = "t", Code = "111111" });

        Assert.That(res.Token, Is.EqualTo("final-token"));
    }

    [Test]
    public async Task ResendTwoFactorCodeAsync_AuthenticatorMethod_ThrowsInvalidOperation()
    {
        var uid = Guid.NewGuid();
        _jwt.Setup(j => j.ValidateTempToken("t")).Returns(new TempTokenPayload(uid, TwoFactorMethod.Authenticator));
        var ex = await AsyncAssert.CatchAsync(() => _sut.ResendTwoFactorCodeAsync("t"));
        Assert.That(ex, Is.TypeOf<InvalidOperationException>());
    }

    [Test]
    public async Task GetProfileAsync_MissingUser_ThrowsKeyNotFound()
    {
        _users.Setup(u => u.FindByIdAsync(It.IsAny<Guid>())).ReturnsAsync((User?)null);
        var ex = await AsyncAssert.CatchAsync(() => _sut.GetProfileAsync(Guid.NewGuid()));
        Assert.That(ex, Is.TypeOf<KeyNotFoundException>());
    }

    [Test]
    public async Task UpdateProfileAsync_EmptyName_Throws()
    {
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.UpdateProfileAsync(Guid.NewGuid(), new UpdateProfileRequest { FullName = "  " }));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task ChangePasswordAsync_ShortNewPassword_Throws()
    {
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.ChangePasswordAsync(Guid.NewGuid(), new ChangePasswordRequest
            {
                CurrentPassword = "a",
                NewPassword = "short"
            }));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task ChangePasswordAsync_WrongCurrent_Throws()
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpass") };
        _users.Setup(u => u.FindByIdAsync(id)).ReturnsAsync(user);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.ChangePasswordAsync(id, new ChangePasswordRequest
            {
                CurrentPassword = "wrong",
                NewPassword = "newpass1"
            }));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task SetAvatarAsync_InvalidContentType_Throws()
    {
        await using var ms = new MemoryStream([1, 2, 3]);
        var ex = await AsyncAssert.CatchAsync(() =>
            _sut.SetAvatarAsync(Guid.NewGuid(), ms, "image/bmp", 3));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
        Assert.That(ex!.Message, Does.Contain("JPEG"));
    }

    [Test]
    public async Task SetAvatarAsync_Valid_WritesFileAndUpdatesUser()
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, Email = "a@b.com", FullName = "A", PhoneNumber = "1", Role = UserRole.Customer };
        _users.Setup(u => u.FindByIdAsync(id)).ReturnsAsync(user);
        await using var ms = new MemoryStream([9, 9, 9]);

        var profile = await _sut.SetAvatarAsync(id, ms, "image/png", 3);

        Assert.That(profile.ProfilePictureUrl, Does.StartWith("/avatars/"));
        Assert.That(profile.ProfilePictureUrl, Does.EndWith(".png"));
        _users.Verify(u => u.UpdateAsync(It.Is<User>(x => x.ProfilePictureUrl == profile.ProfilePictureUrl)), Times.Once);
    }
}
