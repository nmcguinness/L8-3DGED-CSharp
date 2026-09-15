// Exercise 13 C - worked solution. One defensible route.
//
// STRATEGY CHOSEN: remove on destruction, backed by a check at the point of use.
// REJECTED: check on the way in - validating once at the top of the tick and
// trusting the references for the rest of it.
//
// THE SEQUENCE THAT BREAKS THE REJECTED STRATEGY:
//   1. Tick begins. The agent validates its threat list; all three are alive.
//   2. The agent selects threat B as its current target.
//   3. Before the agent acts, a chain explosion resolves and destroys B.
//   4. The agent acts on B, which it validated at step 1 and trusts.
//   5. MissingReferenceException, from a line guarded by a check that was true
//      when it ran.
// The window between validation and use is the whole problem, and it exists
// whenever anything can be destroyed mid-tick.
//
// WHAT MY STRATEGY ASSUMES: that everything destroyed announces it, by calling
// Kill rather than being dropped silently. THE DAY THAT IS VIOLATED - something
// is destroyed by a route that does not raise the event - the agent holds a stale
// reference, and the point-of-use check is what stops it throwing. That is why I
// use both rather than either.
//
// USING TWO STRATEGIES IS NOT INDECISION: they cover different failures. Removal
// keeps the collection bounded and avoids checking a list full of corpses every
// frame; the point-of-use check survives the mid-tick window and any destruction
// that skips the event. Removal is the optimisation; the check is the correctness.

using System;
using System.Collections.Generic;

namespace Solutions.T13.C
{
    /// <summary>A target an agent can chase, which announces its own death.</summary>
    public class Target : T13.B.EngineObject
    {
        /// <summary>Raised immediately before this target is destroyed.</summary>
        public event Action<Target> Destroying;

        /// <summary>Destroys this target, announcing it first.</summary>
        public void Kill()
        {
            Destroying?.Invoke(this);
            Destroy();
        }

        /// <summary>
        /// Destroys this target WITHOUT announcing it, to model a destruction that
        /// bypasses the event. Used to show why the point-of-use check is kept.
        /// </summary>
        public void KillSilently()
        {
            Destroy();
        }
    }

    /// <summary>What the agent did during one tick.</summary>
    public enum AgentAction
    {
        /// <summary>Acted on a live target.</summary>
        ActedOnTarget,

        /// <summary>Chose a new target from the remembered threats.</summary>
        ChoseNewTarget,

        /// <summary>Nothing remained, so it returned home.</summary>
        ReturnedHome
    }

    /// <summary>
    /// An agent that chases targets which may be destroyed at any moment,
    /// including between selection and use.
    /// </summary>
    public class Agent : T13.B.EngineObject
    {
        private readonly List<Target> _threats = new List<Target>();
        private readonly Action<Target> _onThreatDestroying;
        private Target _currentTarget;

        /// <summary>Creates an agent with nothing to chase.</summary>
        public Agent()
        {
            // Stored in a field so the exact delegate can be unsubscribed later -
            // an inline lambda could not be removed.
            _onThreatDestroying = Forget;
        }

        /// <summary>Gets how many threats are currently remembered.</summary>
        public int ThreatCount
        {
            get { return _threats.Count; }
        }

        /// <summary>Gets the target the agent is currently acting on, if any.</summary>
        public Target CurrentTarget
        {
            get { return _currentTarget; }
        }

        /// <summary>Gets how many times the agent has acted on a target.</summary>
        public int ActionsTaken { get; private set; }

        /// <summary>Remembers a threat and listens for its destruction.</summary>
        /// <param name="threat">The threat to remember.</param>
        public void Remember(Target threat)
        {
            if (threat == null || _threats.Contains(threat))
            {
                return;
            }

            _threats.Add(threat);
            threat.Destroying += _onThreatDestroying;
        }

        /// <summary>
        /// Advances the agent by one tick. Never throws, whatever has been
        /// destroyed and whenever it was destroyed.
        /// </summary>
        /// <returns>What the agent did.</returns>
        public AgentAction Tick()
        {
            // Point-of-use check: survives a target destroyed after selection, and
            // a destruction that never raised the event.
            if (_currentTarget != null)
            {
                ActionsTaken++;
                return AgentAction.ActedOnTarget;
            }

            _currentTarget = null;

            for (int i = _threats.Count - 1; i >= 0; i--)
            {
                Target candidate = _threats[i];

                if (candidate == null)
                {
                    // Destroyed without announcing. Drop it here so the list stays
                    // bounded even when the event was missed.
                    Detach(candidate);
                    _threats.RemoveAt(i);
                    continue;
                }

                _currentTarget = candidate;
                return AgentAction.ChoseNewTarget;
            }

            return AgentAction.ReturnedHome;
        }

        /// <summary>Stops listening to every remembered threat.</summary>
        public void DetachAll()
        {
            foreach (Target threat in _threats)
            {
                Detach(threat);
            }

            _threats.Clear();
            _currentTarget = null;
        }

        // Called by the event, before the target is actually destroyed.
        private void Forget(Target threat)
        {
            Detach(threat);
            _threats.Remove(threat);

            if (ReferenceEquals(_currentTarget, threat))
            {
                _currentTarget = null;
            }
        }

        private void Detach(Target threat)
        {
            if (!ReferenceEquals(threat, null))
            {
                threat.Destroying -= _onThreatDestroying;
            }
        }
    }
}
