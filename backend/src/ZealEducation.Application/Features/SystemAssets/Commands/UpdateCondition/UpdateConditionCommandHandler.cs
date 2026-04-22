using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateCondition;

public class UpdateConditionCommandHandler : IRequestHandler<UpdateConditionCommand, Result>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateConditionCommandHandler(IRepository<SystemAsset> repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateConditionCommand request, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(request.Id, cancellationToken);
        
        if (asset == null)
        {
            throw new NotFoundException(nameof(SystemAsset), request.Id);
        }

        try
        {
            asset.UpdateCondition(request.NewStatus);

            _repository.Update(asset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
