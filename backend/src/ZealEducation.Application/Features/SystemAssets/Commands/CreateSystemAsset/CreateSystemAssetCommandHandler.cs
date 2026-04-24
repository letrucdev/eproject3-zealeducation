using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Commands.CreateSystemAsset;

public class CreateSystemAssetCommandHandler : IRequestHandler<CreateSystemAssetCommand, Guid>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public CreateSystemAssetCommandHandler(
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

    public async Task<Guid> Handle(CreateSystemAssetCommand request, CancellationToken cancellationToken)
    {
        var userAccountId = _currentUser.UserId!.Value;

        var staffList = await _staffRepository.FindAsync(s => s.UserAccountId == userAccountId, cancellationToken);
        var staff = staffList.Count > 0 ? staffList[0] : null;
        if (staff == null)
            throw new UnauthorizedAccessException("Logged-in account is not linked to a Staff.");

        var existingAssets = await _repository.FindAsync(a => a.SerialNumber == request.SerialNumber, cancellationToken);
        if (existingAssets.Count > 0)
            throw new InvalidOperationException($"SerialNumber '{request.SerialNumber}' already exists.");

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

        return asset.Id;
    }
}
