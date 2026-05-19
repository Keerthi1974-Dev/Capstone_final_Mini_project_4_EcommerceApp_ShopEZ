using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using ProductService.Controllers;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests;

/// <summary>
/// Builds a CloudinaryService with fake credentials (no real HTTP calls are made in these tests).
/// </summary>
internal static class FakeCloudinaryService
{
    public static CloudinaryService Create()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Cloudinary:CloudName"] = "testcloud",
                ["Cloudinary:ApiKey"] = "000000000000000",
                ["Cloudinary:ApiSecret"] = "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            })
            .Build();
        return new CloudinaryService(config);
    }
}

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly CloudinaryService _cloudinaryService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _cloudinaryService = FakeCloudinaryService.Create();
        _controller = new ProductsController(_serviceMock.Object, _cloudinaryService);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    // ── GetAll ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsOkWithProducts()
    {
        var products = new List<Product>
        {
            new() { ProductId = 1, Name = "A", Price = 10, Stock = 5, Description = "d", Category = "c" }
        };
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsOkWithEmptyList()
    {
        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(new List<Product>());

        var result = await _controller.GetAll();

        result.Should().BeOfType<OkObjectResult>();
        ((OkObjectResult)result).Value.Should().BeEquivalentTo(new List<Product>());
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ValidId_ReturnsOk()
    {
        var product = new Product { ProductId = 1, Name = "Widget", Price = 9.99m, Stock = 10, Description = "d", Category = "c" };
        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _controller.GetById(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetById_ZeroId_ReturnsBadRequest()
    {
        var result = await _controller.GetById(0);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_NegativeId_ReturnsBadRequest()
    {
        var result = await _controller.GetById(-1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _controller.GetById(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // ── Create ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ValidDto_ReturnsOk()
    {
        var dto = new ProductDTO { Name = "Widget", Price = 9.99m, Stock = 10, Description = "d", Category = "c" };
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<ProductDTO>())).Returns(Task.CompletedTask);

        var result = await _controller.Create(dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Create_ServiceThrows_ReturnsBadRequest()
    {
        var dto = new ProductDTO { Name = "Bad", Price = 0, Stock = -1, Description = "d", Category = "c" };
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<ProductDTO>()))
            .ThrowsAsync(new Exception("Price must be greater than zero"));

        var result = await _controller.Create(dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_CallsServiceOnce()
    {
        var dto = new ProductDTO { Name = "Widget", Price = 9.99m, Stock = 10, Description = "d", Category = "c" };
        _serviceMock.Setup(s => s.AddAsync(It.IsAny<ProductDTO>())).Returns(Task.CompletedTask);

        await _controller.Create(dto);

        _serviceMock.Verify(s => s.AddAsync(It.IsAny<ProductDTO>()), Times.Once);
    }

    // ── Update ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ValidRequest_ReturnsOk()
    {
        var dto = new ProductDTO { Name = "Updated", Price = 15, Stock = 3, Description = "d", Category = "c" };
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<ProductDTO>())).ReturnsAsync(true);

        var result = await _controller.Update(1, dto);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Update_ZeroId_ReturnsBadRequest()
    {
        var result = await _controller.Update(0, new ProductDTO { Name = "X", Price = 1, Stock = 1, Description = "d", Category = "c" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NegativeId_ReturnsBadRequest()
    {
        var result = await _controller.Update(-1, new ProductDTO { Name = "X", Price = 1, Stock = 1, Description = "d", Category = "c" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        var dto = new ProductDTO { Name = "X", Price = 1, Stock = 1, Description = "d", Category = "c" };
        _serviceMock.Setup(s => s.UpdateAsync(999, It.IsAny<ProductDTO>())).ReturnsAsync(false);

        var result = await _controller.Update(999, dto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_ServiceThrows_ReturnsBadRequest()
    {
        var dto = new ProductDTO { Name = "X", Price = 1, Stock = 1, Description = "d", Category = "c" };
        _serviceMock.Setup(s => s.UpdateAsync(1, It.IsAny<ProductDTO>()))
            .ThrowsAsync(new Exception("DB error"));

        var result = await _controller.Update(1, dto);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ValidId_ReturnsOk()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

        var result = await _controller.Delete(1);

        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_ZeroId_ReturnsBadRequest()
    {
        var result = await _controller.Delete(0);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_NegativeId_ReturnsBadRequest()
    {
        var result = await _controller.Delete(-3);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _controller.Delete(999);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Delete_ServiceThrows_ReturnsBadRequest()
    {
        _serviceMock.Setup(s => s.DeleteAsync(1)).ThrowsAsync(new Exception("DB error"));

        var result = await _controller.Delete(1);

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
