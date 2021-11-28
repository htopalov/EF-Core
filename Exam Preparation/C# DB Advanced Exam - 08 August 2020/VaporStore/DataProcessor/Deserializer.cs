using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using VaporStore.Data.Models;
using VaporStore.Data.Models.Enums;
using VaporStore.DataProcessor.Dto.Import;

namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Data;

	public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();
            var importedGames = JsonConvert.DeserializeObject<ImportGameDto[]>(jsonString);
            var games = new List<Game>();
            var developers = new List<Developer>();
            var genres = new List<Genre>();
            var tags = new List<Tag>();

            foreach (var gameDto in importedGames)
            {
                if (!IsValid(gameDto))
                {
                    sb.AppendLine("Invalid Data");
					continue;
                }

                if (gameDto.Tags.Length == 0)
                {
                    sb.AppendLine("Invalid Data");
					continue;
                }

                bool isReleaseDateValid = DateTime.TryParseExact(gameDto.ReleaseDate, "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime releaseDate);
                if (!isReleaseDateValid)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var game = new Game()
                {
                    Name = gameDto.Name,
                    ReleaseDate = releaseDate,
                    Price = gameDto.Price
                };
                //game developers part
                var gameDeveloper = developers.FirstOrDefault(d => d.Name == gameDto.Developer);
                if (gameDeveloper == null)
                {
                    var developer = new Developer()
                    {
                        Name = gameDto.Developer
                    };
                    developers.Add(developer);
                    game.Developer = developer;
                }
                else
                {
                    game.Developer = gameDeveloper;
                }

                //game genre part
                var gameGenre = genres.FirstOrDefault(g => g.Name == gameDto.Genre);
                if (gameGenre == null)
                {
                    var genre = new Genre()
                    {
                        Name = gameDto.Genre
                    };
                    genres.Add(genre);
                    game.Genre = genre;
                }
                else
                {
                    game.Genre = gameGenre;
                }

                //tag part

                foreach (var tagName in gameDto.Tags)
                {
                    if (string.IsNullOrEmpty(tagName))
                    {
                        sb.AppendLine("Invalid Data");
                        continue;
                    }

                    Tag tag = tags.FirstOrDefault(x => x.Name == tagName);

                    if (tag == null)
                    {
                        Tag newTag = new Tag()
                        {
                            Name = tagName
                        };
                        tags.Add(newTag);
                        game.GameTags.Add(new GameTag()
                        {
                            Game = game,
                            Tag = newTag
                        });
                    }
                    else
                    {
                        game.GameTags.Add(new GameTag()
                        {
                            Game = game,
                            Tag = tag
                        });
                    }
                }

                if (game.GameTags.Count == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }
                games.Add(game);
                sb.AppendLine($"Added {game.Name} ({game.Genre.Name}) with {game.GameTags.Count} tags");
            }

            context.Games.AddRange(games);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportUsers(VaporStoreDbContext context, string jsonString)
        {
            StringBuilder sb = new StringBuilder();

            ImportUserDto[] userDtos = JsonConvert.DeserializeObject<ImportUserDto[]>(jsonString);

            List<User> users = new List<User>();

            foreach (ImportUserDto userDto in userDtos)
            {
                if (!IsValid(userDto))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                List<Card> userCards = new List<Card>();

                bool areAllCardsValid = true;
                foreach (ImportUserCardDto cardDto in userDto.Cards)
                {
                    if (!IsValid(cardDto))
                    {
                        areAllCardsValid = false;
                        break;
                    }

                    Object cardTypeRes;
                    bool isCardTypeValid = Enum.TryParse(typeof(CardType), cardDto.Type, out cardTypeRes);

                    if (!isCardTypeValid)
                    {
                        areAllCardsValid = false;
                        break;
                    }

                    CardType cardType = (CardType)cardTypeRes;

                    userCards.Add(new Card()
                    {
                        Number = cardDto.Number,
                        Cvc = cardDto.Cvc,
                        Type = cardType
                    });
                }

                if (!areAllCardsValid)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                if (userCards.Count == 0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                User u = new User()
                {
                    Username = userDto.Username,
                    FullName = userDto.FullName,
                    Email = userDto.Email,
                    Age = userDto.Age,
                    Cards = userCards
                };

                users.Add(u);
                sb.AppendLine($"Imported {u.Username} with {u.Cards.Count} cards");
            }

            context.Users.AddRange(users);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
        {
            StringBuilder sb = new StringBuilder();
            XmlRootAttribute root = new XmlRootAttribute("Purchases");
            XmlSerializer serializer = new XmlSerializer(typeof(ImportPurchaseDto[]), root);
            using StringReader reader = new StringReader(xmlString);
            var importedPurchases = (ImportPurchaseDto[])serializer.Deserialize(reader);

            var purchases = new List<Purchase>();
            foreach (var purchaseDto in importedPurchases)
            {
                if (!IsValid(purchaseDto))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                bool isDateValid = DateTime.TryParseExact(purchaseDto.Date, "dd/MM/yyyy HH:mm",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime validPurchaseDate);

                if (!isDateValid)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                Object validPurchaseType;
                bool isPurchaseTypeValid = Enum.TryParse(typeof(PurchaseType), purchaseDto.Type, out validPurchaseType);

                if (!isPurchaseTypeValid)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var card = context.Cards
                    .FirstOrDefault(c => c.Number == purchaseDto.Card);
                if (card == null)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var game = context.Games
                    .FirstOrDefault(g => g.Name == purchaseDto.Title);
                if (game == null)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }
                var purchase = new Purchase()
                {
                    Type = (PurchaseType)validPurchaseType,
                    Date = validPurchaseDate,
                    ProductKey = purchaseDto.ProductKey,
                    Card = card,
                    Game = game
                };

                purchases.Add(purchase);
                sb.AppendLine($"Imported {purchase.Game.Name} for {purchase.Card.User.Username}");
            }

            context.Purchases.AddRange(purchases);
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