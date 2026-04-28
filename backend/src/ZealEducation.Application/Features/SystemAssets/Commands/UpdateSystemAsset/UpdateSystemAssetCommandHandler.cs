using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateSystemAsset;

public class UpdateSystemAssetCommandHandler : IRequestHandler<UpdateSystemAssetCommand>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateSystemAssetCommandHandler(
        IRepository<SystemAsset> repository,
        IRepository<Staff> staffRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _staffRepository = staffRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateSystemAssetCommand request, CancellationToken cancellationToken)
    {
        var userAccountId = _currentUser.UserId!.Value;

        var staffList = await _staffRepository.FindAsync(s => s.UserAccountId == userAccountId, cancellationToken);
        var staff = staffList.Count > 0 ? staffList[0] : null;
        if (staff == null)
            throw new UnauthorizedAccessException("Logged-in account is not linked to a Staff.");

        var asset = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Asset with Id '{request.Id}' not found.");

        asset.UpdateInfo(
            request.AssetName,
            request.AssetType,
            request.Location,
            request.Notes,
            staff.Id // Always re-assign from token
        );

        _repository.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
