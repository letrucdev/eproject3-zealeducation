using MediatR;

namespace ZealEducation.Application.Features.StudyMaterials.Commands.CreateStudyMaterials;

public record CreateStudyMaterialsCommand(
    string Title,
    IReadOnlyList<Guid> CourseIds,
    Stream FileContent,
    string ContentType,
    string FileName,
    long FileLength) : IRequest<CreateStudyMaterialsResponse>;

public class CreateStudyMaterialsResponse
{
    public IReadOnlyList<Guid> MaterialIds { get; set; } = [];
    public string FilePath { get; set; } = default!;
    public decimal FileSizeMb { get; set; }
}
