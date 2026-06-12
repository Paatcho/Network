void meltingGoo_float(float3 objPos, float melting, float meltPuddleScale, float meltPuddlePower, float tinyBloat, out float3 finalPosition)
{
    finalPosition = objPos;
    finalPosition.g += min(0, lerp(melting - 0.5 - objPos.g, objPos.g, tinyBloat * objPos.g));
    finalPosition.rb *= 1 + pow((1 - melting) * meltPuddleScale, meltPuddlePower);
}