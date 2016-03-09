using System;
using System.Collections.Generic;
using System.Linq;

namespace StateMachine
{
    [Serializable]
    public class State : ICloneable
    {
        private static ulong _idGen;

        private byte _bitState;

        public bool Start
        {
            get { return (_bitState & 0x01) == 0x01; }
            set { _bitState = (byte)(value ? _bitState | 0x01 : _bitState & 0xFE); }
        }

        public bool Final
        {
            get { return (_bitState & 0x02) == 0x02; }
            set { _bitState = (byte)(value ? _bitState | 0x02 : _bitState & 0xFD); }
        }

        public ulong Id { get; }

        public State()
        {
            Id = _idGen++;
            _bitState = 0x00;
        }

        public State(ulong id)
        {
            Id = id;
            _bitState = 0x00;
        }

        public State Clone() => new State(Id) { _bitState = _bitState };

        public override bool Equals(object obj)
        {
            var state = obj as State;
            return Id == state?.Id;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => $"[State: Start={Start}, Final={Final}, Id={Id}]";

        object ICloneable.Clone() => Clone();
    }

    public class States : HashSet<State>
    {
        private int _hash;

        public States()
        {
        }

        public States(IEnumerable<State> collection) 
            : base(collection)
        {
        }
        
        public States ReHash()
        {
            unchecked
            {
                int hash = 0;
                foreach (var state in this)
                {
                    hash += state.GetHashCode();
                    hash += hash << 10;
                    hash ^= hash >> 6;
                }
                hash += hash << 3;
                hash ^= hash >> 11;
                _hash = hash + (hash << 15);
            }
            return this;
        }

        public override bool Equals(object obj)
        {
            var other = obj as States;
            if (other == null || other.Count != Count || other._hash != _hash)
                return false;
            return other.All(Contains);
        }

        public override int GetHashCode() => _hash;
    }

    public class StateNotFoundException : KeyNotFoundException
    {
    }
}
