using System;
using System.Threading;

namespace ServerCore
{

    public class SendBuffer
    {
        byte[] _buffer;
        int _usedSize = 0;

        public int FreeSize { get { return _buffer.Length - _usedSize; } }

        public SendBuffer(int chunckSize)
        {
            _buffer = new byte[chunckSize];
        }

        /// <summary>
        /// 버퍼 를 사용한다
        /// </summary>
        /// <param name="reserverSize"></param>
        /// <returns></returns>
        public ArraySegment<byte> Open(int reserveSize)
        {
            // 쓰려는 공간이 더 크면 null

            return new ArraySegment<byte>(_buffer, _usedSize, reserveSize);

        }

        /// <summary>
        /// 버퍼 사용 다 했 다 
        /// </summary>
        /// <param name="usedSized"></param>
        /// <returns></returns>
        public ArraySegment<byte> Close(int usedSized)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(_buffer, _usedSize, usedSized);

            _usedSize += usedSized;
            return segment;
        }

    }
}