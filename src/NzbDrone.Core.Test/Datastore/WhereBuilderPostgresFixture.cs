using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore
{
    [TestFixture]
    public class WhereBuilderPostgresFixture : CoreTest
    {
        private WhereBuilderPostgres _subject;

        [OneTimeSetUp]
        public void MapTables()
        {
            // Generate table mapping
            Mocker.Resolve<DbFactory>();
        }

        private WhereBuilderPostgres Where(Expression<Func<Media, bool>> filter)
        {
            return new WhereBuilderPostgres(filter, true, 0);
        }

        private WhereBuilderPostgres WhereMeta(Expression<Func<MediaMetadata, bool>> filter)
        {
            return new WhereBuilderPostgres(filter, true, 0);
        }

        [Test]
        public void postgres_where_equal_const()
        {
            _subject = Where(x => x.Id == 10);

            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(10);
        }

        [Test]
        public void postgres_where_equal_variable()
        {
            var id = 10;
            _subject = Where(x => x.Id == id);

            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(id);
        }

        [Test]
        public void postgres_where_equal_property()
        {
            var movie = new Media { Id = 10 };
            _subject = Where(x => x.Id == movie.Id);

            _subject.Parameters.ParameterNames.Should().HaveCount(1);
            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(movie.Id);
        }

        [Test]
        public void postgres_where_equal_joined_property()
        {
            _subject = Where(x => x.Profile.Id == 1);

            _subject.Parameters.ParameterNames.Should().HaveCount(1);
            _subject.ToString().Should().Be($"(\"Profiles\".\"Id\" = @Clause1_P1)");
            _subject.Parameters.Get<int>("Clause1_P1").Should().Be(1);
        }

        [Test]
        public void postgres_where_throws_without_concrete_condition_if_requiresConcreteCondition()
        {
            Expression<Func<Media, Media, bool>> filter = (x, y) => x.Id == y.Id;
            _subject = new WhereBuilderPostgres(filter, true, 0);
            Assert.Throws<InvalidOperationException>(() => _subject.ToString());
        }

        [Test]
        public void postgres_where_allows_abstract_condition_if_not_requiresConcreteCondition()
        {
            Expression<Func<Media, Media, bool>> filter = (x, y) => x.Id == y.Id;
            _subject = new WhereBuilderPostgres(filter, false, 0);
            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" = \"Movies\".\"Id\")");
        }

        [Test]
        public void postgres_where_string_is_null()
        {
            _subject = WhereMeta(x => x.CleanTitle == null);

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"CleanTitle\" IS NULL)");
        }

        [Test]
        public void postgres_where_string_is_null_value()
        {
            string cleanTitle = null;
            _subject = WhereMeta(x => x.CleanTitle == cleanTitle);

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"CleanTitle\" IS NULL)");
        }

        [Test]
        public void postgres_where_equal_null_property()
        {
            var movie = new MediaMetadata { CleanTitle = null };
            _subject = WhereMeta(x => x.CleanTitle == movie.CleanTitle);

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"CleanTitle\" IS NULL)");
        }

        [Test]
        public void postgres_where_column_contains_string()
        {
            var test = "small";
            _subject = WhereMeta(x => x.CleanTitle.Contains(test));

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"CleanTitle\" ILIKE '%' || @Clause1_P1 || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void postgres_where_string_contains_column()
        {
            var test = "small";
            _subject = WhereMeta(x => test.Contains(x.CleanTitle));

            _subject.ToString().Should().Be($"(@Clause1_P1 ILIKE '%' || \"MovieMetadata\".\"CleanTitle\" || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void postgres_where_column_starts_with_string()
        {
            var test = "small";
            _subject = WhereMeta(x => x.CleanTitle.StartsWith(test));

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"CleanTitle\" ILIKE @Clause1_P1 || '%')");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void postgres_where_column_ends_with_string()
        {
            var test = "small";
            _subject = WhereMeta(x => x.CleanTitle.EndsWith(test));

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"CleanTitle\" ILIKE '%' || @Clause1_P1)");
            _subject.Parameters.Get<string>("Clause1_P1").Should().Be(test);
        }

        [Test]
        public void postgres_where_in_list()
        {
            var list = new List<int> { 1, 2, 3 };
            _subject = Where(x => list.Contains(x.Id));

            _subject.ToString().Should().Be($"(\"Movies\".\"Id\" = ANY (('{{1, 2, 3}}')))");
        }

        [Test]
        public void postgres_where_in_list_2()
        {
            var list = new List<int> { 1, 2, 3 };
            _subject = WhereMeta(x => x.CleanTitle == "test" && list.Contains(x.Id));

            _subject.ToString().Should().Be($"((\"MovieMetadata\".\"CleanTitle\" = @Clause1_P1) AND (\"MovieMetadata\".\"Id\" = ANY (('{{1, 2, 3}}'))))");
        }

        [Test]
        public void postgres_where_in_string_list()
        {
            var list = new List<string> { "first", "second", "third" };

            _subject = WhereMeta(x => list.Contains(x.CleanTitle));

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"CleanTitle\" = ANY (@Clause1_P1))");
        }

        [Test]
        public void enum_as_int()
        {
            _subject = WhereMeta(x => x.Status == MovieStatusType.Announced);

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"Status\" = @Clause1_P1)");
        }

        [Test]
        public void enum_in_list()
        {
            var allowed = new List<MovieStatusType> { MovieStatusType.Announced, MovieStatusType.InCinemas };
            _subject = WhereMeta(x => allowed.Contains(x.Status));

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"Status\" = ANY (@Clause1_P1))");
        }

        [Test]
        public void enum_in_array()
        {
            var allowed = new MovieStatusType[] { MovieStatusType.Announced, MovieStatusType.InCinemas };
            _subject = WhereMeta(x => allowed.Contains(x.Status));

            _subject.ToString().Should().Be($"(\"MovieMetadata\".\"Status\" = ANY (@Clause1_P1))");
        }
    }
}
