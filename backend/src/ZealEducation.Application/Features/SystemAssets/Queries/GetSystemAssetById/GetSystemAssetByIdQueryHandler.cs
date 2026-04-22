using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.SystemAssets.DTOs;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Queries.GetSystemAssetById;

public class GetSystemAssetByIdQueryHandler : IRequestHandler<GetSystemAssetByIdQuery, Result<SystemAssetDto>>
{
    private readonly IRepository<SystemAsset> _repository;

    public GetSystemAssetByIdQueryHandler(IRepository<SystemAsset> repository)
    {
        _repository = repository;
    }

    public async Task<Result<SystemAssetDto>> Handle(GetSystemAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(request.Id, cancellationToken);

        if (asset == null)
        {
            throw new NotFoundException(nameof(SystemAsset), request.Id);
        }

        var dto = new SystemAssetDto
        {
            Id = asset.Id,
            AssetName = asset.AssetName,
            AssetType = asset.AssetType,
            SerialNumber = asset.SerialNumber,
            Location = asset.Location,
            ConditionStatus = asset.ConditionStatus,
            PurchaseDate = asset.PurchaseDate,
            LastMaintenance = asset.LastMaintenance,
            Notes = asset.Notes,
            ManagedBy = asset.ManagedBy
        };

        return Result<SystemAssetDto>.Success(dto);
    }
}
