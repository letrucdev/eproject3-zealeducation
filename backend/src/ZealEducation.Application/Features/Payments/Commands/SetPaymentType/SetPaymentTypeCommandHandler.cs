using MediatR;
using Microsoft.EntityFrameworkCore;
using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Features.Payments.Common;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.Features.Payments.Commands.SetPaymentType;

public class SetPaymentTypeCommandHandler(
    IRepository<FeeStructure> feeRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<Course> courseRepository,
    IRepository<InstallmentPlan> installmentRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<SetPaymentTypeCommand, SetPaymentTypeResponse>
{
    public async Task<SetPaymentTypeResponse> Handle(SetPaymentTypeCommand request, CancellationToken cancellationToken)
    {
        var fee = await feeRepository.GetByIdAsync(request.FeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(FeeStructure), request.FeeId);

        if (fee.AmountPaid > 0)
            throw new ConflictException("Cannot change payment type after a transaction has been recorded.");

        var existingInstallments = await installmentRepository.Query()
            .Where(i => i.FeeId == fee.Id)
            .ToListAsync(cancellationToken);

        foreach (var inst in existingInstallments)
            installmentRepository.Delete(inst);

        var response = new SetPaymentTypeResponse
        {
            PaymentType = request.PaymentType,
            Frequency = request.PaymentType == PaymentType.Installment ? request.Frequency : null,
            Installments = []
        };

        if (request.PaymentType == PaymentType.Installment)
        {
            var enrollment = await enrollmentRepository.Query()
                .FirstOrDefaultAsync(e => e.FeeId == fee.Id, cancellationToken)
                ?? throw new ConflictException("Cannot set installment plan: enrollment is missing for this fee.");

            var course = await courseRepository.GetByIdAsync(enrollment.CourseId, cancellationToken)
                ?? throw new NotFoundException(nameof(Course), enrollment.CourseId);

            if (course.DurationWeeks < InstallmentPlanCalculator.MinWeeksMonthly)
                throw new ConflictException("Course duration must be at least 2 months (8 weeks) to allow installment.");

            if (!InstallmentPlanCalculator.IsFrequencyAllowed(request.Frequency!.Value, course.DurationWeeks))
                throw new ConflictException($"Frequency {request.Frequency} is not allowed for a {course.DurationWeeks}-week course.");

            var planned = InstallmentPlanCalculator.Build(
                fee.TotalFee,
                course.DurationWeeks,
                enrollment.EnrollmentDate,
                request.Frequency.Value);

            foreach (var item in planned)
            {
                var entity = new InstallmentPlan
                {
                    Id = Guid.NewGuid(),
                    FeeId = fee.Id,
                    InstallmentNo = item.InstallmentNo,
                    AmountDue = item.AmountDue,
                    DueDate = item.DueDate,
                    AmountPaid = 0,
                    Status = InstallmentStatus.Pending,
                    PenaltyAmount = 0
                };
                await installmentRepository.AddAsync(entity, cancellationToken);
                response.Installments.Add(new SetPaymentTypeInstallmentDto
                {
                    Id = entity.Id,
                    InstallmentNo = entity.InstallmentNo,
                    AmountDue = entity.AmountDue,
                    DueDate = entity.DueDate
                });
            }
        }

        fee.PaymentType = request.PaymentType;
        feeRepository.Update(fee);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return response;
    }
}
