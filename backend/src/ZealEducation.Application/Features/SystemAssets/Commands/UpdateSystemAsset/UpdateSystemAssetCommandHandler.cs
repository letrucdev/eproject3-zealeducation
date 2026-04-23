using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Commands.UpdateSystemAsset;

public class UpdateSystemAssetCommandHandler : IRequestHandler<UpdateSystemAssetCommand, Result>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IRepository<UserAccount> _userAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public UpdateSystemAssetCommandHandler(
        IRepository<SystemAsset> repository,
        IRepository<Staff> staffRepository,
        IRepository<UserAccount> userAccountRepository,
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _repository = repository;
        _staffRepository = staffRepository;
        _userAccountRepository = userAccountRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateSystemAssetCommand request, CancellationToken cancellationToken)
    {
        // Step 1: Get UserAccountId from JWT
        var userAccountId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Cannot determine the logged-in user.");

        // Step 2: Verify account exists
        var userAccount = await _userAccountRepository.GetByIdAsync(userAccountId, cancellationToken);
        if (userAccount == null)
            throw new UnauthorizedAccessException("Account not found.");

        // Step 3: Verify SystemAdmin role
        if (userAccount.Role != UserRole.SystemAdmin)
            throw new UnauthorizedAccessException("Only SystemAdmin can perform this action.");

        // Step 4: Resolve StaffId from UserAccountId
        var staffList = await _staffRepository.FindAsync(s => s.UserAccountId == userAccountId, cancellationToken);
        var staff = staffList.Count > 0 ? staffList[0] : null;
        if (staff == null)
            throw new UnauthorizedAccessException("The logged-in account is not linked to a Staff.");

        // Step 5: Find the asset
        var asset = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (asset == null)
            return Result.Failure($"System asset with id '{request.Id}' not found.");

        // Step 6: Apply update (ManagedBy auto-set from current staff)
        asset.UpdateInfo(
            request.AssetName,
            request.AssetType,
            request.Location,
            request.Notes,
            staff.Id
        );

        _repository.Update(asset);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
