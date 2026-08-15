using System;
using Unity.Netcode;
using UnityEngine;

namespace InvadersOverboard.Networking.Player
{
    public struct NetworkPlayerAnimationState :
        INetworkSerializable,
        IEquatable<NetworkPlayerAnimationState>
    {
        private const float MovePrecision = 127f;
        private const float VerticalPrecision = 100f;

        private const byte GroundedFlag = 1 << 0;
        private const byte SwimmingFlag = 1 << 1;


        private sbyte moveX;
        private sbyte moveY;

        private short verticalSpeed;

        private byte flags;


        public float MoveX =>
            moveX / MovePrecision;

        public float MoveY =>
            moveY / MovePrecision;

        public float MoveAmount =>
            Mathf.Clamp01(
                Mathf.Sqrt(
                    MoveX * MoveX +
                    MoveY * MoveY));

        public float VerticalSpeed =>
            verticalSpeed / VerticalPrecision;

        public bool IsGrounded =>
            (flags & GroundedFlag) != 0;

        public bool IsSwimming =>
            (flags & SwimmingFlag) != 0;


        public NetworkPlayerAnimationState(
            float moveX,
            float moveY,
            float verticalSpeed,
            bool isGrounded,
            bool isSwimming)
        {
            this.moveX =
                QuantizeMove(moveX);

            this.moveY =
                QuantizeMove(moveY);

            this.verticalSpeed =
                QuantizeVerticalSpeed(
                    verticalSpeed);

            flags = 0;

            if (isGrounded)
                flags |= GroundedFlag;

            if (isSwimming)
                flags |= SwimmingFlag;
        }


        public void NetworkSerialize<T>(
            BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(
                ref moveX);

            serializer.SerializeValue(
                ref moveY);

            serializer.SerializeValue(
                ref verticalSpeed);

            serializer.SerializeValue(
                ref flags);
        }


        public bool Equals(
            NetworkPlayerAnimationState other)
        {
            return moveX == other.moveX
                   && moveY == other.moveY
                   && verticalSpeed ==
                   other.verticalSpeed
                   && flags == other.flags;
        }


        public override bool Equals(
            object obj)
        {
            return obj is
                       NetworkPlayerAnimationState other
                   && Equals(other);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 31 + moveX;
                hash = hash * 31 + moveY;
                hash = hash * 31 + verticalSpeed;
                hash = hash * 31 + flags;

                return hash;
            }
        }


        private static sbyte QuantizeMove(
            float value)
        {
            float clampedValue =
                Mathf.Clamp(value, -1f, 1f);

            return (sbyte)Mathf.RoundToInt(
                clampedValue * MovePrecision);
        }


        private static short QuantizeVerticalSpeed(
            float value)
        {
            int quantizedValue =
                Mathf.RoundToInt(
                    value * VerticalPrecision);

            quantizedValue =
                Mathf.Clamp(
                    quantizedValue,
                    short.MinValue,
                    short.MaxValue);

            return (short)quantizedValue;
        }
    }
}
