using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace price_comparison.Repository.Components;

public class CategoriesHeaderViewComponent : ViewComponent
{
    private readonly DataContext _dataContext;

    public CategoriesHeaderViewComponent(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IViewComponentResult> InvokeAsync() => View(await _dataContext.Categories.ToListAsync());
}