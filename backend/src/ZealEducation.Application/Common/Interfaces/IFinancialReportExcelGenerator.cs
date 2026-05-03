using ZealEducation.Application.Common.Models;

namespace ZealEducation.Application.Common.Interfaces;

public interface IFinancialReportExcelGenerator
{
    byte[] Generate(FinancialReportExcelModel model);
}
