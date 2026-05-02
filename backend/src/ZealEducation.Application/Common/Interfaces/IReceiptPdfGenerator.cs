using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Common.Interfaces;

public interface IReceiptPdfGenerator
{
    byte[] Generate(ReceiptPdfModel model);
}
