# Content Generation Brief: Week 1 — C# for Unity

This file is an instruction set for Claude. It is not student-facing. Read it
in full before generating anything.

## 1. Task

Generate a set of Markdown notes, exercises and a README for Week 1 of a Stage 3
games development module. The output is pushed to a GitHub repository and read by
students in a browser.

Generate the files listed in Section 9. Do not generate anything else.

## 2. Context

- Institution: DkIT. Programme: BSc (Hons) in Computing in Games Development.
- Module: 3D Game Engine Development, Stage 3, taught in Unity (C#).
- Week 1 of 11. Five contact hours, lab-based.
- Students have basic OOP in C#: classes, fields, methods, constructors,
  inheritance, simple collections. Assume nothing beyond that.
- Week 1 is pure C#. Unity is referenced only to justify why a topic matters.
  Students are not expected to open Unity during Week 1.

The module's later weeks depend on this material:

| Week | Content |
|---|---|
| 2 | ScriptableObjects |
| 3 | Controllers, Input System, Command |
| 4 | Observer via ScriptableObject event channels |
| 5 | State / hierarchical finite state machine |
| 6 | Blackboard and Strategy |
| 8 | Coding exam (scope: weeks 1-6), object pool with factory |
| 9 | Cinemachine, NavMesh |
| 10 | AI capstone: HFSM driving a NavMeshAgent |
| 11 | Lighting and optimisation |

## 3. Governing principle

Every topic must be justified by the later work it unblocks. A topic introduced
without that justification reads as revision and students disengage.

Each note opens by naming what it makes possible, in one or two sentences, with a
concrete forward reference to a named TOPIC and pattern. Not "interfaces are
important for good design" but "every state in the state machine implements a
common interface so the machine can hold them without knowing which is which".

**Never name a week number in a note, an exercise or a worked solution.** Week
numbers change between deliveries and the material goes stale silently. Refer to
the topic — "the state machine", "the object pool", "the optimisation topic" — or
to relative position, such as "later in the module". The week table in Section 2
and the Unblocks column in Section 4 are planning aids for the lecturer and stay
as they are; they must not leak into student-facing text.

## 4. Topics and their justification

Generate one note per topic, in this order. The "unblocks" column is the
justification each note must open with; expand it into prose, do not paste it.

| # | Topic | Unblocks |
|---|---|---|
| 01 | Interfaces | HFSM states (W5), strategies (W6), poolable objects (W8). Programming to the interface; implementing several; interface segregation. Cover explicit implementation briefly, since `IState` and `IPoolable` collide in practice. |
| 02 | Abstract classes vs interfaces | `abstract class StateBase : IState` in W5. When each is the right tool, and why the pair is common. |
| 03 | Delegates, events, Action and Func | Event channels (W4). Build in that order: a delegate is a method reference; `event` restricts who may invoke; `Action<T>` and `Func<T,TResult>` are the generic shorthand. Must cover null-conditional invocation and unsubscribing in `OnDisable`. |
| 04 | Lambdas and closures | Callback registration throughout. Syntax is trivial; closure semantics are not. Cover captured variables, the captured-loop-variable bug, and the fact that a lambda cannot be unsubscribed unless stored in a variable. |
| 05 | Generics | `IPool<T>` (W8), typed event channels (W4). Generic classes and methods; constraints `where T : class`, `new()`, `: Component`. |
| 06 | Collections and LINQ | `List<T>`, `Dictionary<TKey,TValue>`, `Queue<T>` for command history and pooling. LINQ for readability only, with an explicit allocation warning and the rule that it does not belong in `Update`. This warning is picked up again in W11. |
| 07 | Language essentials | Grouped short topics: extension methods; `enum` and `[Flags]` for layer masks; `struct` vs `class` and value semantics. The value-semantics section must explain why `transform.position.x = 5` does not compile, because students will hit it in W3. |
| 08 | Null handling in Unity | Nullable reference types and null handling, then the Unity-specific trap: `UnityEngine.Object` overloads `==` so a destroyed object reports as null, which makes `??` and `?.` behave unexpectedly. Flag this as a bug source that surfaces around W10 if not understood now. |

## 5. Note structure

Each note file follows the same shape:

1. `# Title`
2. **What this unblocks** — two to three sentences, forward reference to a named week.
3. **The idea** — the concept in plain terms before any syntax.
4. **In code** — progressive examples, simple to complex. Plain C# first; a Unity
   example only where the Unity behaviour differs from plain C#.
5. **Common mistakes** — two to four, each with the wrong code, the symptom the
   student will observe, and the fix. This section carries disproportionate value;
   do not skimp on it.
6. **Check yourself** — three to five short questions with answers in a collapsed
   `<details>` block.
7. **Further reading** — two or three links to official Microsoft or Unity
   documentation. No blog posts, no video links.

Target 400-800 words of prose per note, plus code. Notes 03, 04 and 05 may run
longer; note 07 will be longer because it groups several topics, and should use
clear subheadings per topic.

## 6. Exercises

Generate one exercise file per note, numbered to match. Each contains three
exercises at increasing difficulty:

- **A — mechanical.** Apply the syntax. Ten minutes.
- **B — applied.** Solve a small problem where the topic is the natural tool.
  Twenty to thirty minutes.
- **C — design.** An open task with more than one defensible answer, which
  requires a choice to be justified in a comment. Thirty minutes or more.

Rules for exercises:

- State the goal and the constraints. Do not provide the solution.
- Provide starter code only where setup would otherwise waste time; mark the
  sections to be completed with `// TODO:`.
- Exercise C must connect to a later week's pattern without naming the pattern.
  For example, note 01 exercise C might ask for a set of interchangeable
  behaviours behind a common interface, which is Strategy in all but name.
- Give each exercise a short "done when" list of observable criteria, so students
  can self-assess.
- No solutions inside the exercise files themselves. Worked solutions live in
  `code/solutions/`, described in Section 9.2, and are released to students.

## 7. README

The repository README must contain:

- One-paragraph statement of what this set of notes covers and why it precedes Unity.
- The prerequisite statement: what students are assumed to know already.
- A table of contents linking every note, exercise and solution folder.
- A short paragraph on how to use the solutions: attempt the exercise first, then
  compare approaches rather than checking for a match.
- A dependency diagram in Mermaid showing which topics feed which later TOPICS,
  drawn from the tables in Sections 2 and 4. Name the later topics, never the week
  numbers - node ids included, or the coupling survives a relabelling.
- A suggested order of work across the five contact hours.
- A short "if you are already confident" note pointing to the exercises so
  stronger students can self-assess and skip ahead.

## 8. Style

These are firm.

- **No emoji anywhere**, in any file, including headings and tables.
- **UK spelling** throughout.
- Direct and concise. No filler, no motivational padding, no "Let's dive in".
- Code follows the module house style: underscore-prefixed private fields, XML
  documentation comments on public members, `PascalCase` for types and methods,
  `camelCase` for locals and parameters.
- Diagrams in Mermaid only. No images, no ASCII art.
- Progression is always simple to complex. A concept is never used in an example
  before it has been introduced.
- Explanations do not solve later assignment problems. Show the mechanism, not a
  finished subsystem the student can lift.
- Every code block specifies its language for syntax highlighting.
- Prefer short paragraphs. These are read on screen, often on a laptop in a lab.

## 9. Output manifest

### 9.1 Committed to the repository

```
README.md
.gitignore
notes/01-interfaces.md
notes/02-abstract-vs-interface.md
notes/03-delegates-events-action-func.md
notes/04-lambdas-and-closures.md
notes/05-generics.md
notes/06-collections-and-linq.md
notes/07-language-essentials.md
notes/08-null-handling-in-unity.md
exercises/01-interfaces.md
exercises/02-abstract-vs-interface.md
exercises/03-delegates-events-action-func.md
exercises/04-lambdas-and-closures.md
exercises/05-generics.md
exercises/06-collections-and-linq.md
exercises/07-language-essentials.md
exercises/08-null-handling-in-unity.md
```

### 9.2 Code folder, also committed

```
code/solutions/01-interfaces/...
code/solutions/02-abstract-vs-interface/...
code/solutions/03-delegates-events-action-func/...
code/solutions/04-lambdas-and-closures/...
code/solutions/05-generics/...
code/solutions/06-collections-and-linq/...
code/solutions/07-language-essentials/...
code/solutions/08-null-handling-in-unity/...
code/tests/...
```

One solution folder per exercise file, containing a worked solution for each of
exercises A, B and C as separate `.cs` files. Solutions are reference
implementations in module house style, not minimal answers: they are what a
distinction-grade submission looks like. Students can read them, so they must be
exemplary — house style, XML documentation on public members, and a short comment
at the top of each file explaining the design choice made and what the alternative
would have cost.

Because solutions are visible, exercise C in each file must remain genuinely open:
if it has one correct answer that the solution gives away, rewrite the exercise.
The solution shows one defensible route, and says so.

`code/tests/` holds xUnit tests covering the exercise B and C solutions, as a
single plain .NET test project — one project for the whole module, not one per
exercise — so they run outside Unity. These are student-facing and serve two
purposes: they express the "done when" criteria in executable form, and they
introduce test-driven checking before it is needed formally later in the
programme. Each test method name states the behaviour it asserts.

xUnit is the testing framework for this module throughout. Do not introduce NUnit
or MSTest in any week.

## 10. Repository hygiene

All three content folders — `notes/`, `exercises/` and `code/` — are committed and
visible to students. The `.gitignore` excludes build output and editor state only:

```
bin/
obj/
*.user
*.suo
.vs/
.idea/
.DS_Store
[Tt]est[Rr]esults/
```

Do not add `code/`, `code/solutions/` or `code/tests/` to the `.gitignore`.

**Nothing Claude produces is pushed to the repository by Claude.** Generate files
to the local working directory only. Do not run `git add`, `git commit`,
`git push`, or any other Git command. Do not create a repository, a remote, or a
GitHub Pages configuration. Publishing is a manual step performed by the lecturer
after review.

Any working files created during generation — plans, drafts, scratch notes,
progress trackers, summaries of what was produced — stay local and are not part
of the manifest in Section 9. Do not write them into `notes/`, `exercises/` or the
repository root or into `code/`. If such a file is needed, write it outside the
repository folder entirely.

Before finishing, verify that the repository contains only the files listed in
Section 9, and report anything else found rather than deleting it.

## 11. Out of scope

Do not cover, and do not reference as though students know them:

- `async`/`await`, `Task`, coroutines
- Records and `init` accessors
- Pattern matching beyond a basic `switch`
- Reflection and attributes beyond `[Flags]` and `[SerializeField]`
- Dependency injection frameworks
- Unity APIs other than those named explicitly above

If a topic seems to need one of these, the example is too complex. Simplify it.

## 12. Generation order

1. The eight notes, in order, so later notes can reference earlier ones by filename.
2. The eight exercise files.
3. The solutions and tests, written against the final wording of the exercises.
   Re-read each exercise immediately before writing its solution, and if the
   solution turns out to be the only possible answer to exercise C, revise the
   exercise.
4. The `.gitignore`.
5. The README last, once the file set actually exists, so the table of contents
   and the suggested order of work reflect what was written rather than what was
   planned.