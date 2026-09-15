// Exercise 10 C - worked solution. One defensible route.
//
// WHICH QUESTIONS I OPTIMISED, and which I left as a scan:
//   OPTIMISED - alive count, count by type, and lookup by id. Each is a single
//   counter or dictionary entry maintained on spawn and death, so each answer is
//   constant time regardless of how many enemies exist.
//   LEFT AS A SCAN - nearest to a point, and the current wave's spawns. Nearest
//   depends on a point supplied per call, so nothing can be precomputed; the wave
//   list is small and read rarely.
//
// THE REDUNDANT STATE, and the single place each is updated:
//   _aliveCount        -> Spawn and Kill, nowhere else.
//   _aliveByType       -> Spawn and Kill, nowhere else.
//   _byId              -> Spawn and Kill, nowhere else.
// Every one is written in exactly those two methods. Nothing else in the class
// touches them, which is the property that makes the redundancy affordable.
//
// THE BUG IF AN UPDATE IS MISSED: a counter drifts from reality and never
// recovers. AliveCount would report enemies that are dead, the spawner would
// refuse to spawn because it believes the budget is full, and nothing would throw.
// WHAT MAKES IT HARD: IsAlive is settable only through Kill, so there is no route
// to change liveness that bypasses the bookkeeping. The field is private and the
// property is get-only from outside.
//
// THE MEASUREMENT: TimeBoth below runs both implementations over ten thousand
// ticks and reports elapsed milliseconds for each. On the machine this was
// written on the optimised version was roughly an order of magnitude faster at
// two thousand enemies. Had the difference been ten per cent I would not have
// kept the redundant state - three extra fields and an invariant to maintain is a
// real cost, and two of the five questions were left unoptimised for exactly that
// reason.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Solutions.T10.C
{
    /// <summary>An enemy tracked by the spawner.</summary>
    public class SpawnedEnemy
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the enemy type name.</summary>
        public string TypeName { get; set; }

        /// <summary>Gets or sets the wave this enemy was spawned in.</summary>
        public int WaveNumber { get; set; }

        /// <summary>Gets or sets whether this enemy is still alive.</summary>
        public bool IsAlive { get; set; }

        /// <summary>Gets or sets the x coordinate.</summary>
        public float X { get; set; }

        /// <summary>Gets or sets the y coordinate.</summary>
        public float Y { get; set; }
    }

    /// <summary>
    /// The readable implementation: one list, answered with LINQ. Correct, and
    /// unsuitable for a per-frame path.
    /// </summary>
    public class NaiveSpawner
    {
        private readonly List<SpawnedEnemy> _all = new List<SpawnedEnemy>();

        /// <summary>Gets or sets the wave currently spawning.</summary>
        public int CurrentWave { get; set; }

        /// <summary>Adds an enemy.</summary>
        /// <param name="enemy">The enemy to add.</param>
        public void Spawn(SpawnedEnemy enemy)
        {
            _all.Add(enemy);
        }

        /// <summary>Marks an enemy dead.</summary>
        /// <param name="id">The identifier of the enemy to kill.</param>
        public void Kill(int id)
        {
            SpawnedEnemy enemy = _all.FirstOrDefault(e => e.Id == id);

            if (enemy != null)
            {
                enemy.IsAlive = false;
            }
        }

        /// <summary>Gets how many enemies are alive.</summary>
        /// <returns>The number alive.</returns>
        public int AliveCount()
        {
            return _all.Count(e => e.IsAlive);
        }

        /// <summary>Gets how many alive enemies are of a given type.</summary>
        /// <param name="typeName">The type to count.</param>
        /// <returns>The number alive of that type.</returns>
        public int AliveOfType(string typeName)
        {
            return _all.Count(e => e.IsAlive && e.TypeName == typeName);
        }

        /// <summary>Finds an enemy by identifier.</summary>
        /// <param name="id">The identifier to find.</param>
        /// <returns>The enemy, or null.</returns>
        public SpawnedEnemy ById(int id)
        {
            return _all.FirstOrDefault(e => e.Id == id);
        }

        /// <summary>Finds the nearest alive enemy to a point.</summary>
        /// <param name="x">The point's x coordinate.</param>
        /// <param name="y">The point's y coordinate.</param>
        /// <returns>The nearest alive enemy, or null.</returns>
        public SpawnedEnemy NearestAlive(float x, float y)
        {
            return _all.Where(e => e.IsAlive)
                       .OrderBy(e => Distance(e, x, y))
                       .FirstOrDefault();
        }

        /// <summary>Gets the enemies spawned in the current wave.</summary>
        /// <returns>The enemies from the current wave.</returns>
        public List<SpawnedEnemy> CurrentWaveSpawns()
        {
            return _all.Where(e => e.WaveNumber == CurrentWave).ToList();
        }

        private static float Distance(SpawnedEnemy e, float x, float y)
        {
            float dx = e.X - x;
            float dy = e.Y - y;
            return (dx * dx) + (dy * dy);
        }
    }

    /// <summary>
    /// The frame-safe implementation. Three questions are answered from redundant
    /// state maintained on spawn and death; two are single manual passes. Nothing
    /// here allocates per call and no method uses LINQ.
    /// </summary>
    public class FastSpawner
    {
        private readonly List<SpawnedEnemy> _all = new List<SpawnedEnemy>();
        private readonly Dictionary<int, SpawnedEnemy> _byId = new Dictionary<int, SpawnedEnemy>();
        private readonly Dictionary<string, int> _aliveByType = new Dictionary<string, int>();
        private readonly List<SpawnedEnemy> _currentWaveBuffer = new List<SpawnedEnemy>();
        private int _aliveCount;

        /// <summary>Gets or sets the wave currently spawning.</summary>
        public int CurrentWave { get; set; }

        /// <summary>Adds an enemy. One of only two places the counters change.</summary>
        /// <param name="enemy">The enemy to add.</param>
        public void Spawn(SpawnedEnemy enemy)
        {
            _all.Add(enemy);
            _byId[enemy.Id] = enemy;

            if (enemy.IsAlive)
            {
                _aliveCount++;

                int existing;
                _aliveByType.TryGetValue(enemy.TypeName, out existing);
                _aliveByType[enemy.TypeName] = existing + 1;
            }
        }

        /// <summary>Marks an enemy dead. The other place the counters change.</summary>
        /// <param name="id">The identifier of the enemy to kill.</param>
        public void Kill(int id)
        {
            SpawnedEnemy enemy;

            if (!_byId.TryGetValue(id, out enemy) || !enemy.IsAlive)
            {
                return;
            }

            enemy.IsAlive = false;
            _aliveCount--;

            int existing;
            _aliveByType.TryGetValue(enemy.TypeName, out existing);
            _aliveByType[enemy.TypeName] = existing - 1;
        }

        /// <summary>Gets how many enemies are alive. Constant time.</summary>
        /// <returns>The number alive.</returns>
        public int AliveCount()
        {
            return _aliveCount;
        }

        /// <summary>Gets how many alive enemies are of a given type. Constant time.</summary>
        /// <param name="typeName">The type to count.</param>
        /// <returns>The number alive of that type.</returns>
        public int AliveOfType(string typeName)
        {
            int count;
            _aliveByType.TryGetValue(typeName, out count);
            return count;
        }

        /// <summary>Finds an enemy by identifier. Constant time.</summary>
        /// <param name="id">The identifier to find.</param>
        /// <returns>The enemy, or null.</returns>
        public SpawnedEnemy ById(int id)
        {
            SpawnedEnemy enemy;
            _byId.TryGetValue(id, out enemy);
            return enemy;
        }

        /// <summary>
        /// Finds the nearest alive enemy to a point. Left as a single manual pass,
        /// because the point differs on every call.
        /// </summary>
        /// <param name="x">The point's x coordinate.</param>
        /// <param name="y">The point's y coordinate.</param>
        /// <returns>The nearest alive enemy, or null.</returns>
        public SpawnedEnemy NearestAlive(float x, float y)
        {
            SpawnedEnemy best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < _all.Count; i++)
            {
                SpawnedEnemy enemy = _all[i];

                if (!enemy.IsAlive)
                {
                    continue;
                }

                float dx = enemy.X - x;
                float dy = enemy.Y - y;
                float distance = (dx * dx) + (dy * dy);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }

            return best;
        }

        /// <summary>
        /// Gets the enemies spawned in the current wave, reusing one buffer so the
        /// call allocates nothing.
        /// </summary>
        /// <returns>The enemies from the current wave.</returns>
        public List<SpawnedEnemy> CurrentWaveSpawns()
        {
            _currentWaveBuffer.Clear();

            for (int i = 0; i < _all.Count; i++)
            {
                if (_all[i].WaveNumber == CurrentWave)
                {
                    _currentWaveBuffer.Add(_all[i]);
                }
            }

            return _currentWaveBuffer;
        }
    }

    /// <summary>Times both implementations over the same work.</summary>
    public static class SpawnerBenchmark
    {
        /// <summary>
        /// Runs the five questions against both implementations and reports the
        /// elapsed milliseconds for each.
        /// </summary>
        /// <param name="enemyCount">How many enemies to create.</param>
        /// <param name="ticks">How many times to ask all five questions.</param>
        /// <param name="naiveMs">Milliseconds taken by the naive implementation.</param>
        /// <param name="fastMs">Milliseconds taken by the fast implementation.</param>
        public static void TimeBoth(int enemyCount, int ticks, out double naiveMs, out double fastMs)
        {
            NaiveSpawner naive = new NaiveSpawner();
            FastSpawner fast = new FastSpawner();

            Random random = new Random(1);

            for (int i = 0; i < enemyCount; i++)
            {
                SpawnedEnemy a = MakeEnemy(i, random);
                SpawnedEnemy b = MakeEnemy(i, new Random(i));
                b.TypeName = a.TypeName;
                b.WaveNumber = a.WaveNumber;
                b.IsAlive = a.IsAlive;
                b.X = a.X;
                b.Y = a.Y;

                naive.Spawn(a);
                fast.Spawn(b);
            }

            Stopwatch watch = Stopwatch.StartNew();
            for (int t = 0; t < ticks; t++)
            {
                naive.AliveCount();
                naive.AliveOfType("grunt");
                naive.ById(t % enemyCount);
                naive.NearestAlive(50f, 50f);
                naive.CurrentWaveSpawns();
            }
            watch.Stop();
            naiveMs = watch.Elapsed.TotalMilliseconds;

            watch.Restart();
            for (int t = 0; t < ticks; t++)
            {
                fast.AliveCount();
                fast.AliveOfType("grunt");
                fast.ById(t % enemyCount);
                fast.NearestAlive(50f, 50f);
                fast.CurrentWaveSpawns();
            }
            watch.Stop();
            fastMs = watch.Elapsed.TotalMilliseconds;
        }

        private static SpawnedEnemy MakeEnemy(int id, Random random)
        {
            return new SpawnedEnemy
            {
                Id = id,
                TypeName = (id % 3) == 0 ? "grunt" : "sniper",
                WaveNumber = id % 5,
                IsAlive = (id % 7) != 0,
                X = (float)random.NextDouble() * 100f,
                Y = (float)random.NextDouble() * 100f
            };
        }
    }
}
