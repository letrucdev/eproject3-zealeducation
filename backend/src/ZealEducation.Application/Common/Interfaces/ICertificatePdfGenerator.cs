using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Common.Interfaces;

public interface ICertificatePdfGenerator
{
    byte[] Generate(CertificatePdfModel model);
}
