using System.Collections.Generic;
using Newtonsoft.Json;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.ImportListMovies;
using NzbDrone.Core.ImportLists.TMDb;

namespace NzbDrone.Core.ImportLists.WhisparrList
{
    public class WhisparrListParser : IParseImportListResponse
    {
        public WhisparrListParser()
        {
        }

        public IList<ImportListMovie> ParseResponse(ImportListResponse importListResponse)
        {
            var importResponse = importListResponse;

            var movies = new List<ImportListMovie>();

            if (!PreProcess(importResponse))
            {
                return movies;
            }

            var jsonResponse = JsonConvert.DeserializeObject<List<MovieResultResource>>(importResponse.Content);

            // no movies were return
            if (jsonResponse == null)
            {
                return movies;
            }

            return jsonResponse.SelectList(m => new ImportListMovie { ForiegnId = m.Id });
        }

        protected virtual bool PreProcess(ImportListResponse importListResponse)
        {
            try
            {
                var error = JsonConvert.DeserializeObject<WhisparrErrors>(importListResponse.HttpResponse.Content);

                if (error != null && error.Errors != null && error.Errors.Count != 0)
                {
                    throw new WhisparrListException(error);
                }
            }
            catch (JsonSerializationException)
            {
                //No error!
            }

            if (importListResponse.HttpResponse.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new HttpException(importListResponse.HttpRequest, importListResponse.HttpResponse);
            }

            return true;
        }
    }
}
