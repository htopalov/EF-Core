using System.Globalization;
using System.Linq;
using System.Text;

namespace MusicHub
{
    using System;

    using Data;
    using Initializer;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context = 
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            //Console.WriteLine(ExportAlbumsInfo(context,9));
            Console.WriteLine(ExportSongsAboveDuration(context,4));
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albums = context.Albums
                .Where(a => a.ProducerId == producerId)
                .Select(a => new
                {
                    AlbumName = a.Name,
                    ReleaseDate = a.ReleaseDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture),
                    ProducerName = a.Producer.Name,
                    TotalPrice = a.Price,
                    Songs = a.Songs.Select(s => new
                        {
                            Name = s.Name,
                            Price = s.Price,
                            Writer = s.Writer.Name
                        })
                        .OrderByDescending(s => s.Name)
                        .ThenBy(s => s.Writer)
                        .ToList()
                })
                .OrderByDescending(a=>a.TotalPrice)
                .ToList();
            var sb = new StringBuilder();
           
            foreach (var album in albums)
            {
                int counter = 1;
                sb.AppendLine($"-AlbumName: {album.AlbumName}");
                sb.AppendLine($"-ReleaseDate: {album.ReleaseDate}");
                sb.AppendLine($"-ProducerName: {album.ProducerName}");
                sb.AppendLine("-Songs:");
                foreach (var song in album.Songs)
                {
                    sb.AppendLine($"---#{counter}");
                    sb.AppendLine($"---SongName: {song.Name}");
                    sb.AppendLine($"---Price: {song.Price:f2}");
                    sb.AppendLine($"---Writer: {song.Writer}");
                    counter++;
                }

                sb.AppendLine($"-AlbumPrice: {album.TotalPrice:f2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs
                .Where(s => TimeSpan.Parse(s.Duration.ToString()).TotalSeconds > duration)
                .Select(s => new
                {
                    Name = s.Name,
                    Performer = s.SongPerformers
                        .Where(sp => sp.SongId == s.Id)
                        .Select(p => new
                        {
                            FirstName = p.Performer.FirstName,
                            LastName = p.Performer.LastName
                        })
                        .SingleOrDefault(),
                    Writer = s.Writer.Name,
                    Producer = s.Album.Producer.Name,
                    Duration = s.Duration
                })
                .OrderBy(s=> s.Name)
                .ThenBy(s=>s.Writer)
                .ThenBy(s=>s.Performer)
                .ToList();

            var sb = new StringBuilder();
            int counter = 1;
            foreach (var song in songs)
            {
                sb.AppendLine($"-Song #{counter}");
                sb.AppendLine($"---SongName: {song.Name}");
                sb.AppendLine($"---Writer: {song.Writer}");
                sb.AppendLine($"---Performer: {song.Performer}");
                sb.AppendLine($"---AlbumProducer: {song.Producer}");
                sb.AppendLine($"---Duration: {song.Duration.ToString()}");
                counter++;
            }

            return sb.ToString().TrimEnd();
        }
    }
}
