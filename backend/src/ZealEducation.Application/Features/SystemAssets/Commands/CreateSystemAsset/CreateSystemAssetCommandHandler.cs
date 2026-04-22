using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;

public class CreateSystemAssetCommandHandler : IRequestHandler<CreateSystemAssetCommand, Result<Guid>>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSystemAssetCommandHandler(IRepository<SystemAsset> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateSystemAssetCommand request, CancellationToken cancellationToken)
    {
        // Check serial unique
        var existingAssets = await _repository.FindAsync(a => a.SerialNumber == request.SerialNumber, cancellationToken);
        if (existingAssets.Count > 0)
        {
            return Result<Guid>.Failure($"Asset with Serial Number '{request.SerialNumber}' already exists.");
        }

        var asset = new SystemAsset(
            request.AssetName,
            request.AssetType,
            request.SerialNumber,
            request.Location,
            request.PurchaseDate,
            request.ManagedBy,
            request.Notes
        );

        await _repository.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(asset.Id);
    }
}
