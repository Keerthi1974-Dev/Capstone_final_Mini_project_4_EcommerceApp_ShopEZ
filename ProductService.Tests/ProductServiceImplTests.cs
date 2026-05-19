using FluentAssertions;
using Moq;
using ProductService.DTOs;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Services;
using Xunit;

namespace ProductService.Tests;

public class ProductServiceImplTests
{
    private readonly Mock<IProductRepository> _repoMock;
    private readonly ProductServiceImpl _service;

    public ProductServiceImplTests()
    {
        _repoMock = new Mock<IProductRepository>();
        _service = new ProductServiceImpl(_repoMock.Object);
    }

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        var products = new List<Product>
        {
            new() { ProductId = 1, Name = "A", Price = 10, Stock = 5, Description = "d", Category = "c" },
            new() { ProductId = 2, Name = "B", Price = 20, Stock = 3, Description = "d", Category = "c" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var result = await _service.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepo_ReturnsEmptyList()
    {
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());

        var result = await _service.GetAllAsync();

        result.Should().BeEmpty();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ValidId_ReturnsProduct()
    {
        var product = new Product { ProductId = 1, Name = "Widget", Price = 9.99m, Stock = 10, Description = "d", Category = "c" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _service.GetByIdAsync(1);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Widget");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_InvalidId_ThrowsException()
    {
        var act = async () => await _service.GetByIdAsync(0);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid product id*");
    }

    [Fact]
    public async Task GetByIdAsync_NegativeId_ThrowsException()
    {
        var act = async () => await _service.GetByIdAsync(-5);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid product id*");
    }

    // ── AddAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidDto_CallsRepositoryAdd()
    {
        var dto = new ProductDTO { Name = "Widget", Price = 9.99m, Stock = 10, Description = "d", Category = "c" };

        await _service.AddAsync(dto);

        _repoMock.Verify(r => r.AddAsync(It.Is<Product>(p =>
            p.Name == "Widget" && p.Price == 9.99m && p.Stock == 10)), Times.Once);
    }

    [Fact]
    public async Task AddAsync_NullDto_ThrowsException()
    {
        var act = async () => await _service.AddAsync(null!);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid data*");
    }

    [Fact]
    public async Task AddAsync_EmptyName_ThrowsException()
    {
        var dto = new ProductDTO { Name = "", Price = 9.99m, Stock = 10, Description = "d", Category = "c" };

        var act = async () => await _service.AddAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("*name is required*");
    }

    [Fact]
    public async Task AddAsync_WhitespaceName_ThrowsException()
    {
        var dto = new ProductDTO { Name = "   ", Price = 9.99m, Stock = 10, Description = "d", Category = "c" };

        var act = async () => await _service.AddAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("*name is required*");
    }

    [Fact]
    public async Task AddAsync_ZeroPrice_ThrowsException()
    {
        var dto = new ProductDTO { Name = "Widget", Price = 0, Stock = 10, Description = "d", Category = "c" };

        var act = async () => await _service.AddAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Price must be greater than zero*");
    }

    [Fact]
    public async Task AddAsync_NegativePrice_ThrowsException()
    {
        var dto = new ProductDTO { Name = "Widget", Price = -1, Stock = 10, Description = "d", Category = "c" };

        var act = async () => await _service.AddAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Price must be greater than zero*");
    }

    [Fact]
    public async Task AddAsync_NegativeStock_ThrowsException()
    {
        var dto = new ProductDTO { Name = "Widget", Price = 9.99m, Stock = -1, Description = "d", Category = "c" };

        var act = async () => await _service.AddAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Stock cannot be negative*");
    }

    [Fact]
    public async Task AddAsync_NullImageUrl_SetsEmptyString()
    {
        var dto = new ProductDTO { Name = "Widget", Price = 9.99m, Stock = 5, Description = "d", Category = "c", ImageUrl = null };

        await _service.AddAsync(dto);

        _repoMock.Verify(r => r.AddAsync(It.Is<Product>(p => p.ImageUrl == string.Empty)), Times.Once);
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingProduct_ReturnsTrue()
    {
        var existing = new Product { ProductId = 1, Name = "Old", Price = 5, Stock = 1, Description = "d", Category = "c" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        var dto = new ProductDTO { Name = "New", Price = 15, Stock = 3, Description = "d2", Category = "c2" };
        var result = await _service.UpdateAsync(1, dto);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Name == "New" && p.Price == 15)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ProductNotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _service.UpdateAsync(999, new ProductDTO { Name = "X", Price = 1, Stock = 1, Description = "d", Category = "c" });

        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_InvalidId_ThrowsException()
    {
        var act = async () => await _service.UpdateAsync(0, new ProductDTO { Name = "X", Price = 1, Stock = 1, Description = "d", Category = "c" });

        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid product id*");
    }

    [Fact]
    public async Task UpdateAsync_NullImageUrl_KeepsExistingImageUrl()
    {
        var existing = new Product { ProductId = 1, Name = "Old", Price = 5, Stock = 1, Description = "d", Category = "c", ImageUrl = "existing-url.jpg" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        var dto = new ProductDTO { Name = "New", Price = 15, Stock = 3, Description = "d", Category = "c", ImageUrl = null };
        await _service.UpdateAsync(1, dto);

        _repoMock.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.ImageUrl == "existing-url.jpg")), Times.Once);
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingProduct_ReturnsTrue()
    {
        var existing = new Product { ProductId = 1, Name = "Widget", Price = 9.99m, Stock = 5, Description = "d", Category = "c" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);

        var result = await _service.DeleteAsync(1);

        result.Should().BeTrue();
        _repoMock.Verify(r => r.DeleteAsync(existing), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFalse()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _service.DeleteAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_InvalidId_ThrowsException()
    {
        var act = async () => await _service.DeleteAsync(-1);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid product id*");
    }
}
