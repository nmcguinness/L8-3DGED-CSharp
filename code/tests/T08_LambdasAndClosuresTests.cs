// Tests for the note 08 B and C solutions.

using System;
using System.Collections.Generic;
using Xunit;
using B = Solutions.T08.B;
using C = Solutions.T08.C;

namespace Week01.Tests
{
    public class T08_Menu_B_Tests
    {
        private readonly List<B.Button> _buttons;
        private readonly B.Menu _menu;

        public T08_Menu_B_Tests()
        {
            _buttons = new List<B.Button> { new B.Button(), new B.Button(), new B.Button() };
            _menu = new B.Menu(_buttons);
        }

        [Fact]
        public void EachButton_SelectsItsOwnSlot()
        {
            _menu.Open();

            foreach (B.Button button in _buttons)
            {
                button.Press();
            }

            Assert.Equal(new List<int> { 0, 1, 2 }, _menu.Selections);
        }

        [Fact]
        public void AfterClosing_PressingAButtonSelectsNothing()
        {
            _menu.Open();
            _menu.Close();

            foreach (B.Button button in _buttons)
            {
                button.Press();
            }

            Assert.Empty(_menu.Selections);
        }

        [Fact]
        public void Closing_RemovesEverySubscription()
        {
            _menu.Open();
            _menu.Close();

            foreach (B.Button button in _buttons)
            {
                Assert.Equal(0, button.SubscriberCount);
            }
        }

        [Fact]
        public void OpeningTwice_SubscribesOnlyOnce()
        {
            _menu.Open();
            _menu.Open();

            _buttons[0].Press();

            Assert.Single(_menu.Selections);
            Assert.Equal(1, _buttons[0].SubscriberCount);
        }

        [Fact]
        public void ClosingWithoutOpening_ThrowsNothing()
        {
            _menu.Close();

            Assert.False(_menu.IsOpen);
        }

        [Fact]
        public void ReopeningAfterClosing_WorksAgain()
        {
            _menu.Open();
            _menu.Close();
            _menu.Open();

            _buttons[1].Press();

            Assert.Single(_menu.Selections);
            Assert.Equal(1, _menu.Selections[0]);
        }

        [Fact]
        public void RunningTheWholeSequenceTwice_ProducesIdenticalResults()
        {
            for (int run = 0; run < 2; run++)
            {
                B.Menu menu = new B.Menu(new List<B.Button> { new B.Button(), new B.Button() });
                menu.Open();
                menu.Close();
                menu.Open();

                Assert.True(menu.IsOpen);
                Assert.Empty(menu.Selections);
            }
        }
    }

    public class T08_DeferredActions_C_Tests
    {
        private readonly C.World _world = new C.World();
        private readonly C.InstructionQueue _queue = new C.InstructionQueue();

        [Fact]
        public void RecordingAnInstruction_DoesNotPerformIt()
        {
            C.Instructions.RecordMove(_queue, _world, "kitchen");

            Assert.Empty(_world.Log);
            Assert.Equal(1, _queue.PendingCount);
        }

        [Fact]
        public void RunningTheQueue_PerformsEverythingInTheOrderRecorded()
        {
            C.Instructions.RecordMove(_queue, _world, "kitchen");
            C.Instructions.RecordTake(_queue, _world, "knife");
            C.Instructions.RecordLook(_queue, _world);

            _queue.Run();

            Assert.Equal(3, _world.Log.Count);
            Assert.Equal("moved to kitchen", _world.Log[0]);
            Assert.Equal("took knife", _world.Log[1]);
            Assert.Equal("looked around kitchen", _world.Log[2]);
        }

        [Fact]
        public void RunningTheQueueASecondTime_DoesNothing()
        {
            C.Instructions.RecordWait(_queue, _world);
            _queue.Run();

            int performed = _queue.Run();

            Assert.Equal(0, performed);
            Assert.Single(_world.Log);
        }

        [Fact]
        public void Discarding_LeavesTheQueueEmptyAndPerformsNothing()
        {
            C.Instructions.RecordMove(_queue, _world, "cellar");

            _queue.Discard();

            Assert.Equal(0, _queue.PendingCount);
            Assert.Empty(_world.Log);
        }

        [Fact]
        public void FourKindsOfInstruction_CanBeQueuedTogether()
        {
            C.Instructions.RecordMove(_queue, _world, "hall");
            C.Instructions.RecordTake(_queue, _world, "lamp");
            C.Instructions.RecordLook(_queue, _world);
            C.Instructions.RecordWait(_queue, _world);

            Assert.Equal(4, _queue.Run());
        }

        [Fact]
        public void RecordedInstruction_CapturesTheValueAtRecordTime()
        {
            string room = "kitchen";
            C.Instructions.RecordMove(_queue, _world, room);

            room = "cellar";                    // changed after recording
            _queue.Run();

            Assert.Equal("kitchen", _world.Room);
        }

        [Fact]
        public void TwoInstructionsOfTheSameKind_KeepTheirOwnCapturedValues()
        {
            C.Instructions.RecordMove(_queue, _world, "north");
            C.Instructions.RecordMove(_queue, _world, "south");

            _queue.Run();

            Assert.Equal("moved to north", _world.Log[0]);
            Assert.Equal("moved to south", _world.Log[1]);
        }

        [Fact]
        public void PendingInstructions_CanBeDescribedBeforeTheyRun()
        {
            C.Instructions.RecordMove(_queue, _world, "attic");
            C.Instructions.RecordTake(_queue, _world, "box");

            List<string> described = _queue.Describe();

            Assert.Equal(2, described.Count);
            Assert.Equal("move to attic", described[0]);
            Assert.Equal("take box", described[1]);
        }
    }
}
