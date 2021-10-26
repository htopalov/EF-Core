using SoftUni.Data;
using SoftUni.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SoftUni
{
    public class StartUp
    {
        static void Main(string[] args)
        {
            var context = new SoftUniContext();
            //Console.WriteLine(GetEmployeesFullInformation(context));
            //Console.WriteLine(GetEmployeesWithSalaryOver50000(context));
            //Console.WriteLine(GetEmployeesFromResearchAndDevelopment(context));
            //Console.WriteLine(AddNewAddressToEmployee(context));
            //Console.WriteLine(GetEmployeesInPeriod(context));
            //Console.WriteLine(GetAddressesByTown(context));
            //Console.WriteLine(GetEmployee147(context));
            //Console.WriteLine(GetDepartmentsWithMoreThan5Employees(context));
            //Console.WriteLine(GetLatestProjects(context));
            //Console.WriteLine(IncreaseSalaries(context));
            //Console.WriteLine(GetEmployeesByFirstNameStartingWithSa(context));
            //Console.WriteLine(DeleteProjectById(context));
            Console.WriteLine(RemoveTown(context));
        }

        public static string GetEmployeesFullInformation(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees.Select(e => new
             {
                 e.EmployeeId,
                 e.FirstName,
                 e.LastName,
                 e.MiddleName,
                 e.JobTitle,
                 e.Salary
             })
             .OrderBy(e => e.EmployeeId)
             .ToList();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} {employee.MiddleName} {employee.JobTitle} {employee.Salary:f2}");
            }

            return sb.ToString();
        }

        public static string GetEmployeesWithSalaryOver50000(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees.Select(e => new
            {
                e.FirstName,
                e.Salary
            })
            .Where(e => e.Salary > 50000)
            .OrderBy(e => e.FirstName)
            .ToList();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} - {employee.Salary:f2}");
            }

            return sb.ToString();
        }

        public static string GetEmployeesFromResearchAndDevelopment(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees.Select(e => new
            {
                e.FirstName,
                e.LastName,
                e.Department,
                e.Salary
            })
                .Where(e => e.Department.Name == "Research and Development")
                .OrderBy(e => e.Salary)
                .ThenByDescending(e => e.FirstName)
                .ToList();

            foreach (var employee in employees)
            {
                sb.AppendLine($"{employee.FirstName} {employee.LastName} from {employee.Department.Name} - ${employee.Salary:f2}");
            }

            return sb.ToString();
        }

        public static string AddNewAddressToEmployee(SoftUniContext context)
        {
            var address = new Address
            { 
                AddressText = "Vitoshka 15",
                TownId = 4 
            };
            context.Addresses.Add(address);
            context.SaveChanges();
            var employee = context.Employees.FirstOrDefault(e => e.LastName == "Nakov");
            employee.Address = address;
            context.SaveChanges();
            var employees = context.Employees
                        .OrderByDescending(e => e.AddressId)
                        .Take(10)
                        .Select(e=> new
                        {
                            e.Address.AddressText
                        })
                        .ToList();
            var sb = new StringBuilder();
            foreach (var emp in employees)
            {
                sb.AppendLine($"{emp.AddressText}");
            }

            return sb.ToString();
        }

        public static string GetEmployeesInPeriod(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees
                    .Where(e => e.EmployeesProjects
                                .Any(ep => ep.Project.StartDate.Year >= 2001 && ep.Project.StartDate.Year <= 2003))
                    .Take(10)
                    .Select(e => new
                    {
                        e.FirstName,
                        e.LastName,
                        ManagerFirst = e.Manager.FirstName,
                        ManagerLast = e.Manager.LastName,
                        ProjectsList = e.EmployeesProjects
                        .Select(p => new
                        {
                            Project = p.Project.Name,
                            Start = p.Project.StartDate.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture),
                            End = p.Project.EndDate.HasValue 
                            ? p.Project.EndDate.Value.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture) : "not finished"
                        })
                        .ToList()
                    })
                    .ToList();

            foreach (var item in employees)
            {
                sb.AppendLine($"{item.FirstName} {item.LastName} - Manager: {item.ManagerFirst} {item.ManagerLast}");
                foreach (var proj in item.ProjectsList)
                {
                    sb.AppendLine($"--{proj.Project} - {proj.Start} - {proj.End}");
                }
            }

            return sb.ToString();
        }

        public static string GetAddressesByTown(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var addresses = context.Addresses.Select(a => new
            {
                Text = a.AddressText,
                Name = a.Town.Name,
                Count = a.Employees.Count
            })
            .OrderByDescending(a => a.Count)
            .ThenBy(a => a.Name)
            .ThenBy(a => a.Text)
            .Take(10)
            .ToList();

            foreach (var a in addresses)
            {
                sb.AppendLine($"{a.Text}, {a.Name} - {a.Count} employees");
            }

            return sb.ToString();
        }

        public static string GetEmployee147(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var emp = context.Employees
                .Select(e => new
                {
                    e.EmployeeId,
                    e.FirstName,
                    e.LastName,
                    e.JobTitle,
                    Projects = e.EmployeesProjects
                    .Select(p => new
                    {
                        Proj = p.Project.Name
                    })
                    .OrderBy(p => p.Proj)
                    .ToList()
                })
                .Where(e => e.EmployeeId == 147)
                .ToList();

            foreach (var e in emp)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle}");
                foreach (var p in e.Projects)
                {
                    sb.AppendLine($"{p.Proj}");
                }
            }

            return sb.ToString();
        }

        public static string GetDepartmentsWithMoreThan5Employees(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var departments = context.Departments
                .Where(d => d.Employees.Count > 5)
                .OrderBy(d=>d.Employees.Count)
                .ThenBy(d=>d.Name)
                .Select(d => new
            {
                DepartmentName = d.Name,
                DepartmentManagerFirstName = d.Manager.FirstName,
                DepartmentManagerLastName = d.Manager.LastName,
                Employees = d.Employees.Select(e => new
                {
                    EmployeeFirstName = e.FirstName,
                    EmployeeLastName = e.LastName,
                    EmployeeJobTitle = e.JobTitle
                })
                .OrderBy(e=> e.EmployeeFirstName)
                .ThenBy(e=>e.EmployeeLastName)
                .ToList()
            })
            .ToList();

            foreach (var d in departments)
            {
                sb.AppendLine($"{d.DepartmentName} - {d.DepartmentManagerFirstName} {d.DepartmentManagerLastName}");
                foreach (var e in d.Employees)
                {
                    sb.AppendLine($"{e.EmployeeFirstName} {e.EmployeeLastName} - {e.EmployeeJobTitle}");
                }
            }

            return sb.ToString();
        }

        public static string GetLatestProjects(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var projects = context.Projects
                .OrderByDescending(p => p.StartDate)
                .Take(10)
                .OrderBy(p => p.Name)
                .Select(p => new
            {
                ProjectName = p.Name,
                ProjectDescription = p.Description,
                ProjectDate = p.StartDate
            })         
                .ToList();

            foreach (var p in projects)
            {
                sb.AppendLine($"{p.ProjectName}");
                sb.AppendLine($"{p.ProjectDescription}");
                sb.AppendLine($"{p.ProjectDate}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string IncreaseSalaries(SoftUniContext context)
        {
            var sb = new StringBuilder();
            var employees = context.Employees
                .Where(e => e.Department.Name == "Engineering" ||
                           e.Department.Name == "Tool Design" ||
                           e.Department.Name == "Marketing" ||
                           e.Department.Name == "Information Services")
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToList();

            foreach (var e in employees)
            {
                e.Salary *= 1.12m;
            }

            context.SaveChanges();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} (${e.Salary:f2})");
            }

            return sb.ToString();
        }

        public static string GetEmployeesByFirstNameStartingWithSa(SoftUniContext context)
        {
            var sb = new StringBuilder();

            var employees = context.Employees
                                .Where(e => e.FirstName.ToLower().StartsWith("sa"))
                                .Select(e => new
                                {
                                    e.FirstName,
                                    e.LastName,
                                    e.JobTitle,
                                    e.Salary
                                })
                                .OrderBy(e => e.FirstName)
                                .ThenBy(e => e.LastName)
                                .ToList();

            foreach (var e in employees)
            {
                sb.AppendLine($"{e.FirstName} {e.LastName} - {e.JobTitle} - (${e.Salary:f2})");
            }

            return sb.ToString();
        }

        public static string DeleteProjectById(SoftUniContext context)
        {
            var project = context.Projects.Find(2);
            var emp = context.EmployeesProjects.Where(ep => ep.ProjectId == 2);
            context.EmployeesProjects.RemoveRange(emp);
            context.Projects.Remove(project);
            context.SaveChanges();
            var sb = new StringBuilder();
            var projects = context.Projects.Select(p => p.Name).Take(10).ToList();
            foreach (var name in projects)
            {
                sb.AppendLine(name);
            }
            return sb.ToString();
        }

        public static string RemoveTown(SoftUniContext context)
        {
            var emp = context.Employees
                .Where(e => e.Address.Town.Name == "Seattle");

            foreach (var e in emp)
            {
                e.AddressId = null;
            }
         
            var addresses = context.Addresses.Where(a => a.Town.Name == "Seattle");
            int count = addresses.Count();
            context.Addresses.RemoveRange(addresses);
            var town = context.Towns.Where(t => t.Name == "Seattle").ToList();
            context.Towns.Remove(town.First());
            context.SaveChanges();

            return $"{count} addresses in Seattle were deleted";
        }
    }
}
