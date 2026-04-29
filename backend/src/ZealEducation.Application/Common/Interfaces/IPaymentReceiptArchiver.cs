namespace ZealEducation.Application.Common.Interfaces;

public interface IPaymentReceiptArchiver
{
    Task<byte[]> GenerateAndUploadAsync(Guid transactionId, CancellationToken cancellationToken);
}
