using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Functional.Helpers;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// Functional tests for DELETE /api/sales/{id}.
/// DELETE performs a soft-delete: IsCancelled is set to true; data is not removed.
/// </summary>
public class DeleteSaleTests : IClassFixture<SalesApiFactory>
{
    private readonly SalesApiFactory _factory;
    private readonly HttpClient _client;

    public DeleteSaleTests(SalesApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        JwtTokenHelper.AddAuthHeader(_client);
    }

    // ── Authentication ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given no auth token When DELETE /api/sales/{id} Then returns 401")]
    public async Task Given_NoAuthToken_When_DeleteSale_Then_Returns401()
    {
        // Given — fresh unauthenticated client via factory
        var unauthClient = _factory.CreateClient();

        // When
        var response = await unauthClient.DeleteAsync($"/api/sales/{Guid.NewGuid()}");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given existing sale When DELETE /api/sales/{id} Then returns 200 and sale is soft-deleted")]
    public async Task Given_ExistingSale_When_DeleteSale_Then_Returns200AndSaleIsCancelled()
    {
        // Given — create a sale
        var createRequest = SaleRequestFactory.Valid(quantity: 3, unitPrice: 25m);
        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        var saleId = created!.Data!.Id;

        // When — soft-delete
        var deleteResponse = await _client.DeleteAsync($"/api/sales/{saleId}");

        // Then — delete returns 200
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // And the sale still exists but IsCancelled = true (soft-delete verification via GET)
        var getResponse = await _client.GetAsync($"/api/sales/{saleId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await getResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        envelope!.Data!.Id.Should().Be(saleId);
        envelope.Data.IsCancelled.Should().BeTrue();
    }

    // ── Error cases ───────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given non-existent id When DELETE /api/sales/{id} Then returns 404")]
    public async Task Given_NonExistentId_When_DeleteSale_Then_Returns404()
    {
        // Given
        var unknownId = Guid.NewGuid();

        // When
        var response = await _client.DeleteAsync($"/api/sales/{unknownId}");

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Given soft-deleted sale When GET /api/sales/{id} Then still returns the record")]
    public async Task Given_SoftDeletedSale_When_GetById_Then_RecordStillExists()
    {
        // Given — create and cancel a sale
        var createRequest = SaleRequestFactory.Valid(quantity: 2, unitPrice: 15m);
        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var saleId = (await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive))!.Data!.Id;

        await _client.DeleteAsync($"/api/sales/{saleId}");

        // When — retrieve the soft-deleted sale
        var response = await _client.GetAsync($"/api/sales/{saleId}");

        // Then — record is still accessible
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await response.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        envelope!.Data!.IsCancelled.Should().BeTrue();
        envelope.Data.CustomerName.Should().Be(createRequest.CustomerName);
    }

    [Fact(DisplayName = "Given already-cancelled sale When DELETE /api/sales/{id} Then returns 200 or 400")]
    public async Task Given_AlreadyCancelledSale_When_DeleteSale_Then_ReturnsOkOrBadRequest()
    {
        // Given — create and then cancel a sale
        var createRequest = SaleRequestFactory.Valid(quantity: 2, unitPrice: 10m);
        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var saleId = (await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive))!.Data!.Id;

        await _client.DeleteAsync($"/api/sales/{saleId}");

        // When — attempt to cancel again
        var secondDelete = await _client.DeleteAsync($"/api/sales/{saleId}");

        // Then — domain may return either idempotent 200 or 400 (already cancelled)
        secondDelete.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }
}
