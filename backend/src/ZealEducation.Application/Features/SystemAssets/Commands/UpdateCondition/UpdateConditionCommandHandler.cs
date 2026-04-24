using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateCondition;

public class UpdateConditionCommandHandler : IRequestHandler<UpdateConditionCommand>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateConditionCommandHandler(IRepository<SystemAsset> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateConditionCommand request, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Asset with Id '{request.Id}' not found.");

        asset.UpdateCondition(request.ConditionStatus); // call domain method

        _repository.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
