/*
 * Copyright (c) 2009-2013, Architector Inc., Japan
 * All rights reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

/*
 * Architector Inc. All rights reserved.
 * Written by sotaro.suzuki@architector.jp
 */

namespace At.Pkgs.Util.Log
{

    /// <summary>
    /// ロガーの抽象実装です。
    /// </summary>
    public abstract class Logger : ILogger
    {

        #region LogLevel
        /// <summary>
        /// ログレベルを示す列挙子です。
        /// </summary>
        public enum LogLevel
        {

            /// <summary>
            /// デバッグ情報を示します。
            /// </summary>
            Debug,

            /// <summary>
            /// 情報を示します。
            /// </summary>
            Information,

            /// <summary>
            /// 警告を示します。
            /// </summary>
            Warning,

            /// <summary>
            /// 一般エラーを示します。
            /// </summary>
            Error,

            /// <summary>
            /// 致命的なエラーを示します。
            /// </summary>
            Fatal

        }
        #endregion //LogLevel

        /// <summary>
        /// ログに例外が追加される最大のネストレベルです。
        /// </summary>
        protected const int AppendExceptionMaximumNestLevel = 10;

        private LogLevel _level = LogLevel.Information;

        /// <summary>
        /// 指定されたログレベルが出力として有効であるかを返します。
        /// </summary>
        /// <param name="level">ログレベル</param>
        /// <returns>出力対象ならtrue</returns>
        public bool IsEnabled(LogLevel level)
        {
            return level >= this._level;
        }

        /// <summary>
        /// 出力対象の最小ログレベルを取得または設定します。
        /// </summary>
        public LogLevel Level
        {
            get { return this._level; }
            set { this._level = value; }
        }

        /// <summary>
        /// ログを出力します。このメソッドは実装クラスにオーバーライドされます。
        /// </summary>
        /// <param name="level">ログレベル</param>
        /// <param name="message">ログメッセージ</param>
        protected abstract void WriteLog(LogLevel level, string message);

        #region helper methods

        protected void AppendException(StringBuilder builder, Exception throwable)
        {
            for (int i = 0; throwable != null && i < AppendExceptionMaximumNestLevel; i++)
            {
                builder.Append("Exception: ");
                builder.AppendLine(throwable.GetType().FullName);
                builder.Append("Message: ");
                builder.AppendLine(throwable.Message);
                builder.Append("Source: ");
                builder.AppendLine(throwable.Source);
                builder.AppendLine("StackTrace:");
                builder.AppendLine(throwable.StackTrace);
                throwable = throwable.InnerException;
            }
        }

        #endregion //helper methods

        #region Log method

        /// <summary>
        /// ログを出力します。
        /// </summary>
        /// <remarks>
        /// メッセージには"プロセス名(プロセスID)-マネージスレッドID(ネイティブスレッドID): "が先頭に付与されます。
        /// </remarks>
        /// <param name="depth">ログの呼び出し元からのコールスタックの深さ</param>
        /// <param name="level">ログレベル</param>
        /// <param name="message">メッセージ</param>
        protected void Log(int depth, LogLevel level, string message)
        {
            if (!this.IsEnabled(level)) return;
            try
            {
                {
                    Process process;
                    int nativeThreadId;
                    int managedThreadId;
                    StackFrame frame;
                    string fileName;
                    int fileLineNumber;
                    int fileColumnNumber;
                    MethodBase method;
                    string className;
                    string methodName;

                    process = Process.GetCurrentProcess();
#pragma warning disable 618
                    nativeThreadId = AppDomain.GetCurrentThreadId();
#pragma warning restore 618
                    managedThreadId = Thread.CurrentThread.ManagedThreadId;
                    frame = new StackFrame(++depth);
                    fileName = frame.GetFileName();
                    if (fileName == null) fileName = "{unknown}";
                    fileLineNumber = frame.GetFileLineNumber();
                    fileColumnNumber = frame.GetFileColumnNumber();
                    method = frame.GetMethod();
                    className = method.DeclaringType.FullName;
                    methodName = method.Name;
                    if (".ctor".Equals(methodName)) methodName = "{constructor}";
                    message = String.Format(
                        "{0}({1:D})-{2:D}({3:D}): {4}::{5} ({6}:{7}-{8})\r\n{9}",
                        process.ProcessName, process.Id, managedThreadId, nativeThreadId,
                        className, methodName, fileName, fileLineNumber, fileColumnNumber,
                        message);
                }
                this.WriteLog(level, message);
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        /// ログを出力します。
        /// </summary>
        /// <param name="depth">ログの呼び出し元からのコールスタックの深さ</param>
        /// <param name="level">ログレベル</param>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        protected void Log(int depth, LogLevel level, string format, params object[] args)
        {
            if (!this.IsEnabled(level)) return;
            try
            {
                this.Log(++depth, level, String.Format(format, args));
            }
            catch (FormatException)
            {
                this.Log(++depth, level, format);
#if DEBUG
                throw;
#endif
            }
            catch (ArgumentNullException)
            {
                this.Log(++depth, level, format);
#if DEBUG
                throw;
#endif
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        /// ログを出力します。
        /// </summary>
        /// <param name="depth">ログの呼び出し元からのコールスタックの深さ</param>
        /// <param name="level">ログレベル</param>
        /// <param name="cause">原因となった例外</param>
        /// <param name="message">メッセージ</param>
        protected void Log(int depth, LogLevel level, Exception cause, string message)
        {
            if (!this.IsEnabled(level)) return;
            try
            {
                StringBuilder builder;

                builder = new StringBuilder();
                builder.AppendLine(message);
                this.AppendException(builder, cause);
                this.Log(++depth, level, builder.ToString());
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        /// <summary>
        /// ログを出力します。
        /// </summary>
        /// <param name="depth">ログの呼び出し元からのコールスタックの深さ</param>
        /// <param name="level">ログレベル</param>
        /// <param name="cause">原因となった例外</param>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        protected void Log(int depth, LogLevel level, Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(level)) return;
            try
            {
                StringBuilder builder;

                builder = new StringBuilder();
                builder.AppendFormat(format, args);
                builder.AppendLine();
                this.AppendException(builder, cause);
                this.Log(++depth, level, builder.ToString());
            }
            catch (FormatException)
            {
                this.Log(++depth, level, cause, format);
#if DEBUG
                throw;
#endif
            }
            catch (ArgumentNullException)
            {
                this.Log(++depth, level, cause, format);
#if DEBUG
                throw;
#endif
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#endif
            }
        }

        #endregion //Log method

        #region Debug method

        public bool DebugEnabled
        {
            get { return this.IsEnabled(LogLevel.Debug); }
        }

        public void Debug(string message)
        {
            if (!this.IsEnabled(LogLevel.Debug)) return;
            this.Log(1, LogLevel.Debug, message);
        }

        public void Debug(string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Debug)) return;
            this.Log(1, LogLevel.Debug, format, args);
        }

        public void Debug(Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Debug)) return;
            this.Log(1, LogLevel.Debug, cause, message);
        }

        public void Debug(Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Debug)) return;
            this.Log(1, LogLevel.Debug, cause, format, args);
        }

        public void Debug(int depth, string message)
        {
            if (!this.IsEnabled(LogLevel.Debug)) return;
            this.Log(++depth, LogLevel.Debug, message);
        }

        public void Debug(int depth, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Debug)) return;
            this.Log(++depth, LogLevel.Debug, format, args);
        }

        public void Debug(int depth, Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Debug)) return;
            this.Log(++depth, LogLevel.Debug, cause, message);
        }

        public void Debug(int depth, Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Debug)) return;
            this.Log(++depth, LogLevel.Debug, cause, format, args);
        }
        #endregion //Debug method

        #region Info method

        public bool InfoEnabled
        {
            get { return this.IsEnabled(LogLevel.Information); }
        }

        public void Info(string message)
        {
            if (!this.IsEnabled(LogLevel.Information)) return;
            this.Log(1, LogLevel.Information, message);
        }

        public void Info(string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Information)) return;
            this.Log(1, LogLevel.Information, format, args);
        }

        public void Info(Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Information)) return;
            this.Log(1, LogLevel.Information, cause, message);
        }

        public void Info(Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Information)) return;
            this.Log(1, LogLevel.Information, cause, format, args);
        }

        public void Info(int depth, string message)
        {
            if (!this.IsEnabled(LogLevel.Information)) return;
            this.Log(++depth, LogLevel.Information, message);
        }

        public void Info(int depth, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Information)) return;
            this.Log(++depth, LogLevel.Information, format, args);
        }

        public void Info(int depth, Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Information)) return;
            this.Log(++depth, LogLevel.Information, cause, message);
        }

        public void Info(int depth, Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Information)) return;
            this.Log(++depth, LogLevel.Information, cause, format, args);
        }

        #endregion //Info method

        #region Warn method

        public bool WarnEnabled
        {
            get { return this.IsEnabled(LogLevel.Warning); }
        }

        public void Warn(string message)
        {
            if (!this.IsEnabled(LogLevel.Warning)) return;
            this.Log(1, LogLevel.Warning, message);
        }

        public void Warn(string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Warning)) return;
            this.Log(1, LogLevel.Warning, format, args);
        }

        public void Warn(Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Warning)) return;
            this.Log(1, LogLevel.Warning, cause, message);
        }

        public void Warn(Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Warning)) return;
            this.Log(1, LogLevel.Warning, cause, format, args);
        }

        public void Warn(int depth, string message)
        {
            if (!this.IsEnabled(LogLevel.Warning)) return;
            this.Log(++depth, LogLevel.Warning, message);
        }

        public void Warn(int depth, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Warning)) return;
            this.Log(++depth, LogLevel.Warning, format, args);
        }

        public void Warn(int depth, Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Warning)) return;
            this.Log(++depth, LogLevel.Warning, cause, message);
        }

        public void Warn(int depth, Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Warning)) return;
            this.Log(++depth, LogLevel.Warning, cause, format, args);
        }

        #endregion //Warn method

        #region Error method

        public bool ErrorEnabled
        {
            get { return this.IsEnabled(LogLevel.Error); }
        }

        public void Error(string message)
        {
            if (!this.IsEnabled(LogLevel.Error)) return;
            this.Log(1, LogLevel.Error, message);
        }

        public void Error(string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Error)) return;
            this.Log(1, LogLevel.Error, format, args);
        }

        public void Error(Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Error)) return;
            this.Log(1, LogLevel.Error, cause, message);
        }

        public void Error(Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Error)) return;
            this.Log(1, LogLevel.Error, cause, format, args);
        }

        public void Error(int depth, string message)
        {
            if (!this.IsEnabled(LogLevel.Error)) return;
            this.Log(++depth, LogLevel.Error, message);
        }

        public void Error(int depth, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Error)) return;
            this.Log(++depth, LogLevel.Error, format, args);
        }

        public void Error(int depth, Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Error)) return;
            this.Log(++depth, LogLevel.Error, cause, message);
        }

        public void Error(int depth, Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Error)) return;
            this.Log(++depth, LogLevel.Error, cause, format, args);
        }

        #endregion //Error method

        #region Fatal method

        public bool FatalEnabled
        {
            get { return this.IsEnabled(LogLevel.Fatal); }
        }

        public void Fatal(string message)
        {
            if (!this.IsEnabled(LogLevel.Fatal)) return;
            this.Log(1, LogLevel.Fatal, message);
        }

        public void Fatal(string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Fatal)) return;
            this.Log(1, LogLevel.Fatal, format, args);
        }

        public void Fatal(Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Fatal)) return;
            this.Log(1, LogLevel.Fatal, cause, message);
        }

        public void Fatal(Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Fatal)) return;
            this.Log(1, LogLevel.Fatal, cause, format, args);
        }

        public void Fatal(int depth, string message)
        {
            if (!this.IsEnabled(LogLevel.Fatal)) return;
            this.Log(++depth, LogLevel.Fatal, message);
        }

        public void Fatal(int depth, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Fatal)) return;
            this.Log(++depth, LogLevel.Fatal, format, args);
        }

        public void Fatal(int depth, Exception cause, string message)
        {
            if (!this.IsEnabled(LogLevel.Fatal)) return;
            this.Log(++depth, LogLevel.Fatal, cause, message);
        }

        public void Fatal(int depth, Exception cause, string format, params object[] args)
        {
            if (!this.IsEnabled(LogLevel.Fatal)) return;
            this.Log(++depth, LogLevel.Fatal, cause, format, args);
        }

        #endregion //Fatal method

    }

}
