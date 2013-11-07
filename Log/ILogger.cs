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

/*
 * Architector Inc. All rights reserved.
 * Written by sotaro.suzuki@architector.jp
 */

namespace At.Pkgs.Util.Log
{

    public interface ILogger
    {

        #region Debug method

        /// <summary>
        /// デバッグログが有効か取得します。
        /// </summary>
        bool DebugEnabled { get; }

        /// <summary>
        /// デバッグログを出力します。
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Debug(string message);

        /// <summary>
        /// デバッグログを出力します。
        /// </summary>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Debug(string format, params object[] args);

        /// <summary>
        /// デバッグログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="message">メッセージ</param>
        void Debug(Exception cause, string message);

        /// <summary>
        /// デバッグログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Debug(Exception cause, string format, params object[] args);

        #endregion //Debug method

        #region Info method

        /// <summary>
        /// 情報ログが有効か取得します。
        /// </summary>
        bool InfoEnabled { get; }

        /// <summary>
        /// 情報ログを出力します。
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Info(string message);

        /// <summary>
        /// 情報ログを出力します。
        /// </summary>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// 情報ログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="message">メッセージ</param>
        void Info(Exception cause, string message);

        /// <summary>
        /// 情報ログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Info(Exception cause, string format, params object[] args);

        #endregion //Info method

        #region Warn method

        /// <summary>
        /// 警告ログが有効か取得します。
        /// </summary>
        bool WarnEnabled { get; }

        /// <summary>
        /// 警告ログを出力します。
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Warn(string message);

        /// <summary>
        /// 警告ログを出力します。
        /// </summary>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Warn(string format, params object[] args);

        /// <summary>
        /// 警告ログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="message">メッセージ</param>
        void Warn(Exception cause, string message);

        /// <summary>
        /// 警告ログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Warn(Exception cause, string format, params object[] args);

        #endregion //Warn method

        #region Error method

        /// <summary>
        /// エラーログが有効か取得します。
        /// </summary>
        bool ErrorEnabled { get; }

        /// <summary>
        /// エラーログを出力します。
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Error(string message);

        /// <summary>
        /// エラーログを出力します。
        /// </summary>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Error(string format, params object[] args);

        /// <summary>
        /// エラーログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="message">メッセージ</param>
        void Error(Exception cause, string message);

        /// <summary>
        /// エラーログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Error(Exception cause, string format, params object[] args);

        #endregion //Error method

        #region Fatal method

        /// <summary>
        /// 致命的エラーログが有効か取得します。
        /// </summary>
        bool FatalEnabled { get; }

        /// <summary>
        /// 致命的エラーログを出力します。
        /// </summary>
        /// <param name="message">メッセージ</param>
        void Fatal(string message);

        /// <summary>
        /// 致命的エラーログを出力します。
        /// </summary>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Fatal(string format, params object[] args);

        /// <summary>
        /// 致命的エラーログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="message">メッセージ</param>
        void Fatal(Exception cause, string message);

        /// <summary>
        /// 致命的エラーログを出力します。
        /// </summary>
        /// <param name="cause">原因となった例外</param>
        /// <param name="format">メッセージフォーマット</param>
        /// <param name="args">フォーマットパラメタ</param>
        void Fatal(Exception cause, string format, params object[] args);

        #endregion //Fatal method

    }

}
