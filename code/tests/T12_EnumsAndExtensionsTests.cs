// Tests for the note 12 B and C solutions.
//
// The extension method is reached through a real using directive, not a namespace
// alias - an alias does not bring extension methods into scope.

using System.Collections.Generic;
using Xunit;
using Solutions.T12.B;
using B = Solutions.T12.B;
using C = Solutions.T12.C;

namespace Week01.Tests
{
    public class T12_LayerFilter_B_Tests
    {
        private readonly List<B.SceneObject> _objects;

        public T12_LayerFilter_B_Tests()
        {
            _objects = new List<B.SceneObject>
            {
                new B.SceneObject { Name = "player", Layer = B.Layers.Player },
                new B.SceneObject { Name = "goblin", Layer = B.Layers.Enemy },
                new B.SceneObject { Name = "wall", Layer = B.Layers.Scenery },
                new B.SceneObject { Name = "zone", Layer = B.Layers.Trigger }
            };
        }

        [Fact]
        public void Filter_WithPlayerAndEnemy_ReturnsOnlyThose()
        {
            List<B.SceneObject> found =
                B.SceneFilter.Filter(_objects, B.Layers.Player | B.Layers.Enemy);

            Assert.Equal(2, found.Count);
        }

        [Fact]
        public void Filter_WithNone_ReturnsAnEmptyList()
        {
            Assert.Empty(B.SceneFilter.Filter(_objects, B.Layers.None));
        }

        [Fact]
        public void Everything_ReturnsAMaskContainingEveryRealLayer()
        {
            Assert.Equal(_objects.Count, B.SceneFilter.Filter(_objects, B.SceneFilter.Everything()).Count);
        }

        [Fact]
        public void Except_RemovesTheNamedLayersFromAMask()
        {
            B.Layers mask = B.SceneFilter.Except(B.SceneFilter.Everything(), B.Layers.Scenery);

            List<B.SceneObject> found = B.SceneFilter.Filter(_objects, mask);

            Assert.Equal(3, found.Count);
            Assert.DoesNotContain(found, o => o.Name == "wall");
        }

        [Fact]
        public void TheExtensionMethod_GivesIdenticalResultsToTheStaticMethod()
        {
            B.Layers mask = B.Layers.Player | B.Layers.Enemy;

            List<B.SceneObject> viaStatic = B.SceneFilter.Filter(_objects, mask);
            List<B.SceneObject> viaExtension = _objects.OnLayers(mask);

            Assert.Equal(viaStatic.Count, viaExtension.Count);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, true)]
        [InlineData(3, true)]        // Player | Enemy, a legitimate combination
        [InlineData(31, true)]       // every real layer
        [InlineData(32, false)]      // a bit we do not recognise
        [InlineData(64, false)]
        public void TryParseLayer_AcceptsKnownBitsAndRejectsUnknownOnes(int code, bool expected)
        {
            B.Layers layer;

            Assert.Equal(expected, B.SceneFilter.TryParseLayer(code, out layer));
        }

        [Fact]
        public void TryParseLayer_WithAnInvalidCode_AssignsNoneAndThrowsNothing()
        {
            B.Layers layer;

            bool ok = B.SceneFilter.TryParseLayer(64, out layer);

            Assert.False(ok);
            Assert.Equal(B.Layers.None, layer);
        }

        [Theory]
        [InlineData("Easy", true, B.Difficulty.Easy)]
        [InlineData("HARD", true, B.Difficulty.Hard)]
        [InlineData("normal", true, B.Difficulty.Normal)]
        [InlineData("Brutal", false, B.Difficulty.Normal)]
        [InlineData("", false, B.Difficulty.Normal)]
        public void TryParseDifficulty_IsCaseInsensitiveAndRejectsUnknownText(
            string text, bool expectedOk, B.Difficulty expectedValue)
        {
            B.Difficulty difficulty;

            bool ok = B.SceneFilter.TryParseDifficulty(text, out difficulty);

            Assert.Equal(expectedOk, ok);
            Assert.Equal(expectedValue, difficulty);
        }
    }

    public class T12_SceneQuery_C_Tests
    {
        private readonly C.Scene _scene = new C.Scene();
        private readonly C.SceneObject _self;

        public T12_SceneQuery_C_Tests()
        {
            _self = new C.SceneObject
            {
                Name = "self", Layer = B.Layers.Enemy, Position = new C.Point(0f, 0f)
            };

            _scene.Objects.Add(_self);
            _scene.Objects.Add(new C.SceneObject
            {
                Name = "near-enemy", Layer = B.Layers.Enemy, Position = new C.Point(3f, 0f)
            });
            _scene.Objects.Add(new C.SceneObject
            {
                Name = "far-enemy", Layer = B.Layers.Enemy, Position = new C.Point(50f, 0f)
            });
            _scene.Objects.Add(new C.SceneObject
            {
                Name = "wall", Layer = B.Layers.Scenery, Position = new C.Point(1f, 0f)
            });
        }

        [Fact]
        public void Query_ByLayer_ReturnsEverythingOnThatLayer()
        {
            Assert.Equal(3, _scene.Query(B.Layers.Enemy).All().Count);
        }

        [Fact]
        public void Query_WithinARadius_ExcludesAnythingFurtherAway()
        {
            List<C.SceneObject> near =
                _scene.Query(B.Layers.Enemy).Within(new C.Point(0f, 0f), 10f).All();

            Assert.Equal(2, near.Count);
            Assert.DoesNotContain(near, o => o.Name == "far-enemy");
        }

        [Fact]
        public void Query_Nearest_ReturnsTheClosestMatch()
        {
            C.SceneObject nearest = _scene.Query(B.Layers.Enemy)
                                          .Excluding(_self)
                                          .Nearest(new C.Point(0f, 0f));

            Assert.Equal("near-enemy", nearest.Name);
        }

        [Fact]
        public void Query_Excluding_LeavesOutTheNamedObject()
        {
            List<C.SceneObject> others = _scene.Query(B.Layers.Enemy).Excluding(_self).All();

            Assert.Equal(2, others.Count);
            Assert.DoesNotContain(others, o => ReferenceEquals(o, _self));
        }

        [Fact]
        public void Query_CriteriaChainTogether()
        {
            List<C.SceneObject> result = _scene.Query(B.Layers.Enemy)
                                               .Within(new C.Point(0f, 0f), 10f)
                                               .Excluding(_self)
                                               .All();

            Assert.Single(result);
            Assert.Equal("near-enemy", result[0].Name);
        }

        [Fact]
        public void Query_WithNoMatches_ReturnsAnEmptyListRatherThanNull()
        {
            List<C.SceneObject> none = _scene.Query(B.Layers.Projectile).All();

            Assert.NotNull(none);
            Assert.Empty(none);
        }

        [Fact]
        public void Nearest_WithNoMatches_ReturnsNull()
        {
            Assert.Null(_scene.Query(B.Layers.Projectile).Nearest(new C.Point(0f, 0f)));
        }

        [Fact]
        public void TheFrameSafeQuery_AgreesWithTheBuilderVersion()
        {
            C.SceneObject viaBuilder = _scene.Query(B.Layers.Scenery).Nearest(new C.Point(0f, 0f));
            C.SceneObject viaFrameSafe =
                _scene.NearestOnLayerNonAllocating(B.Layers.Scenery, new C.Point(0f, 0f));

            Assert.Same(viaBuilder, viaFrameSafe);
        }

        [Fact]
        public void TheFrameSafeQuery_WithNoMatches_ReturnsNull()
        {
            Assert.Null(_scene.NearestOnLayerNonAllocating(B.Layers.Projectile, new C.Point(0f, 0f)));
        }
    }
}
