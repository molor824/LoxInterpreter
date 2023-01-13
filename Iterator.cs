public struct Iterator<T, TE> where T : IReadOnlyList<TE>
{
    private T _list;

    public T List => _list;
    public int Index;
    public bool ReachedEnd => Index >= _list.Count;

    public Iterator(T list)
    {
        _list = list;
        Index = -1;
    }

    public bool TryGet(int index, out TE result)
    {
        if (index >= _list.Count || index < 0)
        {
            result = default!;
            return false;
        }

        result = _list[index];
        return true;
    }

    public bool TryForward(out TE result)
    {
        Index++;
        return TryGet(Index, out result);
    }

    public bool TryBackward(out TE result)
    {
        Index--;
        return TryGet(Index, out result);
    }

    public bool PeekOffset(int offset, out TE result) => TryGet(Index + offset, out result);
    public bool PeekNext(out TE result) => TryGet(Index + 1, out result);
    public bool PeekPrevious(out TE result) => TryGet(Index - 1, out result);
    public bool TryCurrent(out TE result) => TryGet(Index, out result);
}