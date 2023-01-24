using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using System.Web;
namespace WebScrapper_Prototype.Models
{
    public class UploadImage
    {
        [Required]
        public IFormFile ProductImage { get; set; }


    }
}
