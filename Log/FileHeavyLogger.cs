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
using System.IO;
using System.Threading;
using log4net;
using At.Pkgs.Util;

/*
 * Architector Inc. All rights reserved.
 * Written by sotaro.suzuki@architector.jp
 */

namespace At.Pkgs.Util.Log
{

    /// <summary>
    /// Log4Netを使用してログを出力するクラスです。
    /// </summary>
    /// <remarks>
    /// このクラスのインスタンスメソッドはスレッドセーフです。
    /// このクラスは厳密な排他制御を行なうためログの出力は低速です。
    /// </remarks>
    /// <example>
    /// このクラスの使用例です。
    /// <code>
    /// class Program
    /// {
    /// 
    ///     protected FileHeavyLogger logger;
    /// 
    ///     protected Program()
    ///     {
    ///         this.logger = new FileHeavyLogger("c:\\log.txt", "Test Log");
    ///     }
    /// 
    ///     public static void Main()
    ///     {
    ///         Program program;
    ///         
    ///         program = new Program();
    ///         program.logger.Info("start");
    ///         try
    ///         {
    ///             program.Run();
    ///         }
    ///         catch (Exception throwable)
    ///         {
    ///             program.logger.Error(throwable, "エラーが発生しました。");
    ///         }
    ///         program.logger.Info("stop");
    ///     }
    /// 
    /// }
    /// </code>
    /// </example>
    public class FileHeavyLogger : Logger
    {

        private string _path;
        private ILog _logger;
        private InterProcessLock _lock;

        protected void ConfigureLogger()
        {
            log4net.Layout.ILayout layout;
            log4net.Appender.FileAppender appender;

            layout = new log4net.Layout.PatternLayout("\v%date{ISO8601} %logger %-5level: %message%newline");
            appender = new log4net.Appender.FileAppender();
            appender.AppendToFile = true;
            appender.Encoding = Encoding.UTF8;
            appender.File = this._path;
            appender.ImmediateFlush = true;
            appender.Layout = layout;
            appender.LockingModel = new log4net.Appender.FileAppender.MinimalLock();
            appender.Name = "FileAppender";
            appender.SecurityContext = new log4net.Util.WindowsSecurityContext();
            appender.Threshold = log4net.Core.Level.All;
            log4net.Config.BasicConfigurator.Configure(appender);
        }

        protected FileHeavyLogger(ILog logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// ログを出力先を指定して、<c>FileHeavyLogger</c>クラスの新規インスタンスを初期化します。
        /// </summary>
        /// <param name="path">ログの出力先ファイルパス</param>
        /// <param name="name">ログ名</param>
        /// <param name="lock_">ログの排他制御に使用するロック</param>
        public FileHeavyLogger(string path, string name, InterProcessLock lock_)
        {
            this._path = path;
            this.ConfigureLogger();
            this._logger = LogManager.GetLogger(name);
            this._lock = lock_;
        }

        /// <summary>
        /// ログを出力先を指定して、<c>FileHeavyLogger</c>クラスの新規インスタンスを初期化します。
        /// </summary>
        /// <param name="path">ログの出力先ファイルパス</param>
        /// <param name="name">ログ名</param>
        public FileHeavyLogger(string path, string name)
        {
            this._path = path;
            this.ConfigureLogger();
            this._logger = LogManager.GetLogger(name);
            this._lock = null;
        }

        #region lock methods
        protected bool AcquireLock()
        {
            try
            {
                if (this._lock != null) return this._lock.AcquireLock();
                else return true;
            }
            catch
            {
#if DEBUG
                throw;
#else
                return false;
#endif
            }
        }

        protected void ReleaseLock()
        {
            try
            {
                if (this._lock != null) this._lock.ReleaseLock();
            }
            catch
            {
#if DEBUG
                throw;
#else
                return;
#endif
            }
        }
        #endregion //lock methods

        /// <summary>
        /// ログの出力先ファイルサイズが指定以上であればログファイルを切り詰めます。
        /// </summary>
        /// <param name="limit">ログファイル切り詰め実行ボーダーサイズ</param>
        /// <param name="length">ログファイル切り詰め後のサイズ</param>
        public void Truncate(long limit, long length)
        {
            FileInfo info;

            if (!this.AcquireLock()) return;
            try
            {
                if (!File.Exists(this._path)) return;
                info = new FileInfo(this._path);
                info.Refresh();
                if (info.Length < limit) return;
                this.Debug("start log file truncate");
                string tmp = null;
                Stream output = null;
                StreamWriter writer = null;
                Stream input = null;
                StreamReader reader = null;

                try
                {
                    File.Copy(this._path, this._path + ".tmp");
                    tmp = this._path + ".tmp";
                    output = new FileStream(this._path, FileMode.Create, FileAccess.Write, FileShare.Read);
                    writer = new StreamWriter(output, Encoding.UTF8);
                    input = File.OpenRead(tmp);
                    input.Seek(info.Length - length, SeekOrigin.Begin);
                    reader = new StreamReader(input, Encoding.UTF8);
                    while (!reader.EndOfStream)
                    {
                        reader.ReadLine();
                        if (!reader.EndOfStream && reader.Peek() == '\v') break;
                    }
                    while (!reader.EndOfStream)
                    {
                        writer.WriteLine(reader.ReadLine());
                    }
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                        input = null;
                    }
                    if (input != null)
                    {
                        input.Close();
                    }
                    if (tmp != null)
                    {
                        File.Delete(tmp);
                    }
                    if (writer != null)
                    {
                        writer.Close();
                        output = null;
                    }
                    if (output != null)
                    {
                        output.Close();
                    }
                }
                this.Info("log file truncated");
            }
            catch (Exception throwable)
            {
                this.Error(throwable, "failed on truncate log file {0}", this._path);
#if DEBUG
                throw;
#endif
            }
            finally
            {
                this.ReleaseLock();
            }
        }

        /// <summary>
        /// 名前付きロガーを新規に取得します。
        /// </summary>
        /// <param name="name">ログ名</param>
        /// <returns>新しいロガー</returns>
        public ILogger CrerateNamedLogger(string name)
        {
            FileHeavyLogger logger;

            logger = new FileHeavyLogger(LogManager.GetLogger(name));
            logger._lock = this._lock;
            return logger;
        }

        protected override void WriteLog(LogLevel level, string message)
        {
            if (!this.AcquireLock()) return;
            try
            {
                switch (level)
                {
                    case LogLevel.Debug:
                        this._logger.Debug(message);
                        break;
                    case LogLevel.Information:
                        this._logger.Info(message);
                        break;
                    case LogLevel.Warning:
                        this._logger.Warn(message);
                        break;
                    case LogLevel.Error:
                        this._logger.Error(message);
                        break;
                    case LogLevel.Fatal:
                        this._logger.Fatal(message);
                        break;
                }
            }
            finally
            {
                this.ReleaseLock();
            }
        }

    }

}
