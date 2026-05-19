using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrderService.Controllers;
using OrderService.DTOs;
using OrderService.Services;
using System.Security.Claims;
using Xunit;

namespace OrderService.Tests;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _serviceMock;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _serviceMock = new Mock<IOrderService>();
        _controller = new OrdersController(_serviceMock.Object);
        SetupAuthenticatedUser(userId: 1, role: "User");
    }

    // Helper: configure controller's HttpContext to simulate a logged-in user
    private void SetupAuthenticatedUser(int userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        _controller.HttpContext.Request.Headers["Authorization"] = "Bearer test-token";
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidOrder_ReturnsOk()
    {
        var dto = new OrderDTO { Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 2 } } };
        var response = new OrderResponseDTO { OrderId = 1, TotalAmount = 20m, Items = new() };
        _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<OrderDTO>(), It.IsAny<string?>()))
            .ReturnsAsync(response);

        var result = await _controller.Create(dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_NullDto_ReturnsBadRequest()
    {
        var result = await _controller.Create(null!);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_EmptyItems_ReturnsBadRequest()
    {
        var dto = new OrderDTO { Items = new List<OrderItemDTO>() };

        var result = await _controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ServiceThrowsArgumentException_ReturnsBadRequest()
    {
        var dto = new OrderDTO { Items = new List<OrderItemDTO> { new() { ProductId = 99, Quantity = 1 } } };
        _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<OrderDTO>(), It.IsAny<string?>()))
            .ThrowsAsync(new ArgumentException("Product 99 not found"));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_ServiceThrowsGenericException_Returns500()
    {
        var dto = new OrderDTO { Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 1 } } };
        _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<OrderDTO>(), It.IsAny<string?>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<ObjectResult>();
        ((ObjectResult)result).StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Create_SetsUserIdFromToken()
    {
        SetupAuthenticatedUser(userId: 42, role: "User");
        var dto = new OrderDTO { Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 1 } } };
        var response = new OrderResponseDTO { OrderId = 1, TotalAmount = 10m, Items = new() };

        _serviceMock.Setup(s => s.CreateOrderAsync(It.IsAny<OrderDTO>(), It.IsAny<string?>()))
            .ReturnsAsync(response);

        await _controller.Create(dto);

        _serviceMock.Verify(s => s.CreateOrderAsync(
            It.Is<OrderDTO>(o => o.UserId == 42), It.IsAny<string?>()), Times.Once);
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsAllOrders()
    {
        SetupAuthenticatedUser(userId: 1, role: "Admin");
        var orders = new List<OrderResponseDTO>
        {
            new() { OrderId = 1, UserId = 1, TotalAmount = 10m, Items = new() },
            new() { OrderId = 2, UserId = 2, TotalAmount = 20m, Items = new() }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(orders);

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        var returned = ok.Value as IEnumerable<OrderResponseDTO>;
        returned.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_AsUser_ReturnsOnlyOwnOrders()
    {
        SetupAuthenticatedUser(userId: 1, role: "User");
        var orders = new List<OrderResponseDTO>
        {
            new() { OrderId = 1, UserId = 1, TotalAmount = 10m, Items = new() },
            new() { OrderId = 2, UserId = 2, TotalAmount = 20m, Items = new() }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(orders);

        var result = await _controller.GetAll();

        var ok = (OkObjectResult)result;
        var returned = (ok.Value as IEnumerable<OrderResponseDTO>)!.ToList();
        returned.Should().HaveCount(1);
        returned[0].UserId.Should().Be(1);
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_OwnOrder_ReturnsOk()
    {
        SetupAuthenticatedUser(userId: 1, role: "User");
        var order = new OrderResponseDTO { OrderId = 1, UserId = 1, TotalAmount = 10m, Items = new() };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(order);

        var result = await _controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((OrderResponseDTO?)null);

        var result = await _controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetById_OtherUsersOrder_ReturnsForbid()
    {
        SetupAuthenticatedUser(userId: 1, role: "User");
        var order = new OrderResponseDTO { OrderId = 5, UserId = 99, TotalAmount = 10m, Items = new() };
        _serviceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(order);

        var result = await _controller.GetById(5);

        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetById_AdminCanSeeAnyOrder()
    {
        SetupAuthenticatedUser(userId: 1, role: "Admin");
        var order = new OrderResponseDTO { OrderId = 5, UserId = 99, TotalAmount = 10m, Items = new() };
        _serviceMock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(order);

        var result = await _controller.GetById(5);

        result.Should().BeOfType<OkObjectResult>();
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ExistingOrder_ReturnsOk()
    {
        var dto = new OrderDTO { UserId = 1, Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 1, Price = 10m } } };
        _serviceMock.Setup(s => s.UpdateAsync(1, dto)).ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        var dto = new OrderDTO { UserId = 1, Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 1, Price = 10m } } };
        _serviceMock.Setup(s => s.UpdateAsync(999, dto)).ReturnsAsync(false);

        var result = await _controller.Update(999, dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ExistingOrder_ReturnsOk()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _controller.Delete(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
