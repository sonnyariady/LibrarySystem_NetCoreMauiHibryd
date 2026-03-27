using Library.Api.Common.Controllers;
using Library.Api.Common.Crud;
using Library.Api.Common.Excel;
using Library.Api.Data;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[Route("api/categories")]
public sealed class CategoriesController : CrudControllerBase<AppDbContext, Category, int>
{
    public CategoriesController(
        ICrudQueryService<AppDbContext, Category, int> svc,
        IExcelExporter excel)
        : base(svc, excel, x => x.Id)
    {
    }
}
