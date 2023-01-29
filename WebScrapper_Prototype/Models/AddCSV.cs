using System.ComponentModel.DataAnnotations.Schema;

namespace WebScrapper_Prototype.Models
{
    public class AddCSV
    {
        [NotMapped]
        public IFormFile? CSVFile { get; set; }
    }
}
