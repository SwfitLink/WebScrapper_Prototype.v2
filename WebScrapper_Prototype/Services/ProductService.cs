using CsvHelper;
using System.Text;
using WebScrapper_Prototype.Mappers;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Services
{
    public class ProductService
    {
        public List<Product> ReadCSVFile(string path)
        {
            Console.WriteLine(path);
            try
            {
                using (var reader = new StreamReader(path, Encoding.Default))
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.RegisterClassMap<ProductMap>();
                    var rows = csv.GetRecords<Product>().ToList();
                    return rows;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public void SaveCSVFile(string path, List<Product> product)
        {
            using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(true)))
            using (CsvWriter csvw = new CsvWriter(sw))
            {
                csvw.WriteHeader<Product>();
                csvw.NextRecord();
                foreach (Product prod in product)
                {
                    csvw.WriteRecord<Product>(prod);
                    csvw.NextRecord();
                }
            }
        }
    }
}
