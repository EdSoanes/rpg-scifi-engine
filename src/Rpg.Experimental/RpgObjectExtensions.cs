using NanoidDotNet;
using Rpg.Experimental.Reflection;
using System.Reflection;

namespace Rpg.Experimental
{
    public static class RpgObjectExtensions
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int Size = 10;

        public static string NewId(this object obj)
            => $"{obj.GetType().Name}[{Nanoid.Generate(Alphabet, Size)}]";


    }
}

