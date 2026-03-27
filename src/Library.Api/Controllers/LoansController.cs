using Library.Api.Common.Controllers;
using Library.Api.Common.Crud;
using Library.Api.Common.Excel;
using Library.Api.Data;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[Route("api/loans")]
public sealed class LoansController : CrudControllerBase<AppDbContext, Loan, int>
{
    public LoansController(
        ICrudQueryService<AppDbContext, Loan, int> svc,
        IExcelExporter excel)
        : base(svc, excel, x => x.Id)
    {
    }
}
