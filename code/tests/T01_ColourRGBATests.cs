// Tests for the note 01 B and C solutions.
//
// Every floating-point assertion uses the precision overload, because exact
// equality on a float is a coin toss.

using System;
using Xunit;
using B = Solutions.T01.B;
using C = Solutions.T01.C;

namespace Week01.Tests
{
    public class T01_ColourRGBA_B_Tests
    {
        private const int Precision = 4;

        [Theory]
        [InlineData(0f, 0f)]
        [InlineData(0.5f, 0.5f)]
        [InlineData(1f, 1f)]
        [InlineData(5f, 1f)]
        [InlineData(-3f, 0f)]
        public void SettingAComponent_ClampsItToTheRangeZeroToOne(float assigned, float expected)
        {
            B.ColourRGBA colour = new B.ColourRGBA();

            colour.R = assigned;

            Assert.Equal(expected, colour.R, Precision);
        }

        [Fact]
        public void ParameterlessConstructor_GivesOpaqueWhite()
        {
            B.ColourRGBA colour = new B.ColourRGBA();

            Assert.Equal(1f, colour.R, Precision);
            Assert.Equal(1f, colour.G, Precision);
            Assert.Equal(1f, colour.B, Precision);
            Assert.Equal(1f, colour.A, Precision);
        }

        [Fact]
        public void ToString_ReturnsTheRgbaForm()
        {
            B.ColourRGBA colour = new B.ColourRGBA(1f, 0f, 0f, 1f);

            Assert.Equal("RGBA(1, 0, 0, 1)", colour.ToString());
        }

        [Fact]
        public void NamedColour_Red_IsFullyRedAndOpaque()
        {
            B.ColourRGBA red = B.ColourRGBA.Red;

            Assert.Equal(1f, red.R, Precision);
            Assert.Equal(0f, red.G, Precision);
            Assert.Equal(0f, red.B, Precision);
            Assert.Equal(1f, red.A, Precision);
        }

        [Fact]
        public void NamedColour_ReturnsADistinctInstanceEachTime_SoMutatingOneCannotAffectTheOther()
        {
            B.ColourRGBA first = B.ColourRGBA.Red;
            B.ColourRGBA second = B.ColourRGBA.Red;

            Assert.NotSame(first, second);

            first.G = 1f;

            Assert.Equal(0f, second.G, Precision);
        }

        [Fact]
        public void Brightness_IsComputedFromTheWeightedComponents()
        {
            B.ColourRGBA white = new B.ColourRGBA(1f, 1f, 1f, 1f);

            Assert.Equal(1f, white.Brightness, 3);
        }

        [Fact]
        public void ToGreyscale_ReturnsAColourWithEqualComponents()
        {
            B.ColourRGBA colour = new B.ColourRGBA(1f, 0.5f, 0f, 1f);

            B.ColourRGBA grey = colour.ToGreyscale();

            Assert.Equal(grey.R, grey.G, Precision);
            Assert.Equal(grey.G, grey.B, Precision);
        }

        [Fact]
        public void ToGreyscale_KeepsTheOriginalAlpha()
        {
            B.ColourRGBA colour = new B.ColourRGBA(1f, 0.5f, 0f, 0.25f);

            B.ColourRGBA grey = colour.ToGreyscale();

            Assert.Equal(0.25f, grey.A, Precision);
        }

        [Fact]
        public void ToGreyscale_LeavesTheOriginalUnchanged()
        {
            B.ColourRGBA colour = new B.ColourRGBA(1f, 0.5f, 0f, 1f);

            B.ColourRGBA grey = colour.ToGreyscale();

            Assert.NotSame(colour, grey);
            Assert.Equal(1f, colour.R, Precision);
            Assert.Equal(0.5f, colour.G, Precision);
            Assert.Equal(0f, colour.B, Precision);
        }

        [Fact]
        public void Lerp_AtZero_ReturnsTheFirstColourExactly()
        {
            B.ColourRGBA result = B.ColourRGBA.Lerp(B.ColourRGBA.Black, B.ColourRGBA.White, 0f);

            Assert.Equal(0f, result.R, Precision);
        }

        [Fact]
        public void Lerp_AtOne_ReturnsTheSecondColourExactly()
        {
            B.ColourRGBA result = B.ColourRGBA.Lerp(B.ColourRGBA.Black, B.ColourRGBA.White, 1f);

            Assert.Equal(1f, result.R, Precision);
        }

        [Fact]
        public void Lerp_AtAHalf_ReturnsTheMidpoint()
        {
            B.ColourRGBA result = B.ColourRGBA.Lerp(B.ColourRGBA.Black, B.ColourRGBA.White, 0.5f);

            Assert.Equal(0.5f, result.R, Precision);
        }

        [Theory]
        [InlineData(-1f, 0f)]
        [InlineData(2f, 1f)]
        public void Lerp_ClampsTheBlendPosition(float t, float expected)
        {
            B.ColourRGBA result = B.ColourRGBA.Lerp(B.ColourRGBA.Black, B.ColourRGBA.White, t);

            Assert.Equal(expected, result.R, Precision);
        }

        [Fact]
        public void Count_RisesAsColoursAreConstructed()
        {
            int before = B.ColourRGBA.Count;

            B.ColourRGBA unused = new B.ColourRGBA(0f, 0f, 0f, 1f);

            Assert.Equal(before + 1, B.ColourRGBA.Count);
            Assert.NotNull(unused);
        }
    }

    public class T01_ColourRGBA2_C_Tests
    {
        private const int Precision = 4;

        [Fact]
        public void Constructor_ClampsEveryComponent()
        {
            C.ColourRGBA2 colour = new C.ColourRGBA2(5f, -3f, 0.5f, 99f);

            Assert.Equal(1f, colour.R, Precision);
            Assert.Equal(0f, colour.G, Precision);
            Assert.Equal(0.5f, colour.B, Precision);
            Assert.Equal(1f, colour.A, Precision);
        }

        [Fact]
        public void NamedColour_ReturnsTheSameSharedInstance_WhichIsSafeBecauseTheTypeIsImmutable()
        {
            Assert.Same(C.ColourRGBA2.Red, C.ColourRGBA2.Red);
        }

        [Fact]
        public void ToGreyscale_ReturnsANewColourAndLeavesTheOriginalUnchanged()
        {
            C.ColourRGBA2 colour = new C.ColourRGBA2(1f, 0.5f, 0f, 1f);

            C.ColourRGBA2 grey = colour.ToGreyscale();

            Assert.NotSame(colour, grey);
            Assert.Equal(1f, colour.R, Precision);
            Assert.Equal(grey.R, grey.B, Precision);
        }

        [Fact]
        public void WithAlpha_ReturnsACopyDifferingOnlyInAlpha()
        {
            C.ColourRGBA2 colour = new C.ColourRGBA2(1f, 0.5f, 0f, 1f);

            C.ColourRGBA2 faded = colour.WithAlpha(0.2f);

            Assert.Equal(0.2f, faded.A, Precision);
            Assert.Equal(colour.R, faded.R, Precision);
            Assert.Equal(1f, colour.A, Precision);
        }

        [Fact]
        public void Lerp_AtTheEndpoints_ReturnsThoseColoursExactly()
        {
            C.ColourRGBA2 atZero = C.ColourRGBA2.Lerp(C.ColourRGBA2.Black, C.ColourRGBA2.White, 0f);
            C.ColourRGBA2 atOne = C.ColourRGBA2.Lerp(C.ColourRGBA2.Black, C.ColourRGBA2.White, 1f);

            Assert.Equal(0f, atZero.R, Precision);
            Assert.Equal(1f, atOne.R, Precision);
        }
    }
}
