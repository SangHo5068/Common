using System.Diagnostics;

namespace Common.Utilities
{
    /// <summary>
    /// The get method info strings.
    /// </summary>
    public class GetMethodInfoStrings
    {
        /// <summary>
        /// The get method name.
        /// </summary>
        /// <param name="frame">
        /// The frame.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetMethodName(int frame = 1)
        {
            var stackTrace = new StackTrace(true);
            var stackFrame = stackTrace.GetFrame(frame);

            if (stackFrame == null)
            {
                return "Service";
            }

            return null != stackFrame.GetMethod() ? stackFrame.GetMethod().Name : string.Empty;
        }

        /// <summary>
        /// The get file name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetFileName()
        {
            var stackTrace = new StackTrace(true);
            var stackFrame = stackTrace.GetFrame(1);

            return stackFrame.GetFileName() ?? string.Empty;

        }

        /// <summary>
        /// The get file line number.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetFileLineNumber()
        {
            var stackTrace = new StackTrace(true);
            var stackFrame = stackTrace.GetFrame(1);

            return 0 != stackFrame.GetFileLineNumber() ? stackFrame.GetFileLineNumber().ToString() : string.Empty;
        }

        /// <summary>
        /// The get caller method name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCallerMethodName()
        {
            var stackTrace = new StackTrace(true);
            var stackFrame = stackTrace.GetFrame(2);

            return null != stackFrame.GetMethod() ? stackFrame.GetMethod().Name : string.Empty;
        }

        /// <summary>
        /// The get calller file name.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCalllerFileName()
        {
            var stackTrace = new StackTrace(true);
            var stackFrame = stackTrace.GetFrame(2);

            return stackFrame.GetFileName() ?? string.Empty;
        }

        /// <summary>
        /// The get caller file line number.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetCallerFileLineNumber()
        {
            var stackTrace = new StackTrace(true);
            var stackFrame = stackTrace.GetFrame(2);

            return 0 != stackFrame.GetFileLineNumber() ? stackFrame.GetFileLineNumber().ToString() : string.Empty;
        }
    }
}
