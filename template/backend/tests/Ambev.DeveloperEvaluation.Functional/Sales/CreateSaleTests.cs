using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ambev.DeveloperEvaluation.Functional.Helpers;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// Functional tests for POST /api/sales.
/// Validates HTTP-level behaviour including authentication, discount application,
/// and request validation enforced by the full pipeline.
/// </summary>
public class CreateSaleTests : IClassFixture<SalesApiFactory>
{
    private readonly SalesApiFactory _factory;
    private readonly HttpClient _client;

    public CreateSaleTests(SalesApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        JwtTokenHelper.AddAuthHeader(_client);
    }

    // ── Authentication ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given no auth token When POST /api/sales Then returns 401 Unauthorized")]
    public async Task Given_NoAuthToken_When_CreateSale_Then_Returns401()
    {
        // Given — use a fresh client without auth headers
        var unauthClient = _factory.CreateClient();
        var request = SaleRequestFactory.Valid();

        // When
        var response = await unauthClient.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Discount tiers ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given qty=2 (below 4) When POST /api/sales Then 0% discount applied")]
    public async Task Given_Qty2_When_CreateSale_Then_NoDiscount()
    {
        // Given
        var request = SaleRequestFactory.Valid(quantity: 2, unitPrice: 100m);

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        envelope!.Success.Should().BeTrue();
        envelope.Data!.Items.Should().HaveCount(1);

        var item = envelope.Data.Items[0];
        item.Discount.Should().Be(0m);
        item.TotalAmount.Should().Be(2 * 100m * (1 - 0m)); // 200.00
        envelope.Data.TotalAmount.Should().Be(item.TotalAmount);
    }

    [Fact(DisplayName = "Given qty=4 (first discount tier) When POST /api/sales Then 10% discount applied")]
    public async Task Given_Qty4_When_CreateSale_Then_10PercentDiscount()
    {
        // Given
        var request = SaleRequestFactory.Valid(quantity: 4, unitPrice: 100m);

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        var item = envelope!.Data!.Items[0];

        item.Discount.Should().Be(0.10m);
        item.TotalAmount.Should().Be(4 * 100m * (1 - 0.10m)); // 360.00
    }

    [Fact(DisplayName = "Given qty=10 (second discount tier) When POST /api/sales Then 20% discount applied")]
    public async Task Given_Qty10_When_CreateSale_Then_20PercentDiscount()
    {
        // Given
        var request = SaleRequestFactory.Valid(quantity: 10, unitPrice: 50m);

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        var item = envelope!.Data!.Items[0];

        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(10 * 50m * (1 - 0.20m)); // 400.00
    }

    [Fact(DisplayName = "Given qty=20 (max allowed) When POST /api/sales Then 20% discount applied")]
    public async Task Given_Qty20_When_CreateSale_Then_20PercentDiscount()
    {
        // Given
        var request = SaleRequestFactory.Valid(quantity: 20, unitPrice: 10m);

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        var item = envelope!.Data!.Items[0];

        item.Discount.Should().Be(0.20m);
        item.TotalAmount.Should().Be(20 * 10m * (1 - 0.20m)); // 160.00
    }

    // ── Validation errors ─────────────────────────────────────────────────────

    [Fact(DisplayName = "Given qty=21 (above limit) When POST /api/sales Then returns 400")]
    public async Task Given_Qty21_When_CreateSale_Then_Returns400()
    {
        // Given
        var request = SaleRequestFactory.Valid(quantity: 21, unitPrice: 10m);

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Given empty CustomerName When POST /api/sales Then returns 400")]
    public async Task Given_EmptyCustomerName_When_CreateSale_Then_Returns400()
    {
        // Given
        var request = SaleRequestFactory.Valid();
        request.CustomerName = string.Empty;

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Given no items When POST /api/sales Then returns 400")]
    public async Task Given_NoItems_When_CreateSale_Then_Returns400()
    {
        // Given
        var request = SaleRequestFactory.Valid();
        request.Items.Clear();

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Response shape ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given valid request When POST /api/sales Then response contains id and isCancelled=false")]
    public async Task Given_ValidRequest_When_CreateSale_Then_ResponseShapeIsCorrect()
    {
        // Given
        var request = SaleRequestFactory.Valid(quantity: 5, unitPrice: 20m);

        // When
        var response = await _client.PostAsJsonAsync("/api/sales", request);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        envelope!.Success.Should().BeTrue();

        var sale = envelope.Data!;
        sale.Id.Should().NotBeEmpty();
        sale.IsCancelled.Should().BeFalse();
        sale.CustomerName.Should().Be(request.CustomerName);
        sale.BranchName.Should().Be(request.BranchName);
        sale.Items.Should().HaveCount(1);
        sale.TotalAmount.Should().BeGreaterThan(0);
    }
}
