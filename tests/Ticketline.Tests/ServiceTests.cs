using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ticketline.Api.Data;
using Ticketline.Api.Dtos;
using Ticketline.Api.Models;
using Ticketline.Api.Services;

namespace Ticketline.Tests;

public sealed class ServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly AuthService _auth;
    private readonly TicketService _tickets;

    public ServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "ticketline-dev-key-not-for-production-use",
                ["Jwt:Issuer"] = "ticketline",
                ["Jwt:Audience"] = "ticketline"
            })
            .Build();

        _auth = new AuthService(_db, new PasswordHasher<User>(), new TokenService(configuration));
        _tickets = new TicketService(_db);
    }

    [Fact]
    public async Task Register_StoresHashAndRejectsDuplicateEmail()
    {
        var created = await _auth.RegisterAsync(new RegisterRequest
        {
            Name = "Анна",
            Email = "Anna@Example.com",
            Password = "secret1"
        });

        var user = await _db.Users.SingleAsync();
        Assert.Equal("anna@example.com", user.Email);
        Assert.NotEqual("secret1", user.PasswordHash);
        Assert.False(string.IsNullOrWhiteSpace(created.Token));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _auth.RegisterAsync(new RegisterRequest
            {
                Name = "Анна",
                Email = "anna@example.com",
                Password = "secret1"
            }));
    }

    [Fact]
    public async Task Login_RejectsWrongPassword()
    {
        await _auth.RegisterAsync(new RegisterRequest
        {
            Name = "Илья",
            Email = "ilya@example.com",
            Password = "secret1"
        });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _auth.LoginAsync(new LoginRequest
            {
                Email = "ilya@example.com",
                Password = "wrong-password"
            }));
    }

    [Fact]
    public async Task Tickets_AreVisibleOnlyToOwner_AndClosedTicketStaysClosed()
    {
        var owner = await _auth.RegisterAsync(new RegisterRequest
        {
            Name = "Олег",
            Email = "oleg@example.com",
            Password = "secret1"
        });
        var other = await _auth.RegisterAsync(new RegisterRequest
        {
            Name = "Мария",
            Email = "maria@example.com",
            Password = "secret1"
        });

        var ownerId = (await _db.Users.SingleAsync(user => user.Email == owner.Email)).Id;
        var otherId = (await _db.Users.SingleAsync(user => user.Email == other.Email)).Id;

        var ticket = await _tickets.CreateAsync(ownerId, new CreateTicketRequest
        {
            Title = "Не печатает",
            Body = "Принтер на втором этаже"
        });

        var hidden = await _tickets.GetAsync(otherId, ticket.Id);
        Assert.Null(hidden);

        await _tickets.UpdateAsync(ownerId, ticket.Id, new UpdateTicketRequest
        {
            Title = ticket.Title,
            Body = ticket.Body,
            Status = TicketStatus.Closed
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _tickets.UpdateAsync(ownerId, ticket.Id, new UpdateTicketRequest
            {
                Title = ticket.Title,
                Body = ticket.Body,
                Status = TicketStatus.Open
            }));
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
