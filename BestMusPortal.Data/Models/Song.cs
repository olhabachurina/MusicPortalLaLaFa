using System.ComponentModel.DataAnnotations;

namespace BestMusPortal.Models
{
    public class Song
    {
        public int SongId { get; set; }
        
        public string Title { get; set; }

       
        public string Artist { get; set; }

       
        public int GenreId { get; set; }
        public string  Genre { get; set; }

        public string Mood { get; set; }

        
        public string MusicFilePath { get; set; }

        public string VideoFilePath { get; set; }

        public int UserId { get; set; }
        public string VideoUrl { get; set; }
    }
}
    
    


