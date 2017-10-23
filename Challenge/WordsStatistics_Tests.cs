using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Challenge
{
    [TestFixture]
    public class WordsStatistics_Tests
    {
        public static string Authors = "godjan"; // "Egorov Shagalina"

        public virtual IWordsStatistics CreateStatistics()
        {
            // меняется на разные реализации при запуске exe
            return new WordsStatistics();
        }

        private IWordsStatistics statistics;

        [SetUp]
        public void SetUp()
        {
            statistics = CreateStatistics();
        }

        [Test]
        public void GetStatistics_IsEmpty_AfterCreation()
        {
            statistics.GetStatistics().Should().BeEmpty();
        }

        [Test]
        public void GetStatistics_ContainsItem_AfterAddition()
        {
            statistics.AddWord("abc");
            statistics.GetStatistics().Should().Equal(Tuple.Create(1, "abc"));
        }

        [Test]
        public void GetStatistics_ContainsManyItems_AfterAdditionOfDifferentWords()
        {
            statistics.AddWord("abc");
            statistics.AddWord("def");
            statistics.GetStatistics().Should().HaveCount(2);
        }

        [Test]
        public void GetStatistics_ContainsRightCountOfItem()
        {
            statistics.AddWord("abc");
            statistics.AddWord("abc");
            statistics.GetStatistics().First().Item1.Should().Be(2);
        }

        [Test]
        public void GetStatistics_NotContainsNulls()
        {
            Assert.Throws<ArgumentNullException>(() => statistics.AddWord(null));
        }

        [Test]
        public void GetStatistics_NotContainsWhiteSpacesOrEmptyStrings()
        {
            statistics.AddWord("abc");
            statistics.AddWord("");
            statistics.AddWord("   ");
            statistics.GetStatistics().Count().Should().Be(1);
        }

        [Test]
        public void GetStatistics_OverLength()
        {
            statistics.AddWord("aabbaabbaabb");
            statistics.GetStatistics().First().Item2.Should().Be("aabbaabbaa");
        }

        [Test]
        public void GetStatistics_CollisionsStrings()
        {
            statistics.AddWord("aabbaabbaabb");
            statistics.AddWord("aabbaabbaa");
            statistics.GetStatistics().First().Item1.Should().Be(2);
        }

        [Test]
        public void GetStatistics_10WhiteSpacesAndSymbols()
        {
            statistics.AddWord("            sdfjsdf");
            statistics.GetStatistics().First().Item2.Should().Be("          ");
        }

        [Test]
        public void GetStatistics_IgnoreCasesDifferences()
        {
            statistics.AddWord("abc");
            statistics.AddWord("ABC");
            statistics.GetStatistics().First().Item1.Should().Be(2);
        }

        [Test]
        public void GetStatistics_CyrillicSymbolsYoBehavior()
        {
            statistics.AddWord("ввапЁпав");
            statistics.GetStatistics().First().Item2.Should().Be("ввапёпав");
        }

        [Test]
        public void GetStatistics_SortByValues()
        {
            statistics.AddWord("bbb");
            statistics.AddWord("aaa");
            statistics.AddWord("bbb");
            statistics.GetStatistics()
                .Select(pair => pair.Item1)
                .Should()
                .BeInDescendingOrder();
        }

        [Test]
        public void GetStatistics_SortByKeys()
        {
            statistics.AddWord("bbb");
            statistics.AddWord("aaa");
            statistics.GetStatistics()
                .Select(pair => pair.Item2)
                .ToArray()
                .Should()
                .BeEquivalentTo("aaa", "bbb");
        }

        [Test, Timeout(2000)]
        public void GetStatistics_ALotOfElements()
        {
            for (var i = 0; i < 1e5; i++)
                statistics.AddWord("a" + i);
            statistics.GetStatistics().Count().Should().Be(100000);
        }

        [Test]
        public void GetStatistics_StatisticsKeepAllWords()
        {
            statistics.GetStatistics();
            statistics.AddWord("abc");
            statistics.GetStatistics().Count().Should().Be(1);
        }

        [Test, Timeout(2000)]
        public void GetStatistics_PerformanceTest()
        {
            for (var i = 0; i < 1e5; i++)
                statistics.AddWord("a" + i);
            for (var i = 0; i < 1e5; i++)
                statistics.AddWord("a" + i);
            statistics.GetStatistics().Count().Should().Be(100000);
        }

        [Test]
        public void GetStatistics_11_IsNotLengthOfKey()
        {
            statistics.AddWord("aabbaabbaab");
            statistics.GetStatistics().First().Item2.Should().Be("aabbaabbaa");
        }

        [Test]
        public void GetStatistics_SortByValuesDescendingThanByKeys()
        {
            statistics.AddWord("bbb");
            statistics.AddWord("ccc");
            statistics.AddWord("aaa");
            statistics.AddWord("abc");
            statistics.GetStatistics()
                .Select(pair => pair.Item2)
                .ToArray()
                .ShouldBeEquivalentTo(new[] { "aaa", "abc", "bbb", "ccc" }, options => options.WithStrictOrderingFor(strings => strings));
        }

        [Test]
        public void GetStatistics_StatisticsIsNotStatic()
        {
            statistics.AddWord("abc");
            var firstStat = statistics;
            SetUp();
            firstStat.GetStatistics().Count().Should().Be(1);
        }

        [Test]
        public void GetStatistics_Statistics_IsNotRefreshAfterCallGetStatistics()
        {
            statistics.AddWord("abc");
            statistics.GetStatistics();
            statistics.GetStatistics().Count().Should().Be(1);
        }

        // Документация по FluentAssertions с примерами : https://github.com/fluentassertions/fluentassertions/wiki
    }
}
