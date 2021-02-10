using System.Collections.Generic;

namespace FunFair.BuildVersion.Detection.Extensions
{
    /// <summary>
    ///     Extensions for collections
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        ///     Removes nulls from the source collection.
        /// </summary>
        /// <param name="source">The source collection</param>
        /// <typeparam name="TItemType">The item type</typeparam>
        /// <returns>Collection without nulls.</returns>
        public static IEnumerable<TItemType> RemoveNulls<TItemType>(this IEnumerable<TItemType?> source)
            where TItemType : class
        {
            foreach (TItemType? item in source)
            {
                if (!ReferenceEquals(objA: item, objB: null))
                {
                    yield return item;
                }
            }
        }
    }
}