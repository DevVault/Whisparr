﻿using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieRenamedEvent : IEvent
    {
        public Media Movie { get; private set; }
        public List<RenamedMovieFile> RenamedFiles { get; private set; }

        public MovieRenamedEvent(Media movie, List<RenamedMovieFile> renamedFiles)
        {
            Movie = movie;
            RenamedFiles = renamedFiles;
        }
    }
}
