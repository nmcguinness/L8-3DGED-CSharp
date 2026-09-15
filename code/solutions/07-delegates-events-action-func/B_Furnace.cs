// Exercise 07 B - worked solution.
//
// DESIGN CHOICE: the furnace carries two events - one for every reading, one for
// the threshold crossing - so the alarm subscribes to the thing it actually cares
// about rather than filtering every reading itself.
//
// THE ALTERNATIVE - one event plus an `if` inside the alarm - keeps the furnace
// smaller and moves the crossing logic into every listener that wants it. The
// second listener to want it would duplicate that logic, and the two copies would
// eventually disagree about whether 800 exactly counts as crossing.
//
// ATTACH GUARDS AGAINST DOUBLE SUBSCRIPTION. Subscribing twice is almost always a
// bug rather than an intent, and the symptom - every handler running twice - is
// hard to trace back to the second += . Detach is safe to call when not attached,
// because -= on an absent delegate is a defined no-op.

using System;
using System.Collections.Generic;

namespace Solutions.T07.B
{
    /// <summary>A furnace that reports its temperature to whoever is listening.</summary>
    public class Furnace
    {
        /// <summary>The temperature above which the furnace is considered unsafe.</summary>
        public const int AlarmThreshold = 800;

        private int _temperature;
        private bool _isAboveThreshold;

        /// <summary>Raised on every temperature change, carrying the new reading.</summary>
        public event Action<int> TemperatureChanged;

        /// <summary>Raised only when the temperature crosses above the threshold.</summary>
        public event Action<int> OverheatStarted;

        /// <summary>Gets the current temperature.</summary>
        public int Temperature
        {
            get { return _temperature; }
        }

        /// <summary>Sets a new temperature and notifies subscribers.</summary>
        /// <param name="value">The new temperature.</param>
        public void SetTemperature(int value)
        {
            _temperature = value;
            TemperatureChanged?.Invoke(_temperature);

            bool nowAbove = _temperature > AlarmThreshold;

            if (nowAbove && !_isAboveThreshold)
            {
                OverheatStarted?.Invoke(_temperature);
            }

            _isAboveThreshold = nowAbove;
        }
    }

    /// <summary>Records every reading it is told about.</summary>
    public class Display
    {
        private Action<int> _handler;

        /// <summary>Gets the readings seen, oldest first.</summary>
        public List<int> Seen { get; } = new List<int>();

        /// <summary>Starts listening to a furnace.</summary>
        /// <param name="furnace">The furnace to observe.</param>
        public void Attach(Furnace furnace)
        {
            if (_handler != null)
            {
                return;                     // already attached; do not subscribe twice
            }

            _handler = value => Seen.Add(value);
            furnace.TemperatureChanged += _handler;
        }

        /// <summary>Stops listening to a furnace.</summary>
        /// <param name="furnace">The furnace to stop observing.</param>
        public void Detach(Furnace furnace)
        {
            if (_handler == null)
            {
                return;                     // never attached; nothing to do
            }

            furnace.TemperatureChanged -= _handler;
            _handler = null;
        }
    }

    /// <summary>Warns when a furnace first goes above its safe temperature.</summary>
    public class Alarm
    {
        private Action<int> _handler;

        /// <summary>Gets how many times the alarm has sounded.</summary>
        public int TimesSounded { get; private set; }

        /// <summary>Starts listening to a furnace.</summary>
        /// <param name="furnace">The furnace to observe.</param>
        public void Attach(Furnace furnace)
        {
            if (_handler != null)
            {
                return;
            }

            _handler = value => TimesSounded++;
            furnace.OverheatStarted += _handler;
        }

        /// <summary>Stops listening to a furnace.</summary>
        /// <param name="furnace">The furnace to stop observing.</param>
        public void Detach(Furnace furnace)
        {
            if (_handler == null)
            {
                return;
            }

            furnace.OverheatStarted -= _handler;
            _handler = null;
        }
    }

    /// <summary>Records every reading and reports the highest seen.</summary>
    public class Logger
    {
        private Action<int> _handler;
        private readonly List<int> _readings = new List<int>();

        /// <summary>Gets how many readings have been recorded.</summary>
        public int Count
        {
            get { return _readings.Count; }
        }

        /// <summary>Starts listening to a furnace.</summary>
        /// <param name="furnace">The furnace to observe.</param>
        public void Attach(Furnace furnace)
        {
            if (_handler != null)
            {
                return;
            }

            _handler = value => _readings.Add(value);
            furnace.TemperatureChanged += _handler;
        }

        /// <summary>Stops listening to a furnace.</summary>
        /// <param name="furnace">The furnace to stop observing.</param>
        public void Detach(Furnace furnace)
        {
            if (_handler == null)
            {
                return;
            }

            furnace.TemperatureChanged -= _handler;
            _handler = null;
        }

        /// <summary>Returns the highest reading recorded.</summary>
        /// <returns>The highest reading, or zero when none have been recorded.</returns>
        public int Highest()
        {
            int highest = 0;

            foreach (int reading in _readings)
            {
                if (reading > highest)
                {
                    highest = reading;
                }
            }

            return highest;
        }
    }
}
