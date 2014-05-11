package global;

import java.io.Serializable;

public class Message
{
	public boolean isHeader;
	public int size;
	
	public Message()
	{
		reset();
	}
	
	public void reset()
	{
		this.isHeader = true;
		this.size = Header.SIZE;
	}
	
	@SuppressWarnings("serial")
	public static class Header implements Serializable
	{
		public static int SIZE = Util.object2ByteArray(new Header()).length;
		public static int SEED = 0;
		
		public int sequence;
		public int bodyLength;
		
		@Override
		public String toString()
		{
			return String.format("head -> seq: %d, body_len: %d", sequence, bodyLength);
		}
	}
	
	@SuppressWarnings("serial")
	public static class Body implements Serializable
	{
		public String name;
		public int age;
		public String desc;
		
		public Body()
		{
			
		}

		public Body(String name, int age, String desc)
		{
			this.name = name;
			this.age = age;
			this.desc = desc;
		}
		
		@Override
		public String toString()
		{
			return String.format("body -> name: %s, age: %d, desc: %s", name, age, desc);
		}
		
	}
	
	public static byte[] toByteArray(Header header, Body body)
	{
		byte[] b = Util.object2ByteArray(body);
		
		header.sequence		= ++Header.SEED;
		header.bodyLength	= b.length;
		
		byte[] h = Util.object2ByteArray(header);
		byte[] m = new byte[h.length + b.length];
		
		System.arraycopy(h, 0, m, 0, h.length);
		System.arraycopy(b, 0, m, h.length, b.length);
		
		return m;
	}
}
