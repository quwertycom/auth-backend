using Moq;
using NUnit.Framework;
using NSubstitute;
using API.Shared.Interfaces.Database.Repositories;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using API.Shared.Interfaces.Email;
using API.Shared.Interfaces.Security;

namespace API.UnitTests;

public abstract class TestBase
{
    protected IUserRepository MockUserRepository { get; private set; } = null!;
    protected ISessionRepository MockSessionRepository { get; private set; } = null!;
    protected IVerificationRepository MockVerificationRepository { get; private set; } = null!;
    protected IEmailSender MockEmailSender { get; private set; } = null!;
    protected IEmailService MockEmailService { get; private set; } = null!;
    protected IHasher MockHasher { get; private set; } = null!;
    protected IJwtService MockJwtService { get; private set; } = null!;
    protected IRandomGenerator MockRandomGenerator { get; private set; } = null!;

    [SetUp]
    public virtual void Setup()
    {
        MockUserRepository = Substitute.For<IUserRepository>();
        MockSessionRepository = Substitute.For<ISessionRepository>();
        MockVerificationRepository = Substitute.For<IVerificationRepository>();
        MockEmailSender = Substitute.For<IEmailSender>();
        MockEmailService = Substitute.For<IEmailService>();
        MockHasher = Substitute.For<IHasher>();
        MockJwtService = Substitute.For<IJwtService>();
        MockRandomGenerator = Substitute.For<IRandomGenerator>();
    }
}
