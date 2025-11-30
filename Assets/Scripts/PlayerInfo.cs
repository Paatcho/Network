using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerInfo : INetworkSerializable, IEquatable<PlayerInfo>
{
    public int playerId;
    public FixedString32Bytes name;
    public int cheese;
    public int lives;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref cheese);
        serializer.SerializeValue(ref lives);
    }

    public bool Equals(PlayerInfo other)
    {
        return playerId == other.playerId;
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return playerId;
    }
}