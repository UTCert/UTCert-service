using System.ComponentModel.DataAnnotations;

namespace UTCert.Model.Web.Dtos
{
    public class PagedInputDto
    {
        [Range(1, 10000)]
        public int PageNumber { get; set; }

        [Range(0, int.MaxValue)]
        public int PageSize { get; set; }

        public string? Sorting { get; set; }

        public PagedInputDto()
        {
            PageSize = 10;
        }
    }
}
