using System;
using System.Collections;

public class Maths
{
    // Constant values inside mxo
    private const float walkingSpeed = 0.176f;
    private const float runningSpeed = 0.607f;
    private const float degreesPerByte = 1.41f;
    private Random rand = new Random();
    private const int maxDegrees = 255;

    public Maths()
    {
    }


    public float currentpos(float init, float destination, int millisecsPassed)
    {
        if (init > destination)
            return init - (walkingSpeed * millisecsPassed);
        else
            return init + (walkingSpeed * millisecsPassed);
    }

    public byte getByteForDegrees(int degrees)
    {
        if (degrees > maxDegrees)
            degrees -= maxDegrees;

        byte result = (byte) (0x00 + degrees);
        return result;
    }

    public int getOppositeDegrees(int actual)
    {
        int opposite = (actual + 128);
        if (opposite > maxDegrees)
            opposite -= maxDegrees;
        return opposite;
    }

    public float distance2Coords(float x1, float x2, float z1, float z2)
    {
        return (float) Math.Sqrt(((x2 - x1) * (x2 - x1)) + ((z2 - z1) * (z2 - z1)));
    }

    public bool IsInCircle(float src_x, float src_z, float otherpoint_x, float otherpoint_z, float radius)
    {
        return (Math.Pow((src_x - otherpoint_x), 2) + Math.Pow(src_z - otherpoint_z, 2)) <= radius * radius;
    }

    public int diceRollOperant()
    {
        Random rand = new Random();
        int dice = rand.Next(1, 3);

        return dice;
    }


    public float RandomBetween(float min, float max)
    {
        return min + (float) rand.NextDouble() * (max - min);
    }

    // ToDo: Implement the methods from the file..it could be the paradise^^


    public static float getDistance(float x1, float y1, float z1, float x2, float y2, float z2)
    {
        float deltaX = x1 - x2;
        float deltaY = y1 - y2;
        float deltaZ = z1 - z2;
        float distance = (float) Math.Sqrt(deltaX * deltaX + deltaY * deltaY + deltaZ * deltaZ);
        return distance;
    }
}