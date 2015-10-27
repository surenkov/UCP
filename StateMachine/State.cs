namespace StateMachine
{
    public class State
    {
        private static ulong _idGen = 0;

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

        public ulong Id { get; private set; }

        public string Name { get; set; }

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
            Name = other.Name;
        }

        public override bool Equals(object other)
        {
            return Id == (other as State)?.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
