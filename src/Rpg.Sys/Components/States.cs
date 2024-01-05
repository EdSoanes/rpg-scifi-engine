using Newtonsoft.Json;
using System.Collections;

namespace Rpg.Sys.Components
{
    public class States : IList<IState>
    {
        [JsonProperty] private List<IState> _states { get; set; } = new List<IState>();

        public void Add(params IState[] items)
        {
            foreach (var item in items)
                _states.Add(item);
        }

        public bool CanActivate(string stateName)
        {
            foreach (var state in  _states.Where(x => x.IsActive && x.StatesPermittedWhenActive() != null))
            {
                if (!state.StatesPermittedWhenActive()!.Contains(stateName))
                    return false;
            }

            return true;
        }

        public void Activate(string stateName)
        {
            var state = _states.SingleOrDefault(x => x.Name == stateName);
            if (state != null)
                state.IsActive = true;
        }

        public void Deactivate(string stateName)
        {
            var state = _states.SingleOrDefault(x => x.Name == stateName);
            if (state != null)
                state.IsActive = false;
        }

        public IState? Remove(string stateName)
        {
            var toRemove = _states.SingleOrDefault(x => x.Name == stateName);
            if (toRemove != null)
                _states.Remove(toRemove);

            return toRemove;
        }

        #region IList

        public IState this[int index] { get => _states[index]; set => _states[index] = value; }
        public int Count => _states.Count;
        public bool IsReadOnly => false;
        public void Add(IState item) => _states.Add(item);
        public void Clear() => _states.Clear();
        public bool Contains(IState item) => _states.Contains(item);
        public void CopyTo(IState[] array, int arrayIndex) => _states.CopyTo(array, arrayIndex);
        public IEnumerator<IState> GetEnumerator() => _states.GetEnumerator();
        public int IndexOf(IState item) => _states.IndexOf(item);
        public void Insert(int index, IState item) => _states.Insert(index, item);
        public bool Remove(IState item) => _states.Remove(item);
        public void RemoveAt(int index) => _states.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => _states.GetEnumerator();

        #endregion IList
    }
}
