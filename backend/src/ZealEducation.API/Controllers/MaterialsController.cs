using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.StudyMaterials.Commands.CreateStudyMaterials;
using ZealEducation.Application.Features.StudyMaterials.Commands.DeleteStudyMaterial;
using ZealEducation.Application.Features.StudyMaterials.Commands.ReplaceStudyMaterialFile;
using ZealEducation.Application.Features.StudyMaterials.Commands.ToggleStudyMaterialActive;
using ZealEducation.Application.Features.StudyMaterials.Commands.UpdateStudyMaterialTitle;
using ZealEducation.Application.Features.StudyMaterials.Queries;
using ZealEducation.Application.Features.StudyMaterials.Queries.GetCourseMaterials;
using ZealEducation.Application.Features.StudyMaterials.Queries.GetMaterialCourses;
using ZealEducation.Application.Features.StudyMaterials.Queries.GetMaterialSiblingCourses;
using ZealEducation.Application.Features.StudyMaterials.Queries.GetStudyMaterialFile;
using ZealEducation.Domain.Enums;

namespace ZealEducation.API.Controllers;

[ApiController]
[Route("api/materials")]
[Authorize]
public class MaterialsController(ISender sender) : ControllerBase
{
    private const string InchargeOnly = nameof(UserRole.Incharge);
    private const long MaxRequestBytes = 60L * 1024 * 1024;

    [HttpGet("courses")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<MaterialCourseListItemDto>>>> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await sender.Send(new GetMaterialCoursesQuery(page, pageSize, search));
        return Ok(ApiResponse<PaginatedList<MaterialCourseListItemDto>>.Success(result));
    }

    [HttpGet("courses/{courseId:guid}/items")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<PaginatedList<StudyMaterialListItemDto>>>> GetCourseMaterials(
        Guid courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null,
        [FromQuery] bool includeInactive = true)
    {
        var result = await sender.Send(new GetCourseMaterialsQuery(courseId, page, pageSize, search, includeInactive));
        return Ok(ApiResponse<PaginatedList<StudyMaterialListItemDto>>.Success(result));
    }

    [HttpPost]
    [Authorize(Roles = InchargeOnly)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<ApiResponse<CreateStudyMaterialsResponse>>> Create(
        [FromForm] CreateMaterialsForm form)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest(ApiResponse<object>.Error("A file is required."));

        await using var stream = form.File.OpenReadStream();

        var command = new CreateStudyMaterialsCommand(
            form.Title ?? string.Empty,
            form.CourseIds ?? new List<Guid>(),
            stream,
            form.File.ContentType,
            form.File.FileName,
            form.File.Length);

        var result = await sender.Send(command);
        return Ok(ApiResponse<CreateStudyMaterialsResponse>.Success(result, "Material created successfully"));
    }

    [HttpPut("{id:guid}/title")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateTitle(Guid id, [FromBody] UpdateTitleRequest body)
    {
        await sender.Send(new UpdateStudyMaterialTitleCommand(id, body.Title));
        return Ok(ApiResponse<object>.Success(null, "Title updated successfully"));
    }

    [HttpPut("{id:guid}/file")]
    [Authorize(Roles = InchargeOnly)]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxRequestBytes)]
    public async Task<ActionResult<ApiResponse<object>>> ReplaceFile(Guid id, [FromForm] ReplaceFileForm form)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest(ApiResponse<object>.Error("A file is required."));

        await using var stream = form.File.OpenReadStream();

        await sender.Send(new ReplaceStudyMaterialFileCommand(
            id,
            stream,
            form.File.ContentType,
            form.File.FileName,
            form.File.Length));

        return Ok(ApiResponse<object>.Success(null, "File replaced successfully"));
    }

    [HttpPatch("{id:guid}/active")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> ToggleActive(Guid id, [FromBody] ToggleActiveRequest body)
    {
        await sender.Send(new ToggleStudyMaterialActiveCommand(id, body.IsActive));
        return Ok(ApiResponse<object>.Success(null, body.IsActive ? "Material activated" : "Material hidden"));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await sender.Send(new DeleteStudyMaterialCommand(id));
        return Ok(ApiResponse<object>.Success(null, "Material deleted successfully"));
    }

    [HttpGet("{id:guid}/siblings")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<ActionResult<ApiResponse<List<MaterialSiblingCourseDto>>>> GetSiblings(Guid id)
    {
        var result = await sender.Send(new GetMaterialSiblingCoursesQuery(id));
        return Ok(ApiResponse<List<MaterialSiblingCourseDto>>.Success(result));
    }

    [HttpGet("{id:guid}/file")]
    [Authorize(Roles = InchargeOnly)]
    public async Task<IActionResult> GetFile(Guid id)
    {
        var result = await sender.Send(new GetStudyMaterialFileQuery(id));
        return File(result.Content, result.ContentType, result.FileName);
    }

    public class CreateMaterialsForm
    {
        public string? Title { get; set; }
        public List<Guid>? CourseIds { get; set; }
        public IFormFile? File { get; set; }
    }

    public class ReplaceFileForm
    {
        public IFormFile? File { get; set; }
    }

    public record UpdateTitleRequest(string Title);
    public record ToggleActiveRequest(bool IsActive);
}
