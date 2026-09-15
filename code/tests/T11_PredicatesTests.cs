// Tests for the note 11 B and C solutions.

using System;
using System.Collections.Generic;
using Xunit;
using A = Solutions.T11.A;
using B = Solutions.T11.B;
using C = Solutions.T11.C;

namespace Week01.Tests
{
    public class T11_SortingAndBest_B_Tests
    {
        private readonly List<A.Player> _players;

        public T11_SortingAndBest_B_Tests()
        {
            _players = new List<A.Player>
            {
                new A.Player { Id = "p1", Name = "Cara", Health = 40, MoveSpeed = 3f, IsActive = true },
                new A.Player { Id = "p2", Name = "Ana", Health = 90, MoveSpeed = 7f, IsActive = true },
                new A.Player { Id = "p3", Name = "Bo", Health = 0, MoveSpeed = 5f, IsActive = false }
            };
        }

        [Fact]
        public void SortByHealthDescending_PutsTheHighestHealthFirst()
        {
            B.SortingAndBest.SortByHealthDescending(_players);

            Assert.Equal("Ana", _players[0].Name);
            Assert.Equal("Bo", _players[2].Name);
        }

        [Fact]
        public void SortByName_OrdersAlphabetically()
        {
            B.SortingAndBest.SortByName(_players);

            Assert.Equal("Ana", _players[0].Name);
            Assert.Equal("Bo", _players[1].Name);
            Assert.Equal("Cara", _players[2].Name);
        }

        [Fact]
        public void SortBy_AcceptsAnyRuleWithoutChangingItsOwnCode()
        {
            B.SortingAndBest.SortBy(_players, (a, b) => a.MoveSpeed.CompareTo(b.MoveSpeed));

            Assert.Equal("Cara", _players[0].Name);
            Assert.Equal("Ana", _players[2].Name);
        }

        [Fact]
        public void SortedCopyBy_LeavesTheCallersListInItsOriginalOrder()
        {
            List<A.Player> sorted = B.SortingAndBest.SortedCopyBy(
                _players, (a, b) => a.Name.CompareTo(b.Name));

            Assert.Equal("Cara", _players[0].Name);     // untouched
            Assert.Equal("Ana", sorted[0].Name);
        }

        [Fact]
        public void FindFastest_ReturnsThePlayerWithTheHighestMoveSpeed()
        {
            Assert.Equal("Ana", B.SortingAndBest.FindFastest(_players).Name);
        }

        [Fact]
        public void FindFastest_DoesNotReorderTheList()
        {
            B.SortingAndBest.FindFastest(_players);

            Assert.Equal("Cara", _players[0].Name);
        }

        [Fact]
        public void FindFastest_OnAnEmptyList_ReturnsNull()
        {
            Assert.Null(B.SortingAndBest.FindFastest(new List<A.Player>()));
        }

        [Fact]
        public void FindFastest_WithOnePlayer_ReturnsThatPlayer()
        {
            List<A.Player> one = new List<A.Player> { _players[0] };

            Assert.Same(_players[0], B.SortingAndBest.FindFastest(one));
        }
    }

    public class T11_Scoreboard_C_Tests
    {
        private readonly List<A.Player> _players;
        private readonly C.Scoreboard _scoreboard;

        public T11_Scoreboard_C_Tests()
        {
            _players = new List<A.Player>
            {
                new A.Player { Id = "p1", Name = "Cara", Health = 40, MoveSpeed = 3f, IsActive = true },
                new A.Player { Id = "p2", Name = "Ana", Health = 90, MoveSpeed = 7f, IsActive = true },
                new A.Player { Id = "p3", Name = "Bo", Health = 20, MoveSpeed = 5f, IsActive = false }
            };

            _scoreboard = new C.Scoreboard(_players);
        }

        [Fact]
        public void Where_ReturnsOnlyMatchingPlayers()
        {
            List<A.Player> wounded = _scoreboard.Where(p => p.Health < 50);

            Assert.Equal(2, wounded.Count);
        }

        [Fact]
        public void And_CombinesTwoConditionsWithoutANewNamedMethod()
        {
            Predicate<A.Player> activeAndWounded =
                C.Scoreboard.And(p => p.IsActive, p => p.Health < 50);

            List<A.Player> result = _scoreboard.Where(activeAndWounded);

            Assert.Single(result);
            Assert.Equal("Cara", result[0].Name);
        }

        [Fact]
        public void Or_CombinesTwoConditions()
        {
            Predicate<A.Player> inactiveOrStrong =
                C.Scoreboard.Or(p => !p.IsActive, p => p.Health > 80);

            Assert.Equal(2, _scoreboard.Where(inactiveOrStrong).Count);
        }

        [Fact]
        public void ThreeConditions_CanBeCombinedByNesting()
        {
            Predicate<A.Player> all = C.Scoreboard.And(
                p => p.IsActive,
                C.Scoreboard.And(p => p.Health > 0, p => p.MoveSpeed > 5f));

            Assert.Single(_scoreboard.Where(all));
        }

        [Fact]
        public void Ordered_ReturnsASortedCopyAndLeavesTheScoreboardsListUntouched()
        {
            List<A.Player> byName = _scoreboard.Ordered((a, b) => a.Name.CompareTo(b.Name));

            Assert.Equal("Ana", byName[0].Name);
            Assert.Equal("Cara", _players[0].Name);
        }

        [Fact]
        public void TwoDifferentQueries_DoNotInterfereWithEachOther()
        {
            List<A.Player> byName = _scoreboard.Ordered((a, b) => a.Name.CompareTo(b.Name));
            List<A.Player> byHealth = _scoreboard.Ordered((a, b) => b.Health.CompareTo(a.Health));

            Assert.Equal("Ana", byName[0].Name);
            Assert.Equal("Ana", byHealth[0].Name);
            Assert.Equal("Cara", _players[0].Name);     // still the original order
        }

        [Fact]
        public void Top_ReturnsTheRequestedNumberOfHighestRankedPlayers()
        {
            List<A.Player> top2 = _scoreboard.Top((a, b) => b.Health.CompareTo(a.Health), 2);

            Assert.Equal(2, top2.Count);
            Assert.Equal("Ana", top2[0].Name);
            Assert.Equal("Cara", top2[1].Name);
        }

        [Fact]
        public void Top_AskedForMoreThanExist_ReturnsEveryone()
        {
            Assert.Equal(3, _scoreboard.Top((a, b) => b.Health.CompareTo(a.Health), 99).Count);
        }

        [Fact]
        public void TheLinqAndHandWrittenVersions_ProduceIdenticalResults()
        {
            List<A.Player> handWritten = _scoreboard.Top((a, b) => b.Health.CompareTo(a.Health), 2);
            List<A.Player> withLinq = _scoreboard.TopWithLinq(p => p.Health, 2);

            Assert.Equal(handWritten.Count, withLinq.Count);

            for (int i = 0; i < handWritten.Count; i++)
            {
                Assert.Same(handWritten[i], withLinq[i]);
            }
        }

        [Fact]
        public void Best_FindsTheSinglePlayerMatchingARule()
        {
            A.Player fastest = _scoreboard.Best((a, b) => a.MoveSpeed > b.MoveSpeed);

            Assert.Equal("Ana", fastest.Name);
        }

        [Fact]
        public void Best_OnAnEmptyScoreboard_ReturnsNull()
        {
            C.Scoreboard empty = new C.Scoreboard(new List<A.Player>());

            Assert.Null(empty.Best((a, b) => a.Health > b.Health));
        }

        [Fact]
        public void AFourthQuestion_NeedsNoChangeToTheScoreboard()
        {
            // A question nobody anticipated, asked without editing Scoreboard.
            List<A.Player> slowAndActive = _scoreboard.Where(
                C.Scoreboard.And(p => p.MoveSpeed < 6f, p => p.IsActive));

            Assert.Single(slowAndActive);
        }
    }
}
