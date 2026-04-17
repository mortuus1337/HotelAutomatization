using System.Security.Claims;
using Hotel.Application.Interfaces;
using Hotel.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Admin + "," + Roles.Owner)]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet("service-contract/{reservationId:int}/pdf")]
    public async Task<IActionResult> DownloadServiceContractPdf(
        int reservationId,
        CancellationToken cancellationToken)
    {
        var currentUserId = TryGetCurrentUserId();
        var file = await _documentService.GenerateServiceContractPdfAsync(reservationId, currentUserId, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("checkout-act/{stayId:int}/pdf")]
    public async Task<IActionResult> DownloadCheckoutActPdf(
        int stayId,
        CancellationToken cancellationToken)
    {
        var currentUserId = TryGetCurrentUserId();
        var file = await _documentService.GenerateCheckoutActPdfAsync(stayId, currentUserId, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("invoice/{reservationId:int}/pdf")]
    public async Task<IActionResult> DownloadInvoicePdf(
        int reservationId,
        CancellationToken cancellationToken)
    {
        var currentUserId = TryGetCurrentUserId();
        var file = await _documentService.GenerateInvoicePdfAsync(reservationId, currentUserId, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? documentType,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var history = await _documentService.GetHistoryAsync(documentType, from, to, limit, cancellationToken);
        return Ok(history);
    }

    private int? TryGetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
