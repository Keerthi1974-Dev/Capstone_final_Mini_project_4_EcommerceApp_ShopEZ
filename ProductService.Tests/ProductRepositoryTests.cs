using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;
using ProductService.Repositories;
using Xunit;

namespace ProductService.Tests;

public class ProductRepositoryTests : IDisposable
{
    private readonly ProductDbContext _context;
    private readonly ProductRepository _repo;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ProductDbContext(options);
        _repo = new ProductRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private Product MakeProduct(string name = "Widget", decimal price = 9.99m, int stock = 10)
        => new() { Name = name, Price = price, Stock = stock, Description = "desc", Category = "cat", ImageUrl = "" };

    // ── GetAllAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        _context.Products.AddRange(MakeProduct("A"), MakeProduct("B"));
        await _context.SaveChangesAsync();

        var result = await _repo.GetAllAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDb_ReturnsEmptyList()
    {
        var result = await _repo.GetAllAsync();

        result.Should().BeEmpty();
    }

    // ── GetByIdAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProduct()
    {
        var p = MakeProduct();
        _context.Products.Add(p);
        await _context.SaveChangesAsync();

        var result = await _repo.GetByIdAsync(p.ProductId);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Widget");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        var result = await _repo.GetByIdAsync(999);

        result.Should().BeNull();
    }

    // ── AddAsync ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidProduct_PersistsToDb()
    {
        var p = MakeProduct();

        await _repo.AddAsync(p);

        _context.Products.Should().HaveCount(1);
    }

    [Fact]
    public async Task AddAsync_NullProduct_ThrowsArgumentNullException()
    {
        var act = async () => await _repo.AddAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ── UpdateAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesFields()
    {
        var p = MakeProduct();
        _context.Products.Add(p);
        await _context.SaveChangesAsync();

        p.Name = "Updated";
        p.Price = 99m;
        await _repo.UpdateAsync(p);

        var saved = await _context.Products.FindAsync(p.ProductId);
        saved!.Name.Should().Be("Updated");
        saved.Price.Should().Be(99m);
    }

    [Fact]
    public async Task UpdateAsync_NullProduct_ThrowsArgumentNullException()
    {
        var act = async () => await _repo.UpdateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_NonExistentProduct_ThrowsException()
    {
        var ghost = MakeProduct();
        ghost.ProductId = 999;

        var act = async () => await _repo.UpdateAsync(ghost);

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }

    // ── DeleteAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingProduct_RemovesFromDb()
    {
        var p = MakeProduct();
        _context.Products.Add(p);
        await _context.SaveChangesAsync();

        await _repo.DeleteAsync(p);

        _context.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_NullProduct_ThrowsArgumentNullException()
    {
        var act = async () => await _repo.DeleteAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentProduct_ThrowsException()
    {
        var ghost = MakeProduct();
        ghost.ProductId = 999;

        var act = async () => await _repo.DeleteAsync(ghost);

        await act.Should().ThrowAsync<Exception>().WithMessage("*not found*");
    }
}
