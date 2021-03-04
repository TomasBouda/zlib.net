// Copyright (c) 2006, ComponentAce
// http://www.componentace.com
// All rights reserved.

// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

// Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer. 
// Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution. 
// Neither the name of ComponentAce nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission. 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.


/*
Copyright (c) 2001 Lapo Luchini.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright 
notice, this list of conditions and the following disclaimer in 
the documentation and/or other materials provided with the distribution.

3. The names of the authors may not be used to endorse or promote products
derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED ``AS IS'' AND ANY EXPRESSED OR IMPLIED WARRANTIES,
INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS
OR ANY CONTRIBUTORS TO THIS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
/*
* This program is based on zlib-1.1.3, so all credit should go authors
* Jean-loup Gailly(jloup@gzip.org) and Mark Adler(madler@alumni.caltech.edu)
* and contributors of zlib.
*/
using System;

namespace ComponentAce.Compression.Libs.Zlib
{

	public class ZOutputStream : System.IO.Stream, IDisposable
	{
		private void InitBlock()
		{
			Flush_Renamed_Field = ZlibConst.Z_NO_FLUSH;
			Buf = new byte[Bufsize];
		}

		public virtual int FlushMode
		{
			get => (Flush_Renamed_Field);

			set => Flush_Renamed_Field = value;
		}

		/// <summary> Returns the total number of bytes input so far.</summary>
		public virtual long TotalIn => Z.total_in;

		/// <summary> Returns the total number of bytes output so far.</summary>
		public virtual long TotalOut => Z.total_out;

		protected internal ZStream Z = new ZStream();
		protected internal int Bufsize = 4096;
		protected internal int Flush_Renamed_Field;
		protected internal byte[] Buf, Buf1 = new byte[1];
		protected internal bool Compress;

		private System.IO.Stream _outRenamed;

		public ZOutputStream(System.IO.Stream outRenamed) : base()
		{
			InitBlock();
			this._outRenamed = outRenamed;
			Z.inflateInit();
			Compress = false;
		}

		public ZOutputStream(System.IO.Stream outRenamed, int level) : base()
		{
			InitBlock();
			this._outRenamed = outRenamed;
			Z.deflateInit(level);
			Compress = true;
		}

		public void WriteByte(int b)
		{
			Buf1[0] = (byte)b;
			Write(Buf1, 0, 1);
		}

		//UPGRADE_TODO: The differences in the Expected value  of parameters for method 'WriteByte'  may cause compilation errors.  'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1092_3"'
		public override void WriteByte(byte b)
		{
			WriteByte((int)b);
		}

		public override void Write(Byte[] b1, int off, int len)
		{
			if (len == 0)
			{
				return;
			}

			int err;
			byte[] b = new byte[b1.Length];
			Array.Copy(b1, 0, b, 0, b1.Length);
			Z.next_in = b;
			Z.next_in_index = off;
			Z.avail_in = len;
			do
			{
				Z.next_out = Buf;
				Z.next_out_index = 0;
				Z.avail_out = Bufsize;
				if (Compress)
				{
					err = Z.deflate(Flush_Renamed_Field);
				}
				else
				{
					err = Z.inflate(Flush_Renamed_Field);
				}

				if (err != ZlibConst.Z_OK && err != ZlibConst.Z_STREAM_END)
				{
					throw new ZStreamException((Compress ? "de" : "in") + "flating: " + Z.msg);
				}

				_outRenamed.Write(Buf, 0, Bufsize - Z.avail_out);
			}
			while (Z.avail_in > 0 || Z.avail_out == 0);
		}

		public virtual void Finish()
		{
			int err;
			do
			{
				Z.next_out = Buf;
				Z.next_out_index = 0;
				Z.avail_out = Bufsize;
				if (Compress)
				{
					err = Z.deflate(ZlibConst.Z_FINISH);
				}
				else
				{
					err = Z.inflate(ZlibConst.Z_FINISH);
				}
				if (err != ZlibConst.Z_STREAM_END && err != ZlibConst.Z_OK)
				{
					throw new ZStreamException((Compress ? "de" : "in") + "flating: " + Z.msg);
				}

				if (Bufsize - Z.avail_out > 0)
				{
					_outRenamed.Write(Buf, 0, Bufsize - Z.avail_out);
				}
			}
			while (Z.avail_in > 0 || Z.avail_out == 0);
			try
			{
				Flush();
			}
			catch
			{
			}
		}
		public virtual void End()
		{
			if (Compress)
			{
				Z.deflateEnd();
			}
			else
			{
				Z.inflateEnd();
			}
			Z.free();
			Z = null;
		}

		protected override void Dispose(bool disposing)
		//!public override void Close()
		{
			if (disposing)
			{
				try
				{
					try
					{
						Finish();
					}
					catch
					{
					}
				}
				finally
				{
					End();
					//!out_Renamed.Close();
					_outRenamed.Dispose();
					_outRenamed = null;
				}
			}
		}

		public override void Flush()
		{
			_outRenamed.Flush();
		}
		//UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 count)
		{
			return 0;
		}
		//UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
		public override void SetLength(Int64 value)
		{
		}
		//UPGRADE_TODO: The following method was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
		public override Int64 Seek(Int64 offset, System.IO.SeekOrigin origin)
		{
			return 0;
		}
		//UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
		public override Boolean CanRead => false;

		//UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
		public override Boolean CanSeek => false;

		public override Boolean CanWrite => true;

		//UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
		public override Int64 Length => 0;

		//UPGRADE_TODO: The following property was automatically generated and it must be implemented in order to preserve the class logic. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1232_3"'
		public override Int64 Position
		{
			get => 0;

			set
			{
			}

		}
	}
}