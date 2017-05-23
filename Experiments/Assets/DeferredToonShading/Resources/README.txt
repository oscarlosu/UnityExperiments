Source: http://www.gamasutra.com/blogs/DavidLeon/20170519/298374/NextGen_Cel_Shading_in_Unity_56.php

Note: Author forgets to mentions that DeferredShadingToon.shader needs to have #include "UnityStandardBRDFCustom.cginc"

1. Set Custom Deferred Shader in Editor->Project Settings->Graphics in Built-in shader stettings.
2. Remember that the camera must be set to use deferred rendering and that Orthographics Perspective doesn't work with Deferred Shading (this includes the Scene View)

