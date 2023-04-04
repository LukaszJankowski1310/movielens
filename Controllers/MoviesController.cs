using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace web_services_l1.Controllers;
[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{   
    private MoviesContext dbContext;

    public MoviesController() {
        dbContext = new MoviesContext();
    }

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

        // MoviesContext dbContext = new MoviesContext();
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
        return dbContext.Genres.AsEnumerable();
    }

    [HttpGet("GetMoviesByName/{search_phrase}")]
    public IEnumerable<Movie> GetMoviesByName(string search_phrase)
    {
        // MoviesContext dbContext = new MoviesContext();
        return dbContext.Movies.Where(e => e.Title.Contains(search_phrase));
    }

    [HttpPost("GetMoviesByGenre")]
    public IEnumerable<Movie> GetMoviesByGenre(string search_phrase)
    {
        // MoviesContext dbContext = new MoviesContext();
        return dbContext.Movies.Where(
        m => m.Genres.Any( p => p.Name.Contains(search_phrase))
        );
    }

    [HttpGet("GetMovie/{id}")]
    public Movie getMovieById(int id) {
        var movie = dbContext.Movies.Include(m => m.Genres).Where(m => m.MovieID == id).First();               
        return movie;
    }

// zad 1

    [HttpGet("GetMovieGenresByMovieId/{movie_id}")]
     public IEnumerable<Genre> GetAllGenres(int movie_id) {
        return dbContext.Genres.Where(g => g.Movies.Any(m => m.MovieID == movie_id));
     }


// zad 2

    [HttpGet("GetVectorOfMovieGenres/{id}")]
    public List<int> GetVectorOfMovieGenres(int id) {
        List<int> numVector = new List<int>();
        List<Genre> allGenres = GetAllGenres().OrderBy(g => g.GenreID).ToList();
        Movie movie = getMovieById(id);
        if (movie != null) {
            foreach(Genre genre in allGenres){
                if (movie.Genres.Any(g => g.GenreID == genre.GenreID)) numVector.Add(1);   
                else numVector.Add(0);   
            }
        }
        return numVector;
    }

    // zad 3

    double cosineSimilarity(List<int> l1, List<int> l2) {
        double num = 0;
        double dem = 0;
        double dem1 = 0;
        double dem2 = 0;
        for (int i = 0; i < l1.Count; i++) {
            num += l1[i]*l2[i];
            dem1+= l1[i]*l1[i];
            dem2+= l2[i]*l2[i];
        }
        dem = Math.Sqrt(dem1) * Math.Sqrt(dem2);
        return num/dem;
    }

    [HttpGet("GetCosineSim/{id1}/{id2}")]
    public double GetCosineSim(int id1, int id2) {
        List<int> v1 = GetVectorOfMovieGenres(id1);
        List<int> v2 = GetVectorOfMovieGenres(id2);
        return cosineSimilarity(v1, v2);
    }


}
