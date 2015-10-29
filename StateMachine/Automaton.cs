using System.Collections.Generic;
using System;

namespace StateMachine
{
	using States = HashSet<State>;

	public class StateNotFoundException : KeyNotFoundException
	{
		public StateNotFoundException()
		{
		}

		public StateNotFoundException(string message) : base(message)
		{
		}

		public StateNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}

	/// <summary>
	/// Abstract state machine
	/// </summary>
	public abstract class Automaton<Event>
	{
		protected State _start;
		protected States _states;
		protected HashSet<Event> _events;

		public State Start {
			get { return _start; }
			set {
				_states.Add(_start = value);
				_start.Start = true;
			}
		}

		public State LastAdded { get; protected set; }

		protected Automaton()
		{
			_states = new HashSet<State>();
			_events = new HashSet<Event>();

			_states.Add(LastAdded = Start = new State());
		}

		public virtual void AddTransition(State from, State to, Event e)
		{
			if (!_states.Contains(from))
				throw new StateNotFoundException();

			if (_states.Add(to))
				LastAdded = to;
			_events.Add(e);
		}

		public abstract void Trigger(Event e);
	}

	/// <summary>
	/// Deterministic finite-state machine
	/// </summary>
	public class DFA<Event> : Automaton<Event>
	{
		private readonly Dictionary<KeyValuePair<State, Event>, State> _table;

		public State Current { get; private set; }

		public DFA()
		{
			_table = new Dictionary<KeyValuePair<State, Event>, State>();
			Current = _start;
		}

		public override void AddTransition(State from, State to, Event e)
		{
			base.AddTransition(from, to, e);
			_table.Add(new KeyValuePair<State, Event>(from, e), to);
		}

		public override void Trigger(Event e)
		{
			Current = _table[new KeyValuePair<State, Event>(Current, e)];
		}
	}

	/// <summary>
	/// Non-deterministic finite-state machine
	/// </summary>
	public class NFA<Event> : Automaton<Event>
	{
		private readonly Dictionary<KeyValuePair<State, Event>, States> _table;
		private readonly Dictionary<State, States> _epsClosures;

		public States Current { get; private set; }

		public NFA()
		{
			_table = new Dictionary<KeyValuePair<State, Event>, States>();
			_epsClosures = new Dictionary<State, States> { { _start, new States { _start } } };
		}

		public override void AddTransition(State from, State to, Event e)
		{
			base.AddTransition(from, to, e);
			var key = new KeyValuePair<State, Event>(from, e);
			if (!_table.ContainsKey(key))
				_table.Add(key, new States());
			_table[key].Add(to);
		}

		/// <summary>
		/// Adds epsilon transition
		/// </summary>
		public void AddTransition(State from, State to)
		{
			if (!_states.Contains(from))
				throw new StateNotFoundException();

			if (_states.Add(to))
				LastAdded = to;

			if (!_epsClosures.ContainsKey(from))
				_epsClosures.Add(from, new States { from });

			if (!_epsClosures.ContainsKey(to))
				_epsClosures.Add(to, new States { to });

			//TODO: Unite with eps-closure of to-state
			foreach (var pair in _epsClosures)
				if (pair.Value.Contains(from))
					pair.Value.UnionWith(_epsClosures[to]);
		}

		public void Initialize()
		{
			Current = new States();
			Current.UnionWith(_epsClosures[_start]);
		}

		public override void Trigger(Event e)
		{
			var state = new States();
			foreach (State s in Current) {
				States tmp;
				if (_table.TryGetValue(new KeyValuePair<State, Event>(s, e), out tmp))
					state.UnionWith(tmp);
			}

			var epsilonState = new States();
			foreach (State s in state) {
				States tmp;
				if (_epsClosures.TryGetValue(s, out tmp))
					epsilonState.UnionWith(tmp);
				epsilonState.Add(s);
			}

			Current = epsilonState;

			if (epsilonState.Count == 0)
				throw new StateNotFoundException();
		}

		public DFA<Event> ToDFA()
		{
			throw new NotImplementedException();
		}

		public void Merge(NFA<Event> other)
		{
			_states.UnionWith(other._states);
			_events.UnionWith(other._events);

			foreach (var item in other._epsClosures) {
				if (!_epsClosures.ContainsKey(item.Key))
					_epsClosures.Add(item.Key, new States());
				_epsClosures[item.Key].UnionWith(item.Value);
			}

			foreach (var item in other._table) {
				if (!_table.ContainsKey(item.Key))
					_table.Add(item.Key, new States());
				_table[item.Key].UnionWith(item.Value);
			}
		}
	}
}
