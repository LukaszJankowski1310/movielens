using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace web_services_l1.Controllers;
[ApiController]
[Route("[controller]")]
public class MoviesController : ControllerBase
{   
    // private MoviesContext dbContext;

    // public MoviesController() {
    //     // dbContext = new MoviesContext();
    // }

    [HttpPost("UploadMovieCsv")]
    public string UploadMovieCsv(IFormFile inputFile)
    {
         MoviesContext dbContext = new MoviesContext();
        var strm = inputFile.OpenReadStream();
        byte[] buffer = new byte[inputFile.Length];
        strm.Read(buffer,0,(int)inputFile.Length);
        string fileContent = System.Text.Encoding.Default.GetString(buffer);
        strm.Close();

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
            List<Genre> movieGenres = new List<Genre>();
            try {
                movieID = int.Parse(tokens[0]);
            }catch (FormatException e) {
                Console.WriteLine("Parsing error: " + e);
                continue;
            }
            foreach(string genre_name in genres) {
                Genre genre = new Genre {Name = genre_name};
                if(!genreList.Any(g => g.Name == genre.Name) && !dbContext.Genres.Any(g => g.Name == genre.Name)){ 
                    genreList.Add(genre);
                    dbContext.Genres.Add(genre);
                    dbContext.SaveChanges();
                }
                IQueryable<Genre> results = dbContext.Genres.Where(g => g.Name == genre.Name);
                if (results.Count() > 0) {
                    movieGenres.Add(results.First());
                }
            }
            Movie movie = new Movie {Title = title, Genres = movieGenres, MovieID = movieID};

            if (!movieList.Any(m => m.MovieID == movie.MovieID) && !dbContext.Movies.Any(m => movieID == movie.MovieID)) {
                movieList.Add(movie);
            }
        }

        Console.WriteLine(genreList.Count());
        dbContext.Movies.AddRange(movieList);
        dbContext.SaveChanges();
        Console.WriteLine(dbContext.Genres.Count());

        return "OK";
    }

 
    [HttpPost("UploadUsersRatingCsv")]
    public string UploadUsersRatingCsv(IFormFile inputFile) {
         MoviesContext dbContext = new MoviesContext();
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

    [HttpGet("GetMovie/{id}")]
    public Movie getMovieById(int id) {
         MoviesContext dbContext = new MoviesContext();
        var movie = dbContext.Movies.Include(m => m.Genres).Where(m => m.MovieID == id).First();               
        return movie;
    }

// zad 1

    [HttpGet("GetMovieGenresByMovieId/{movie_id}")]
     public IEnumerable<Genre> GetAllGenres(int movie_id) {
         MoviesContext dbContext = new MoviesContext();

        return dbContext.Genres.Where(g => g.Movies.Any(m => m.MovieID == movie_id));
     }


     [HttpGet("GetAllMovies")]
      public IEnumerable<Movie> GetAllMovies() {
        MoviesContext dbContext = new MoviesContext();
        return dbContext.Movies.Include(m => m.Genres);
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

    // zad 4

    [HttpGet("GetSimilarMoviesToMovie/{id}")]
    public IEnumerable<Movie> GetSimilarMoviesToMovie(int id) {
        MoviesContext dbContext = new MoviesContext();
        List<Movie> simMovies = new List<Movie>();
        Movie movie = dbContext.Movies.Include(m => m.Genres)
                                .Where(m=> m.MovieID ==  id)
                                .First();
        var genres = movie.Genres;
        var allMovies = GetAllMovies();
        Console.WriteLine(genres.Count());
        int i = 0;
        foreach (var m in allMovies) {
            if (m.MovieID == movie.MovieID) continue;

            foreach (var genre in genres) {      
                i++;
                if (m.Genres.Any(g => g.GenreID == genre.GenreID)) { 
                    simMovies.Add(m);
                    break;
                }
            }
        }
        Console.WriteLine(i);       
        return simMovies;

    }
    
     [HttpGet("GetSimilarMoviesWithThreshold/{id}/{threshold}")]
     public IEnumerable<Movie> GetSimilarMoviesWithThreshold(int id, double threshold) {
        MoviesContext dbContext = new MoviesContext();

        List<Movie> finalMovies  = new List<Movie>();
        Movie movie = getMovieById(id);
        List<int> movieGenresVector = GetVectorOfMovieGenres(id);
        var allMovies = dbContext.Movies;
        foreach(var m in allMovies) {
            if (m.MovieID == id) continue;
            List<int> mGenreVector = GetVectorOfMovieGenres(m.MovieID);
            double similarity = cosineSimilarity(movieGenresVector, mGenreVector);
            if (similarity >= threshold) {
                finalMovies.Add(m);
            }
        }

        return finalMovies;
     }

    [HttpGet("GetMoviesRatedByUser/{userId}")]
    public IEnumerable<Movie> GetMoviesRatedByUser(int userId){
        MoviesContext dbContext = new MoviesContext();
       var rat =  dbContext.Ratings
        .Include(r => r.RatingUser)
        .Include(r => r.RatedMovie)
        .Where(r => r.RatingUser.UserID == userId);
        return dbContext.Movies.Where(m => rat.Any(r=> r.RatedMovie.MovieID == m.MovieID));
    }
    [HttpGet("GetOrderedByValueMoviesRatedByUser/{userId}")]
    public IEnumerable<Rating> GetOrderedByValueMoviesRatedByUser(int userId) {
        MoviesContext dbContext = new MoviesContext();
         var rat =  dbContext.Ratings
        .Include(r => r.RatingUser)
        .Include(r => r.RatedMovie)
        .Where(r => r.RatingUser.UserID == userId).OrderByDescending(r => r.RatingValue);
        return rat;

    }
    [HttpGet("GetHighestRatedMovieByUser/{userId}")]
    public Movie GetHighestRatedMovieByUser(int userId) {
        return GetOrderedByValueMoviesRatedByUser(userId).First().RatedMovie;
    }

    [HttpGet("GetSimilarMoviesToHighesRatedMovieByUser/{userId}")]
    public IEnumerable<Movie> GetSimilarMoviesToHighesRatedMovieByUser(int userId) {
        Movie highestRatedMovie = GetHighestRatedMovieByUser(userId);
        return GetSimilarMoviesWithThreshold(highestRatedMovie.MovieID, 0.5);

    }
    
    [HttpGet("GetSetOfSimilarMovies/{userId}")]
    public IEnumerable<Movie> GetSetOfSimilarMovies(int userId) {
       return GetSimilarMoviesToHighesRatedMovieByUser(userId).Take(10);
    }

}