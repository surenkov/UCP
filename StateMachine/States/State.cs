using System;

namespace StateMachine.States
{
    [Serializable]
    public class State : ICloneable
    {
        private static ulong _idGen;

        private byte _bitState;

        public State()
        {
            Id = _idGen++;
        }

        public State(ulong id)
        {
            Id = id;
        }

        public bool Start
        {
            get { return (_bitState & 0x01) == 0x01; }
            set { _bitState = (byte) (value ? _bitState | 0x01 : _bitState & 0xFE); }
        }

        public bool Final
        {
            get { return (_bitState & 0x02) == 0x02; }
            set { _bitState = (byte) (value ? _bitState | 0x02 : _bitState & 0xFD); }
        }

        public ulong Id { get; }

        object ICloneable.Clone() => Clone();

        public State Clone() => new State(Id) { _bitState = _bitState };

        public override bool Equals(object obj)
        {
            var state = obj as State;
            return Id == state?.Id;
        }

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => $"State: Start={Start}, Final={Final}, Id={Id}";
    }
}
