using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SoftJail.Data.Models;
using SoftJail.Data.Models.Enums;
using SoftJail.DataProcessor.ImportDto;

namespace SoftJail.DataProcessor
{

    using Data;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid Data";
        private const string SuccessOfficerMessage = "Imported {0} ({1} prisoners)";

        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var departmentCells = JsonConvert.DeserializeObject<ImportDepartmentCellsDto[]>(jsonString);

            var sb = new StringBuilder();

            var departments = new List<Department>();

            foreach (var dto in departmentCells)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isValidCell = true;
                var cells = new List<Cell>();
                foreach (var cellDto in dto.Cells)
                {
                    if (!IsValid(cellDto))
                    {
                        isValidCell = false;
                        break;
                    }

                    var cell = new Cell
                    {
                        CellNumber = cellDto.CellNumber,
                        HasWindow = cellDto.HasWindow
                    };

                    cells.Add(cell);
                }

                if (!isValidCell)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var department = new Department
                {
                    Name = dto.Name,
                    Cells = cells
                };

                departments.Add(department);

                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
            }

            context.Departments.AddRange(departments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedPrisonersMails = JsonConvert.DeserializeObject<ImportPrisonerMail[]>(jsonString);

            var prisoners = new List<Prisoner>();

            foreach (var prisonerDto in importedPrisonersMails)
            {
                if (!IsValid(prisonerDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                bool isMailValid = true;
                var mails = new List<Mail>();

                foreach (var mailDto in prisonerDto.Mails)
                {
                    if (!IsValid(mailDto))
                    {
                        //sb.AppendLine(ErrorMessage);
                        isMailValid = false;
                        break;
                    }

                    var mail = new Mail()
                    {
                        Description = mailDto.Description,
                        Sender = mailDto.Sender,
                        Address = mailDto.Address
                    };

                    mails.Add(mail);
                }

                if (!isMailValid)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var incarcerationDate = DateTime.ParseExact(prisonerDto.IncarcerationDate, "dd/MM/yyyy",
                    CultureInfo.InvariantCulture, DateTimeStyles.None);

                DateTime? releaseDate = null;
                if (prisonerDto.ReleaseDate != null)
                {
                    releaseDate = DateTime.ParseExact(prisonerDto.ReleaseDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }

                var prisoner = new Prisoner()
                {
                    FullName = prisonerDto.FullName,
                    Nickname = prisonerDto.Nickname,
                    Age = prisonerDto.Age,
                    IncarcerationDate = incarcerationDate,
                    ReleaseDate = releaseDate,
                    Bail = prisonerDto.Bail,
                    CellId = prisonerDto.CellId,
                    Mails = mails
                };

                prisoners.Add(prisoner);
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var serializer = new XmlSerializer(typeof(ImportOfficerDto[]), new XmlRootAttribute("Officers"));
            var dtos = (ImportOfficerDto[])serializer.Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));

            var sb = new StringBuilder();
            var officers = new List<Officer>();
            foreach (var dto in dtos)
            {
                if (!IsValid(dto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var isValidPosition = Enum.TryParse(typeof(Position), dto.Position, out object positionResult);
                var isValidWeapon = Enum.TryParse(typeof(Weapon), dto.Weapon, out object weaponResult);

                if (!isValidPosition || !isValidWeapon)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var position = (Position)positionResult;
                var weapon = (Weapon)weaponResult;

                var officersPrisoners = new List<OfficerPrisoner>();
                foreach (var prisoner in dto.Prisoners)
                {
                    var officerPrisoner = new OfficerPrisoner
                    {
                        PrisonerId = prisoner.Id
                    };

                    officersPrisoners.Add(officerPrisoner);
                }

                var officer = new Officer
                {
                    FullName = dto.Name,
                    Salary = dto.Money,
                    Position = position,
                    Weapon = weapon,
                    DepartmentId = dto.DepartmentId,
                    OfficerPrisoners = officersPrisoners
                };

                officers.Add(officer);

                sb.AppendLine(String.Format(SuccessOfficerMessage, officer.FullName, officer.OfficerPrisoners.Count));
            }

            context.Officers.AddRange(officers);
            context.SaveChanges();

            var result = sb.ToString();

            return result;
        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}
