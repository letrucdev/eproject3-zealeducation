using MediatR;

namespace ZealEducation.Application.Features.Payments.Queries.GetBankTransferProof;

public record GetBankTransferProofQuery(Guid TransactionId) : IRequest<BankTransferProofResult>;

public record BankTransferProofResult(byte[] Content, string ContentType, string FileName);
