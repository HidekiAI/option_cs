// Unit-test'ish cod to test out (demonstrate) Option<T> usages and patterns:

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Optional;

// I want to just express it as Some(T) instead of Option.Some(T):
using static Optional.Option;
using static Optional.OptionExtensions;

internal class Program
{
    public static void Main(string[] args)
    {
        OptionTest.Test();
    }
}