using ZealEducation.Application.Common.Exceptions;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Application.Common.Models;
using ZealEducation.Application.Common.Services;
using ZealEducation.Application.UnitTests.Helpers;
using ZealEducation.Domain.Entities;
using ZealEducation.Domain.Enums;
using ZealEducation.Domain.Interfaces;

namespace ZealEducation.Application.UnitTests.Common.Services;

/// Smoke tests covering the cheap branches (NotFound + cached path).
/// The full PDF-generation + persistence path has heavy multi-repository
/// orchestration and is verified end-to-end by the Infrastructure
/// integration tests against a real database.
public class PaymentReceiptArchiverTests
{
    private readonly Mock<IRepository<PaymentTransaction>> _paymentRepo = new();
    private readonly Mock<IRepository<FeeStructure>> _feeRepo = new();
    private readonly Mock<IRepository<Candidate>> _candidateRepo = new();
    private readonly Mock<IRepository<UserAccount>> _userRepo = new();
    private readonly Mock<IRepository<Enrollment>> _enrollmentRepo = new();
    private readonly Mock<IRepository<Course>> _courseRepo = new();
    private readonly Mock<IRepository<InstallmentPlan>> _installmentRepo = new();
    private readonly Mock<IRepository<Staff>> _staffRepo = new();
    private readonly Mock<IReceiptPdfGenerator> _pdfGen = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();
    private readonly Mock<IUnitOfWork> _uow = MockRepositoryExtensions.CreateUnitOfWork();

    private PaymentReceiptArchiver CreateService() => new(
        _paymentRepo.Object,
        _feeRepo.Object,
        _candidateRepo.Object,
        _userRepo.Object,
        _enrollmentRepo.Object,
        _courseRepo.Object,
        _installmentRepo.Object,
        _staffRepo.Object,
        _pdfGen.Object,
        _fileStorage.Object,
        _uow.Object);

    [Fact]
    public async Task Should_throw_NotFoundException_when_transaction_does_not_exist()
    {
        var transactionId = Guid.NewGuid();
        _paymentRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<PaymentTransaction>([]));

        var act = async () => await CreateService().GenerateAndUploadAsync(transactionId, default);

        await act.Should().ThrowAsync<NotFoundException>();
        _pdfGen.Verify(p => p.Generate(It.IsAny<ReceiptPdfModel>()), Times.Never);
    }

    [Fact]
    public async Task Should_return_cached_receipt_when_already_archived()
    {
        var transactionId = Guid.NewGuid();
        var cachedBytes = new byte[] { 0xCA, 0xFE };
        var transaction = new PaymentTransaction
        {
            Id = transactionId,
            FeeId = Guid.NewGuid(),
            ProcessedByStaffId = Guid.NewGuid(),
            ReceiptNumber = "RC-001",
            PaymentDate = DateTime.UtcNow,
            Amount = 100m,
            PaymentMethod = PaymentMethod.Cash,
            ReceiptFilePath = "receipts/2030/05/RC-001.pdf"
        };
        _paymentRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<PaymentTransaction>([transaction]));
        _fileStorage.Setup(s => s.DownloadAsync(transaction.ReceiptFilePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedBytes);

        var result = await CreateService().GenerateAndUploadAsync(transactionId, default);

        result.Should().BeEquivalentTo(cachedBytes);
        _pdfGen.Verify(p => p.Generate(It.IsAny<ReceiptPdfModel>()), Times.Never);
        _fileStorage.Verify(s => s.UploadAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_throw_NotFound_for_fee_when_transaction_references_missing_fee()
    {
        var transactionId = Guid.NewGuid();
        var feeId = Guid.NewGuid();
        var transaction = new PaymentTransaction
        {
            Id = transactionId,
            FeeId = feeId,
            ProcessedByStaffId = Guid.NewGuid(),
            ReceiptNumber = "RC-002",
            PaymentDate = DateTime.UtcNow,
            Amount = 100m,
            PaymentMethod = PaymentMethod.Cash,
            ReceiptFilePath = null
        };
        _paymentRepo.Setup(r => r.Query()).Returns(new TestAsyncEnumerable<PaymentTransaction>([transaction]));
        _feeRepo.SetupGetById(feeId, null);

        var act = async () => await CreateService().GenerateAndUploadAsync(transactionId, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
