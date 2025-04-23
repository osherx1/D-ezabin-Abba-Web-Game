using System;
public static class GameManager
{
    public static int Zuzim
    {
        get => _zuzim;
        set { _zuzim = value; OnZuzChanged?.Invoke(_zuzim); }
    }
    static int _zuzim;
    public static Action<int> OnZuzChanged;
}