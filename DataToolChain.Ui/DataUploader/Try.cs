using System;

namespace DataToolChain
{
    /// <summary>
    ///     Tries to get things by repeating the action until successful, or number of tries has been met.
    /// </summary>
    public static class Try
    {
        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static T Get<T>(
            this Func<T> action,
            Func<Exception, T> replacement)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return replacement(ex);
            }
        }

        /// <summary>
        ///     Tries to evaluate a function. If successful, returns, otherwise waits for requested interval and tries again.
        ///     If not successful will return replacement.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static T Get<T>(
            this Func<T> action,
            T replacement)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                return replacement;
            }
        }

    }
}
