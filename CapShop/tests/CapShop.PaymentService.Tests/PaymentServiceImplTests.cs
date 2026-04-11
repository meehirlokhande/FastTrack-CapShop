using CapShop.Tests.Common;
using CapShop.PaymentService.Dtos;
using CapShop.PaymentService.Models;
using CapShop.PaymentService.Repositories;
using CapShop.PaymentService.Services;
using CapShop.Shared.Events;
using CapShop.Shared.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CapShop.PaymentService.Tests;

[TestFixture]
public class PaymentServiceImplTests
{
    private Mock<ITransactionRepository> _tx = null!;
    private Mock<IEventPublisher> _events = null!;
    private PaymentServiceImpl _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _tx = new Mock<ITransactionRepository>();
        _events = new Mock<IEventPublisher>();
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);
        _sut = new PaymentServiceImpl(_tx.Object, _events.Object, httpContextAccessor.Object, NullLogger<PaymentServiceImpl>.Instance);
    }

    [Test]
    public async Task SimulateAsync_InvalidMethod_ThrowsArgumentException()
    {
        var req = new SimulatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            Amount = 10m,
            PaymentMethod = "Bitcoin"
        };
        var ex = await AsyncAssert.CatchAsync(() => _sut.SimulateAsync(Guid.NewGuid(), req));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task SimulateAsync_NonPositiveAmount_ThrowsArgumentException()
    {
        var req = new SimulatePaymentRequest
        {
            OrderId = Guid.NewGuid(),
            Amount = 0,
            PaymentMethod = "COD"
        };
        var ex = await AsyncAssert.CatchAsync(() => _sut.SimulateAsync(Guid.NewGuid(), req));
        Assert.That(ex, Is.TypeOf<ArgumentException>());
    }

    [Test]
    public async Task SimulateAsync_Cod_CapturesAndPublishesPaymentCompleted()
    {
        var userId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var req = new SimulatePaymentRequest { OrderId = orderId, Amount = 42m, PaymentMethod = "cod" };

        PaymentCompletedEvent? published = null;
        _events
            .Setup(e => e.PublishAsync(QueueNames.PaymentCompleted, It.IsAny<PaymentCompletedEvent>()))
            .Callback<string, PaymentCompletedEvent>((_, evt) => published = evt)
            .Returns(Task.CompletedTask);

        var res = await _sut.SimulateAsync(userId, req);

        Assert.That(res.Status, Is.EqualTo("Captured"));
        Assert.That(res.Amount, Is.EqualTo(42m));
        Assert.That(res.PaymentMethod, Is.EqualTo("COD"));
        _tx.Verify(t => t.AddAsync(It.IsAny<Transaction>()), Times.Once);
        _tx.Verify(t => t.UpdateAsync(It.Is<Transaction>(x => x.Status == TransactionStatus.Captured)), Times.Once);
        Assert.That(published, Is.Not.Null);
        Assert.That(published!.OrderId, Is.EqualTo(orderId));
        Assert.That(published.UserId, Is.EqualTo(userId));
        Assert.That(published.Status, Is.EqualTo("Captured"));
    }

    [Test]
    public async Task GetByOrderIdAsync_NoTransaction_ReturnsNull()
    {
        _tx.Setup(t => t.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync((Transaction?)null);
        var r = await _sut.GetByOrderIdAsync(Guid.NewGuid());
        Assert.That(r, Is.Null);
    }

    [Test]
    public async Task GetByOrderIdAsync_Found_MapsResponse()
    {
        var orderId = Guid.NewGuid();
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            UserId = Guid.NewGuid(),
            Amount = 5m,
            Method = PaymentMethod.UPI,
            Status = TransactionStatus.Captured,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };
        _tx.Setup(t => t.GetByOrderIdAsync(orderId)).ReturnsAsync(tx);

        var r = await _sut.GetByOrderIdAsync(orderId);

        Assert.That(r, Is.Not.Null);
        Assert.That(r!.TransactionId, Is.EqualTo(tx.Id));
        Assert.That(r.Status, Is.EqualTo("Captured"));
    }
}
