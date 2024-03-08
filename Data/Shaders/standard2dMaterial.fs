#version 330 core

uniform int configDrawType;

in vec2 UV;
in vec4 InstanceColor;

uniform vec4 color;

uniform vec4 strokeColor;
uniform uint strokeSize;
uniform uvec4 cornerRadius;

uniform sampler2D color_tex0;

uniform vec2 pixel_size;
uniform uvec2 size_in_pixels;

out vec4 out_color;

void main()
{
    vec2 pixelCoord = UV / pixel_size;

    if (configDrawType == 0) // Solid color
    {
        if (int(pixelCoord.x) < int(strokeSize) || int(size_in_pixels.x - pixelCoord.x) < int(strokeSize) ||
            int(pixelCoord.y) < int(strokeSize) || int(size_in_pixels.y - pixelCoord.y) < int(strokeSize)  )

            out_color = strokeColor;
        
        else out_color = color;
    }

    else if (configDrawType == 1) // Texture
        out_color = texture(color_tex0, UV);

    else if (configDrawType == 2) // Text
    {
        out_color.rgb = InstanceColor.rgb;
        out_color.a = texture(color_tex0, UV).r * InstanceColor.a;
    }

    // calclulate corner roundness

    if (cornerRadius.x > 0 &&
    pixelCoord.x < cornerRadius.x && pixelCoord.y < cornerRadius.x)
    {
        float d = distance(vec2(cornerRadius.x, cornerRadius.x), pixelCoord);

        if (d > cornerRadius.x + strokeSize) discard;
        else if (d > cornerRadius.x) out_color = strokeColor;

    }else if (cornerRadius.y > 0 &&
    size_in_pixels.x - pixelCoord.x < cornerRadius.y && pixelCoord.y < cornerRadius.y)
    {
        float d = distance(vec2(size_in_pixels.x - cornerRadius.y, cornerRadius.y), pixelCoord);

        if (d > cornerRadius.y + strokeSize) discard;
        else if (d > cornerRadius.y) out_color = strokeColor;

    }else if (cornerRadius.z > 0 &&
    size_in_pixels.x - pixelCoord.x < cornerRadius.z && size_in_pixels.y - pixelCoord.y < cornerRadius.z)
    {
        float d = distance(vec2(size_in_pixels.x - cornerRadius.z, size_in_pixels.y - cornerRadius.z), pixelCoord);

        if (d > cornerRadius.z + strokeSize) discard;
        else if (d > cornerRadius.z) out_color = strokeColor;

    }else if (cornerRadius.w > 0 &&
    pixelCoord.x < cornerRadius.w && size_in_pixels.y - pixelCoord.y < cornerRadius.w)
    {
        float d = distance(vec2(cornerRadius.w, size_in_pixels.y - cornerRadius.w), pixelCoord);

        if (d > cornerRadius.w + strokeSize) discard;
        else if (d > cornerRadius.w) out_color = strokeColor;
    }
}