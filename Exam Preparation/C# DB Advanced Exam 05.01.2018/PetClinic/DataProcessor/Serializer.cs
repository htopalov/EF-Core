using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PetClinic.DataProcessor.Dto.Export;

namespace PetClinic.DataProcessor
{
    using Data;

    public class Serializer
    {
        public static string ExportAnimalsByOwnerPhoneNumber(PetClinicContext context, string phoneNumber)
        {
            var animalsByOwner = context.Animals
                .Where(a => a.Passport.OwnerPhoneNumber == phoneNumber)
                .OrderBy(a => a.Age)
                .ThenBy(a => a.PassportSerialNumber)
                .Select(a => new
                {
                    OwnerName = a.Passport.OwnerName,
                    AnimalName = a.Name,
                    Age = a.Age,
                    SerialNumber = a.PassportSerialNumber,
                    RegisteredOn = a.Passport.RegistrationDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture)
                })
                .ToArray();

            return JsonConvert.SerializeObject(animalsByOwner, Formatting.Indented);

        }

        public static string ExportAllProcedures(PetClinicContext context)
        {
            var procedures = context.Procedures
                .Include(x => x.ProcedureAnimalAids)
                .OrderBy(p => p.DateTime)
                .ThenBy(p => p.Animal.Passport.SerialNumber)
                .Select(p => new ExportProcedureDtoXml()
                {
                    PassportNumber = p.Animal.PassportSerialNumber,
                    OwnerPhoneNumber = p.Animal.Passport.OwnerPhoneNumber,
                    DateTime = p.DateTime.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                    AnimalAids = p.ProcedureAnimalAids.Select(aa => new ExportAnimalAidsDtoXml()
                        {
                            Name = aa.AnimalAid.Name,
                            Price = aa.AnimalAid.Price
                        })
                        .ToArray(),
                    TotalPrice = p.ProcedureAnimalAids.Sum(x => x.AnimalAid.Price)
                })
                .ToArray();




            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Procedures");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportProcedureDtoXml[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty,string.Empty);
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer,procedures, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}