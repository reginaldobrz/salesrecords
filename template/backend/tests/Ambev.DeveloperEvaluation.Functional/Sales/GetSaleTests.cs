using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Functional.Helpers;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// Functional tests for GET /api/sales/{id}.
/// </summary>
public class GetSaleTests : IClassFixture<SalesApiFactory>
{
    private readonly SalesApiFactory _factory;
    private readonly HttpClient _client;

    public GetSaleTests(SalesApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        JwtTokenHelper.AddAuthHeader(_client);
    }

    // ── Authentication ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given no auth token When GET /api/sales/{id} Then returns 401")]
    public async Task Given_NoAuthToken_When_GetSaleById_Then_Returns401()
    {
        // Given — fresh unauthenticated client via factory (routes to test server)
        var unauthClient = _factory.CreateClient();

        // When
        var response = await unauthClient.GetAsync($"/api/sales/{Guid.NewGuid()}");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given existing sale When GET /api/sales/{id} Then returns 200 with sale details")]
    public async Task Given_ExistingSale_When_GetSaleById_Then_Returns200WithSale()
    {
        // Given — create a sale first
        var createRequest = SaleRequestFactory.Valid(quantity: 5, unitPrice: 30m);
        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        var saleId = created!.Data!.Id;

        // When
        var response = await _client.GetAsync($"/api/sales/{saleId}");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        envelope!.Success.Should().BeTrue();

        var sale = envelope.Data!;
        sale.Id.Should().Be(saleId);
        sale.CustomerName.Should().Be(createRequest.CustomerName);
        sale.IsCancelled.Should().BeFalse();
        sale.Items.Should().HaveCount(1);
        sale.Items[0].Discount.Should().Be(0.10m); // qty=5 → 10%
    }

    // ── Error cases ───────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given non-existent id When GET /api/sales/{id} Then returns 404")]
    public async Task Given_NonExistentId_When_GetSaleById_Then_Returns404()
    {
        // Given
        var unknownId = Guid.NewGuid();

        // When
        var response = await _client.GetAsync($"/api/sales/{unknownId}");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Given empty GUID When GET /api/sales/{id} Then returns 400")]
    public async Task Given_EmptyGuid_When_GetSaleById_Then_Returns400()
    {
        // Given — Guid.Empty is an invalid identifier per business rules
        var emptyId = Guid.Empty;

        // When
        var response = await _client.GetAsync($"/api/sales/{emptyId}");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
