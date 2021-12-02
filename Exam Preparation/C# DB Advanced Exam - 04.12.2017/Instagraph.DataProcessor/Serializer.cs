using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Instagraph.Data;
using Instagraph.DataProcessor.Dto.Export;
using Newtonsoft.Json;

namespace Instagraph.DataProcessor
{
    public class Serializer
    {
        public static string ExportUncommentedPosts(InstagraphContext context)
        {
            var uncommentedPosts = context.Posts
                .Where(p => p.Comments.Count == 0)
                .Select(p => new
                {
                    Id = p.Id,
                    Picture = p.Picture.Path,
                    User = p.User.Username
                })
                .OrderBy(p => p.Id)
                .ToArray();

            return JsonConvert.SerializeObject(uncommentedPosts, Formatting.Indented);
        }

        public static string ExportPopularUsers(InstagraphContext context)
        {
            var popularUsers = context.Users
                .Where(u => u.Posts
                    .Any(p => p.Comments
                        .Select(c => c.UserId)
                        .Intersect(u.Followers
                            .Select(f => f.FollowerId))
                        .Any()))
                .OrderBy(u => u.Id)
                .Select(u => new
                {
                    Username = u.Username,
                    Followers = u.Followers.Count
                })
                .ToArray();

            return JsonConvert.SerializeObject(popularUsers, Formatting.Indented);
        }

        public static string ExportCommentsOnPosts(InstagraphContext context)
        {
            var users = context.Users
                .Select(u => new
                {
                    Username = u.Username,
                    Posts = u.Posts
                        .OrderByDescending(p => p.Comments.Count)
                        .FirstOrDefault()
                })
                .Select(u => new ExportUserCommentsDto()
                {
                    Username = u.Username,
                    MostComments = u.Posts.Comments == null ? 0 : u.Posts.Comments.Count
                })
                .OrderByDescending(u => u.MostComments)
                .ThenBy(u => u.Username)
                .ToArray();

            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("users");
            XmlSerializer serializer = new XmlSerializer(typeof(ExportUserCommentsDto[]), root);
            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty,string.Empty);
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, users, namespaces);
            }

            return sb.ToString().TrimEnd();
        }
    }
}
