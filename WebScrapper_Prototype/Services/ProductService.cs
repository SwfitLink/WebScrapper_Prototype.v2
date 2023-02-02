using CsvHelper;
using System.Text;
using WebScrapper_Prototype.Mappers;
using WebScrapper_Prototype.Models;

namespace WebScrapper_Prototype.Services
{
    public class ProductService
    {
        public List<ProductModel> ReadCSVFile(string path)
        {
            Console.WriteLine(path);
            try
            {
                using (var reader = new StreamReader(path, Encoding.Default))
                using (var csv = new CsvReader(reader))
                {
                    csv.Configuration.RegisterClassMap<ProductMap>();
                    var rows = csv.GetRecords<ProductModel>().ToList();
                    return rows;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        public void SaveCSVFile(string path, List<ProductModel> product)
        {
            using (StreamWriter sw = new StreamWriter(path, false, new UTF8Encoding(true)))
            using (CsvWriter csvw = new CsvWriter(sw))
            {
                csvw.WriteHeader<ProductModel>();
                csvw.NextRecord();
                foreach (ProductModel prod in product)
                {
                    csvw.WriteRecord<ProductModel>(prod);
                    csvw.NextRecord();
                }
            }
        }
    }
}
