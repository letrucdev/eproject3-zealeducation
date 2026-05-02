using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Features.Candidates.Queries.GetCandidateRegistrationsTrend;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/incharge/candidates")]
[Authorize(Roles = nameof(UserRole.Incharge))]
public class CandidateInchargeController(ISender sender) : ControllerBase
{
    [HttpGet("registrations-trend")]
    public async Task<ActionResult<ApiResponse<List<CandidateRegistrationTrendPointDto>>>> GetRegistrationsTrend(
        [FromQuery] int days = 90)
    {
        var result = await sender.Send(new GetCandidateRegistrationsTrendQuery(days));
        return Ok(ApiResponse<List<CandidateRegistrationTrendPointDto>>.Success(result));
    }
}
