using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.Services;
using Xunit;

namespace OrderService.Tests;

public class OrderServiceImplTests : IDisposable
{
    private readonly OrderDbContext _context;
    private readonly Mock<ProductServiceClient> _productClientMock;
    private readonly OrderServiceImpl _service;

    public OrderServiceImplTests()
    {
        var options = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new OrderDbContext(options);

        var httpClient = new HttpClient();
        var loggerMock = new Mock<ILogger<ProductServiceClient>>();
        _productClientMock = new Mock<ProductServiceClient>(httpClient, loggerMock.Object);

        _service = new OrderServiceImpl(_context, _productClientMock.Object);
    }

    public void Dispose() => _context.Dispose();

    private ProductInfoDTO MakeProduct(int id = 1, string name = "Widget", decimal price = 10m)
        => new() { ProductId = id, Name = name, Price = price, ImageUrl = "img.jpg", Stock = 50 };

    // ── CreateOrderAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrderAsync_ValidOrder_ReturnsOrderResponse()
    {
        _productClientMock.Setup(c => c.GetProductAsync(1, It.IsAny<string?>()))
            .ReturnsAsync(MakeProduct(1, "Widget", 10m));

        var dto = new OrderDTO
        {
            UserId = 42,
            Items = new List<OrderItemDTO>
            {
                new() { ProductId = 1, Quantity = 3 }
            }
        };

        var result = await _service.CreateOrderAsync(dto, "token");

        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(30m);
        result.UserId.Should().Be(42);
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductName.Should().Be("Widget");
    }

    [Fact]
    public async Task CreateOrderAsync_MultipleItems_SumsTotalCorrectly()
    {
        _productClientMock.Setup(c => c.GetProductAsync(1, It.IsAny<string?>()))
            .ReturnsAsync(MakeProduct(1, "A", 10m));
        _productClientMock.Setup(c => c.GetProductAsync(2, It.IsAny<string?>()))
            .ReturnsAsync(MakeProduct(2, "B", 25m));

        var dto = new OrderDTO
        {
            UserId = 1,
            Items = new List<OrderItemDTO>
            {
                new() { ProductId = 1, Quantity = 2 },
                new() { ProductId = 2, Quantity = 1 }
            }
        };

        var result = await _service.CreateOrderAsync(dto);

        result.TotalAmount.Should().Be(45m); // 10*2 + 25*1
    }

    [Fact]
    public async Task CreateOrderAsync_NullDto_ThrowsArgumentException()
    {
        var act = async () => await _service.CreateOrderAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Order items cannot be empty*");
    }

    [Fact]
    public async Task CreateOrderAsync_EmptyItems_ThrowsArgumentException()
    {
        var dto = new OrderDTO { UserId = 1, Items = new List<OrderItemDTO>() };

        var act = async () => await _service.CreateOrderAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Order items cannot be empty*");
    }

    [Fact]
    public async Task CreateOrderAsync_ZeroQuantity_ThrowsArgumentException()
    {
        _productClientMock.Setup(c => c.GetProductAsync(1, It.IsAny<string?>()))
            .ReturnsAsync(MakeProduct());

        var dto = new OrderDTO
        {
            UserId = 1,
            Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 0 } }
        };

        var act = async () => await _service.CreateOrderAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Quantity must be greater than zero*");
    }

    [Fact]
    public async Task CreateOrderAsync_NegativeQuantity_ThrowsArgumentException()
    {
        _productClientMock.Setup(c => c.GetProductAsync(1, It.IsAny<string?>()))
            .ReturnsAsync(MakeProduct());

        var dto = new OrderDTO
        {
            UserId = 1,
            Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = -5 } }
        };

        var act = async () => await _service.CreateOrderAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Quantity must be greater than zero*");
    }

    [Fact]
    public async Task CreateOrderAsync_ProductNotFound_ThrowsArgumentException()
    {
        _productClientMock.Setup(c => c.GetProductAsync(999, It.IsAny<string?>()))
            .ReturnsAsync((ProductInfoDTO?)null);

        var dto = new OrderDTO
        {
            UserId = 1,
            Items = new List<OrderItemDTO> { new() { ProductId = 999, Quantity = 1 } }
        };

        var act = async () => await _service.CreateOrderAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*999*not found*");
    }

    [Fact]
    public async Task CreateOrderAsync_PersistsToDatabase()
    {
        _productClientMock.Setup(c => c.GetProductAsync(1, It.IsAny<string?>()))
            .ReturnsAsync(MakeProduct());

        var dto = new OrderDTO
        {
            UserId = 5,
            Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 2 } }
        };

        await _service.CreateOrderAsync(dto);

        _context.Orders.Should().HaveCount(1);
        _context.Orders.First().UserId.Should().Be(5);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrders()
    {
        _context.Orders.AddRange(
            new Order { UserId = 1, TotalAmount = 10m, OrderDate = DateTime.UtcNow },
            new Order { UserId = 2, TotalAmount = 20m, OrderDate = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDb_ReturnsEmpty()
    {
        var result = await _service.GetAllAsync();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_MapsOrderItemsCorrectly()
    {
        var order = new Order
        {
            UserId = 1, TotalAmount = 50m, OrderDate = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new() { ProductId = 1, ProductName = "Widget", Quantity = 2, Price = 25m, ProductImageUrl = "img.jpg" }
            }
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var result = (await _service.GetAllAsync()).ToList();

        result[0].Items[0].ProductName.Should().Be("Widget");
        result[0].Items[0].Quantity.Should().Be(2);
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsOrder()
    {
        var order = new Order { UserId = 1, TotalAmount = 99m, OrderDate = DateTime.UtcNow };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(order.OrderId);

        result.Should().NotBeNull();
        result!.TotalAmount.Should().Be(99m);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingOrder_ReturnsTrue()
    {
        var order = new Order { UserId = 1, TotalAmount = 10m, OrderDate = DateTime.UtcNow };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var dto = new OrderDTO
        {
            UserId = 1,
            Items = new List<OrderItemDTO>
            {
                new() { ProductId = 2, Quantity = 1, Price = 99m }
            }
        };

        var result = await _service.UpdateAsync(order.OrderId, dto);

        result.Should().BeTrue();
        var updated = await _context.Orders.FindAsync(order.OrderId);
        updated!.TotalAmount.Should().Be(99m);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsFalse()
    {
        var dto = new OrderDTO { UserId = 1, Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 1, Price = 10m } } };

        var result = await _service.UpdateAsync(999, dto);

        result.Should().BeFalse();
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingOrder_ReturnsTrue()
    {
        var order = new Order { UserId = 1, TotalAmount = 10m, OrderDate = DateTime.UtcNow };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(order.OrderId);

        result.Should().BeTrue();
        _context.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFalse()
    {
        var result = await _service.DeleteAsync(999);

        result.Should().BeFalse();
    }
}
