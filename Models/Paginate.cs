namespace price_comparison.Models;

public class Paginate
{
    public int TotalItems { get; private set; }
    public int PageSize { get; private set; }
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int StartPage { get; private set; }
    public int EndPage { get; private set; }

    public Paginate()
    {
    }

    public Paginate(int totalItems, int page, int pageSize=9) 
    {
        int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        int currentPage = page;
        
        int startPage = currentPage-5;
        int endPage = currentPage+4;

        if (startPage <= 0)
        {
            endPage = endPage - (startPage - 1);
            startPage = 1;
        }

        if (endPage  > totalPages)
        {
            endPage = totalPages;
            if (endPage > 10)
            {
                startPage = endPage - 9;
            }
        }
        TotalItems = totalItems;
        CurrentPage = currentPage;
        PageSize = pageSize;
        StartPage = startPage;
        EndPage = endPage;
        TotalPages = totalPages;
    }
}