using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Commands.DecommissionAsset;

public class DecommissionAssetCommandHandler : IRequestHandler<DecommissionAssetCommand>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DecommissionAssetCommandHandler(IRepository<SystemAsset> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DecommissionAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Asset with Id '{request.Id}' not found.");

        asset.Decommission(); // call domain method

        _repository.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
