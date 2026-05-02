using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.CertificateApplications.Commands.ApproveCertificateApplication;
using ZealEducation.Application.Features.CertificateApplications.Commands.RegenerateCertificate;
using ZealEducation.Application.Features.CertificateApplications.Queries.GetCertificateApplications;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/incharge/certificates")]
[Authorize(Roles = nameof(UserRole.Incharge))]
public class CertificateInchargeController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedList<CertificateApplicationListItemDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] CertificateApplicationStatus? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        var result = await sender.Send(new GetCertificateApplicationsQuery(
            page, pageSize, search, status, sortBy, sortDirection));
        return Ok(ApiResponse<PaginatedList<CertificateApplicationListItemDto>>.Success(result));
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<ActionResult<ApiResponse<ApproveCertificateApplicationResponse>>> Approve(Guid id)
    {
        var result = await sender.Send(new ApproveCertificateApplicationCommand(id));
        return Ok(ApiResponse<ApproveCertificateApplicationResponse>.Success(
            result, "Certificate application approved."));
    }

    [HttpPost("{id:guid}/regenerate")]
    public async Task<ActionResult<ApiResponse<RegenerateCertificateResponse>>> Regenerate(Guid id)
    {
        var result = await sender.Send(new RegenerateCertificateCommand(id));
        return Ok(ApiResponse<RegenerateCertificateResponse>.Success(
            result, "Certificate regenerated."));
    }
}
