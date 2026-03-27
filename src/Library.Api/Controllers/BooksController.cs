using Library.Api.Common.Controllers;
using Library.Api.Common.Crud;
using Library.Api.Common.Excel;
using Library.Api.Data;
using Library.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[Route("api/books")]
public sealed class BooksController : CrudControllerBase<AppDbContext, Book, int>
{
    public BooksController(
        ICrudQueryService<AppDbContext, Book, int> svc,
        IExcelExporter excel)
        : base(svc, excel, x => x.Id)
    {
    }
}
