namespace Facility.Core;

public abstract class ServiceDataEquivalenceStream : Stream
{
	public static ServiceDataEquivalenceStream Create(ReadOnlyMemory<byte> source) => new MemoryServiceDataEquivalenceStreamImpl(source);

	public abstract bool Equivalent { get; }

	private sealed class MemoryServiceDataEquivalenceStreamImpl(ReadOnlyMemory<byte> source) : ServiceDataEquivalenceStream
	{
		public override bool Equivalent => !m_notEquivalent && m_position == source.Length;

		public override bool CanRead => false;

		public override bool CanSeek => false;

		public override bool CanWrite => true;

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

		public override void SetLength(long value) => throw new NotSupportedException();

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (!m_notEquivalent && m_position + count <= source.Length && buffer.AsSpan(offset, count).SequenceEqual(source.Span.Slice(m_position, count)))
				m_position += count;
			else
				m_notEquivalent = true;
		}

		private int m_position;
		private bool m_notEquivalent;
	}
}
