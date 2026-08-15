using System;
using Unity.Netcode;
using UnityEngine;

namespace InvadersOverboard.Networking.Player
{
    public struct NetworkPlayerPlatformState :
        INetworkSerializable,
        IEquatable<NetworkPlayerPlatformState>
    {
        public bool HasPlatform;

        public NetworkObjectReference Platform;

        public Vector3 LocalPosition;

        public float LocalYaw;


        public static NetworkPlayerPlatformState None =>
            new()
            {
                HasPlatform = false,
                LocalPosition = Vector3.zero,
                LocalYaw = 0f
            };


        public NetworkPlayerPlatformState(
            NetworkObject platform,
            Vector3 localPosition,
            float localYaw)
        {
            HasPlatform = true;

            Platform =
                new NetworkObjectReference(
                    platform);

            LocalPosition = localPosition;
            LocalYaw = localYaw;
        }


        public void NetworkSerialize<T>(
            BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(
                ref HasPlatform);

            if (!HasPlatform)
                return;

            serializer.SerializeValue(
                ref Platform);

            serializer.SerializeValue(
                ref LocalPosition);

            serializer.SerializeValue(
                ref LocalYaw);
        }


        public bool Equals(
            NetworkPlayerPlatformState other)
        {
            if (HasPlatform != other.HasPlatform)
                return false;

            if (!HasPlatform)
                return true;

            return Platform.Equals(other.Platform)
                   && LocalPosition.Equals(
                       other.LocalPosition)
                   && LocalYaw.Equals(
                       other.LocalYaw);
        }


        public override bool Equals(object obj)
        {
            return obj
                   is NetworkPlayerPlatformState other
                   && Equals(other);
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(
                HasPlatform,
                Platform,
                LocalPosition,
                LocalYaw);
        }
    }
}
