namespace Rpg.ModObjects.Server
{
    public class RpgResponse<T>
    {
        public RpgGraphState GraphState { get; init; }
        public T Data { get; init; }
    }
}
