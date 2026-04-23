using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Features.SystemAssets.DTOs;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.SystemAssets.Queries.GetSystemAssets;

public class GetSystemAssetsQueryHandler : IRequestHandler<GetSystemAssetsQuery, Result<IReadOnlyList<SystemAssetDto>>>
{
    private readonly IRepository<SystemAsset> _repository;
    private readonly IRepository<Staff> _staffRepository;
    private readonly IRepository<UserAccount> _userAccountRepository;

    public GetSystemAssetsQueryHandler(
        IRepository<SystemAsset> repository,
        IRepository<Staff> staffRepository,
        IRepository<UserAccount> userAccountRepository)
    {
        _repository = repository;
        _staffRepository = staffRepository;
        _userAccountRepository = userAccountRepository;
    }

    public async Task<Result<IReadOnlyList<SystemAssetDto>>> Handle(GetSystemAssetsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.Query();

        if (request.ConditionStatus.HasValue)
        {
            query = query.Where(a => a.ConditionStatus == request.ConditionStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.AssetType))
        {
            query = query.Where(a => a.AssetType == request.AssetType);
        }

        // Ideally here we would use ProjectTo<SystemAssetDto>() from AutoMapper 
        // or a select projection. For simplicity without knowing the mapper config:
        
        var assetsQuery = from a in query
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

        var assets = assetsQuery.ToList();

        return await Task.FromResult(Result<IReadOnlyList<SystemAssetDto>>.Success(assets));
    }
}
