using System;
using System.Linq;
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
    private readonly IRepository<Staff> _staffRepository;
    private readonly IRepository<UserAccount> _userAccountRepository;

    public GetSystemAssetByIdQueryHandler(
        IRepository<SystemAsset> repository,
        IRepository<Staff> staffRepository,
        IRepository<UserAccount> userAccountRepository)
    {
        _repository = repository;
        _staffRepository = staffRepository;
        _userAccountRepository = userAccountRepository;
    }

    public async Task<Result<SystemAssetDto>> Handle(GetSystemAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var query = from a in _repository.Query()
                    where a.Id == request.Id
                    join s in _staffRepository.Query() on a.ManagedBy equals s.Id into sj
                    from s in sj.DefaultIfEmpty()
                    join u in _userAccountRepository.Query() on (s != null ? s.UserAccountId : Guid.Empty) equals u.Id into uj
                    from u in uj.DefaultIfEmpty()
                    select new SystemAssetDto
                    {
                        Id = a.Id,
                        AssetName = a.AssetName,
                        AssetType = a.AssetType,
                        SerialNumber = a.SerialNumber,
                        Location = a.Location,
                        ConditionStatus = a.ConditionStatus,
                        PurchaseDate = a.PurchaseDate,
                        LastMaintenance = a.LastMaintenance,
                        Notes = a.Notes,
                        ManagedBy = a.ManagedBy,
                        ManagedByName = u != null ? u.FullName : null
                    };

        var dto = query.FirstOrDefault();

        if (dto == null)
        {
            throw new NotFoundException(nameof(SystemAsset), request.Id);
        }

        return await Task.FromResult(Result<SystemAssetDto>.Success(dto));
    }
}
