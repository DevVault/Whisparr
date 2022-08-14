using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.Events;

namespace NzbDrone.Core.Movies.Credits
{
    public interface ICreditService
    {
        List<Credit> GetAllCreditsForMovieMetadata(int movieMetadataId);
        Credit AddCredit(Credit credit, MediaMetadata movie);
        List<Credit> AddCredits(List<Credit> credits, MediaMetadata movie);
        Credit GetById(int id);
        List<Credit> GetAllCredits();
        List<Credit> UpdateCredits(List<Credit> credits, MediaMetadata movie);
    }

    public class CreditService : ICreditService, IHandleAsync<MoviesDeletedEvent>
    {
        private readonly ICreditRepository _creditRepo;

        public CreditService(ICreditRepository creditRepo)
        {
            _creditRepo = creditRepo;
        }

        public List<Credit> GetAllCreditsForMovieMetadata(int movieMetadataId)
        {
            return _creditRepo.FindByMovieMetadataId(movieMetadataId).ToList();
        }

        public Credit AddCredit(Credit credit, MediaMetadata movie)
        {
            credit.MovieMetadataId = movie.Id;
            return _creditRepo.Insert(credit);
        }

        public List<Credit> AddCredits(List<Credit> credits, MediaMetadata movie)
        {
            credits.ForEach(t => t.MovieMetadataId = movie.Id);
            _creditRepo.InsertMany(credits);
            return credits;
        }

        public Credit GetById(int id)
        {
            return _creditRepo.Get(id);
        }

        public List<Credit> GetAllCredits()
        {
            return _creditRepo.All().ToList();
        }

        public void RemoveTitle(Credit credit)
        {
            _creditRepo.Delete(credit);
        }

        public List<Credit> UpdateCredits(List<Credit> credits, MediaMetadata movieMetadata)
        {
            int movieMetadataId = movieMetadata.Id;

            // First update the movie ids so we can correlate them later.
            credits.ForEach(t => t.MovieMetadataId = movieMetadataId);

            // Now find credits to delete, update and insert.
            var existingCredits = _creditRepo.FindByMovieMetadataId(movieMetadataId);

            // Should never have multiple credits with same credit_id, but check to ensure incase TMDB is on fritz
            var dupeFreeCredits = credits.DistinctBy(m => m.CreditForeignId).ToList();

            dupeFreeCredits.ForEach(c => c.Id = existingCredits.FirstOrDefault(t => t.CreditForeignId == c.CreditForeignId)?.Id ?? 0);

            var insert = dupeFreeCredits.Where(t => t.Id == 0).ToList();
            var update = dupeFreeCredits.Where(t => t.Id > 0).ToList();
            var delete = existingCredits.Where(t => !dupeFreeCredits.Any(c => c.CreditForeignId == t.CreditForeignId)).ToList();

            _creditRepo.DeleteMany(delete);
            _creditRepo.UpdateMany(update);
            _creditRepo.InsertMany(insert);

            return credits;
        }

        public void HandleAsync(MoviesDeletedEvent message)
        {
            // TODO handle metadata deletions and not movie deletions
            _creditRepo.DeleteForMovies(message.Movies.Select(m => m.MovieMetadataId).ToList());
        }
    }
}
