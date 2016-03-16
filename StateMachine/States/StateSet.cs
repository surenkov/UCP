using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StateMachine.States
{
    public class StateSet : IEnumerable<State>
    {
        private readonly HashSet<State> _states;
        private int _hash;

        public StateSet()
        {
            _states = new HashSet<State>();
        }

        public StateSet(IEnumerable<State> collection)
        {
            foreach (var state in collection)
                Add(state);
        }

        public int Count => _states.Count;

        public IEnumerator<State> GetEnumerator() => _states.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Add(State item)
        {
            if (item == null || !_states.Add(item))
                return false;
            _hash ^= item.GetHashCode();
            return true;
        }

        public bool Remove(State item)
        {
            if (item == null || !_states.Remove(item))
                return false;
            _hash ^= item.GetHashCode();
            return true;
        }

        public bool Contains(State item) => _states.Contains(item);

        public void UnionWith(StateSet other)
        {
            _hash ^= other._hash;
            _states.UnionWith(other);
        }

        public override bool Equals(object obj)
        {
            var other = obj as StateSet;
            return other != null
                   && other.Count == Count
                   && other._hash == _hash
                   && other.All(Contains);
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => _hash;
    }

    public class StateNotFoundException : KeyNotFoundException
    {
    }
}
