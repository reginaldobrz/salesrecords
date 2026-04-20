using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Functional.Helpers;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// Functional tests for GET /api/sales (paginated list).
/// </summary>
public class GetSalesTests : IClassFixture<SalesApiFactory>
{
    private readonly SalesApiFactory _factory;
    private readonly HttpClient _client;

    public GetSalesTests(SalesApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        JwtTokenHelper.AddAuthHeader(_client);
    }

    // ── Authentication ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given no auth token When GET /api/sales Then returns 401")]
    public async Task Given_NoAuthToken_When_GetSales_Then_Returns401()
    {
        // Given
        var unauthClient = _factory.CreateClient();

        // When
        var response = await unauthClient.GetAsync("/api/sales?_page=1&_size=10");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given empty database When GET /api/sales Then returns 200 with empty list")]
    public async Task Given_EmptyDatabase_When_GetSales_Then_ReturnsEmptyList()
    {
        // No pre-seeded data — just query
        // When
        var response = await _client.GetAsync("/api/sales?_page=1&_size=10");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<PaginatedSalesDto>>(JsonOptions.CaseInsensitive);
        envelope!.Success.Should().BeTrue();
        envelope.Data!.Data.Should().NotBeNull();
        envelope.Data.CurrentPage.Should().Be(1);
    }

    [Fact(DisplayName = "Given existing sales When GET /api/sales Then returns 200 with correct pagination")]
    public async Task Given_ExistingSales_When_GetSales_Then_ReturnsPaginatedData()
    {
        // Given — seed 3 sales
        for (int i = 0; i < 3; i++)
        {
            var createRequest = SaleRequestFactory.Valid(quantity: 2, unitPrice: 10m);
            createRequest.CustomerName = $"PaginationTestCustomer_{i + 1}";
            var r = await _client.PostAsJsonAsync("/api/sales", createRequest);
            r.EnsureSuccessStatusCode();
        }

        // When — page=1, size=2
        var response = await _client.GetAsync("/api/sales?_page=1&_size=2");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<PaginatedSalesDto>>(JsonOptions.CaseInsensitive);
        envelope!.Success.Should().BeTrue();

        var paged = envelope.Data!;
        paged.Data.Should().HaveCount(2);
        paged.CurrentPage.Should().Be(1);
        paged.TotalItems.Should().BeGreaterThanOrEqualTo(3);
        paged.TotalPages.Should().BeGreaterThanOrEqualTo(2);
    }

    // ── Validation errors ─────────────────────────────────────────────────────

    [Fact(DisplayName = "Given _page=0 When GET /api/sales Then returns 400")]
    public async Task Given_PageZero_When_GetSales_Then_Returns400()
    {
        // When
        var response = await _client.GetAsync("/api/sales?_page=0&_size=10");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Given _size=101 When GET /api/sales Then returns 400")]
    public async Task Given_SizeOver100_When_GetSales_Then_Returns400()
    {
        // When
        var response = await _client.GetAsync("/api/sales?_page=1&_size=101");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
