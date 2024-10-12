using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MeF.Client.Extensions
{
    /// <summary>
    /// Contains collection-related extensions.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Adds a range of values to an existing <see cref="string">List</see>.
        /// </summary>
        /// <typeparam name="T">Type of the items in the <see cref="string">List</see>.</typeparam>
        /// <param name="list"><see cref="string">List</see> to append.</param>
        /// <param name="values">Range of values to add.</param>
        public static void AddRange<T>(this List<T> list, params T[] values)
        {
            foreach (T value in values)
            {
                list.Add(value);
            }
        }

        /// <summary>
        /// Checks whether an <see cref="IEnumerable">IEnumerable</see> contains at least a certain number of items.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IEnumerable">IEnumerable</see>.</typeparam>
        /// <param name="enumeration"><see cref="IEnumerable">IEnumerable</see> to inspect.</param>
        /// <param name="count">Number to use in the comparison.</param>
        /// <returns>Indicator if the <see cref="IEnumerable">IEnumerable</see> is at least the specified length.</returns>
        public static bool ContainsAtLeast<T>(this IEnumerable<T> enumeration, int count)
        {
            // Check to see that enumeration is not null
            if (enumeration == null)
            {
                throw new ArgumentNullException("enumeration");
            }

            return (from t in enumeration.Take(count)
                    select t)
                    .Count() == count;
        }

        /// <summary>
        /// Iterates each item in an IEnumerable of a type and performs an operation.
        /// </summary>
        /// <typeparam name="T">Type to enumerate.</typeparam>
        /// <param name="enumeration">List of IEnumerable objects.</param>
        /// <param name="mapFunction">Function to execute for each type.</param>
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> mapFunction)
        {
            foreach (var item in enumeration)
            {
                mapFunction(item);
            }
        }

        /// <summary>
        /// Returns the index of the first occurrence of a value in a sequence by using the default EqualityComparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="enumeration">A sequence in which to locate a value.</param>
        /// <param name="value">The object to locate in the sequence.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire sequence, if found; otherwise, –1.</returns>
        public static int IndexOf<T>(this IEnumerable<T> enumeration, T value) where T : IEquatable<T>
        {
            return enumeration.IndexOf<T>(value, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Returns the index of the first occurrence in a sequence by using a specified IEqualityComparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the IEnumerable.</typeparam>
        /// <param name="enumeration">A sequence in which to locate a value.</param>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire sequence, if found; otherwise, –1.</returns>
        public static int IndexOf<T>(this IEnumerable<T> enumeration, T value, IEqualityComparer<T> comparer)
        {
            int index = 0;

            foreach (var item in enumeration)
            {
                if (comparer.Equals(item, value))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        /// <summary>
        /// Returns the index of the first occurrence, starting at the indicated start index, in a sequence by using the default equality comparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the IEnumerable.</typeparam>
        /// <param name="enumeration">A sequence in which to locate a value.</param>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire sequence, if found; otherwise, –1.</returns>
        public static int IndexOf<T>(this IEnumerable<T> enumeration, T value, int startIndex) where T : IEquatable<T>
        {
            return enumeration.IndexOf<T>(value, startIndex, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Returns the index of the first occurrence in a sequence by using a specified IEqualityComparer.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="enumeration">A sequence in which to locate a value.</param>
        /// <param name="value">The value to locate in the sequence.</param>
        /// <param name="startIndex">The index at which to begin the search.</param>
        /// <param name="comparer">An equality comparer to compare values.</param>
        /// <returns>The zero-based index of the first occurrence of value within the entire sequence, if found; otherwise, –1.</returns>
        public static int IndexOf<T>(this IEnumerable<T> enumeration, T value, int startIndex, IEqualityComparer<T> comparer)
        {
            for (int i = startIndex; i < enumeration.Count(); i++)
            {
                T item = enumeration.ElementAt(i);

                if (comparer.Equals(item, value))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns a Boolean indicating whether the Array is null or empty.
        /// </summary>
        /// <param name="array">The Array to inspect.</param>
        /// <returns>Returns a boolean indicating whether the Array is null empty.</returns>
        public static bool IsNullOrEmpty(this Array array)
        {
            return (array == null) || (array.Length == 0);
        }

        /// <summary>
        /// Returns a Boolean indicating whether the ArrayList is null or empty..
        /// </summary>
        /// <param name="list">The ArrayList to inspect.</param>
        /// <returns>Returns a Boolean indicating whether the ArrayList is null or empty.</returns>
        /// <returns>Array with only unique items.</returns>
        public static bool IsNullOrEmpty(this ArrayList list)
        {
            return (list == null) || (list.Count == 0);
        }

        /// <summary>
        /// Adds a value to the end of a list.
        /// </summary>
        /// <typeparam name="T">Type of the items in the list.</typeparam>
        /// <param name="list">List to add the new value to.</param>
        /// <param name="value">Value to place at the end of the list.</param>
        public static void Push<T>(this List<T> list, T value)
        {
            list.Add(value);
        }

        /// <summary>
        /// Removes duplicate strings from an array.
        /// </summary>
        /// <typeparam name="T">Type of array.</typeparam>
        /// <param name="array">Array for which to remove duplicates.</param>
        /// <returns>Array with only unique items.</returns>
        public static T[] RemoveDuplicates<T>(this T[] array)
        {
            ArrayList al = new ArrayList();

            for (int i = 0; i < array.Length; i++)
            {
                if (!al.Contains(array[i]))
                {
                    al.Add(array[i]);
                }
            }

            return (T[])al.ToArray(typeof(T));
        }

        /// <summary>
        /// Return the element that the specified property's value is contained in the specifiec values
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertySelector">The property to be tested.</param>
        /// <param name="values">The accepted values of the property.</param>
        /// <returns>The accepted elements.</returns>
        public static IQueryable<TElement> WhereIn<TElement, TValue>(this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, params TValue[] values)
        {
            return source.Where(GetWhereInExpression(propertySelector, values));
        }

        /// <summary>
        /// Returns the element that the specified property's value is contained in the specifiec values.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertySelector">The property to be tested.</param>
        /// <param name="values">The accepted values of the property.</param>
        /// <returns>The accepted elements.</returns>
        public static IQueryable<TElement> WhereIn<TElement, TValue>(this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            return source.Where(GetWhereInExpression(propertySelector, values));
        }

        /// <summary>
        /// Returns the element that the specified property's value is not contained in the specifiec values.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertySelector">The property to be tested.</param>
        /// <param name="values">The accepted values of the property.</param>
        /// <returns>The accepted elements.</returns>
        public static IQueryable<TElement> WhereNotIn<TElement, TValue>(this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, params TValue[] values)
        {
            return source.Where(GetWhereNotInExpression(propertySelector, values));
        }

        /// <summary>
        /// Returns the element that the specified property's value is not contained in the specifiec values.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertySelector">The property to be tested.</param>
        /// <param name="values">The accepted values of the property.</param>
        /// <returns>The accepted elements.</returns>
        public static IQueryable<TElement> WhereNotIn<TElement, TValue>(this IQueryable<TElement> source, Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            return source.Where(GetWhereNotInExpression(propertySelector, values));
        }

        /// <summary>
        /// Returns an expression that will assist in retrieving values included in a collection.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="propertySelector">The property to be tested.</param>
        /// <param name="values">The accepted values of the property.</param>
        /// <returns>A LINQ expression.</returns>
        private static Expression<Func<TElement, bool>> GetWhereInExpression<TElement, TValue>(Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            ParameterExpression p = propertySelector.Parameters.Single();

            if (!values.Any())
            {
                return e => false;
            }

            var equals = values.Select(value => (Expression)Expression.Equal(propertySelector.Body, Expression.Constant(value, typeof(TValue))));
            var body = equals.Aggregate<Expression>((accumulate, equal) => Expression.Or(accumulate, equal));

            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }

        /// <summary>
        /// Returns an expression that will assist in retrieving values not included in a collection.
        /// </summary>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <typeparam name="TValue">The type of the values.</typeparam>
        /// <param name="propertySelector">The property to be tested.</param>
        /// <param name="values">The accepted values of the property.</param>
        /// <returns>A LINQ expression.</returns>
        private static Expression<Func<TElement, bool>> GetWhereNotInExpression<TElement, TValue>(Expression<Func<TElement, TValue>> propertySelector, IEnumerable<TValue> values)
        {
            ParameterExpression p = propertySelector.Parameters.Single();

            if (!values.Any())
            {
                return e => true;
            }

            var unequals = values.Select(value => (Expression)Expression.NotEqual(propertySelector.Body, Expression.Constant(value, typeof(TValue))));

            var body = unequals.Aggregate<Expression>((accumulate, unequal) => Expression.And(accumulate, unequal));

            return Expression.Lambda<Func<TElement, bool>>(body, p);
        }
    }
}