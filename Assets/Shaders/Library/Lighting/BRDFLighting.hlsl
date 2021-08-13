﻿half3 BRDFLighting(BRDFSurface surface,BRDFLight light)
{
    half3 brdf = surface.diffuse;
    
    half D = light.normalDistribution;
    half invVF = light.invNormalizationTerm;
    
    brdf += surface.specular * D / invVF / 4;
    return brdf*light.radiance;
}

half3 BRDFGlobalIllumination(BRDFSurface surface,half3 indirectDiffuse,half3 indirectSpecular)
{
    indirectDiffuse *= surface.ao;
    indirectSpecular *= surface.ao;
    
    half3 giDiffuse = indirectDiffuse * surface.diffuse;
    
    float fresnelTerm = surface.fresnelTerm;
    float3 surfaceReduction = 1.0 / (surface.roughness2 + 1.0) * lerp(surface.specular, surface.grazingTerm, fresnelTerm);
    half3 giSpecular = indirectSpecular * surfaceReduction;
    
    return giDiffuse + giSpecular;
}