using System;

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
}
