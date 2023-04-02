using Microsoft.AspNetCore.Mvc;
namespace web_services_l1.Controllers;
[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{
    [HttpPost("UploadMovieCsv")]
    public string UploadMovieCsv(IFormFile inputFile)
    {/*
        var strm = inputFile.OpenReadStream();
        byte[] buffer = new byte[inputFile.Length];
        strm.Read(buffer,0,(int)inputFile.Length);
        string fileContent = System.Text.Encoding.Default.GetString(buffer);
        strm.Close();

        MoviesContext dbContext = new MoviesContext();
        List<Genre> genreList = new List<Genre>();
        List<Movie> movieList = new List<Movie>();

        bool skip_header = true;
        foreach(string line in fileContent.Split('\n'))
        {
            if(skip_header)
            {
                skip_header =false;
                continue;

            }
            var tokens = line.Split(",");
            if(tokens.Length != 3) continue;
            int movieID;
            string title = tokens[1];
            string[] genres = tokens[2].Split("|");

            try {
                movieID = int.Parse(tokens[0]);
            }catch (FormatException e) {
                Console.WriteLine("Parsing error: " + e);
                continue;
            }
            // List<Genre> movieGenre = 
            Movie movie = new Movie {
                MovieID = movieID,
                Title = title,
                Genres = new List<Genre>()
            }; 
           
            // do naprawy

            foreach(string genre in genres) {
                movie.Genres.Add(
                    new Genre {
                        
                    }
                );
            }



            foreach(string genre in Genres)
            {
                Genre g = new Genre();
                g.Name = genre;
                if(!dbContext.Genres.Any(e => e.Name == g.Name))
                {
                dbContext.Genres.Add(g);
                dbContext.SaveChanges();
                }
                IQueryable<Genre> results = dbContext.Genres.Where(e => e.Name == g.Name);
                if(results.Count() > 0)
                movieGenres.Add(results.First());
            }
            Movie m = new Movie();
            m.MovieID = int.Parse(MovieID);
            m.Title = MovieName;
            m.Genres = movieGenres;
            if(!dbContext.Movies.Any(e=>e.MovieID == m.MovieID)) dbContext.Movies.Add(m);
            dbContext.SaveChanges();
            }
            dbContext.SaveChanges();
        */
            return "OK";
    }

 
    [HttpPost("UploadUsersRatingCsv")]
    public string UploadUsersRatingCsv(IFormFile inputFile) {
        var strm = inputFile.OpenReadStream();
        byte[] buffer = new byte[inputFile.Length];
        strm.Read(buffer,0,(int)inputFile.Length);
        string fileContent = System.Text.Encoding.Default.GetString(buffer);
        strm.Close();

        MoviesContext dbContext = new MoviesContext();
        var usersList = new List<User>();
        var ratingList = new List<Rating>();
        int ratingID;
        try { 
            ratingID = dbContext.Ratings.Max(x => x.RatingID) + 1;
        } catch (System.InvalidOperationException ex ) {
            ratingID = 1;
            Console.WriteLine("Baza danych pusta: " + ex);
        }

        bool skip_header = true;
        foreach(string line in fileContent.Split('\n')) {
            if (skip_header) {
                skip_header = false;
                continue;
            }

            var tokens = line.Split(",");
             if (tokens.Length != 4) continue;
            int userID;
            try {
                userID = int.Parse(tokens[0]);
            } catch (FormatException e) {
                Console.WriteLine("Parsing error " + e.Message + " " + tokens[2]);
                continue;
            }

            if (!dbContext.Users.Any(u => u.UserID == userID)  && 
            !usersList.Any(u => u.UserID == userID)) {
                usersList.Add(
                    new User {UserID = userID, Name = null }
                );
            }

        }
        
        dbContext.Users.AddRange(usersList);
        dbContext.SaveChanges();

        skip_header = true;
        foreach(string line in fileContent.Split('\n')) {
            if (skip_header) {
                skip_header = false;
                continue;
            }

            var tokens = line.Split(",");
            if (tokens.Length != 4) continue;
            
            int movieID;
            float ratingValue;
            int userID;
            try {
                 userID = int.Parse(tokens[0]);
                 movieID = int.Parse(tokens[1]);
                 ratingValue = Utils.parseFloat(tokens[2]);

            } catch(FormatException e){
                 Console.WriteLine("Parsing error " + e.Message);
                 continue;
            } 
            if (!( dbContext.Ratings.Any(r => r.RatedMovie.MovieID == movieID) && 
                 dbContext.Ratings.Any(u => u.RatingUser.UserID == userID)) )   {

                ratingList.Add(
                    new Rating {
                        RatingID = ratingID,
                        RatingValue = ratingValue, 
                        RatedMovie = dbContext.Movies.Find(movieID), 
                        RatingUser = dbContext.Users.Find(userID)
                    }
                );
                ratingID+=1;
            }

        }

        dbContext.Ratings.AddRange(ratingList);
        dbContext.SaveChanges();

        return "OK";
    }


    [HttpGet("GetAllGenres")]
    public IEnumerable<Genre> GetAllGenres()
    {
        MoviesContext dbContext = new MoviesContext();
        return dbContext.Genres.AsEnumerable();
    }

    [HttpGet("GetMoviesByName/{search_phrase}")]
    public IEnumerable<Movie> GetMoviesByName(string search_phrase)
    {
        MoviesContext dbContext = new MoviesContext();
        return dbContext.Movies.Where(e => e.Title.Contains(search_phrase));
    }

    [HttpPost("GetMoviesByGenre")]
    public IEnumerable<Movie> GetMoviesByGenre(string search_phrase)
    {
        MoviesContext dbContext = new MoviesContext();
        return dbContext.Movies.Where(
        m => m.Genres.Any( p => p.Name.Contains(search_phrase))
        );
    }



}
