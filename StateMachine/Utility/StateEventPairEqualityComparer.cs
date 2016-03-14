using System;
using System.Collections.Generic;

namespace StateMachine.Utility
{
    internal class StateEventPairEqualityComparer<T> : IEqualityComparer<KeyValuePair<State, T>>
        where T : IEquatable<T>
    {
        public bool Equals(KeyValuePair<State, T> x, KeyValuePair<State, T> y)
        {
            return x.Key.Equals(y.Key) && x.Value.Equals(x.Value);
        }

        public int GetHashCode(KeyValuePair<State, T> obj)
        {
            return obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
        }
    }
}