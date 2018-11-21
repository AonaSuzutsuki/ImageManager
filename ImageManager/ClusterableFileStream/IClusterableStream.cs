using System;
using System.IO;

namespace Clusterable.IO
{
    /// <summary>
    /// 分割したファイルあるいは単一ファイルのアクセスを提供します。
    /// </summary>
    public interface IClusterableStream : IDisposable
    {
        /// <summary>
        /// 内部で使用するファイル群のパスを取得します。
        /// </summary>
        string[] Filenames { get; }

        /// <summary>
        /// 分割ファイルを一つのファイルとみなした仮想的なファイル合計サイズを取得します。
        /// </summary>
        /// <value>仮想的なファイルサイズ</value>
        long Length { get; }

        /// <summary>
        /// 分割ファイルを一つのファイルとみなした仮想的なストリームの現在位置を取得します。
        /// </summary>
        /// <value>現在の</value>
        long Position { get; }

        /// <summary>
        /// 分割するファイルサイズをバイト単位で取得します。
        /// </summary>
        /// <value>分割するファイルサイズ</value>
        long SplitSize { get; }

        /// <summary>
        /// ストリームに関連するファイル群を削除します。
        /// </summary>
        /// <returns>削除したファイルの名前を指定します。</returns>
        /// <param name="func">分割ファイルを削除する度に実行する処理を指定します。</param>
        string[] Delete(Func<string, string> func = null);

        /// <summary>
        /// ストリームからバイトのブロックを読み込みます。
        /// </summary>
        /// <param name="buffer">データを書き込むバッファ領域を指定します。</param>
        /// <param name="offset">読み込みを開始する現在地からの相対位置を指定します。</param>
        /// <param name="length">読み込む長さを指定します。</param>
        /// <returns>実際に読み込んだバイトサイズ</returns>
        int Read(byte[] buffer, int offset, int length);

        /// <summary>
        /// ストリームの現在地を指定します。
        /// </summary>
        /// <param name="offset">SeekOriginに基づいた相対的なストリームの位置を指定します。</param>
        /// <param name="seekOrigin">オフセットをどこから開始するかを指定します。</param>
        void Seek(long offset, SeekOrigin seekOrigin);

        /// <summary>
        /// ストリームにバイトのブロックを書き込みます。
        /// </summary>
        /// <param name="data">書き込むデータを指定します。</param>
        /// <param name="offset">書き込みを開始する現在地からの相対位置を指定します。</param>
        /// <param name="length">書き込むデータの長さを指定します。</param>
        void Write(byte[] data, int offset, long length);
    }
}