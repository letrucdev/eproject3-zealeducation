using MediatR;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Staffs.Commands.SetStaffActive;

public class SetStaffActiveCommandHandler(
    IRepository<Staff> staffRepository,
    IRepository<UserAccount> userRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SetStaffActiveCommand, Unit>
{
    public async Task<Unit> Handle(SetStaffActiveCommand request, CancellationToken cancellationToken)
    {
        var staff = await staffRepository.GetByIdAsync(request.StaffId, cancellationToken)
            ?? throw new NotFoundException(nameof(Staff), request.StaffId);

        var user = await userRepository.GetByIdAsync(staff.UserAccountId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAccount), staff.UserAccountId);

        staff.IsActive = request.IsActive;
        user.IsActive = request.IsActive;

        staffRepository.Update(staff);
        userRepository.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
