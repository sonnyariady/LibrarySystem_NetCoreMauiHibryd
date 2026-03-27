using Library.Api.Common.Controllers;
using Library.Api.Common.Crud;
using Library.Api.Common.Excel;
using Library.Api.Data;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[Route("api/members")]
public sealed class MembersController : CrudControllerBase<AppDbContext, Member, int>
{
    public MembersController(
        ICrudQueryService<AppDbContext, Member, int> svc,
        IExcelExporter excel)
        : base(svc, excel, x => x.Id)
    {
    }
}
