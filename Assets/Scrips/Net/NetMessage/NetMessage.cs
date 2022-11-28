using Unity.Networking.Transport;
using UnityEngine;


public class NetMessage
{
    public OpCode Code { set; get; }

    public virtual void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }

    public virtual void Deserialize(DataStreamReader reader)
    {

    }
    public virtual void ReceiveOnClient()
    {

    }
    public virtual void ReceiveOnServer(NetworkConnection cnn) 
    {

    }
}
