// Tests for the note 02 B and C solutions.
//
// The C tests deliberately demonstrate the two versions DISAGREEING, because that
// disagreement is the finding the exercise asks for.

using System.Collections.Generic;
using Xunit;
// A namespace ALIAS does not bring extension methods into scope - only a real
// using directive does. This is the note 12 mistake, met in practice.
using Solutions.T02.B;
using B = Solutions.T02.B;
using C = Solutions.T02.C;

namespace Week01.Tests
{
    public class T02_Mover_B_Tests
    {
        private const int Precision = 4;

        private readonly B.Transform _transform;

        public T02_Mover_B_Tests()
        {
            _transform = new B.Transform();
            _transform.Position = new B.Vector3(1f, 2f, 3f);
        }

        [Fact]
        public void SetHeight_ChangesOnlyTheYComponent()
        {
            B.Mover.SetHeight(_transform, 9f);

            Assert.Equal(1f, _transform.Position.X, Precision);
            Assert.Equal(9f, _transform.Position.Y, Precision);
            Assert.Equal(3f, _transform.Position.Z, Precision);
        }

        [Fact]
        public void SetHeight_ActuallyInvokesTheSetter_ProvingTheValueWasWrittenBack()
        {
            int before = _transform.MoveCount;

            B.Mover.SetHeight(_transform, 9f);

            Assert.Equal(before + 1, _transform.MoveCount);
        }

        [Fact]
        public void Nudge_AddsTheOffsetToEveryComponent()
        {
            B.Mover.Nudge(_transform, new B.Vector3(1f, 1f, 1f));

            Assert.Equal(2f, _transform.Position.X, Precision);
            Assert.Equal(3f, _transform.Position.Y, Precision);
            Assert.Equal(4f, _transform.Position.Z, Precision);
        }

        [Fact]
        public void Flatten_SetsYToZeroAndLeavesXAndZ()
        {
            B.Mover.Flatten(_transform);

            Assert.Equal(1f, _transform.Position.X, Precision);
            Assert.Equal(0f, _transform.Position.Y, Precision);
            Assert.Equal(3f, _transform.Position.Z, Precision);
        }

        [Fact]
        public void ExtensionMethods_ProduceTheSameResultAsTheStaticMethods()
        {
            B.Transform viaExtension = new B.Transform();
            viaExtension.Position = new B.Vector3(1f, 2f, 3f);

            B.Mover.SetHeight(_transform, 7f);
            viaExtension.SetHeight(7f);

            Assert.Equal(_transform.Position.Y, viaExtension.Position.Y, Precision);
        }

        [Fact]
        public void RaiseAll_OnAList_SetsYOnEveryElement()
        {
            List<B.Vector3> points = new List<B.Vector3>
            {
                new B.Vector3(0f, 0f, 0f),
                new B.Vector3(1f, 1f, 1f)
            };

            B.Mover.RaiseAll(points, 5f);

            Assert.Equal(5f, points[0].Y, Precision);
            Assert.Equal(5f, points[1].Y, Precision);
        }

        [Fact]
        public void RaiseAll_OnAnArray_SetsYOnEveryElement()
        {
            B.Vector3[] points =
            {
                new B.Vector3(0f, 0f, 0f),
                new B.Vector3(1f, 1f, 1f)
            };

            B.Mover.RaiseAll(points, 5f);

            Assert.Equal(5f, points[0].Y, Precision);
            Assert.Equal(5f, points[1].Y, Precision);
        }
    }

    public class T02_ScreenRect_C_Tests
    {
        [Fact]
        public void Struct_WidthAndHeight_AreComputedFromTheEdges()
        {
            C.ScreenRectStruct rect = new C.ScreenRectStruct(0, 0, 1280, 720);

            Assert.Equal(1280, rect.Width);
            Assert.Equal(720, rect.Height);
        }

        [Theory]
        [InlineData(64, 64, true)]
        [InlineData(0, 0, true)]
        [InlineData(1280, 720, true)]
        [InlineData(-1, 64, false)]
        [InlineData(64, 721, false)]
        public void Struct_Contains_ReportsWhetherThePointIsInside(int x, int y, bool expected)
        {
            C.ScreenRectStruct rect = new C.ScreenRectStruct(0, 0, 1280, 720);

            Assert.Equal(expected, rect.Contains(x, y));
        }

        [Fact]
        public void Struct_Inflate_ReturnsANewRegionAndLeavesTheOriginalUnchanged()
        {
            C.ScreenRectStruct original = new C.ScreenRectStruct(10, 10, 20, 20);

            C.ScreenRectStruct bigger = original.Inflate(5);

            Assert.Equal(10, original.Width);
            Assert.Equal(20, bigger.Width);
        }

        [Fact]
        public void Struct_Translate_ReturnsANewRegionAndLeavesTheOriginalUnchanged()
        {
            C.ScreenRectStruct original = new C.ScreenRectStruct(0, 0, 10, 10);

            C.ScreenRectStruct moved = original.Translate(5, 5);

            Assert.Equal(0, original.Left);
            Assert.Equal(5, moved.Left);
        }

        [Fact]
        public void Struct_HandingTheSameRegionToTwoHolders_GivesEachAnIndependentCopy()
        {
            C.ScreenRectStruct original = new C.ScreenRectStruct(0, 0, 100, 100);

            C.ScreenRectStruct holderA = original;
            C.ScreenRectStruct holderB = original;

            holderA = holderA.Inflate(10);

            Assert.Equal(120, holderA.Width);
            Assert.Equal(100, holderB.Width);
        }

        [Fact]
        public void Class_HandingTheSameRegionToTwoHolders_LetsOneChangeWhatTheOtherSees()
        {
            C.ScreenRectClass original = new C.ScreenRectClass(0, 0, 100, 100);

            C.ScreenRectClass holderA = original;
            C.ScreenRectClass holderB = original;

            holderA.MutateInflate(10);

            Assert.Equal(120, holderA.Width);
            Assert.Equal(120, holderB.Width);    // the disagreement the exercise is about
        }
    }
}
