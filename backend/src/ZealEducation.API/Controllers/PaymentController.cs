using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.Payments.Commands.ConfirmPayment;
using ZealEducation.Application.Features.Payments.Commands.SetPaymentType;
using ZealEducation.Application.Features.Payments.Queries.GetBankTransferProof;
using ZealEducation.Application.Features.Payments.Queries.GetFeeStructureDetail;
using ZealEducation.Application.Features.Payments.Queries.GetFeeStructures;
using ZealEducation.Application.Features.Payments.Queries.GetPaymentReceipt;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentController(ISender sender) : ControllerBase
{
    private const string ReadRoles = nameof(UserRole.AccountsStaff) + "," + nameof(UserRole.Incharge);
    private const string WriteRoles = nameof(UserRole.AccountsStaff);

    [HttpGet("fee-structures")]
    [Authorize(Roles = ReadRoles)]
    public async Task<ActionResult<ApiResponse<PaginatedList<FeeStructureListItemDto>>>> GetFeeStructures(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] PaymentStatus? status = null,
        [FromQuery] FeeType? type = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetFeeStructuresQuery(page, pageSize, search, status, type, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<FeeStructureListItemDto>>.Success(result));
    }

    [HttpGet("fee-structures/{feeId:guid}")]
    [Authorize(Roles = ReadRoles)]
    public async Task<ActionResult<ApiResponse<FeeStructureDetailDto>>> GetFeeStructureDetail(Guid feeId)
    {
        var result = await sender.Send(new GetFeeStructureDetailQuery(feeId));
        return Ok(ApiResponse<FeeStructureDetailDto>.Success(result));
    }

    [HttpPut("fee-structures/{feeId:guid}/payment-type")]
    [Authorize(Roles = WriteRoles)]
    public async Task<ActionResult<ApiResponse<SetPaymentTypeResponse>>> SetPaymentType(
        Guid feeId,
        [FromBody] SetPaymentTypeRequest body)
    {
        var command = new SetPaymentTypeCommand(feeId, body.PaymentType, body.Frequency);
        var result = await sender.Send(command);
        return Ok(ApiResponse<SetPaymentTypeResponse>.Success(result, "Payment type updated successfully"));
    }

    [HttpPost("fee-structures/{feeId:guid}/confirm")]
    [Authorize(Roles = WriteRoles)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<ActionResult<ApiResponse<ConfirmPaymentResponse>>> ConfirmPayment(
        Guid feeId,
        [FromForm] ConfirmPaymentForm form)
    {
        Stream? proofStream = null;
        string? proofContentType = null;
        string? proofFileName = null;
        long proofLength = 0;

        if (form.BankTransferProof is { Length: > 0 } file)
        {
            proofStream = file.OpenReadStream();
            proofContentType = file.ContentType;
            proofFileName = file.FileName;
            proofLength = file.Length;
        }

        try
        {
            var command = new ConfirmPaymentCommand(
                feeId,
                form.InstallmentPlanId,
                form.PaymentMethod,
                proofStream,
                proofContentType,
                proofFileName,
                proofLength);

            var result = await sender.Send(command);
            return Ok(ApiResponse<ConfirmPaymentResponse>.Success(result, "Payment confirmed successfully"));
        }
        finally
        {
            proofStream?.Dispose();
        }
    }

    [HttpGet("transactions/{transactionId:guid}/receipt")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetReceipt(Guid transactionId)
    {
        var result = await sender.Send(new GetPaymentReceiptQuery(transactionId));
        return File(result.Content, "application/pdf", result.FileName);
    }

    [HttpGet("transactions/{transactionId:guid}/bank-transfer-proof")]
    [Authorize(Roles = ReadRoles)]
    public async Task<IActionResult> GetBankTransferProof(Guid transactionId)
    {
        var result = await sender.Send(new GetBankTransferProofQuery(transactionId));
        return File(result.Content, result.ContentType, result.FileName);
    }

    public record SetPaymentTypeRequest(PaymentType PaymentType, InstallmentFrequency? Frequency);

    public class ConfirmPaymentForm
    {
        public Guid? InstallmentPlanId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public IFormFile? BankTransferProof { get; set; }
    }
}
