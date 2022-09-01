using System;
namespace ServerCore
{
    public class RecvBuffer
    {
        ArraySegment<byte> _buffer;
        int _readPos;
        int _writePos;
        // pos 는 각각 마우스 커서 역할
        // rpos와 wpos를 뒤로 쭉쭉 밀면서 쓰다가 다시 0 자리로 초기화 하기도 

        public RecvBuffer(int bufferSize)
        {
            _buffer = new ArraySegment<byte>(new byte[bufferSize], 0, bufferSize);
        }

        public int DataSize { get { return _writePos - _readPos; } }
        // 아직 처리되지 않은 데이터의 크기
        public int FreeSize { get { return _buffer.Count - _writePos; } }
        // 버퍼의 남은 공간


        /// <summary>
        /// 읽어야 할 데이터의 범위
        /// </summary>
        public ArraySegment<byte> ReadSegment
        {

            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _readPos, DataSize); }
        }

        /// <summary>
        /// 받을 수 있는 데이터의 유효 범위
        /// </summary>
        public ArraySegment<byte> WriteSegment
        {
            get { return new ArraySegment<byte>(_buffer.Array, _buffer.Offset + _writePos, FreeSize); }
        }


        /// <summary>
        /// r이랑 w를 정리해주는 함수
        /// </summary>
        public void Clean()
        {
            // r이랑 w가 다를 경우 r과 w사이의 값을 그대로 복사해 와야함
            // r이랑 w가 같을 경우 r과 w의 값을 0으로 올기면 됨

            int dataSize = DataSize;
            if (dataSize == 0)
            {
                // 남은 데이터가 없으면 복사하지 않고 커서 위치만 리셋
                _readPos = _writePos = 0;
            }
            else
            {
                // 남은 찌끄레기가 있으면 시작 위치로 복사
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, dataSize);
                _readPos = 0;
                _writePos = dataSize;
            }
        }



        /// <summary>
        /// 데이터 리시브가 성공적으로 됐으면 커서를 옮겨줌
        /// </summary>
        /// <param name="numOfBytes"></param>
        /// <returns></returns>
        public bool OnRead(int numOfBytes)
        {
            if (numOfBytes > DataSize)
                return false;

            _readPos += numOfBytes;
            return true;
        }


        public bool OnWrite(int numOfBytes)
        {
            if (numOfBytes > FreeSize)
                return false;

            _writePos += numOfBytes;
            return true;
        }
    }
}