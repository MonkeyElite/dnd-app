using System.Text.Json;
using DndApp.Sales.Contracts;
using DndApp.Sales.Data;
using DndApp.Sales.Data.Entities;
using DndApp.Sales.Options;
using DndShop.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DndApp.Sales.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/sales")]
public sealed class SalesController(
    SalesDbContext dbContext,
    IOptions<SalesOptions> salesOptions) : SalesControllerBase
{
    private readonly string _defaultCurrencyCode =
        string.IsNullOrWhiteSpace(salesOptions.Value.DefaultCurrencyCode)
            ? "GSC"
            : salesOptions.Value.DefaultCurrencyCode.Trim();

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateSaleRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetRequestingUserId(out var createdByUserId))
        {
            return Unauthorized();
        }

        var validationError = ValidateCreateRequest(request, out var normalizedRequest);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var now = DateTimeOffset.UtcNow;
        var sale = new SalesOrder
        {
            SaleId = Guid.NewGuid(),
            CampaignId = campaignId,
            Status = SaleStatus.Draft.ToString(),
            CustomerId = normalizedRequest.CustomerId,
            StorageLocationId = normalizedRequest.StorageLocationId,
            SoldWorldDay = normalizedRequest.SoldWorldDay,
            SubtotalMinor = 0,
            DiscountTotalMinor = 0,
            TaxTotalMinor = 0,
            TotalMinor = 0,
            Notes = normalizedRequest.Notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.SalesOrders.Add(sale);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateSaleResponse(sale.SaleId));
    }

    [HttpPut("{saleId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid campaignId,
        Guid saleId,
        [FromBody] UpdateSaleRequest request,
        CancellationToken cancellationToken)
    {
        var sale = await dbContext.SalesOrders
            .Include(x => x.Lines)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.SaleId == saleId, cancellationToken);

        if (sale is null)
        {
            return NotFound(new ErrorResponse("Sale not found."));
        }

        if (!sale.Status.Equals(SaleStatus.Draft.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new ErrorResponse("Only draft sales can be updated."));
        }

        var validationError = ValidateUpdateRequest(request, out var normalizedRequest);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var now = DateTimeOffset.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.SalesOrderLines.RemoveRange(sale.Lines);
        dbContext.SalesPayments.RemoveRange(sale.Payments);
        await dbContext.SaveChangesAsync(cancellationToken);

        var lines = new List<SalesOrderLine>(normalizedRequest.Lines.Count);
        foreach (var line in normalizedRequest.Lines)
        {
            lines.Add(new SalesOrderLine
            {
                SaleLineId = line.SaleLineId ?? Guid.NewGuid(),
                SaleId = sale.SaleId,
                ItemId = line.ItemId,
                Quantity = line.Quantity,
                UnitSoldPriceMinor = line.UnitSoldPriceMinor,
                UnitTrueValueMinor = line.UnitTrueValueMinor,
                DiscountMinor = line.DiscountMinor,
                Notes = line.Notes,
                LineSubtotalMinor = line.LineSubtotalMinor
            });
        }

        var payments = new List<SalesPayment>(normalizedRequest.Payments.Count);
        foreach (var payment in normalizedRequest.Payments)
        {
            payments.Add(new SalesPayment
            {
                PaymentId = payment.PaymentId ?? Guid.NewGuid(),
                SaleId = sale.SaleId,
                Method = payment.Method,
                AmountMinor = payment.AmountMinor,
                DetailsJson = payment.DetailsJson
            });
        }

        dbContext.SalesOrderLines.AddRange(lines);
        dbContext.SalesPayments.AddRange(payments);

        sale.SoldWorldDay = normalizedRequest.SoldWorldDay;
        sale.StorageLocationId = normalizedRequest.StorageLocationId;
        sale.CustomerId = normalizedRequest.CustomerId;
        sale.Notes = normalizedRequest.Notes;
        sale.SubtotalMinor = normalizedRequest.SubtotalMinor;
        sale.DiscountTotalMinor = normalizedRequest.DiscountTotalMinor;
        sale.TaxTotalMinor = 0;
        sale.TotalMinor = normalizedRequest.TotalMinor;
        sale.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new UpdateSaleResponse(Updated: true));
    }

    [HttpPost("{saleId:guid}/complete")]
    public async Task<IActionResult> CompleteAsync(
        Guid campaignId,
        Guid saleId,
        [FromBody] CompleteSaleRequest _,
        CancellationToken cancellationToken)
    {
        var sale = await dbContext.SalesOrders
            .Include(x => x.Lines)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.SaleId == saleId, cancellationToken);

        if (sale is null)
        {
            return NotFound(new ErrorResponse("Sale not found."));
        }

        if (!sale.Status.Equals(SaleStatus.Draft.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new ErrorResponse("Only draft sales can be completed."));
        }

        if (sale.Lines.Count == 0)
        {
            return BadRequest(new ErrorResponse("Sale must contain at least one line before completion."));
        }

        var recalculateError = RecalculateFromLines(sale.Lines, out var totals);
        if (recalculateError is not null)
        {
            return BadRequest(new ErrorResponse(recalculateError));
        }

        var now = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var completedEvent = new EventEnvelope<SaleCompletedEvent>
        {
            EventId = Guid.NewGuid(),
            EventType = SalesEventTypes.SaleCompletedV1,
            OccurredAt = now,
            CampaignId = sale.CampaignId,
            CorrelationId = correlationId,
            Data = new SaleCompletedEvent
            {
                SaleId = sale.SaleId,
                CampaignId = sale.CampaignId,
                SoldWorldDay = sale.SoldWorldDay,
                StorageLocationId = sale.StorageLocationId,
                CustomerId = sale.CustomerId,
                Total = new MoneyDto
                {
                    AmountMinor = totals.TotalMinor,
                    CurrencyCode = _defaultCurrencyCode
                },
                TaxTotal = new MoneyDto
                {
                    AmountMinor = 0,
                    CurrencyCode = _defaultCurrencyCode
                },
                Lines = sale.Lines
                    .Select(line => new SaleCompletedLine
                    {
                        ItemId = line.ItemId,
                        Quantity = line.Quantity,
                        UnitSoldPrice = new MoneyDto
                        {
                            AmountMinor = line.UnitSoldPriceMinor,
                            CurrencyCode = _defaultCurrencyCode
                        },
                        UnitTrueValue = new MoneyDto
                        {
                            AmountMinor = line.UnitTrueValueMinor ?? line.UnitSoldPriceMinor,
                            CurrencyCode = _defaultCurrencyCode
                        }
                    })
                    .ToList()
            }
        };

        var payloadJson = JsonSerializer.Serialize(completedEvent);
        var outboxMessage = new OutboxMessage
        {
            OutboxMessageId = Guid.NewGuid(),
            OccurredAt = now,
            Type = SalesEventTypes.SaleCompletedV1,
            AggregateId = sale.SaleId,
            CampaignId = sale.CampaignId,
            CorrelationId = correlationId,
            PayloadJson = payloadJson,
            PublishAttempts = 0
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        sale.Status = SaleStatus.Completed.ToString();
        sale.SubtotalMinor = totals.SubtotalMinor;
        sale.DiscountTotalMinor = totals.DiscountTotalMinor;
        sale.TaxTotalMinor = 0;
        sale.TotalMinor = totals.TotalMinor;
        sale.CompletedAt = now;
        sale.UpdatedAt = now;

        dbContext.OutboxMessages.Add(outboxMessage);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new CompleteSaleResponse(SaleStatus.Completed.ToString()));
    }

    [HttpPost("{saleId:guid}/void")]
    public async Task<IActionResult> VoidAsync(
        Guid campaignId,
        Guid saleId,
        [FromBody] VoidSaleRequest request,
        CancellationToken cancellationToken)
    {
        var reason = request.Reason?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(reason))
        {
            return BadRequest(new ErrorResponse("reason is required."));
        }

        if (reason.Length > 500)
        {
            return BadRequest(new ErrorResponse("reason must be 500 characters or fewer."));
        }

        var sale = await dbContext.SalesOrders
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.SaleId == saleId, cancellationToken);

        if (sale is null)
        {
            return NotFound(new ErrorResponse("Sale not found."));
        }

        if (!sale.Status.Equals(SaleStatus.Draft.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new ErrorResponse("Only draft sales can be voided."));
        }

        sale.Status = SaleStatus.Voided.ToString();
        sale.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new VoidSaleResponse(SaleStatus.Voided.ToString()));
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(
        Guid campaignId,
        [FromQuery] int? fromWorldDay,
        [FromQuery] int? toWorldDay,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        if (fromWorldDay.HasValue && fromWorldDay.Value < 0)
        {
            return BadRequest(new ErrorResponse("fromWorldDay must be greater than or equal to 0."));
        }

        if (toWorldDay.HasValue && toWorldDay.Value < 0)
        {
            return BadRequest(new ErrorResponse("toWorldDay must be greater than or equal to 0."));
        }

        if (fromWorldDay.HasValue && toWorldDay.HasValue && fromWorldDay.Value > toWorldDay.Value)
        {
            return BadRequest(new ErrorResponse("fromWorldDay must be less than or equal to toWorldDay."));
        }

        if (customerId.HasValue && customerId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("customerId must be a non-empty GUID when provided."));
        }

        var query = dbContext.SalesOrders
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId);

        if (fromWorldDay.HasValue)
        {
            query = query.Where(x => x.SoldWorldDay >= fromWorldDay.Value);
        }

        if (toWorldDay.HasValue)
        {
            query = query.Where(x => x.SoldWorldDay <= toWorldDay.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(x => x.CustomerId == customerId.Value);
        }

        var sales = await query
            .OrderByDescending(x => x.SoldWorldDay)
            .ThenByDescending(x => x.CreatedAt)
            .Select(x => new SaleListItemDto(
                x.SaleId,
                x.Status,
                x.SoldWorldDay,
                x.CustomerId,
                x.StorageLocationId,
                x.TotalMinor))
            .ToListAsync(cancellationToken);

        return Ok(sales);
    }

    [HttpGet("{saleId:guid}")]
    public async Task<IActionResult> GetByIdAsync(
        Guid campaignId,
        Guid saleId,
        CancellationToken cancellationToken)
    {
        var sale = await dbContext.SalesOrders
            .AsNoTracking()
            .Include(x => x.Lines)
            .Include(x => x.Payments)
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.SaleId == saleId, cancellationToken);

        if (sale is null)
        {
            return NotFound(new ErrorResponse("Sale not found."));
        }

        var response = new SaleDetailDto(
            sale.SaleId,
            sale.CampaignId,
            sale.Status,
            sale.SoldWorldDay,
            sale.CustomerId,
            sale.StorageLocationId,
            sale.Notes,
            new SaleTotalsDto(
                sale.SubtotalMinor,
                sale.DiscountTotalMinor,
                sale.TaxTotalMinor,
                sale.TotalMinor),
            sale.Lines
                .OrderBy(x => x.SaleLineId)
                .Select(x => new SaleLineDto(
                    x.SaleLineId,
                    x.ItemId,
                    x.Quantity,
                    x.UnitSoldPriceMinor,
                    x.UnitTrueValueMinor,
                    x.DiscountMinor,
                    x.Notes,
                    x.LineSubtotalMinor))
                .ToList(),
            sale.Payments
                .OrderBy(x => x.PaymentId)
                .Select(x => new SalePaymentDto(
                    x.PaymentId,
                    x.Method,
                    x.AmountMinor,
                    ParseJsonElement(x.DetailsJson)))
                .ToList());

        return Ok(response);
    }

    private static string? ValidateCreateRequest(
        CreateSaleRequest request,
        out NormalizedCreateSaleRequest normalizedRequest)
    {
        if (request.SoldWorldDay < 0)
        {
            normalizedRequest = default;
            return "soldWorldDay must be greater than or equal to 0.";
        }

        if (request.StorageLocationId == Guid.Empty)
        {
            normalizedRequest = default;
            return "storageLocationId is required.";
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value == Guid.Empty)
        {
            normalizedRequest = default;
            return "customerId must be a non-empty GUID when provided.";
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes?.Length > 500)
        {
            normalizedRequest = default;
            return "notes must be 500 characters or fewer.";
        }

        normalizedRequest = new NormalizedCreateSaleRequest(
            request.SoldWorldDay,
            request.StorageLocationId,
            request.CustomerId,
            notes);

        return null;
    }

    private static string? ValidateUpdateRequest(
        UpdateSaleRequest request,
        out NormalizedUpdateSaleRequest normalizedRequest)
    {
        if (request.SoldWorldDay < 0)
        {
            normalizedRequest = default;
            return "soldWorldDay must be greater than or equal to 0.";
        }

        if (request.StorageLocationId == Guid.Empty)
        {
            normalizedRequest = default;
            return "storageLocationId is required.";
        }

        if (request.CustomerId.HasValue && request.CustomerId.Value == Guid.Empty)
        {
            normalizedRequest = default;
            return "customerId must be a non-empty GUID when provided.";
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes?.Length > 500)
        {
            normalizedRequest = default;
            return "notes must be 500 characters or fewer.";
        }

        var lines = new List<NormalizedUpdateSaleLineRequest>();
        var seenLineIds = new HashSet<Guid>();
        foreach (var line in request.Lines ?? [])
        {
            if (line.ItemId == Guid.Empty)
            {
                normalizedRequest = default;
                return "line itemId is required.";
            }

            if (line.SaleLineId.HasValue && line.SaleLineId.Value == Guid.Empty)
            {
                normalizedRequest = default;
                return "saleLineId must be a non-empty GUID when provided.";
            }

            if (line.SaleLineId.HasValue && !seenLineIds.Add(line.SaleLineId.Value))
            {
                normalizedRequest = default;
                return "saleLineId values must be unique.";
            }

            var quantity = NormalizeQuantity(line.Quantity);
            if (quantity <= 0)
            {
                normalizedRequest = default;
                return "line quantity must be greater than 0.";
            }

            if (line.UnitSoldPriceMinor < 0)
            {
                normalizedRequest = default;
                return "line unitSoldPriceMinor must be greater than or equal to 0.";
            }

            if (line.UnitTrueValueMinor.HasValue && line.UnitTrueValueMinor.Value < 0)
            {
                normalizedRequest = default;
                return "line unitTrueValueMinor must be greater than or equal to 0.";
            }

            if (line.DiscountMinor < 0)
            {
                normalizedRequest = default;
                return "line discountMinor must be greater than or equal to 0.";
            }

            var grossMinor = NormalizeMinor(quantity * line.UnitSoldPriceMinor);
            if (line.DiscountMinor > grossMinor)
            {
                normalizedRequest = default;
                return "line discountMinor cannot exceed line gross amount.";
            }

            var lineSubtotalMinor = grossMinor - line.DiscountMinor;
            var lineNotes = string.IsNullOrWhiteSpace(line.Notes) ? null : line.Notes.Trim();
            if (lineNotes?.Length > 500)
            {
                normalizedRequest = default;
                return "line notes must be 500 characters or fewer.";
            }

            lines.Add(new NormalizedUpdateSaleLineRequest(
                line.SaleLineId,
                line.ItemId,
                quantity,
                line.UnitSoldPriceMinor,
                line.UnitTrueValueMinor,
                line.DiscountMinor,
                lineNotes,
                lineSubtotalMinor));
        }

        var payments = new List<NormalizedUpdateSalePaymentRequest>();
        var seenPaymentIds = new HashSet<Guid>();
        foreach (var payment in request.Payments ?? [])
        {
            if (payment.PaymentId.HasValue && payment.PaymentId.Value == Guid.Empty)
            {
                normalizedRequest = default;
                return "paymentId must be a non-empty GUID when provided.";
            }

            if (payment.PaymentId.HasValue && !seenPaymentIds.Add(payment.PaymentId.Value))
            {
                normalizedRequest = default;
                return "paymentId values must be unique.";
            }

            if (!Enum.TryParse<PaymentMethod>(payment.Method?.Trim(), ignoreCase: true, out var parsedMethod))
            {
                normalizedRequest = default;
                return "payment method is invalid.";
            }

            if (payment.AmountMinor <= 0)
            {
                normalizedRequest = default;
                return "payment amountMinor must be greater than 0.";
            }

            string? detailsJson = null;
            if (payment.Details.HasValue
                && payment.Details.Value.ValueKind is not JsonValueKind.Null and not JsonValueKind.Undefined)
            {
                detailsJson = payment.Details.Value.GetRawText();
            }

            payments.Add(new NormalizedUpdateSalePaymentRequest(
                payment.PaymentId,
                parsedMethod.ToString(),
                payment.AmountMinor,
                detailsJson));
        }

        long subtotalMinor;
        long discountTotalMinor;
        try
        {
            subtotalMinor = checked(lines.Sum(x => x.LineSubtotalMinor));
            discountTotalMinor = checked(lines.Sum(x => x.DiscountMinor));
        }
        catch (OverflowException)
        {
            normalizedRequest = default;
            return "totals exceed supported range.";
        }

        var totalMinor = subtotalMinor;
        normalizedRequest = new NormalizedUpdateSaleRequest(
            request.SoldWorldDay,
            request.StorageLocationId,
            request.CustomerId,
            notes,
            lines,
            payments,
            subtotalMinor,
            discountTotalMinor,
            totalMinor);

        return null;
    }

    private static string? RecalculateFromLines(
        IEnumerable<SalesOrderLine> lines,
        out RecalculatedTotals totals)
    {
        long subtotalMinor = 0;
        long discountTotalMinor = 0;

        foreach (var line in lines)
        {
            var quantity = NormalizeQuantity(line.Quantity);
            if (quantity <= 0)
            {
                totals = default;
                return "line quantity must be greater than 0.";
            }

            if (line.UnitSoldPriceMinor < 0)
            {
                totals = default;
                return "line unitSoldPriceMinor must be greater than or equal to 0.";
            }

            if (line.DiscountMinor < 0)
            {
                totals = default;
                return "line discountMinor must be greater than or equal to 0.";
            }

            var grossMinor = NormalizeMinor(quantity * line.UnitSoldPriceMinor);
            if (line.DiscountMinor > grossMinor)
            {
                totals = default;
                return "line discountMinor cannot exceed line gross amount.";
            }

            var lineSubtotalMinor = grossMinor - line.DiscountMinor;
            line.LineSubtotalMinor = lineSubtotalMinor;

            try
            {
                subtotalMinor = checked(subtotalMinor + lineSubtotalMinor);
                discountTotalMinor = checked(discountTotalMinor + line.DiscountMinor);
            }
            catch (OverflowException)
            {
                totals = default;
                return "totals exceed supported range.";
            }
        }

        totals = new RecalculatedTotals(subtotalMinor, discountTotalMinor, subtotalMinor);
        return null;
    }

    private static JsonElement? ParseJsonElement(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private readonly record struct NormalizedCreateSaleRequest(
        int SoldWorldDay,
        Guid StorageLocationId,
        Guid? CustomerId,
        string? Notes);

    private readonly record struct NormalizedUpdateSaleRequest(
        int SoldWorldDay,
        Guid StorageLocationId,
        Guid? CustomerId,
        string? Notes,
        IReadOnlyList<NormalizedUpdateSaleLineRequest> Lines,
        IReadOnlyList<NormalizedUpdateSalePaymentRequest> Payments,
        long SubtotalMinor,
        long DiscountTotalMinor,
        long TotalMinor);

    private readonly record struct NormalizedUpdateSaleLineRequest(
        Guid? SaleLineId,
        Guid ItemId,
        decimal Quantity,
        long UnitSoldPriceMinor,
        long? UnitTrueValueMinor,
        long DiscountMinor,
        string? Notes,
        long LineSubtotalMinor);

    private readonly record struct NormalizedUpdateSalePaymentRequest(
        Guid? PaymentId,
        string Method,
        long AmountMinor,
        string? DetailsJson);

    private readonly record struct RecalculatedTotals(
        long SubtotalMinor,
        long DiscountTotalMinor,
        long TotalMinor);
}
