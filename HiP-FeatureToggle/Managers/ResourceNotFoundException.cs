using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Managers
{
    /// <summary>
    /// Indicates that one or more resources referenced by keys/IDs do not exist.
    /// </summary>
    /// <typeparam name="T">Resource type</typeparam>
    public class ResourceNotFoundException<T> : ResourceNotFoundException
    {
        public override Type ResourceType => typeof(T);

        public ResourceNotFoundException(object key) : base(key, typeof(T))
        {
        }
    }

    public abstract class ResourceNotFoundException : Exception
    {
        public IReadOnlyCollection<object> Keys { get; }

        public abstract Type ResourceType { get; }

        /// <param name="key">The key or a collection of keys corresponding to the missing resource(s)</param>
        public ResourceNotFoundException(object key, Type resourceType) : base(BuildMessage(key, resourceType))
        {
            Keys = (key as IEnumerable)?.Cast<object>().ToArray() ?? new[] { key };
        }

        private static string BuildMessage(object key, Type resourceType)
        {
            var keys = (key as IEnumerable)?.Cast<object>().ToArray() ?? new[] { key };
            var suffix = $"of type '{resourceType.Name}' cannot be found";

            switch (keys?.Count() ?? 0)
            {
                case 0: return $"A resource {suffix}";
                case 1: return $"The resource '{keys.First()}' {suffix}";
                default: return $"The resources [{string.Join(", ", keys)}] {suffix}";
            }
        }
    }
}
