using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using PetClinic.DataProcessor.Dto.Import;
using PetClinic.Models;

namespace PetClinic.DataProcessor
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Data;

    public class Deserializer
    {
        private const string SuccessMessage = "Record {0} successfully imported.";
        private const string SuccessMessageAnimal = "Record {0} Passport №: {1} successfully imported.";
        private const string SuccessMessageProcedure = "Record successfully imported.";
        private const string ErrorMessage = "Error: Invalid data.";

        public static string ImportAnimalAids(PetClinicContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedAnimalAids = JsonConvert.DeserializeObject<List<ImportAnimalAidDto>>(jsonString);
            var validAnimalAids = new List<AnimalAid>();
            foreach (var animalAidDto in importedAnimalAids)
            {
                if (!IsValid(animalAidDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isAnimalAidExist = validAnimalAids.Any(a => a.Name == animalAidDto.Name);
                if (isAnimalAidExist)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var animalAid = new AnimalAid()
                {
                    Name = animalAidDto.Name,
                    Price = animalAidDto.Price
                };
                validAnimalAids.Add(animalAid);
                sb.AppendLine(String.Format(SuccessMessage, animalAid.Name));
            }
            context.AnimalAids.AddRange(validAnimalAids);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportAnimals(PetClinicContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedAnimals = JsonConvert.DeserializeObject<List<ImportAnimalDto>>(jsonString);

            var validAnimals = new List<Animal>();
            foreach (var animalDto in importedAnimals)
            {
                if (!IsValid(animalDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                if (!IsValid(animalDto.Passport))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }


                var isPassportExist = context.Passports.Any(p => p.SerialNumber == animalDto.Passport.SerialNumber);
                if (isPassportExist)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isRegDateValid = DateTime.TryParseExact(animalDto.Passport.RegistrationDate, "dd-MM-yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validRegDate);

                var passport = new Passport()
                {
                    SerialNumber = animalDto.Passport.SerialNumber,
                    OwnerName = animalDto.Passport.OwnerName,
                    OwnerPhoneNumber = animalDto.Passport.OwnerPhoneNumber,
                    RegistrationDate = validRegDate
                };

                context.Passports.Add(passport);
                context.SaveChanges();

                var animal = new Animal()
                {
                    Name = animalDto.Name,
                    Type = animalDto.Type,
                    Age = animalDto.Age,
                    Passport = passport
                };

                validAnimals.Add(animal);
                sb.AppendLine(String.Format(SuccessMessageAnimal,animal.Name,animal.Passport.SerialNumber));
            }
            context.Animals.AddRange(validAnimals);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportVets(PetClinicContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Vets");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportVetDto>), root);
            var importedVets = new List<ImportVetDto>();
            using (var reader = new StringReader(xmlString))
            {
                importedVets = (List<ImportVetDto>) serializer.Deserialize(reader);
            }

            var validVets = new List<Vet>();

            foreach (var vetDto in importedVets)
            {
                if (!IsValid(vetDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isVetExisting = validVets.Any(v => v.PhoneNumber == vetDto.PhoneNumber);
                if (isVetExisting)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var vet = new Vet()
                {
                    Name = vetDto.Name,
                    Profession = vetDto.Profession,
                    Age = vetDto.Age,
                    PhoneNumber = vetDto.PhoneNumber
                };
                validVets.Add(vet);
                sb.AppendLine(String.Format(SuccessMessage, vet.Name));
            }
            context.Vets.AddRange(validVets);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportProcedures(PetClinicContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Procedures");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportProcedureDto>), root);
            var importedProcedures = new List<ImportProcedureDto>();
            using (var reader = new StringReader(xmlString))
            {
                importedProcedures = (List<ImportProcedureDto>)serializer.Deserialize(reader);
            }

            var validProcedures = new List<Procedure>();

            foreach (var procedureDto in importedProcedures)
            {
                if (!IsValid(procedureDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isVetExisting = context.Vets
                    .Any(v => v.Name == procedureDto.VetName);
                if (!isVetExisting)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isAnimalExisting =
                    context.Animals
                        .Any(a => a.Passport.SerialNumber == procedureDto.AnimalPassportSerialNumber);
                if (!isAnimalExisting)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var procedureAnimalAids = procedureDto.AnimalAids
                    .Select(a => a.AnimalAidName)
                    .ToList();

                var dbAnimalAids = context.AnimalAids
                    .Select(aa => aa.Name)
                    .ToList();

                var isAnimalAid = true;

                foreach (var animalAidName in procedureAnimalAids)
                {
                    if (!dbAnimalAids.Contains(animalAidName))
                    {
                        isAnimalAid = false;
                        break;
                    }
                }

                if (!isAnimalAid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var checkDuplicateAnimalAids =
                    (procedureAnimalAids.Count - procedureAnimalAids.Distinct().Count()) == 0;

                if (!checkDuplicateAnimalAids)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                //constructing new procedure
                var vet = context.Vets.FirstOrDefault(v => v.Name == procedureDto.VetName);
                var animal = context.Animals.FirstOrDefault(a =>
                    a.PassportSerialNumber == procedureDto.AnimalPassportSerialNumber);
                var dateTime = DateTime.ParseExact(procedureDto.DateTime, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                var procedure = new Procedure()
                {
                    Vet = vet,
                    Animal = animal,
                    DateTime = dateTime
                };

                context.Procedures.AddRange(validProcedures);
                context.SaveChanges();

                var dbAA = context.AnimalAids
                    .Where(x => procedureDto.AnimalAids.Any(k => k.AnimalAidName == x.Name))
                    .ToList();
                var procedureAA = new List<ProcedureAnimalAid>();

                foreach (var animalAid in dbAA)
                {
                    var procedureAnimalAid = new ProcedureAnimalAid()
                    {
                        Procedure = procedure,
                        AnimalAid = animalAid
                    };
                    procedureAA.Add(procedureAnimalAid);
                }

                context.ProceduresAnimalAids.AddRange(procedureAA);
                context.SaveChanges();

                sb.AppendLine(SuccessMessageProcedure);
            }

            return sb.ToString().TrimEnd();
        }

        public static bool IsValid(object obj)
        {
            ValidationContext validationContext = new ValidationContext(obj);
            List<ValidationResult> validationResults = new List<ValidationResult>();

            return Validator.TryValidateObject(obj, validationContext, validationResults, true);
        }
    }
}