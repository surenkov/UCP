using System;

namespace StateMachine
{
    [Serializable]
    public class State
    {
        private static ulong _idGen;

        private byte _bitstate;

        public bool Start
        {
            get { return (_bitstate & 0x01) == 0x01; }
            set { _bitstate = (byte)(value ? _bitstate | 0x01 : _bitstate & 0xFE); }
        }

        public bool Final
        {
            get { return (_bitstate & 0x02) == 0x02; }
            set { _bitstate = (byte)(value ? _bitstate | 0x02 : _bitstate & 0xFD); }
        }

        public ulong Id { get; }

        public State()
        {
            Id = _idGen++;
            _bitstate = 0x00;
        }

        public State(ulong id)
        {
            Id = id;
            _bitstate = 0x00;
        }

        public State(State other)
        {
            _bitstate = other._bitstate;
            Id = other.Id;
        }

        public override bool Equals(object obj)
        {
            var state = obj as State;
            return Id == state?.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override string ToString()
        {
            return $"[State: Start={Start}, Final={Final}, Id={Id}]";
        }
    }
}
