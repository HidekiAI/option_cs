// =====================================================================================
// NOTICE: To anybody who encounters this code, PLEASE code-review and provide feedback!
//         Suggestions as well as any missing features are highly appreciated!
//         Thank you in advance!
// =====================================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// static import of Option<T> and OptionExtensions to make it easier to use, e.g. Some(T) instead of Option.Some(T)
using static Optional.Option;
using static Optional.OptionExtensions;

// Option<T> ressembles Rust's Option<T> type.
// It is a generic type that can either hold a value of type T or no value at all.
// Supporting usage:
// - Some(T value) creates an Option<T> with a value.
// - None creates an Option<T> with no value.
// - Match(Func<T, TResult> some, Func<TResult> none) executes the appropriate function based on the Option's value.
// - Map(Func<T, TResult> map) maps the Option's value to a new value.
// - Flatten() flattens an Option<Option<T>> to an Option<T>.
// - Unwrap() unwraps the Option's value (very biased to Rust, do I need Bind() if I have Unwrap()?)
// Example:
//      Option<int> some = Some(42);
//      Option<int> none = None;
//      some.Match( value => Console.WriteLine(value), () => Console.WriteLine("No value"));
//      some.Map( value => value * 2).Match( value => Console.WriteLine(value), () => Console.WriteLine("No value"));   
//      some.Flatten().Match( value => Console.WriteLine(value), () => Console.WriteLine("No value"));
//      var v = some.Unwrap();
//      var vs = new Option<int>[] { Some(1), None, Some(2) };
//      foreach(var v in vs.Flatten()) {...}
//      foreach(var v in vs.SelectMany( value => Some(value * 2))) {...}
//      foreach(var v in vs.SelectMany( value => Some(value * 2).Flatten())) {...}
namespace Optional
{
    public class Option<T> : IEnumerable<T>
    {
        // making T nullable (via `T?`) would disallow `Option<DateTime>` which is non-nullable type,
        // so we'll use a flag (_hasValue) to indicate whether the value is present or not.
        private T _value = default(T);      // Possible null reference assignment.
        private bool _hasValue = false;  // always default as None()

        // to match Rust's is_some() and is_none()
        public bool IsSome() => _hasValue;
        public bool IsNone() => !_hasValue;
        public T GetValue()
        {
            // panic if no value
            if (IsSome() == false)
            {
                throw new InvalidOperationException("Option does not have a value");
            }
            return _value;
        }

        // 
        private Option(T init_value)
        {
            // panic if value is null
            if (init_value == null) // Q:what if T is Option<U>?  A:it's not null, so it's okay...
            {
                throw new ArgumentNullException("Option value cannot be null, use None() instead");
            }
            this._hasValue = true;
            this._value = init_value;
        }

        private Option()
        {
            _hasValue = false;
            _value = default(T);    // Possible null reference assignment
        }

        public static Option<T> Some(T new_value)
        {
            // should panic if new_value is null
            return new Option<T>(new_value);
        }

        public static Option<T> None
        {
            get { return new Option<T>(); }
        }

        // Match() should return TResult where TResult can be void...  For void, we need to override
        public TResult Match<TResult>(Func<T, TResult> fn_some, Func<TResult> fn_none)
        {
            return IsSome() ? fn_some(_value) : fn_none();
        }
        public void Match(Action<T> fn_some, Action fn_none)    // explicit override for void
        {
            // nothing to return...
            if (IsSome())
            {
                fn_some(_value);
            }
            else
            {
                fn_none();
            }
        }

        public Option<TResult> Map<TResult>(Func<T, TResult> fn_map)
        {
            return IsSome() ? Option<TResult>.Some(fn_map(_value)) : Option<TResult>.None;
        }

        /// <summary>
        /// Flattens Option<T> in which if T is of collection type including Option<U>,
        /// which is iterable, it'll return value T.  For example:
        /// - Option<Option<int>> => Option<int> IF Option<int> has a value, else None
        /// - Option<Array<int>> => Array<int> IF Array<int> has 1 or more elements, else None
        /// - Option<List<int>> => List<int> IF List<int> has 1 or more elements, else None
        /// If T is not iterable, Flatten() will return None.
        /// </summary>
        /// <returns>Some<T> if T has value, else None</returns>
        public Option<T> Flatten()
        {
            //return IsSome() ? _value as Option<T> : Option<T>.None;
            if (IsSome())
            {
                if (_value is IEnumerable<T>)
                {
                    var enumerable = _value as IEnumerable<T>;
                    if (enumerable.Any())
                    {
                        return Some(_value);
                    }
                    else
                    {
                        return None;
                    }
                }
                else
                {
                    return Some(_value);
                }
            }
            else
            {
                return None;
            }
        }

        public T Unwrap()
        {
            if (IsSome())
            {
                return _value;
            }
            else
            {
                throw new InvalidOperationException("Option does not have a value");
            }
        }

        /// <summary>
        /// Derivation of IEnumerable<T>, so mapping and flattening works
        /// Option<T> as enumerable collection is treated as if it's a collection of T
        /// of either 0 or 1 element in the collection/list/array
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (IsSome())
            {
                yield return _value;
            }
        }

        /// <summary>
        /// Derivation of IEnumerable<T>, so mapping and flattening works
        /// Option<T> as enumerable collection is treated as if it's a collection of T
        /// of either 0 or 1 element in the collection/list/array
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public static class OptionExtensions
    {
        public static IEnumerable<T> Flatten<T>(this IEnumerable<Option<T>> options)
        {
            foreach (var option in options)
            {
                if (option.IsSome())
                {
                    yield return option.GetValue();
                }
            }
        }

        public static Option<TResult> SelectMany<T, TResult>(this Option<T> option, Func<T, Option<TResult>> fn_map)
        {
            if (option.IsSome())
            {
                return fn_map(option.GetValue());
            }
            else
            {
                return Option<TResult>.None;
            }
        }
    }

    // I want to just express it as Some(T) instead of Option.Some(T):
    public static class Option
    {
        public static Option<T> Some<T>(T value) => Option<T>.Some(value);
        public static Option<T> None<T>() => Option<T>.None;
    }

    // for unit-test'ish code to test out (demonstrate) Option<T> usages and patterns:
    class OptionTest
    {
        public static void Test()
        {
            // note that we're using `using static Optional.Option;` and `using static Optional.OptionExtensions;` so that
            // we can use Some(T) and None<T> directly without Option.Some(T) and Option.None<T>
            Option<int> possible_int = Some(42);
            Option<int> possible_int2 = None<int>();    // TODO: The explicity variable declartion should not require None<int>() to be explicit (should be able to do None() here?)

            // type inference
            var possible_type_inferred_value = Some(42);
            var possible_type_inferred_value2 = None<bool>();   // you MUST specify the type for None() to work (obvious, but )

            // pattern matching (void return type and non-void return type)
            possible_int.Match(value => Console.WriteLine(value), () => Console.WriteLine("No value"));
            var match_result = possible_int.Match(value => value, () => -1);    // pattern matching (non-void return type)

            // mapping
            possible_int.Map(value =>
                value * 2).Match(
                    value => Console.WriteLine(value),  // Some
                    () => Console.WriteLine("No value"));   // None

            // flattening
            possible_int.Flatten().Match(
                value => Console.WriteLine(value),  // Some
                () => Console.WriteLine("No value"));   // None

            // unwrapping
            var v_some = possible_int.Unwrap(); // will throw if it is none
            try
            {
                var v_none = possible_int2.Unwrap(); // this SHOULD throw
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
            }

            // enumeration
            var vs = new Option<int>[] { Some(1), None<int>(), Some(2) };
            // declaring `int` explicitly instead of `var v` to demonstrate expected type, but usually you want to do `foreach(var v...)`
            foreach (int v_int in vs.Flatten())
            {
                Console.WriteLine(v_int);
            }
            // Note the type inferences on Select<Option<int>, Option<int>> here:
            foreach (Option<int> possible_v_int in vs.Select(possible_val_t => possible_int.Match(val_t => Some(val_t * 2), () => None<int>())))
            {
                Console.WriteLine(possible_v_int.Match(
                    value => value.ToString(),
                    () => "No value") + " (mapped)");
            }
            foreach (int v_int in vs.Select(possible_val_t => possible_int.Match(val_t => Some(val_t * 2), () => None<int>())).Flatten())
            {
                Console.WriteLine(v_int);
            }
            foreach (int v_int in vs.SelectMany(possible_val_t => possible_int.Match(val_t => Some(val_t * 2), () => None<int>())))
            {
                Console.WriteLine(v_int);
            }
        }
    }
}