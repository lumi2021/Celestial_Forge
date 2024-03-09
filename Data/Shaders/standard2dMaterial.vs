#version 330 core

uniform int configDrawType;
/* DRAW TYPES:

0   Solid Color;
1   Texture;
2   Text;

*/

in vec2 aPosition;
in vec2 aTextureCoord;

in mat4 aInstanceWorldMatrix;
in mat4 aInstanceTexCoordMatrix;
in vec4 aInstanceColor;

uniform mat4 world;
uniform mat4 projection;

out vec2 UV;
out vec4 InstanceColor;

void main()
{
    if (configDrawType == 2) // TEXT
    {
        gl_Position = vec4(aPosition, 0, 1.0) * aInstanceWorldMatrix * world * projection;
        UV = (vec4(aTextureCoord, 0, 1.0) * aInstanceTexCoordMatrix).xy;
        InstanceColor = aInstanceColor;
    }

    else // OTHERS
    {
        gl_Position = vec4(aPosition, 0, 1.0) * world * projection;
        UV = aTextureCoord;
    }

}