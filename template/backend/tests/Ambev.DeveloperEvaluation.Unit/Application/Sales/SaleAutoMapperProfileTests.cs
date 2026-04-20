using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.GetSales;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Domain.Entities;
using AutoMapper;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.Application.Sales;

/// <summary>
/// Tests that exercise AutoMapper profiles for the Sales feature,
/// ensuring all profile configurations and mapped DTO/result classes are covered.
/// </summary>
public class SaleAutoMapperProfileTests
{
    // ────────────────────────── CreateSaleProfile ──────────────────────────

    [Fact(DisplayName = "CreateSaleProfile: maps CreateSaleItemDto to SaleItem correctly")]
    public void CreateSaleProfile_Maps_CreateSaleItemDto_To_SaleItem()
    {
        // Given — use MemberList.Source so only source properties need to be mapped;
        // extra SaleItem destination properties are set programmatically by the handler.
        var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<CreateSaleItemDto, SaleItem>(MemberList.None));
        var mapper = config.CreateMapper();

        var dto = new CreateSaleItemDto
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Widget",
            Quantity = 5,
            UnitPrice = 9.99m
        };

        // When
        var item = mapper.Map<SaleItem>(dto);

        // Then
        item.ProductId.Should().Be(dto.ProductId);
        item.ProductName.Should().Be(dto.ProductName);
        item.Quantity.Should().Be(dto.Quantity);
        item.UnitPrice.Should().Be(dto.UnitPrice);
    }

    [Fact(DisplayName = "CreateSaleProfile: maps SaleItem to CreateSaleItemResult correctly")]
    public void CreateSaleProfile_Maps_SaleItem_To_CreateSaleItemResult()
    {
        // Given
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CreateSaleProfile>());
        var mapper = config.CreateMapper();

        var item = new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Widget",
            Quantity = 5,
            UnitPrice = 9.99m,
            Discount = 0.10m,
            TotalAmount = 44.955m
        };

        // When
        var result = mapper.Map<CreateSaleItemResult>(item);

        // Then
        result.Id.Should().Be(item.Id);
        result.ProductId.Should().Be(item.ProductId);
        result.ProductName.Should().Be(item.ProductName);
        result.Quantity.Should().Be(item.Quantity);
        result.Discount.Should().Be(item.Discount);
        result.TotalAmount.Should().Be(item.TotalAmount);
    }

    [Fact(DisplayName = "CreateSaleProfile: maps Sale to CreateSaleResult including items")]
    public void CreateSaleProfile_Maps_Sale_To_CreateSaleResult()
    {
        // Given
        var config = new MapperConfiguration(cfg => cfg.AddProfile<CreateSaleProfile>());
        var mapper = config.CreateMapper();

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 42,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Acme",
            BranchId = Guid.NewGuid(),
            BranchName = "HQ",
            TotalAmount = 100m,
            IsCancelled = false
        };

        var item = new SaleItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Widget",
            Quantity = 4,
            UnitPrice = 25m,
            Discount = 0.10m,
            TotalAmount = 90m
        };
        item.CalculateDiscount();
        sale.AddItem(item);

        // When
        var result = mapper.Map<CreateSaleResult>(sale);

        // Then
        result.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.CustomerName.Should().Be(sale.CustomerName);
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductName.Should().Be("Widget");
    }

    // ────────────────────────── GetSaleProfile ──────────────────────────

    [Fact(DisplayName = "GetSaleProfile: maps SaleItem to GetSaleItemResult correctly")]
    public void GetSaleProfile_Maps_SaleItem_To_GetSaleItemResult()
    {
        // Given
        var config = new MapperConfiguration(cfg => cfg.AddProfile<GetSaleProfile>());
        config.AssertConfigurationIsValid();
        var mapper = config.CreateMapper();

        var item = new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Gadget",
            Quantity = 10,
            UnitPrice = 5m,
            Discount = 0.20m,
            TotalAmount = 40m,
            IsCancelled = false
        };

        // When
        var result = mapper.Map<GetSaleItemResult>(item);

        // Then
        result.Id.Should().Be(item.Id);
        result.ProductId.Should().Be(item.ProductId);
        result.ProductName.Should().Be(item.ProductName);
        result.Quantity.Should().Be(item.Quantity);
        result.Discount.Should().Be(item.Discount);
        result.TotalAmount.Should().Be(item.TotalAmount);
    }

    [Fact(DisplayName = "GetSaleProfile: maps Sale to GetSaleResult including items")]
    public void GetSaleProfile_Maps_Sale_To_GetSaleResult()
    {
        // Given
        var config = new MapperConfiguration(cfg => cfg.AddProfile<GetSaleProfile>());
        var mapper = config.CreateMapper();

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 7,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Beta Corp",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch B",
            TotalAmount = 200m,
            IsCancelled = false
        };

        var item = new SaleItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Gadget",
            Quantity = 5,
            UnitPrice = 40m
        };
        item.CalculateDiscount();
        sale.AddItem(item);

        // When
        var result = mapper.Map<GetSaleResult>(sale);

        // Then
        result.Id.Should().Be(sale.Id);
        result.CustomerName.Should().Be(sale.CustomerName);
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductName.Should().Be("Gadget");
    }

    // ────────────────────────── GetSalesProfile ──────────────────────────

    [Fact(DisplayName = "GetSalesProfile: maps Sale to GetSalesItemResult correctly")]
    public void GetSalesProfile_Maps_Sale_To_GetSalesItemResult()
    {
        // Given
        var config = new MapperConfiguration(cfg => cfg.AddProfile<GetSalesProfile>());
        config.AssertConfigurationIsValid();
        var mapper = config.CreateMapper();

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 3,
            SaleDate = DateTime.UtcNow.AddDays(-1),
            CustomerId = Guid.NewGuid(),
            CustomerName = "Gamma Ltd",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch C",
            TotalAmount = 500m,
            IsCancelled = false
        };

        // When
        var result = mapper.Map<GetSalesItemResult>(sale);

        // Then
        result.Id.Should().Be(sale.Id);
        result.SaleNumber.Should().Be(sale.SaleNumber);
        result.CustomerName.Should().Be(sale.CustomerName);
        result.BranchName.Should().Be(sale.BranchName);
        result.TotalAmount.Should().Be(sale.TotalAmount);
        result.IsCancelled.Should().BeFalse();
    }

    // ────────────────────────── UpdateSaleProfile ──────────────────────────

    [Fact(DisplayName = "UpdateSaleProfile: maps UpdateSaleItemDto to SaleItem correctly")]
    public void UpdateSaleProfile_Maps_UpdateSaleItemDto_To_SaleItem()
    {
        // Given — use MemberList.Source so only source properties need to be mapped;
        // extra SaleItem destination properties are set programmatically by the handler.
        var config = new MapperConfiguration(cfg =>
            cfg.CreateMap<UpdateSaleItemDto, SaleItem>(MemberList.None));
        var mapper = config.CreateMapper();

        var dto = new UpdateSaleItemDto
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Updated Widget",
            Quantity = 8,
            UnitPrice = 15m
        };

        // When
        var item = mapper.Map<SaleItem>(dto);

        // Then
        item.ProductId.Should().Be(dto.ProductId);
        item.ProductName.Should().Be(dto.ProductName);
        item.Quantity.Should().Be(dto.Quantity);
        item.UnitPrice.Should().Be(dto.UnitPrice);
    }

    [Fact(DisplayName = "UpdateSaleProfile: maps SaleItem to UpdateSaleItemResult correctly")]
    public void UpdateSaleProfile_Maps_SaleItem_To_UpdateSaleItemResult()
    {
        // Given
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UpdateSaleProfile>());
        var mapper = config.CreateMapper();

        var item = new SaleItem
        {
            Id = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            ProductName = "Updated Widget",
            Quantity = 8,
            UnitPrice = 15m,
            Discount = 0.10m,
            TotalAmount = 108m
        };

        // When
        var result = mapper.Map<UpdateSaleItemResult>(item);

        // Then
        result.Id.Should().Be(item.Id);
        result.ProductName.Should().Be(item.ProductName);
        result.Quantity.Should().Be(item.Quantity);
        result.Discount.Should().Be(item.Discount);
        result.TotalAmount.Should().Be(item.TotalAmount);
    }

    [Fact(DisplayName = "UpdateSaleProfile: maps Sale to UpdateSaleResult including items")]
    public void UpdateSaleProfile_Maps_Sale_To_UpdateSaleResult()
    {
        // Given
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UpdateSaleProfile>());
        var mapper = config.CreateMapper();

        var sale = new Sale
        {
            Id = Guid.NewGuid(),
            SaleNumber = 99,
            SaleDate = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            CustomerName = "Delta Inc",
            BranchId = Guid.NewGuid(),
            BranchName = "Branch D",
            TotalAmount = 800m,
            IsCancelled = false
        };

        var item = new SaleItem
        {
            ProductId = Guid.NewGuid(),
            ProductName = "Updated Widget",
            Quantity = 8,
            UnitPrice = 100m
        };
        item.CalculateDiscount();
        sale.AddItem(item);

        // When
        var result = mapper.Map<UpdateSaleResult>(sale);

        // Then
        result.Id.Should().Be(sale.Id);
        result.CustomerName.Should().Be(sale.CustomerName);
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductName.Should().Be("Updated Widget");
    }
}
