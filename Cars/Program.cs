using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Cars
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<CarDb>());

            InsertData();
            QueryData();
        }

        private static void QueryData()
        {
            var db = new CarDb();

            var query = from car in db.Cars
                        orderby car.Combined descending, car.Name ascending
                        select car;

            foreach (var car in query.Take(10))
            {
                Console.WriteLine($"{car.Name}: {car.Combined}");
            }
        }

        private static void InsertData()
        {
            var cars = ProcessCars("fuel.csv");
            var db = new CarDb();

            if (!db.Cars.Any())
            {
                foreach (var car in cars)
                {
                    db.Cars.Add(car);
                }

                db.SaveChanges();
            }
        }

        private static void QueryXml()
        {
            var document = XDocument.Load("fuel.xml");

            var query =
                from elements in document.Element("Cars").Elements("Car")
                where elements.Attribute("Manufacture").Value == "BMW"
                select elements.Attribute("Name").Value;

            foreach (var name in query)
            {
                Console.WriteLine(name);
            }
        }

        private static void CreateXml()
        {
            var records = ProcessCars("fuel.csv");

            var document = new XDocument();
            var cars = new XElement("Cars");

            foreach (var record in records)
            {
                var name = new XAttribute("Name", record.Name);
                var combine = new XAttribute("Combined", record.Combined);
                var manufacture = new XAttribute("Manufacture", record.Manufacturer);

                var car = new XElement("Car", name, combine, manufacture);

                cars.Add(car);
            }

            document.Add(cars);
            document.Save("fuel.xml");
        }

        private static List<Car> ProcessCars(string path)
        {
            return
                File.ReadAllLines(path)
                    .Skip(1)
                    .Where(line => line.Length > 1)
                    .Select(Car.ParseFromCsv)
                    .ToList();
        }

        private static List<Manufacture> ProcessManufactures(string path)
        {
            var query =
                File.ReadAllLines(path)
                    .Where(l => l.Length > 1)
                    .Select(l =>
                    {
                        var columns = l.Split(',');
                        return new Manufacture
                        {
                            Name = columns[0],
                            Headquarters = columns[1],
                            Year = int.Parse(columns[2])
                        };
                    });
            return query.ToList();
        }

    }
}
