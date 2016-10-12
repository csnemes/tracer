using System;
using log4net.Core;

namespace Tracer.Log4Net
{
    public static class Log 
    {
        /// <overloads>Log a message object with the <see cref="Level.Debug"/> level.</overloads>
		/// <summary>
		/// Log a message object with the <see cref="Level.Debug"/> level.
		/// </summary>
		/// <param name="message">The message object to log.</param>
        public static void Debug(object message) { }

        /// <summary>
        /// Log a message object with the <see cref="Level.Debug"/> level including
        /// the stack trace of the <see cref="Exception"/> passed
        /// as a parameter.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        public static void Debug(object message, Exception exception) { }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Debug"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void DebugFormat(string format, params object[] args) { }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Debug"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        public static void DebugFormat(string format, object arg0) { }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Debug"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        public static void DebugFormat(string format, object arg0, object arg1) { }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Debug"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        /// <param name="arg2">An Object to format</param>
        public static void DebugFormat(string format, object arg0, object arg1, object arg2) { }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Debug"/> level.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void DebugFormat(IFormatProvider provider, string format, params object[] args) { }

        /// <overloads>Log a message object with the <see cref="Level.Info"/> level.</overloads>
        /// <summary>
        /// Logs a message object with the <see cref="Level.Info"/> level.
        /// </summary>
        public static void Info(object message) { }

        /// <summary>
        /// Logs a message object with the <c>INFO</c> level including
        /// the stack trace of the <see cref="Exception"/> passed
        /// as a parameter.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        public static void Info(object message, Exception exception){ }

        /// <overloads>Log a formatted message string with the <see cref="Level.Info"/> level.</overloads>
        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Info"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void InfoFormat(string format, params object[] args){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Info"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        public static void InfoFormat(string format, object arg0){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Info"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        public static void InfoFormat(string format, object arg0, object arg1){ }


        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Info"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        /// <param name="arg2">An Object to format</param>
        public static void InfoFormat(string format, object arg0, object arg1, object arg2){ }


        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Info"/> level.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void InfoFormat(IFormatProvider provider, string format, params object[] args){ }

        /// <overloads>Log a message object with the <see cref="Level.Warn"/> level.</overloads>
        /// <summary>
        /// Log a message object with the <see cref="Level.Warn"/> level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        public static void Warn(object message){ }

        /// <summary>
        /// Log a message object with the <see cref="Level.Warn"/> level including
        /// the stack trace of the <see cref="Exception"/> passed
        /// as a parameter.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        public static void Warn(object message, Exception exception){ }

        /// <overloads>Log a formatted message string with the <see cref="Level.Warn"/> level.</overloads>
        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Warn"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void WarnFormat(string format, params object[] args){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Warn"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        public static void WarnFormat(string format, object arg0){ }


        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Warn"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        public static void WarnFormat(string format, object arg0, object arg1){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Warn"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        /// <param name="arg2">An Object to format</param>
        public static void WarnFormat(string format, object arg0, object arg1, object arg2){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Warn"/> level.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void WarnFormat(IFormatProvider provider, string format, params object[] args){ }

        /// <overloads>Log a message object with the <see cref="Level.Error"/> level.</overloads>
        /// <summary>
        /// Logs a message object with the <see cref="Level.Error"/> level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        public static void Error(object message){ }

        /// <summary>
        /// Log a message object with the <see cref="Level.Error"/> level including
        /// the stack trace of the <see cref="Exception"/> passed
        /// as a parameter.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        public static void Error(object message, Exception exception){ }

        /// <overloads>Log a formatted message string with the <see cref="Level.Error"/> level.</overloads>
        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Error"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void ErrorFormat(string format, params object[] args){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Error"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        public static void ErrorFormat(string format, object arg0){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Error"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        public static void ErrorFormat(string format, object arg0, object arg1){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Error"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        /// <param name="arg2">An Object to format</param>
        public static void ErrorFormat(string format, object arg0, object arg1, object arg2){ }
        
        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Error"/> level.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void ErrorFormat(IFormatProvider provider, string format, params object[] args){ }
        
        /// <overloads>Log a message object with the <see cref="Level.Fatal"/> level.</overloads>
        /// <summary>
        /// Log a message object with the <see cref="Level.Fatal"/> level.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        public static void Fatal(object message){ }

        /// <summary>
        /// Log a message object with the <see cref="Level.Fatal"/> level including
        /// the stack trace of the <see cref="Exception"/> passed
        /// as a parameter.
        /// </summary>
        /// <param name="message">The message object to log.</param>
        /// <param name="exception">The exception to log, including its stack trace.</param>
        public static void Fatal(object message, Exception exception){ }
        
        /// <overloads>Log a formatted message string with the <see cref="Level.Fatal"/> level.</overloads>
        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void FatalFormat(string format, params object[] args){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        public static void FatalFormat(string format, object arg0){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        public static void FatalFormat(string format, object arg0, object arg1){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
        /// </summary>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="arg0">An Object to format</param>
        /// <param name="arg1">An Object to format</param>
        /// <param name="arg2">An Object to format</param>
        public static void FatalFormat(string format, object arg0, object arg1, object arg2){ }

        /// <summary>
        /// Logs a formatted message string with the <see cref="Level.Fatal"/> level.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information</param>
        /// <param name="format">A String containing zero or more format items</param>
        /// <param name="args">An Object array containing zero or more objects to format</param>
        public static void FatalFormat(IFormatProvider provider, string format, params object[] args){ }

        /// <summary>
        /// Checks if this logger is enabled for the <see cref="Level.Debug"/> level.
        /// </summary>
        /// <value>
        /// <c>true</c> if this logger is enabled for <see cref="Level.Debug"/> events, <c>false</c> otherwise.
        /// </value>
        public static bool IsDebugEnabled { get; }

        /// <summary>
        /// Checks if this logger is enabled for the <see cref="Level.Info"/> level.
        /// </summary>
        /// <value>
        /// <c>true</c> if this logger is enabled for <see cref="Level.Info"/> events, <c>false</c> otherwise.
        /// </value>
        public static bool IsInfoEnabled { get; }

        /// <summary>
        /// Checks if this logger is enabled for the <see cref="Level.Warn"/> level.
        /// </summary>
        /// <value>
        /// <c>true</c> if this logger is enabled for <see cref="Level.Warn"/> events, <c>false</c> otherwise.
        /// </value>
        public static bool IsWarnEnabled { get; }

        /// <summary>
        /// Checks if this logger is enabled for the <see cref="Level.Error"/> level.
        /// </summary>
        /// <value>
        /// <c>true</c> if this logger is enabled for <see cref="Level.Error"/> events, <c>false</c> otherwise.
        /// </value>
        public static bool IsErrorEnabled { get; }
        
        /// <summary>
        /// Checks if this logger is enabled for the <see cref="Level.Fatal"/> level.
        /// </summary>
        /// <value>
        /// <c>true</c> if this logger is enabled for <see cref="Level.Fatal"/> events, <c>false</c> otherwise.
        /// </value>
        public static bool IsFatalEnabled { get; }
    }
}
