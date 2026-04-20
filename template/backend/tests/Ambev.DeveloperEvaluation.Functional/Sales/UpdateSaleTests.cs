using System.Net;
using System.Net.Http.Json;
using Ambev.DeveloperEvaluation.Functional.Helpers;
using FluentAssertions;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Sales;

/// <summary>
/// Functional tests for PUT /api/sales/{id}.
/// </summary>
public class UpdateSaleTests : IClassFixture<SalesApiFactory>
{
    private readonly SalesApiFactory _factory;
    private readonly HttpClient _client;

    public UpdateSaleTests(SalesApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        JwtTokenHelper.AddAuthHeader(_client);
    }

    // ── Authentication ────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given no auth token When PUT /api/sales/{id} Then returns 401")]
    public async Task Given_NoAuthToken_When_UpdateSale_Then_Returns401()
    {
        // Given — fresh unauthenticated client via factory
        var unauthClient = _factory.CreateClient();
        var body = SaleRequestFactory.ToUpdate(SaleRequestFactory.Valid(), 3);

        // When
        var response = await unauthClient.PutAsJsonAsync($"/api/sales/{Guid.NewGuid()}", body);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given existing sale When PUT /api/sales/{id} Then returns 200 with updated data")]
    public async Task Given_ExistingSale_When_UpdateSale_Then_Returns200WithUpdatedData()
    {
        // Given — create a sale
        var createRequest = SaleRequestFactory.Valid(quantity: 3, unitPrice: 40m);
        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        var saleId = created!.Data!.Id;

        // When — update it
        var updateRequest = SaleRequestFactory.ToUpdate(createRequest, newQuantity: 10);
        var updateResponse = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        // Then
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await updateResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        envelope!.Success.Should().BeTrue();

        var sale = envelope.Data!;
        sale.Id.Should().Be(saleId);
        sale.CustomerName.Should().Contain("(updated)");
        sale.Items[0].Quantity.Should().Be(10);
        sale.Items[0].Discount.Should().Be(0.20m); // qty=10 → 20%
    }

    // ── Error cases ───────────────────────────────────────────────────────────

    [Fact(DisplayName = "Given non-existent id When PUT /api/sales/{id} Then returns 404")]
    public async Task Given_NonExistentId_When_UpdateSale_Then_Returns404()
    {
        // Given
        var updateRequest = SaleRequestFactory.ToUpdate(SaleRequestFactory.Valid(), 3);

        // When
        var response = await _client.PutAsJsonAsync($"/api/sales/{Guid.NewGuid()}", updateRequest);

        // Then
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "Given cancelled sale When PUT /api/sales/{id} Then returns 400")]
    public async Task Given_CancelledSale_When_UpdateSale_Then_Returns400()
    {
        // Given — create, then cancel
        var createRequest = SaleRequestFactory.Valid(quantity: 2, unitPrice: 10m);
        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        var saleId = created!.Data!.Id;

        // Cancel via DELETE
        var deleteResponse = await _client.DeleteAsync($"/api/sales/{saleId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // When — try to update the cancelled sale
        var updateRequest = SaleRequestFactory.ToUpdate(createRequest, newQuantity: 5);
        var updateResponse = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        // Then
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "Given qty=21 in update When PUT /api/sales/{id} Then returns 400")]
    public async Task Given_InvalidQty_When_UpdateSale_Then_Returns400()
    {
        // Given — create a sale
        var createRequest = SaleRequestFactory.Valid(quantity: 2, unitPrice: 10m);
        var createResponse = await _client.PostAsJsonAsync("/api/sales", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ApiEnvelope<SaleDto>>(JsonOptions.CaseInsensitive);
        var saleId = created!.Data!.Id;

        // When — try to update with invalid quantity
        var updateRequest = SaleRequestFactory.ToUpdate(createRequest, newQuantity: 21);
        var updateResponse = await _client.PutAsJsonAsync($"/api/sales/{saleId}", updateRequest);

        // Then
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
