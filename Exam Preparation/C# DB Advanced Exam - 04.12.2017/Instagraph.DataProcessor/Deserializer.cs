using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Instagraph.Data;
using Instagraph.DataProcessor.Dto.Import;
using Instagraph.Models;
using Newtonsoft.Json;

namespace Instagraph.DataProcessor
{
    public class Deserializer
    {
        private static string successMessage = "Successfully imported {0}.";
        private static string errorMessage = "Error: Invalid data.";

        public static string ImportPictures(InstagraphContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedPictures = JsonConvert.DeserializeObject<List<ImportPictureDto>>(jsonString);
            var validPictures = new List<Picture>();

            foreach (var pictureDto in importedPictures)
            {
                if (!IsValid(pictureDto))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                if (string.IsNullOrEmpty(pictureDto.Path))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var picture = new Picture()
                {
                    Path = pictureDto.Path,
                    Size = pictureDto.Size
                };
                validPictures.Add(picture);
                sb.AppendLine($"Successfully imported Picture {picture.Path}.");
            }
            context.Pictures.AddRange(validPictures);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(InstagraphContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedUsers = JsonConvert.DeserializeObject<List<ImportUserDto>>(jsonString);
            var validUsers = new List<User>();

            foreach (var userDto in importedUsers)
            {
                if (!IsValid(userDto))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var dbPicture = context.Pictures
                    .FirstOrDefault(p => p.Path == userDto.ProfilePicture);

                if (dbPicture == null)
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var user = new User()
                {
                    Username = userDto.Username,
                    Password = userDto.Password,
                    ProfilePicture = dbPicture
                };
                validUsers.Add(user);
                sb.AppendLine($"Successfully imported User {user.Username}.");
            }
            context.Users.AddRange(validUsers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportFollowers(InstagraphContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedFollowers = JsonConvert.DeserializeObject<List<ImportFollowerDto>>(jsonString);
            var validUserFollowers = new List<UserFollower>();

            foreach (var followerDto in importedFollowers)
            {
                if (!IsValid(followerDto))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var user = context.Users
                    .FirstOrDefault(u => u.Username == followerDto.User);
                
                var follower = context.Users
                    .FirstOrDefault(f => f.Username == followerDto.Follower);

                var hasAlreadyFollowed = validUserFollowers
                    .Any(f => f.User == user && f.Follower == follower);

                if (user == null || follower == null || hasAlreadyFollowed)
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var userFollower = new UserFollower()
                {
                    User = user,
                    Follower = follower
                };
                validUserFollowers.Add(userFollower);
                sb.AppendLine($"Successfully imported Follower {follower.Username} to User {user.Username}.");
            }
            context.UsersFollowers.AddRange(validUserFollowers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPosts(InstagraphContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("posts");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportPostDto>), root);
            var importedPosts = new List<ImportPostDto>();
            using (var reader = new StringReader(xmlString))
            {
                importedPosts = (List<ImportPostDto>) serializer.Deserialize(reader);
            }

            var validPosts = new List<Post>();
            foreach (var postDto in importedPosts)
            {
                if (!IsValid(postDto))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var user = context.Users
                    .FirstOrDefault(u => u.Username == postDto.User);

                var picture = context.Pictures
                    .FirstOrDefault(p => p.Path == postDto.Picture);

                if (user == null || picture == null)
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var post = new Post()
                {
                    Caption = postDto.Caption,
                    User = user,
                    Picture = picture
                };
                validPosts.Add(post);
                sb.AppendLine($"Successfully imported Post {post.Caption}.");
            }
            context.Posts.AddRange(validPosts);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportComments(InstagraphContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("comments");
            XmlSerializer serializer = new XmlSerializer(typeof(List<ImportCommentDto>), root);
            var importedComments = new List<ImportCommentDto>();
            using (var reader = new StringReader(xmlString))
            {
                importedComments = (List<ImportCommentDto>)serializer.Deserialize(reader);
            }

            var validComments = new List<Comment>();

            foreach (var commentDto in importedComments)
            {
                if (!IsValid(commentDto))
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var user = context.Users
                    .FirstOrDefault(u => u.Username == commentDto.Username);

                var post = context.Posts
                    .FirstOrDefault(p => p.Id == commentDto.Post.Id);

                if (user == null || post == null)
                {
                    sb.AppendLine(errorMessage);
                    continue;
                }

                var comment = new Comment()
                {
                    Content = commentDto.Content,
                    Post = post,
                    User = user
                };
                validComments.Add(comment);
                sb.AppendLine($"Successfully imported Comment {comment.Content}.");
            }
            context.Comments.AddRange(validComments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }


        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
