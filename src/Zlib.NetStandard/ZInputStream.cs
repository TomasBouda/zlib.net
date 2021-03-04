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
namespace ComponentAce.Compression.Libs.Zlib
{

	public class ZInputStream : System.IO.BinaryReader
	{
		internal void InitBlock()
		{
			Flush = ZlibConst.Z_NO_FLUSH;
			Buf = new byte[Bufsize];
		}
		public virtual int FlushMode
		{
			get => (Flush);

			set => Flush = value;
		}
		/// <summary> Returns the total number of bytes input so far.</summary>
		public virtual long TotalIn => Z.total_in;

		/// <summary> Returns the total number of bytes output so far.</summary>
		public virtual long TotalOut => Z.total_out;

		protected ZStream Z = new ZStream();
		protected int Bufsize = 512;
		protected int Flush;
		protected byte[] Buf, Buf1 = new byte[1];
		protected bool Compress;

		internal System.IO.Stream In_Renamed = null;

		public ZInputStream(System.IO.Stream inRenamed) : base(inRenamed)
		{
			InitBlock();
			this.In_Renamed = inRenamed;
			Z.inflateInit();
			Compress = false;
			Z.next_in = Buf;
			Z.next_in_index = 0;
			Z.avail_in = 0;
		}

		public ZInputStream(System.IO.Stream inRenamed, int level) : base(inRenamed)
		{
			InitBlock();
			this.In_Renamed = inRenamed;
			Z.deflateInit(level);
			Compress = true;
			Z.next_in = Buf;
			Z.next_in_index = 0;
			Z.avail_in = 0;
		}

		/*public int available() throws IOException {
		return inf.finished() ? 0 : 1;
		}*/

		public override int Read()
		{
			if (read(Buf1, 0, 1) == -1)
				return (-1);
			return (Buf1[0] & 0xFF);
		}

		internal bool Nomoreinput = false;

		public int read(byte[] b, int off, int len)
		{
			if (len == 0)
				return (0);
			int err;
			Z.next_out = b;
			Z.next_out_index = off;
			Z.avail_out = len;
			do
			{
				if ((Z.avail_in == 0) && (!Nomoreinput))
				{
					// if buffer is empty and more input is avaiable, refill it
					Z.next_in_index = 0;
					Z.avail_in = SupportClass.ReadInput(In_Renamed, Buf, 0, Bufsize); //(bufsize<z.avail_out ? bufsize : z.avail_out));
					if (Z.avail_in == -1)
					{
						Z.avail_in = 0;
						Nomoreinput = true;
					}
				}
				if (Compress)
					err = Z.deflate(Flush);
				else
					err = Z.inflate(Flush);
				if (Nomoreinput && (err == ZlibConst.Z_BUF_ERROR))
					return (-1);
				if (err != ZlibConst.Z_OK && err != ZlibConst.Z_STREAM_END)
					throw new ZStreamException((Compress ? "de" : "in") + "flating: " + Z.msg);
				if (Nomoreinput && (Z.avail_out == len))
					return (-1);
			}
			while (Z.avail_out == len && err == ZlibConst.Z_OK);
			//System.err.print("("+(len-z.avail_out)+")");
			return (len - Z.avail_out);
		}

		public long Skip(long n)
		{
			int len = 512;
			if (n < len)
				len = (int)n;
			byte[] tmp = new byte[len];
			return ((long)SupportClass.ReadInput(BaseStream, tmp, 0, tmp.Length));
		}

		protected override void Dispose(bool disposing)
		//!public override void  Close()
		{
			if (disposing)
			{
				//!in_Renamed.Close();
				In_Renamed.Dispose();
			}
		}
	}
}