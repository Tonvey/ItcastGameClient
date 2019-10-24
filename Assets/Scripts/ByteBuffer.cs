using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ByteBuffer
{
    private MemoryStream stream=null;
    private BinaryWriter writer=null;
    public ByteBuffer()
    {
        stream = new MemoryStream();
        writer = new BinaryWriter(stream);
    }
    public void WriteInt32(int v)
    {
        writer.Write(v);
    }
    public void WriteBytes(byte []v)
    {
        writer.Write(v);
    }
    public byte[] ToArray()
    {
        writer.Flush();
        return stream.ToArray();
    }
    ~ByteBuffer()
    {
        writer.Close();
        writer = null;
        stream.Close();
        stream = null;
    }
}
