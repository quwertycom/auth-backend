using Microsoft.Extensions.Options;
using API.Shared.Interfaces.Database.Repositories;
using API.Shared.Interfaces.Email;
using API.Shared.Interfaces.Security;
using API.Features.Authentication.Register.Interfaces;
using API.Features.Session.Refresh.Interfaces;
using API.Shared.Utilities;
using API.Shared.Configuration;
using API.Tests.DataGenerator;

namespace API.Tests.Unit;

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

    protected IGenerate _generate { get; private set; } = null!;

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
        _generate = new Generate();

        // Initialize Snowflake for tests
        var snowflakeSettings = new SnowflakeSettings
        {
            DatacenterId = 1, // Example Datacenter ID
            WorkerId = 1,     // Example Worker ID
            Epoch = "2024-01-01" // Example Epoch
        };
        Snowflake.Initialize(Options.Create(snowflakeSettings));
    }
}
