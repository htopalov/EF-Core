namespace BookShop.DataProcessor.ExportDto
{
    public class ExportAuthorDto
    {
        public string AuthorName { get; set; }

        public ExportAuthorBookDto[] Books { get; set; }
    }
}
