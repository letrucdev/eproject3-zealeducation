using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;

public class CreateSystemAssetCommandHandler : IRequestHandler<CreateSystemAssetCommand, Result<Guid>>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IRepository<UserAccount> _userAccountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateSystemAssetCommandHandler(
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

    public async Task<Result<Guid>> Handle(CreateSystemAssetCommand request, CancellationToken cancellationToken)
    {
        var userAccountId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("Cannot determine the logged-in user.");

        var userAccount = await _userAccountRepository.GetByIdAsync(userAccountId, cancellationToken);
        if (userAccount == null)
            throw new UnauthorizedAccessException("Account not found.");

        if (userAccount.Role != UserRole.SystemAdmin)
            throw new UnauthorizedAccessException("Only SystemAdmin can perform this action.");

        var staffList = await _staffRepository.FindAsync(s => s.UserAccountId == userAccountId, cancellationToken);
        var staff = staffList.Count > 0 ? staffList[0] : null;
        if (staff == null)
            throw new UnauthorizedAccessException("The logged-in account is not linked to a Staff.");

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
            staff.Id,
            request.Notes
        );

        await _repository.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(asset.Id);
    }
}
